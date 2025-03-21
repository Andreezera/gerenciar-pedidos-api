namespace GerenciarPedidos.Domain.Dtos;

public class PedidoCreateDto
{
    public int PedidoId { get; set; }
    public int ClienteId { get; set; }
    public List<ItemPedidoDto> Itens { get; set; } = new();
}
