using LoupixDeck.PluginSdk;

namespace QPlug;

public sealed class QPlugin : LoupixPlugin, IMenuContributor
{
    private QPluginImpl? impl;
    private QPluginImpl Impl => impl ?? throw new InvalidOperationException("Plugin not initialized");

    public override PluginMetadata Metadata => QPluginImpl.Metadata;
    public override void Initialize(IPluginHost host) => impl = new(host);
    public override void Shutdown() => Interlocked.Exchange(ref impl, null)?.Dispose();
    public override IEnumerable<IPluginCommand> GetCommands() => Impl.GetCommandsList();
    public Task<IReadOnlyList<MenuNode>> GetMenuNodes(ButtonTargets target) => Impl.GetMenuNodes(target);
}