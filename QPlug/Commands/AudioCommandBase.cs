using LoupixDeck.PluginSdk;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace QPlug.Commands;

public abstract class AudioCommandBase(ILogger logger) : PluginCommandBase(logger)
{
    protected abstract string CommandName { get; }
    protected abstract string DisplayName { get; }
    public override CommandDescriptor Descriptor => field ??= new()
    {
        CommandName = CommandName,
        DisplayName = DisplayName,
        Group = "Q Plug",
    };

    protected override int MinimumParameterCount => 0;
    private static ReadOnlySpan<Role> AllRoles => [Role.Console, Role.Multimedia, Role.Communications];
    public override async Task Execute(CommandContext ctx)
    {
        HashSet<string> alreadySeenIDs = new(3);
        using MMDeviceEnumerator enumerator = new();
        foreach (var role in AllRoles)
        {
            if (!enumerator.HasDefaultAudioEndpoint(DataFlow.Render, role))
                continue;
            using MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, role);
            if (!alreadySeenIDs.Add(defaultDevice.ID))
                continue;

            Execute(ctx, defaultDevice, defaultDevice.AudioEndpointVolume, role);
        }
    }
    public override ButtonTargets SupportedTargets => ButtonTargets.All;

    protected abstract void Execute(CommandContext ctx, MMDevice device, AudioEndpointVolume volumeEndpoint, Role role);
}
