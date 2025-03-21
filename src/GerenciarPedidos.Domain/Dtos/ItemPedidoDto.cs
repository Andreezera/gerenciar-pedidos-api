namespace GerenciarPedidos.Domain.Dtos;

public class ItemPedidoDto
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal Valor { get; set; }
}
