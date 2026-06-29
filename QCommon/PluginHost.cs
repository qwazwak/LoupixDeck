using System.Collections.Immutable;
using LoupixDeck.PluginSdk;

namespace QCommon;

public abstract class PluginHost<TPlugin> : LoupixPlugin, IMenuContributor
    where TPlugin : PluginBase, IPlugin<TPlugin>
{
    private TPlugin? impl;
    private TPlugin Impl => impl ?? throw new InvalidOperationException("Plugin not initialized");

    public override sealed PluginMetadata Metadata => TPlugin.Metadata;
    public override sealed void Initialize(IPluginHost host)
    {
        //SetAllLoggers(host.Logger);
        impl = TPlugin.Init(host);
    }

    public override sealed void Shutdown()
    {
        Interlocked.Exchange(ref impl, null)?.Dispose();
        //ClearAllLoggers();
    }

    public override sealed IEnumerable<IPluginCommand> GetCommands()
    {
        ImmutableArray<PluginCommandBase> cmds = Impl.GetCommandsList();
        if (cmds.IsDefaultOrEmpty)
            return ImmutableArray<IPluginCommand>.Empty;
        return ImmutableArray<IPluginCommand>.CastUp(cmds);
    }

    public Task<IReadOnlyList<MenuNode>> GetMenuNodes(ButtonTargets target) => Impl.GetMenuNodes(target);
}