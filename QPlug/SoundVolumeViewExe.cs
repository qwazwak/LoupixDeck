using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace QPlug;

public sealed partial class SoundVolumeViewExe(ILogger<SoundVolumeViewExe> log)
{
    private const string exe = "svcl.exe";
    private static string LocalExeDir => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
    //private static string LocalExePath => Path.GetDirectoryName(typeof(SoundVolumeViewExe).Assembly.Location)!;

    private void Execute(ReadOnlySpan<string> args)
    {
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            ExecuteCore(args);
        }
        finally
        {
            log?.LogInformation("Finished executing svcl.exe in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
        }
    }

    private static void ExecuteCore(ReadOnlySpan<string> args)
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
        // we could tack on "/Stdout" to get back info, but that is an overhead/delay
        // we dont accept.
        Process.Start(startInfo);
    }
    public void SwitchDefault(string defaultA, string defaultB)
        => Execute([ "/SwitchDefault", defaultA, defaultB, "all" ]);

    public void SetOutput(string name)
        => Execute([ "/SetDefault", name, "all" ]);

#if false
    public void AdjustAllAppVolume(VolumeAdjustmentAmount amount)
        => AdjustVolumeCore(amount, "AllAppVolume");

    private void AdjustVolumeCore(VolumeAdjustmentAmount amount, string target)
        => Execute(["/ChangeVolume", target, amount.ToString()]);

    public void AdjustVolume(VolumeAdjustmentAmount amount, string? target)
        => AdjustVolumeCore(amount, target ?? "AllAppVolume");
#endif
}
#if false
public readonly struct VolumeAdjustmentAmount : ISpanParsable<VolumeAdjustmentAmount>
{
    public VolumeAdjustmentAmount(sbyte amount)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(amount, 100);
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, -100);
        Amount = amount;
    }
    public sbyte Amount { get; }
    public bool IsZero => Amount is 0;

    public override string ToString()
    {
        if (Amount > 0)
            return $"+{Amount:F0}";
        else // Negative number already has negative sign
            return $"{Amount:F0}";
    }

    public static VolumeAdjustmentAmount Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
        => TryParse(s, provider, out VolumeAdjustmentAmount result) ? result : throw new FormatException($"Cannot parse {s} as a VolumeAdjustmentAmount");
    public static VolumeAdjustmentAmount Parse(string s, IFormatProvider? provider = null)
        => TryParse(s.AsSpan(), provider, out VolumeAdjustmentAmount result) ? result : throw new FormatException($"Cannot parse {s} as a VolumeAdjustmentAmount");
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out VolumeAdjustmentAmount result)
        => TryParse(s.AsSpan(), provider, out result);

    public static bool TryParse(ReadOnlySpan<char> value, out VolumeAdjustmentAmount result) => TryParse(value, null, out result);
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out VolumeAdjustmentAmount result)
    {
        if (!sbyte.TryParse(value, provider, out sbyte raw) || raw > 100 || raw < -100)
        {
            result = default; // 0
            return false;
        }
        result = new(raw);
        return true;
    }
}
#endif