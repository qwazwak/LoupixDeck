using System.Collections.Immutable;
using LoupixDeck.PluginSdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using QPlug.Commands;

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
            .AddCommand<MuteToggleCommand>()
            .AddCommand<VolumeAdjustUpCommand>()
            .AddCommand<VolumeAdjustDownCommand>()
            ;

        services.AddTransient(typeof(ILogger<>), typeof(Loggers.LoggerT<>));
        services.AddScoped<SoundVolumeViewExe>();


        services.AddKeyedSingleton<DefaultDeviceReferencer>(Role.Console);
        services.AddKeyedSingleton<DefaultDeviceReferencer>(Role.Multimedia);
        services.AddKeyedSingleton<DefaultDeviceReferencer>(Role.Communications);
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