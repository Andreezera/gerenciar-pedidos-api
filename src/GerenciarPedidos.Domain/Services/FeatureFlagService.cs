using Microsoft.Extensions.Configuration;

namespace GerenciarPedidos.Domain.Services;

public class FeatureFlagService
{
    private readonly IConfiguration _configuration;
    
    public FeatureFlagService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsFeatureEnabled(string featureName)
    {
        return _configuration.GetValue<bool>($"FeatureFlags:{featureName}");
    }
}
