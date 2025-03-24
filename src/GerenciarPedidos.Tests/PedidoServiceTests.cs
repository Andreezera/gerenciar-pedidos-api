using FluentAssertions;
using GerenciarPedidos.Data.Interfaces;
using GerenciarPedidos.Domain.Dtos;
using GerenciarPedidos.Domain.Entities;
using GerenciarPedidos.Domain.Interfaces;
using GerenciarPedidos.Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace GerenciarPedidos.Tests;

public class PedidoServiceTests
{
    private readonly Mock<IPedidoRepository> _pedidoRepositoryMock;
    private readonly Mock<ILogger<PedidoService>> _loggerMock;
    private readonly Mock<ICalculoImpostoService> _calculoImpostoMock;
    private readonly Mock<ICalculoImpostoFactory> _calculoImpostoFactoryMock;
    private readonly IMemoryCache _cache;
    private readonly PedidoService _pedidoService;

    private const string CacheKey = "UltimosPedidos";

    public PedidoServiceTests()
    {
        _pedidoRepositoryMock = new Mock<IPedidoRepository>();
        _loggerMock = new Mock<ILogger<PedidoService>>();
        _calculoImpostoMock = new Mock<ICalculoImpostoService>();
        _calculoImpostoFactoryMock = new Mock<ICalculoImpostoFactory>();

        _calculoImpostoFactoryMock.Setup(f => f.CriarCalculo())
                                  .Returns(_calculoImpostoMock.Object);

        var memoryCacheOptions = new MemoryCacheOptions();
        _cache = new MemoryCache(memoryCacheOptions);

        _pedidoService = new PedidoService(
            _pedidoRepositoryMock.Object,
            _loggerMock.Object,
            _calculoImpostoFactoryMock.Object,
            _cache);
    }

    [Fact]
    public async Task CreatePedidoAsync_DeveCriarPedidoComSucesso()
    {
        // Arrange
        var pedidoDto = new PedidoCreateDto
        {
            ClienteId = 1,
            Itens = new List<ItemPedidoDto>
            {
                new ItemPedidoDto { ProdutoId = 1001, Quantidade = 2, Valor = 50 }
            }
        };

        var pedidoEsperado = new Pedido
        {
            PedidoId = 1,
            ClienteId = 1,
            Itens = new List<ItemPedido>
            {
                new ItemPedido { ProdutoId = 1001, Quantidade = 2, Valor = 50 }
            },
            Status = "Criado",
            Imposto = 15
        };

        _calculoImpostoMock.Setup(ci => ci.Calcular(It.IsAny<decimal>()))
                           .Returns(15);

        _pedidoRepositoryMock.Setup(pr => pr.AddPedidoAsync(It.IsAny<Pedido>()))
                             .ReturnsAsync((Pedido p) => p)
                             .Callback<Pedido>(p => p.PedidoId = 1);

        // Act
        var resultado = await _pedidoService.CreatePedidoAsync(pedidoDto);

        // Assert
        resultado.Should().BeEquivalentTo(pedidoEsperado, options => options.Excluding(p => p.PedidoId));
        resultado.PedidoId.Should().Be(1);

        _pedidoRepositoryMock.Verify(pr => pr.AddPedidoAsync(It.IsAny<Pedido>()), Times.Once);
        _calculoImpostoMock.Verify(ci => ci.Calcular(It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public async Task CreatePedidoAsync_DeveLancarExcecao_QuandoPedidoForDuplicado()
    {
        // Arrange
        var pedidoDto = new PedidoCreateDto
        {
            ClienteId = 1,
            Itens = new List<ItemPedidoDto>
            {
                new ItemPedidoDto { ProdutoId = 1001, Quantidade = 2, Valor = 50 }
            }
        };

        _pedidoRepositoryMock.Setup(repo => repo.IsPedidoDuplicadoAsync(pedidoDto.ClienteId, pedidoDto.Itens))
                             .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _pedidoService.CreatePedidoAsync(pedidoDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Existe um pedido duplicado para esse cliente");

        _pedidoRepositoryMock.Verify(repo => repo.IsPedidoDuplicadoAsync(pedidoDto.ClienteId, pedidoDto.Itens), Times.Once);
        _pedidoRepositoryMock.Verify(repo => repo.AddPedidoAsync(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public async Task GetPedidoByIdAsync_DeveRetornarPedido_QuandoIdExistir()
    {
        // Arrange
        var pedidoId = 1;
        var pedidoEsperado = new Pedido { PedidoId = pedidoId, ClienteId = 1, Status = "Criado" };

        _pedidoRepositoryMock.Setup(pr => pr.GetPedidoByIdAsync(pedidoId))
                             .ReturnsAsync(pedidoEsperado);

        // Act
        var resultado = await _pedidoService.GetPedidoByIdAsync(pedidoId);

        // Assert
        resultado.Should().BeEquivalentTo(pedidoEsperado);
        _pedidoRepositoryMock.Verify(pr => pr.GetPedidoByIdAsync(pedidoId), Times.Once);
    }

    [Fact]
    public async Task GetPedidoByIdAsync_DeveRetornarNull_QuandoIdNaoExistir()
    {
        // Arrange
        var pedidoId = 1;

        _pedidoRepositoryMock.Setup(pr => pr.GetPedidoByIdAsync(pedidoId))
                             .ReturnsAsync((Pedido)null);

        // Act
        var resultado = await _pedidoService.GetPedidoByIdAsync(pedidoId);

        // Assert
        resultado.Should().BeNull();
        _pedidoRepositoryMock.Verify(pr => pr.GetPedidoByIdAsync(pedidoId), Times.Once);
    }

    [Fact]
    public async Task ListPedidosByStatusAsync_DeveRetornarPedidosDoCache_QuandoDisponivel()
    {
        // Arrange
        var status = "Criado";
        var pedidosEsperados = new List<Pedido>
        {
            new Pedido { PedidoId = 1, ClienteId = 1, Status = status },
            new Pedido { PedidoId = 2, ClienteId = 2, Status = status }
        };

        _cache.Set(CacheKey, pedidosEsperados, TimeSpan.FromMinutes(5));

        // Act
        var resultado = await _pedidoService.ListPedidosByStatusAsync(status);

        // Assert
        resultado.Should().BeEquivalentTo(pedidosEsperados);
    }

    [Fact]
    public async Task ListPedidosByStatusAsync_DeveRetornarPedidosDoRepositorio_QuandoCacheNaoDisponivel()
    {
        // Arrange
        var status = "Criado";
        var pedidosEsperados = new List<Pedido>
        {
            new Pedido { PedidoId = 1, ClienteId = 1, Status = status },
            new Pedido { PedidoId = 2, ClienteId = 2, Status = status }
        };

        _pedidoRepositoryMock.Setup(pr => pr.ListPedidosByStatusAsync(status))
                             .ReturnsAsync(pedidosEsperados);

        // Act
        var resultado = await _pedidoService.ListPedidosByStatusAsync(status);

        // Assert
        resultado.Should().BeEquivalentTo(pedidosEsperados);
        _pedidoRepositoryMock.Verify(pr => pr.ListPedidosByStatusAsync(status), Times.Once);
    }

    [Fact]
    public void AtualizarCache_DeveAdicionarPedido_QuandoCacheJaExiste()
    {
        // Arrange
        var pedido = new Pedido { PedidoId = 1, ClienteId = 10, Status = "Criado" };
        var pedidosIniciais = new List<Pedido> { new Pedido { PedidoId = 2, ClienteId = 20, Status = "Criado" } };

        _cache.Set(CacheKey, pedidosIniciais, TimeSpan.FromMinutes(5));

        // Act
        var service = new PedidoService(_pedidoRepositoryMock.Object, _loggerMock.Object, _calculoImpostoFactoryMock.Object, _cache);
        var metodoPrivado = typeof(PedidoService).GetMethod("AtualizarCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        metodoPrivado?.Invoke(service, new object[] { pedido });

        // Assert
        _cache.TryGetValue(CacheKey, out List<Pedido>? pedidosCache).Should().BeTrue();
        pedidosCache.Should().NotBeNull();
        pedidosCache.Should().HaveCount(2);
        pedidosCache.Should().Contain(p => p.PedidoId == 1 && p.ClienteId == 10);
    }

    [Fact]
    public void AtualizarCache_DeveCriarCache_SeNaoExistir()
    {
        // Arrange
        var pedido = new Pedido { PedidoId = 1, ClienteId = 10, Status = "Criado" };

        // Act
        var service = new PedidoService(_pedidoRepositoryMock.Object, _loggerMock.Object, _calculoImpostoFactoryMock.Object, _cache);
        var metodoPrivado = typeof(PedidoService).GetMethod("AtualizarCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        metodoPrivado?.Invoke(service, new object[] { pedido });

        // Assert
        _cache.TryGetValue(CacheKey, out List<Pedido>? pedidosCache).Should().BeTrue();
        pedidosCache.Should().NotBeNull();
        pedidosCache.Should().HaveCount(1);
        pedidosCache.Should().Contain(p => p.PedidoId == 1 && p.ClienteId == 10);
    }

}
