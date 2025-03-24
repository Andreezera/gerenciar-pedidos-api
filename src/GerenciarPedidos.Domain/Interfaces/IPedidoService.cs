using GerenciarPedidos.Domain.Dtos;
using GerenciarPedidos.Domain.Entities;

namespace GerenciarPedidos.Domain.Interfaces;

public interface IPedidoService
{
    Task<Pedido> CreatePedidoAsync(PedidoCreateDto pedido);
    Task<Pedido?> GetPedidoByIdAsync(int id);
    Task<IEnumerable<Pedido>> ListPedidosByStatusAsync(string status);
}
