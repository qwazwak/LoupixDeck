using System.Collections.Immutable;
using LoupixDeck.PluginSdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QPlug;

public partial class QPlugin
{
    private static ServiceCollection CreateServiceCollection(IPluginHost host)
    {
        ServiceCollection services = new();
        services.AddSingleton(host);

        services.AddMenuContributor<AudioOutControlMenuContributor>();

        services.AddCommand<TestCommand>()
            .AddCommand<AudioOutCycler>()
            .AddCommand<AudioOutSetter>()
            .AddCommand<VolumeAdjustCommand>();

        services.AddTransient(typeof(ILogger<>), typeof(Loggers.LoggerT<>));
        services.AddSingleton<MetaDefaultDeviceReferencer>();

        return services;
    }
}

file static class ServicesExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCommand<TCommand>() where TCommand : PluginCommandBase => services.AddSingleton<PluginCommandBase, TCommand>();
        public IServiceCollection AddMenuContributor<TContributor>() where TContributor : MenuContributorBase => services.AddSingleton<MenuContributorBase, TContributor>();
    }
}