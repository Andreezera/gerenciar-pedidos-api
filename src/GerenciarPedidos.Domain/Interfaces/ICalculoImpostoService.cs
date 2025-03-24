namespace GerenciarPedidos.Domain.Interfaces;

public interface ICalculoImpostoService
{
    decimal Calcular(decimal valorTotalItens);
}
