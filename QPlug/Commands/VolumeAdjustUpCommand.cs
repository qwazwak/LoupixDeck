using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace QPlug.Commands;

public sealed class VolumeAdjustUpCommand(ILogger<VolumeAdjustUpCommand> logger) : VolumeAdjustCommandBase(logger)
{
    protected override string CommandName => "q-plug-audio-adjust-up";
    protected override string DisplayName => "Adjust volume up";
    protected override bool StepUp => true;
}
