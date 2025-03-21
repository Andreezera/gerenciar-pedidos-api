using GerenciarPedidos.Domain.Interfaces;
using GerenciarPedidos.Data.Interfaces;
using Microsoft.Extensions.Logging;
using GerenciarPedidos.Domain.Entities;
using GerenciarPedidos.Domain.Dtos;
using GerenciarPedidos.Domain.Services.CalculoImposto;

namespace GerenciarPedidos.Domain.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly ILogger<PedidoService> _logger;
    private readonly CalculoImpostoFactory _calculoImpostoFactory;

    public PedidoService(IPedidoRepository pedidoRepository, ILogger<PedidoService> logger, CalculoImpostoFactory calculoImpostoFactory)
    {
        _pedidoRepository = pedidoRepository;
        _logger = logger;
        _calculoImpostoFactory = calculoImpostoFactory;
    }

    public async Task<Pedido> CreatePedidoAsync(PedidoCreateDto pedidoDto)
    {
        _logger.LogInformation("Criando pedido para ClienteId: {ClienteId}", pedidoDto.ClienteId);

        if (pedidoDto.Itens == null || !pedidoDto.Itens.Any())
        {
            _logger.LogWarning("Pedido rejeitado: Nenhum item foi encontrado.");
            throw new ArgumentException("O pedido deve conter itens");
        }

        var pedido = new Pedido
        {
            PedidoId = pedidoDto.ClienteId,
            ClienteId = pedidoDto.ClienteId,
            Itens = pedidoDto.Itens.Select(i => new ItemPedido
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade,
                Valor = i.Valor
            }).ToList(),
            Status = "Criado"
        };

        decimal totalItens = pedido.Itens.Sum(i => i.Valor);

        var calculoImposto = _calculoImpostoFactory.CriarCalculo();
        pedido.Imposto = calculoImposto.Calcular(totalItens);

        _logger.LogInformation("Imposto calculado com a regra [{Regra}]: {Imposto}", calculoImposto.GetType().Name, pedido.Imposto);

        return await _pedidoRepository.AddPedidoAsync(pedido);
    }

    public async Task<Pedido?> GetPedidoByIdAsync(int id)
    {
        _logger.LogInformation("Buscando pedido com ID: {Id}", id);
        var pedido = await _pedidoRepository.GetPedidoByIdAsync(id);

        if (pedido == null)
        {
            _logger.LogWarning("Pedido com ID {Id} não encontrado", id);
        }

        return pedido;
    }

    public async Task<IEnumerable<Pedido>> ListPedidosByStatusAsync(string status)
    {
        _logger.LogInformation("Listando pedidos com status: {Status}", status);
        return await _pedidoRepository.ListPedidosByStatusAsync(status);
    }
}
