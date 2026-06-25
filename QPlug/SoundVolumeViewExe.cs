using System.Diagnostics;

namespace QPlug;

public sealed class SoundVolumeViewExe
{
    private const string exe = "svcl.exe";

    private static void Execute(ReadOnlySpan<string> args)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = exe,
            //ArgumentList = args,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (string arg in args)
            startInfo.ArgumentList.Add(arg);
        Process.Start(startInfo);
    }
    public static void SwitchDefault(string defaultA, string defaultB)
        => Execute([ "/SwitchDefault", defaultA, defaultB, "all" ]);

    public static void SetOutput(string name)
        => Execute(["/SetDefault", name, "all" ]);
}
