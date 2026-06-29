using LoupixDeck.PluginSdk;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace QPlug.Commands;

public abstract class VolumeAdjustCommandBase(ILogger logger) : AudioCommandBase(logger)
{
    protected abstract bool StepUp { get; }
    private string StepArrow => StepUp ? "↑" : "↓";
    private string LimitText => StepUp ? "Max" : "Min";
    public override sealed ButtonTargets SupportedTargets => ButtonTargets.All;
    protected override void Execute(CommandContext ctx, MMDevice device, AudioEndpointVolume volumeEndpoint, Role role)
    {
        float oldLevel = volumeEndpoint.MasterVolumeLevel;
        if (StepUp)
            volumeEndpoint.VolumeStepUp();
        else
            volumeEndpoint.VolumeStepDown();
        float newLevel = volumeEndpoint.MasterVolumeLevel;
        float limitValue = StepUp ? volumeEndpoint.VolumeRange.MaxDecibels : volumeEndpoint.VolumeRange.MinDecibels;
        bool atLimit = limitValue == newLevel;

        log.LogInformation("Volume stepped {name} ({role}) to {newLevel:F1} (changed by {diff:F1})", device.FriendlyName, role, newLevel, newLevel - oldLevel);

        if (ctx.SourceIndex is null)
            return;
        int sourceIndex = ctx.SourceIndex.Value;
        string deviceNameFormatted = device.FriendlyName.Replace("(R)", "®");
        string valueText = atLimit ? LimitText : $"{StepArrow} {newLevel:F0}dB";
        string text =
        $"""
        {deviceNameFormatted}
        {valueText}
        """;
        TimeSpan displayTime = TimeSpan.FromMilliseconds(600);
        if (atLimit)
            displayTime *= .7;
        ctx.Host.OverlayTouchText(sourceIndex, text, displayTime);
    }
}
