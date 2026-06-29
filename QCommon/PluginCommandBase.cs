using LoupixDeck.PluginSdk;
using Microsoft.Extensions.Logging;

namespace QCommon;

public abstract class PluginCommandBase(ILogger logger) : IPluginCommand
{
    protected readonly ILogger log = logger;

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
            //log.LogWarning($"Insufficient parameters provided. Expected at least {MinimumParameterCount}, got {ctx.Parameters.Length}: {string.Join(", ", ctx.Parameters)}");
            log.LogWarning("Insufficient parameters provided. Expected at least {MinimumParameterCount}, got {ActualCount}: {Parameters}", MinimumParameterCount, ctx.Parameters.Length, string.Join(", ", ctx.Parameters));
            return true;
        }
        return false;
    }

    protected virtual int MinimumParameterCount => Descriptor.Parameters.Count;
}
