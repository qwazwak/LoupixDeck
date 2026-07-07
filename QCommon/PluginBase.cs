using System.Collections.Immutable;
using LoupixDeck.PluginSdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QCommon.Internal;
using QCommon.Internal.Logging;

namespace QCommon;

public interface IPlugin : IDisposable
{
    ImmutableArray<PluginCommandBase> GetCommandsList();
}

public interface IPlugin<TSelf> : IPlugin
    where TSelf : class, IPlugin<TSelf>
{
    static abstract PluginMetadata Metadata { get; }
    static abstract TSelf Init(IPluginHost host);
}

public abstract class PluginBase : IPlugin
{
    protected readonly IPluginHost host;
    protected readonly ServiceProvider sp;
    private readonly CompositeMenuContributor menuContributor;
    private readonly ImmutableArray<PluginCommandBase> CommandsList;
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Standard name")]
    protected bool isDisposed { get; private set; }

    protected PluginBase(IPluginHost host)
    {
        this.host = host;
        sp = CreateServiceProvider(host, ConfigureServices);
        menuContributor = sp.GetRequiredService<CompositeMenuContributor>();
        CommandsList = sp.GetServices<PluginCommandBase>().ToImmutableArray();
    }

    private static ServiceProvider CreateServiceProvider(IPluginHost host, Action<ServiceCollection>? configureServices)
        => CreateServices(host, configureServices).BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

    private static ServiceCollection CreateServices(IPluginHost host, Action<ServiceCollection>? configureServices)
    {
        ServiceCollection services = new();
        services.AddSingleton(host);
        services.AddLogging(static l =>
        {
            l.ClearProviders();
#if !DEBUG
            l.SetMinimumLevel(LogLevel.Information);
#endif
            l.Services.AddSingleton<ILoggerProvider, PluginLoggerProvider>();
            l.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
        });

        services.AddSingleton<CompositeMenuContributor>();

        configureServices?.Invoke(services);
        return services;
    }

    protected abstract void ConfigureServices(ServiceCollection services);

    public Task<IReadOnlyList<MenuNode>> GetMenuNodes(ButtonTargets target) => menuContributor.GetMenuNodes(target);

    public ImmutableArray<PluginCommandBase> GetCommandsList() => CommandsList;

    protected virtual void DisposeBase(bool disposing)
    {
        if (isDisposed)
            return;
        Dispose(disposing);
        isDisposed = true;
    }
    protected virtual void Dispose(bool disposing)
    {
        // if (disposing)
            sp.Dispose();
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
