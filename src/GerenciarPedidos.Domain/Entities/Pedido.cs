namespace GerenciarPedidos.Domain.Entities;

public class Pedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int ClienteId { get; set; }
    public decimal Imposto { get; set; }
    public List<ItemPedido> Itens { get; set; } = new();
    public string Status { get; set; } = "Criado";
}
