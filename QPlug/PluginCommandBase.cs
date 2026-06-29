using System.Collections.Immutable;
using LoupixDeck.PluginSdk;

namespace QPlug;

public abstract class MenuContributorBase(IPluginHost Host)
{
    protected readonly IPluginHost host = Host;
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "std field name")]
    protected IPluginLogger log => host.Logger;

    public abstract ValueTask<ImmutableList<MenuNode>> GetMenuNodes(ButtonTargets target);
}

public abstract class PluginCommandBase(IPluginHost Host) : IPluginCommand
{
    protected readonly IPluginHost host = Host;
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "std field name")]
    protected IPluginLogger log => host.Logger;

    public abstract CommandDescriptor Descriptor { get; }
    public abstract ButtonTargets SupportedTargets { get; }
    public virtual MenuContributorBase? MenuContributor => null;

    public abstract Task Execute(CommandContext ctx);

    protected bool CheckValidParameterCount(CommandContext ctx) => !CheckInvalidParameterCount(ctx, MinimumParameterCount);
    protected bool CheckInvalidParameterCount(CommandContext ctx) => CheckInvalidParameterCount(ctx, MinimumParameterCount);

    protected bool CheckValidParameterCount(CommandContext ctx, int minimumParameterCount) => !CheckInvalidParameterCount(ctx, minimumParameterCount);
    protected bool CheckInvalidParameterCount(CommandContext ctx, int minimumParameterCount)
    {
        if (ctx.Parameters.Length < minimumParameterCount)
        {
            log.Warn($"Insufficient parameters provided. Expected at least {MinimumParameterCount}, got {ctx.Parameters.Length}: {string.Join(", ", ctx.Parameters)}");
            return true;
        }
        return false;
    }

    protected virtual int MinimumParameterCount => Descriptor.Parameters.Count;
}
