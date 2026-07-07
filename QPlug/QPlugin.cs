using LoupixDeck.PluginSdk;
using Microsoft.Extensions.DependencyInjection;
using NAudio.CoreAudioApi;
using QCommon;
using QPlug.Commands;

namespace QPlug;
public sealed class QPluginHost : PluginHost<QPlugin>; //IPluginSettingsPage

public sealed class QPlugin(IPluginHost host) : PluginBase(host), IPlugin<QPlugin>
{
    static QPlugin IPlugin<QPlugin>.Init(IPluginHost host) => new(host);

    public static PluginMetadata Metadata { get; } = new()
    {
        Id = "qplug-alpha",
        Author = "Qwazwak",
        Name = "QPlug",

        Version = new Version(0, 1, 0),
        SdkVersion = SdkInfo.Version,
    };

    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddMenuContributor<AudioOutControlMenuContributor>();

        services.AddCommand<TestCommand>()
            .AddCommand<AudioOutCycler>()
            .AddCommand<AudioOutSetter>()
            .AddCommand<MuteToggleCommand>()
            .AddCommand<VolumeAdjustUpCommand>()
            .AddCommand<VolumeAdjustDownCommand>()
            ;

        services.AddScoped<SoundVolumeViewExe>();

        services.AddKeyedSingleton<DefaultDeviceReferencer>(Role.Console);
        services.AddKeyedSingleton<DefaultDeviceReferencer>(Role.Multimedia);
        services.AddKeyedSingleton<DefaultDeviceReferencer>(Role.Communications);
        services.AddSingleton<MetaDefaultDeviceReferencer>();
    }
}