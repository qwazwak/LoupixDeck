using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LoupixDeck.PluginSdk;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;

namespace QPlug;

public sealed partial class AudioOutCycler(IPluginHost host) : IPluginCommand
{
    private readonly IPluginHost host = host;
    private IPluginLogger log => host.Logger;

    public CommandDescriptor Descriptor { get; } = new()
    {
        CommandName = "test-command",
        DisplayName = "Bring program to front",
        Group = "Test Commands",
        Parameters = [
            new("process-name", typeof(string))
            ],
        ParameterTemplate = "({process-name})"
    };

    public ButtonTargets SupportedTargets => ButtonTargets.TouchButton | ButtonTargets.SimpleButton;

    public Task Execute(CommandContext ctx)
    {
        log.Info($"Called with args: {string.Join(", ", ctx.Parameters)}");
        if (ctx.Parameters.Length is 0)
            log.Warn($"No process name provided");
        else
            Execute(ctx, ctx.Parameters[0]);
        return Task.CompletedTask;
    }

    private Process? TryFindProcess(string processName)
    {
        foreach (Process process in Process.GetProcesses())
        {
            string name = process.ProcessName;
            if (!string.Equals(name, processName, StringComparison.OrdinalIgnoreCase))
                continue;
            log.Info($"Found process {name} with ID {process.Id}");
            return process;
        }
        return null;
    }

    private void RTest(string targetName)
    {
        using var pEnum = new MMDeviceEnumerator();
        using (var pDefDevice = pEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
        {


            // Create a multimedia device enumerator.
            //Determine if it is the default audio device
            if (pDefDevice.Properties.TryGetValue(PropertyKeys.PKEY_Device_FriendlyName, out string currentActiveName))
            {
                if (targetName == currentActiveName)
                {
                    log.Info($"The default audio device is already set to {targetName}");
                    return;
                }
            }
        }
        var pDevices = pEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        foreach (var pDevice in pDevices)
        {

            bool bFind = false;
            string wstrID = pDevice.ID;
            if (pDevice.Properties.TryGetValue(PropertyKeys.PKEY_Device_FriendlyName, out string deviceName))
            {
                if (targetName == deviceName)
                {
                    // Create a new audio PolicyConfigClient

                    PolicyConfigClient client = new PolicyConfigClient();
                    // Using PolicyConfigClient, set the given device as the default playback communication device
                    client.SetDefaultEndpoint(DeviceCollection[i].ID, ERole.eCommunications);
                    // Using PolicyConfigClient, set the given device as the default playback device
                    client.SetDefaultEndpoint(DeviceCollection[i].ID, ERole.eMultimedia);
                    break;
                }
            }
        }
    }

    public void Execute(CommandContext ctx, string processName)
    {
        var enumerator = new MMDeviceEnumerator();
        foreach (var endpoint in
                 enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        {
            endpoint.
            Console.WriteLine(endpoint.FriendlyName);
        }

        Process? process = TryFindProcess(processName);
        if (process == null)
        {
            log.Warn($"No process found with name {processName}");
            return;
        }
        bool s = SetForegroundWindow(process.MainWindowHandle);
        if (!s)
        {
            int error = Marshal.GetLastWin32Error();
            log.Error($"Failed to set foreground window for process {processName} (PID {process.Id}). Error code: {error}");
        }
        else
        {
            log.Info($"Successfully set foreground window for process {processName} (PID {process.Id}).");
        }
        return;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);
}