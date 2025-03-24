using GerenciarPedidos.Data.Interfaces;
using GerenciarPedidos.Domain.Dtos;
using GerenciarPedidos.Domain.Entities;
using GerenciarPedidos.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GerenciarPedidos.Domain.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly ILogger<PedidoService> _logger;
    private readonly ICalculoImpostoFactory _calculoImpostoFactory;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "UltimosPedidos";

    public PedidoService(IPedidoRepository pedidoRepository, ILogger<PedidoService> logger, ICalculoImpostoFactory calculoImpostoFactory, IMemoryCache cache)
    {
        _pedidoRepository = pedidoRepository;
        _logger = logger;
        _calculoImpostoFactory = calculoImpostoFactory;
        _cache = cache;
    }

    public async Task<Pedido> CreatePedidoAsync(PedidoCreateDto pedidoDto)
    {
        _logger.LogInformation("Criando pedido para ClienteId: {ClienteId} | Itens: {@Itens}", pedidoDto.ClienteId, pedidoDto.Itens);

        if (pedidoDto.Itens == null || !pedidoDto.Itens.Any())
        {
            _logger.LogWarning("Pedido rejeitado | Motivo: Nenhum item foi encontrado | ClienteId: {ClienteId}", pedidoDto.ClienteId);
            throw new ArgumentException("O pedido deve conter itens");
        }

        var pedidoDuplicado = await _pedidoRepository.IsPedidoDuplicadoAsync(pedidoDto.ClienteId, pedidoDto.Itens);
        if (pedidoDuplicado)
        {
            _logger.LogWarning("Pedido rejeitado | Motivo: Pedido duplicado | ClienteId: {ClienteId}", pedidoDto.ClienteId);
            throw new InvalidOperationException("Existe um pedido duplicado para esse cliente");
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

        _logger.LogInformation("Imposto calculado | Regra aplicada: {Regra} | Valor: {Imposto}", calculoImposto.GetType().Name, pedido.Imposto);

        var pedidoCriado = await _pedidoRepository.AddPedidoAsync(pedido);

        AtualizarCache(pedidoCriado);

        _logger.LogInformation("Pedido criado com sucesso | PedidoId: {PedidoId} | ClienteId: {ClienteId} | Status: {Status}",
                               pedidoCriado.PedidoId, pedidoCriado.ClienteId, pedidoCriado.Status);

        return pedidoCriado;
    }

    public async Task<Pedido?> GetPedidoByIdAsync(int id)
    {
        _logger.LogInformation("Buscando pedido por ID | PedidoId: {PedidoId}", id);

        var pedido = await _pedidoRepository.GetPedidoByIdAsync(id);

        if (pedido == null)
        {
            _logger.LogWarning("Pedido não encontrado | PedidoId: {PedidoId}", id);
        }
        else
        {
            _logger.LogInformation("Pedido encontrado | PedidoId: {PedidoId} | ClienteId: {ClienteId} | Status: {Status}",
                                   pedido.PedidoId, pedido.ClienteId, pedido.Status);
        }

        return pedido;
    }

    public async Task<IEnumerable<Pedido>> ListPedidosByStatusAsync(string status)
    {
        _logger.LogInformation("Listando pedidos com status: {Status}", status);

        if (_cache.TryGetValue(CacheKey, out List<Pedido>? pedidosCache))
        {
            pedidosCache ??= new List<Pedido>();

            _logger.LogInformation("Retornando pedidos do cache | Status: {Status}", status);
            return pedidosCache.Where(p => p.Status == status);
        }

        var pedidos = (await _pedidoRepository.ListPedidosByStatusAsync(status)).ToList();

        _cache.Set(CacheKey, pedidos, TimeSpan.FromMinutes(5));

        _logger.LogInformation("Pedidos armazenados no cache | Total: {TotalPedidos} | Status: {Status}", pedidos.Count, status);

        return pedidos;
    }

    private void AtualizarCache(Pedido novoPedido)
    {
        if (!_cache.TryGetValue(CacheKey, out List<Pedido>? pedidosCache) || pedidosCache == null)
        {
            pedidosCache = new List<Pedido>();
        }

        pedidosCache.Add(novoPedido);

        _cache.Set(CacheKey, pedidosCache, TimeSpan.FromMinutes(5));

        _logger.LogInformation("Cache atualizado com novo pedido | PedidoId: {PedidoId} | ClienteId: {ClienteId}",
                               novoPedido.PedidoId, novoPedido.ClienteId);
    }
}
