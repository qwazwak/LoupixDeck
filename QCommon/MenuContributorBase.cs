using System.Collections.Immutable;
using LoupixDeck.PluginSdk;

namespace QCommon;

public abstract class MenuContributorBase(IPluginHost Host)
{
    protected readonly IPluginHost host = Host;
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "std field name")]
    protected IPluginLogger log => host.Logger;

    public abstract ValueTask<ImmutableList<MenuNode>> GetMenuNodes(ButtonTargets target);
}
