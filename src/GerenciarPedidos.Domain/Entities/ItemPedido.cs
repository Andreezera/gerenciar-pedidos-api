namespace GerenciarPedidos.Domain.Entities;

public class ItemPedido
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal Valor { get; set; }
}
