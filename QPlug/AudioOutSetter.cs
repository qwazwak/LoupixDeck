using LoupixDeck.PluginSdk;

namespace QPlug;

public sealed class AudioOutSetter(IPluginHost Host) : PluginCommandBase(Host)
{
    public override CommandDescriptor Descriptor { get; } = new()
    {
        CommandName = "set-audio-output",
        DisplayName = "Set Audio Output To...",
        Group = "Test Commands",
        Parameters = [
            new("audio-output", typeof(string)),
            ],
        ParameterTemplate = "({audio-output})"
    };

    public override ButtonTargets SupportedTargets => ButtonTargets.TouchButton | ButtonTargets.SimpleButton;

    public override Task Execute(CommandContext ctx)
    {
        if (ctx.Parameters.Length < 1)
        {
            log.Warn($"Insufficient parameters provided. Expected 1, got {ctx.Parameters.Length}: {string.Join(", ", ctx.Parameters)}");
            return Task.CompletedTask;
        }
        string audioOutput = ctx.Parameters[0];
        try
        {
            Execute(audioOutput);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    public void Execute(string audioOutput)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioOutput);
        log.Info($"switching audio output to {audioOutput}");
        SoundVolumeViewExe.SetOutput(audioOutput);
    }
}
