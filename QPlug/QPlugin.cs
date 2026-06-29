using System.Collections.Immutable;
using LoupixDeck.PluginSdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QPlug;

public sealed partial class QPlugin
{
    private readonly IPluginHost host;
    private readonly ServiceProvider sp;
    private readonly CompositeMenuContributor menuContributor;
    private readonly ImmutableArray<PluginCommandBase> CommandsList;

    public QPlugin(IPluginHost host)
    {
        this.host = host;
        sp = CreateServiceCollection(host).BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        menuContributor = new(sp.GetServices<MenuContributorBase>().ToImmutableArray());
        CommandsList = sp.GetServices<PluginCommandBase>().ToImmutableArray();
    }

    public static PluginMetadata Metadata { get; } = new()
    {
        Id = "qplug-alpha",
        Author = "Qwazwak",
        Name = "QPlug",

        Version = new Version(0, 1, 0),
        SdkVersion = SdkInfo.Version,
    };

    public Task<IReadOnlyList<MenuNode>> GetMenuNodes(ButtonTargets target) => menuContributor.GetMenuNodes(target);
    public ImmutableArray<IPluginCommand> GetCommandsList() => ImmutableArray<IPluginCommand>.CastUp(CommandsList);
    public void Dispose()
    {
        sp.Dispose();
    }
}