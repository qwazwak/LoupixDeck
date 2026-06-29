using LoupixDeck.PluginSdk;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace QPlug.Commands;

public sealed class MuteToggleCommand(ILogger<MuteToggleCommand> logger) : AudioCommandBase(logger)
{
    protected override string CommandName => "q-plug-audio-mute-toggle";
    protected override string DisplayName => "Toggle mute";
    public override ButtonTargets SupportedTargets => ButtonTargets.SimpleButton | ButtonTargets.TouchButton;

    protected override void Execute(CommandContext ctx, MMDevice device, AudioEndpointVolume volumeEndpoint, Role role)
    {
        bool startedMuted = device.AudioEndpointVolume.Mute;
        if (log.IsEnabled(LogLevel.Debug))
            log.LogDebug("Device {deviceId} started muted: {startedMuted}", device.ID, startedMuted);
        device.AudioEndpointVolume.Mute = !startedMuted;
        bool endedMuted = device.AudioEndpointVolume.Mute;
        if (log.IsEnabled(LogLevel.Debug))
            log.LogDebug("Device {deviceId} ended muted: {endedMuted}", device.ID, endedMuted);
        if (startedMuted == endedMuted)
            log.LogWarning("Device {deviceId} mute state did not change after toggling", device.ID);
    }
}
