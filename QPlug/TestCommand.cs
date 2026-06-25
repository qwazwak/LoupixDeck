using System.Diagnostics;
using System.Runtime.InteropServices;
using LoupixDeck.PluginSdk;

namespace QPlug;

// rat ugly aweful plugin of mine
public sealed partial class TestCommand(IPluginHost host) : IPluginCommand
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

    public void Execute(CommandContext ctx, string processName)
    {
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
