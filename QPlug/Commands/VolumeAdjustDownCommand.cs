using Microsoft.Extensions.Logging;

namespace QPlug.Commands;

public sealed class VolumeAdjustDownCommand(ILogger<VolumeAdjustDownCommand> logger) : VolumeAdjustCommandBase(logger)
{
    protected override string CommandName => "q-plug-audio-adjust-down";
    protected override string DisplayName => "Adjust volume down";
    protected override bool StepUp => false;
}