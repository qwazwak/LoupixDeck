using LoupixDeck.PluginSdk;

namespace QPlug;

#if false
public sealed class AudioOutCyclerMenuContributor(IPluginHost Host) : MenuContributorBase(Host)
{
    public override ValueTask<ImmutableList<MenuNode>> GetMenuNodes(ButtonTargets target)
    {
        try
        {
        return ValueTask.FromResult(GetMenuNodesSync(target));
        }
        catch (Exception ex)
        {
            log.Error($"Failed to get audio output devices: {ex.Message}");
            return ValueTask.FromException<ImmutableList<MenuNode>>(ex);
        }
    }
    private ImmutableList<MenuNode> GetMenuNodesSync(ButtonTargets target)
    {
        MenuNode n = new()
        {
            Name = "Audio Output Devices",
            CommandName = null,

            Parameters = new Dictionary<string, string>(),
            Children = Array.Empty<MenuNode>(),

        }
            using MMDeviceEnumerator enumerator = new();
        MMDeviceCollection devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        var nodes = devices.Select(device => new MenuNode(device.FriendlyName, device.FriendlyName)).ToList();
        return nodes;
    }
}
#endif

public sealed class AudioOutCycler(IPluginHost Host) : PluginCommandBase(Host)
{
    //public override AudioOutCyclerMenuContributor? MenuContributor { get; } = new(Host);

    public override CommandDescriptor Descriptor { get; } = new()
    {
        CommandName = "toggle-audio-output-default-a-b",
        DisplayName = "Toggle Audio Output Default A/B",
        Group = "Test Commands",
        Parameters = [
            new("audio-output-a", typeof(string)),
            new("audio-output-b", typeof(string)),
            ],
        ParameterTemplate = "({audio-output-a}, {audio-output-b})"
    };

    public override ButtonTargets SupportedTargets => ButtonTargets.TouchButton | ButtonTargets.SimpleButton;

    public override Task Execute(CommandContext ctx)
    {
        if (ctx.Parameters.Length < 2)
        {
            log.Warn($"Insufficient parameters provided. Expected 2, got {ctx.Parameters.Length}: {string.Join(", ", ctx.Parameters)}");
            return Task.CompletedTask;
        }
        string audioOutputA = ctx.Parameters[0];
        string audioOutputB = ctx.Parameters[1];
        try
        {
            Execute(audioOutputA, audioOutputB);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    public void Execute(string audioOutputA, string audioOutputB)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioOutputA);
        ArgumentException.ThrowIfNullOrWhiteSpace(audioOutputB);
        log.Info($"switching default audio output between {audioOutputA} and {audioOutputB}");
        SoundVolumeViewExe.SwitchDefault(audioOutputA, audioOutputB);
    }
}