using System.Diagnostics;
using System.Runtime.InteropServices;
using LoupixDeck.PluginSdk;

namespace QPlug;

// rat ugly aweful plugin of mine
public sealed partial class TestCommand(IPluginHost Host) : PluginCommandBase(Host)
{
    public override CommandDescriptor Descriptor { get; } = new()
    {
        CommandName = "test-command",
        DisplayName = "Bring program to front",
        Group = "Test Commands",
        Parameters = [
            new("process-name", typeof(string))
            ],
        ParameterTemplate = "({process-name})"
    };

    public override ButtonTargets SupportedTargets => ButtonTargets.TouchButton | ButtonTargets.SimpleButton;

    public override Task Execute(CommandContext ctx)
    {
        if (CheckValidParameterCount(ctx))
            Execute(ctx.Parameters[0]);
        return Task.CompletedTask;
    }

    private static Process? TryFindProcess(string processName)
    {
        ArgumentException.ThrowIfNullOrEmpty(processName);

        foreach (Process process in Process.GetProcesses())
        {
            string name = process.ProcessName;
            if (string.Equals(name, processName, StringComparison.OrdinalIgnoreCase))
                return process;
        }
        return null;
    }

    private void Execute(string processName)
    {
        Process? process = TryFindProcess(processName);
        if (process == null)
        {
            log.Warn($"No process found with name {processName}");
            return;
        }
        log.Info($"Found process {processName} with ID {process.Id} (native {process.MainWindowHandle})");
        bool s = SetForegroundWindow(process.MainWindowHandle);
        if (!s)
        {
            int error = Marshal.GetLastWin32Error();
            string message = Marshal.GetPInvokeErrorMessage(error);
            log.Error($"Failed to set foreground window for process {processName} (PID {process.Id}). ({error}) {message}");
        }
        else
        {
            log.Info($"Successfully set foreground window for process {processName} (PID {process.Id}).");
        }
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);
}
