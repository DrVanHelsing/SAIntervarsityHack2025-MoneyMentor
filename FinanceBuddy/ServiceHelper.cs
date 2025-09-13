using Microsoft.Extensions.DependencyInjection;

namespace FinanceBuddy;

public static class ServiceHelper
{
    private static IServiceProvider? _provider;
    public static void Initialize(IServiceProvider provider) => _provider = provider;
    public static T GetRequiredService<T>() where T : notnull
    {
        if (_provider == null) throw new InvalidOperationException("Service provider not initialized.");
        return _provider.GetRequiredService<T>();
    }
}