namespace GerenciarPedidos.Domain.Interfaces;

public interface ICalculoImposto
{
    decimal Calcular(decimal valorTotalItens);
}
