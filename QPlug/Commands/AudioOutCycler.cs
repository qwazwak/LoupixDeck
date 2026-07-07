using LoupixDeck.PluginSdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QCommon;
using QCommon.Utils.Extensions.Logging;

namespace QPlug.Commands;

public sealed class AudioOutCycler(ILogger<AudioOutCycler> logger, IServiceScopeFactory scopeFactory) : PluginCommandBase(logger)
{
    public override CommandDescriptor Descriptor { get; } = new()
    {
        CommandName = "toggle-audio-output-default-a-b",
        DisplayName = "Toggle Audio Output Default A/B",
        Group = "Q Plug",
        Parameters = [
            new("audio-output-a", typeof(string)),
            new("audio-output-b", typeof(string)),
            ],
        ParameterTemplate = "({audio-output-a}, {audio-output-b})"
    };

    public override ButtonTargets SupportedTargets => ButtonTargets.TouchButton | ButtonTargets.SimpleButton;

    public override async Task Execute(CommandContext ctx)
    {
        if (CheckInvalidParameterCount(ctx, 2))
            return;

        string audioOutputA = ctx.Parameters[0];
        string audioOutputB = ctx.Parameters[1];
        ArgumentException.ThrowIfNullOrWhiteSpace(audioOutputA);
        ArgumentException.ThrowIfNullOrWhiteSpace(audioOutputB);
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        SoundVolumeViewExe sndVol = scope.ServiceProvider.GetRequiredService<SoundVolumeViewExe>();
        if (log.DebugEnabled)
            log.LogDebug("switching default audio output between {audioOutputA} and {audioOutputB}", audioOutputA, audioOutputB);
        sndVol.SwitchDefault(audioOutputA, audioOutputB);
    }
}
