using System.Diagnostics;

namespace QPlug;

public static class SoundVolumeViewExe
{
    private const string exe = "svcl.exe";
    private static string LocalExeDir => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
    //private static string LocalExePath => Path.GetDirectoryName(typeof(SoundVolumeViewExe).Assembly.Location)!;

    private static void Execute(ReadOnlySpan<string> args)
    {
        string exePath = Path.Combine(LocalExeDir, exe);
        if (!File.Exists(exePath))
            throw new InvalidOperationException($"Cannot find {exe}, looked in {exePath}");
        ProcessStartInfo startInfo = new()
        {
            FileName = exePath,
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
        => Execute([ "/SetDefault", name, "all" ]);
}
