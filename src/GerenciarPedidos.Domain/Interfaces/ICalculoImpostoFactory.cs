namespace GerenciarPedidos.Domain.Interfaces;

public interface ICalculoImpostoFactory
{
    ICalculoImpostoService CriarCalculo();
}
