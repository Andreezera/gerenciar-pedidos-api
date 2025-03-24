using GerenciarPedidos.Domain.Interfaces;

namespace GerenciarPedidos.Domain.Services.CalculoImposto;

public class CalculoImpostoReforma : ICalculoImpostoService
{
    public decimal Calcular(decimal valorTotalItens)
    {
        return valorTotalItens * 0.2m;
    }
}