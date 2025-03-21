using GerenciarPedidos.Domain.Entities;

namespace GerenciarPedidos.Data.Interfaces;

public interface IPedidoRepository
{
    Task<Pedido> AddPedidoAsync(Pedido pedido);
    Task<Pedido?> GetPedidoByIdAsync(int id);
    Task<IEnumerable<Pedido>> ListPedidosByStatusAsync(string status);
}
