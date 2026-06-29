using LoupixDeck.PluginSdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QPlug.Commands;

public sealed class AudioOutSetter(ILogger<AudioOutSetter> logger, IServiceScopeFactory scopeFactory) : PluginCommandBase(logger)
{
    public override CommandDescriptor Descriptor { get; } = new()
    {
        CommandName = "set-audio-output",
        DisplayName = "Set Audio Output To...",
        Group = "Q Plug",
        Parameters = [
            new("audio-output", typeof(string)),
            ],
        ParameterTemplate = "({audio-output})"
    };

    public override ButtonTargets SupportedTargets => ButtonTargets.TouchButton | ButtonTargets.SimpleButton;

    public override async Task Execute(CommandContext ctx)
    {
        if (CheckInvalidParameterCount(ctx, 1))
            return;

        string audioOutput = ctx.Parameters[0];
        ArgumentException.ThrowIfNullOrWhiteSpace(audioOutput);
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        SoundVolumeViewExe sndVol = scope.ServiceProvider.GetRequiredService<SoundVolumeViewExe>();
        if (log.DebugEnabled)
            log.LogDebug("switching audio output to {audioOutput}", audioOutput);
        sndVol.SetOutput(audioOutput);
    }
}
