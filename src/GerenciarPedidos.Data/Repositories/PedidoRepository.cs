using GerenciarPedidos.Data.Interfaces;
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
}
