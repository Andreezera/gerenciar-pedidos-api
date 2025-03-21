using GerenciarPedidos.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciarPedidos.Domain.Services.CalculoImposto;

public class CalculoImpostoFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FeatureFlagService _featureFlagService;

    public CalculoImpostoFactory(IServiceProvider serviceProvider, FeatureFlagService featureFlagService)
    {
        _serviceProvider = serviceProvider;
        _featureFlagService = featureFlagService;
    }

    public ICalculoImposto CriarCalculo()
    {
        if (_featureFlagService.IsFeatureEnabled("UsarReformaTributaria"))
        {
            return _serviceProvider.GetRequiredService<CalculoImpostoReforma>();
        }

        return _serviceProvider.GetRequiredService<CalculoImpostoVigente>();
    }
}
