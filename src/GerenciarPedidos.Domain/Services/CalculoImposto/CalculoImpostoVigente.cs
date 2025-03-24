using GerenciarPedidos.Domain.Interfaces;

namespace GerenciarPedidos.Domain.Services.CalculoImposto;

public class CalculoImpostoVigente : ICalculoImpostoService
{
    public decimal Calcular(decimal valorTotalItens)
    {
        return valorTotalItens * 0.3m;
    }
}
