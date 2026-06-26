using System.Collections.Immutable;
using LoupixDeck.PluginSdk;
using NAudio.CoreAudioApi;

namespace QPlug;

public sealed class AudioOutControlMenuContributor(IPluginHost Host) : MenuContributorBase(Host)
{
    public override ValueTask<ImmutableList<MenuNode>> GetMenuNodes(ButtonTargets target)
    {
        try
        {
            //if (!target.HasAnyButton())
                return ValueTask.FromResult(ImmutableList<MenuNode>.Empty);

            //return ValueTask.FromResult(GetMenuNodesSync());
        }
        catch (Exception ex)
        {
            log.Error($"Failed to get audio output devices: {ex.Message}");
            return ValueTask.FromException<ImmutableList<MenuNode>>(ex);
        }
    }
    private ImmutableList<MenuNode> GetMenuNodesSync()
    {
        MenuNode outputControl = new()
        {
            Name = "Audio Output Control",
            CommandName = null,
            Children = ImmutableList.Create(
                //GetNodeFor_ToggleAB(),
                GetNodeFor_SetOutput()
            ),
        };
        return [
            outputControl
        ];
    }

    /*
    private static MenuNode GetNodeFor_ToggleAB()
        => new()
        {
            Name = "Toggle Audio Output Default A/B",
            CommandName = "toggle-audio-output-default-a-b",
            Parameters = ImmutableDictionary.CreateRange<string, string>([
                new("audio-output-a", "Speakers (Realtek(R) Audio)"),
                new("audio-output-b", "Headphones (Realtek(R) Audio)")
            ]),
        };
    */

    private MenuNode GetNodeFor_SetOutput()
    {
        ImmutableList<MenuNode>.Builder nodesBuilder = ImmutableList.CreateBuilder<MenuNode>();
        using (MMDeviceEnumerator enumerator = new())
        {
            MMDeviceCollection devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            log.Info($"Enumerated audio output devices ({devices.Count})");
            for (int i = 0; i < devices.Count; i++)
            {
                using MMDevice device = devices[i];
                MenuNode node = new()
                {
                    Name = $"Set Audio Output to {device.FriendlyName}",
                    CommandName = $"set-audio-output-{device.ID}",
                    Parameters = ImmutableDictionary.CreateRange<string, string>([
                            new("audio-output", device.ID)
                        ]),
                };
                nodesBuilder.Add(node);
            }
        }

        return new()
        {
            Name = "Set Audio Output to...",
            CommandName = null,
            Children = nodesBuilder.ToImmutable(),
        };
    }
}
