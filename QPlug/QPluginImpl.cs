using System.Collections.Immutable;
using LoupixDeck.PluginSdk;

namespace QPlug;

public class QPluginImpl(IPluginHost host)
{
    private readonly CompositeMenuContributor menuContributor = new([
            new AudioOutControlMenuContributor(host)
        ]);
    private readonly ImmutableArray<IPluginCommand> CommandsList = [
        new TestCommand(host),
        new AudioOutCycler(host),
        new AudioOutSetter(host),
    ];

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
        foreach (IPluginCommand item in CommandsList)
        {
            if (item is IDisposable disposable)
                disposable.Dispose();
        }
    }

}
