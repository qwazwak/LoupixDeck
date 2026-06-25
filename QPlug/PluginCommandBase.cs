using System.Collections.Immutable;
using LoupixDeck.PluginSdk;
using NAudio.CoreAudioApi;

namespace QPlug;

public abstract class MenuContributorBase(IPluginHost Host)
{
    protected readonly IPluginHost host = Host;
    protected IPluginLogger log => host.Logger;

    public abstract ValueTask<ImmutableList<MenuNode>> GetMenuNodes(ButtonTargets target);
}

public abstract class PluginCommandBase(IPluginHost Host) : IPluginCommand
{
    protected readonly IPluginHost host = Host;
    protected IPluginLogger log => host.Logger;

    public abstract CommandDescriptor Descriptor { get; }
    public abstract ButtonTargets SupportedTargets { get; }
    public virtual MenuContributorBase? MenuContributor => null;

    public abstract Task Execute(CommandContext ctx);
}
