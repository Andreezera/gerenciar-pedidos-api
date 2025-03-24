using GerenciarPedidos.Data.Interfaces;
using GerenciarPedidos.Domain.Dtos;
using GerenciarPedidos.Domain.Entities;

namespace GerenciarPedidos.Data.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly List<Pedido> _pedidos = new();
    private int _idCounter = 1;

    public async Task<Pedido> AddPedidoAsync(Pedido pedido)
    {
        pedido.Id = _idCounter++;
        _pedidos.Add(pedido);

        return await Task.FromResult(pedido);
    }

    public async Task<Pedido?> GetPedidoByIdAsync(int id)
    {
        return await Task.FromResult(_pedidos.FirstOrDefault(p => p.Id == id));
    }

    public async Task<IEnumerable<Pedido>> ListPedidosByStatusAsync(string status)
    {
        var result = await Task.FromResult(_pedidos.Where(p => p.Status == status));

        return result;
    }

    public async Task<bool> IsPedidoDuplicadoAsync(int clienteId, List<ItemPedidoDto> itens)
    {
        return await Task.FromResult(_pedidos.Any(p =>
            p.ClienteId == clienteId &&
            p.Itens.Count == itens.Count &&
            p.Itens.All(i => itens.Any(dto =>
                dto.ProdutoId == i.ProdutoId &&
                dto.Quantidade == i.Quantidade &&
                dto.Valor == i.Valor))));
    }
}
