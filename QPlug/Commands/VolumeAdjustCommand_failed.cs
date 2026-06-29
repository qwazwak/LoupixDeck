#if false
using System.Runtime.InteropServices;
using LoupixDeck.PluginSdk;
using Microsoft.Extensions.DependencyInjection;

namespace QPlug;

public sealed class VolumeAdjustCommand(IPluginHost Host, IServiceScopeFactory scopeFactory) : PluginCommandBase(Host)
{
    public override CommandDescriptor Descriptor { get; } = new()
    {
        CommandName = "adjust-audio-output-relative",
        DisplayName = "Adjust Audio Output Relative",
        Group = "Q Plug",
        Parameters = [
            new("adjustment-amount", typeof(sbyte)),
            ],
        ParameterTemplate = "({adjustment-amount})"
    };

    public override ButtonTargets SupportedTargets => ButtonTargets.All;

    protected override int MinimumParameterCount => 1;

    public override async Task Execute(CommandContext ctx)
    {
        CheckValidParameterCount(ctx);
        if (ctx.Parameters.Length < 1)
        {
            log.Warn($"Insufficient parameters provided. Expected 1, got {ctx.Parameters.Length}: {string.Join(", ", ctx.Parameters)}");
            return;
        }
        VolumeAdjustmentAmount audioOutputA = VolumeAdjustmentAmount.Parse(ctx.Parameters[0]);
        await using var scope = scopeFactory.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<>
        try
        {
            Execute(audioOutputA);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    public void Execute(VolumeAdjustmentAmount amount)
    {
        log.Info($"Adjusting volume by {amount}");
        if (amount.IsZero)
            return;
        if (MMApi.WaveOutGetVolume(IntPtr.Zero, out uint currentVolumeUint) is not MMApi.MMRESULT.MMSYSERR_NOERROR)
        {
            log.Warn($"Failed to get current volume");
            return;
        }
        WaveOutVolume currentVolume = WaveOutVolume.FromNative(currentVolumeUint);
        WaveOutVolume newVolume = currentVolume + amount;
        int change = newVolume.Left - currentVolume.Left;
        log.Info($"Current volume: {currentVolume}, new volume: {newVolume}, change: {change}");
        uint newVolumeUint = WaveOutVolume.ToNative(newVolume);
        //MMApi.WaveOutSetVolume(IntPtr.Zero, newVolumeUint);

        WAVEFORMATEX wf = new()
        {
        };

        uint numDevices = MMApi.WaveOutGetNumDevs();
        for (IntPtr id = 0; id < numDevices; id++)
        {
            IntPtr hwo = IntPtr.Zero;
            if (MMApi.WaveOutOpen(ref hwo, id, ref wf, null, 0, 0) == MMApi.MMRESULT.MMSYSERR_NOERROR)
            {
                MMApi.WaveOutSetVolume(hwo, newVolumeUint);
                MMApi.WaveOutClose(hwo);
               // break;
            }
            log.Info($"Set volume for device {id} to {newVolume} (0x{newVolumeUint:X8})");
        }
    }
}

internal static partial class MMApi
{
    public enum MMRESULT : uint
    {
        MMSYSERR_NOERROR = 0,
        MMSYSERR_ERROR = 1,
        MMSYSERR_BADDEVICEID = 2,
        MMSYSERR_NOTENABLED = 3,
        MMSYSERR_ALLOCATED = 4,
        MMSYSERR_INVALHANDLE = 5,
        MMSYSERR_NODRIVER = 6,
        MMSYSERR_NOMEM = 7,
        MMSYSERR_NOTSUPPORTED = 8,
        MMSYSERR_BADERRNUM = 9,
        MMSYSERR_INVALFLAG = 10,
        MMSYSERR_INVALPARAM = 11,
        MMSYSERR_HANDLEBUSY = 12,
        MMSYSERR_INVALIDALIAS = 13,
        MMSYSERR_BADDB = 14,
        MMSYSERR_KEYNOTFOUND = 15,
        MMSYSERR_READERROR = 16,
        MMSYSERR_WRITEERROR = 17,
        MMSYSERR_DELETEERROR = 18,
        MMSYSERR_VALNOTFOUND = 19,
        MMSYSERR_NODRIVERCB = 20,
        WAVERR_BADFORMAT = 32,
        WAVERR_STILLPLAYING = 33,
        WAVERR_UNPREPARED = 34
    }

    [LibraryImport("winmm.dll", EntryPoint = "waveOutGetNumDevs", SetLastError = true)]
    public static partial uint WaveOutGetNumDevs();

    public unsafe delegate void delegateWaveOutProc(IntPtr hwo, uint uMsg, uint* dwInstance, uint* dwParam1, uint* dwParam2);

    [LibraryImport("winmm.dll", EntryPoint = "waveOutOpen", SetLastError = true)]
    public static partial MMRESULT WaveOutOpen(ref IntPtr hWaveOut, IntPtr uDeviceID, ref WAVEFORMATEX lpFormat, delegateWaveOutProc? dwCallback, IntPtr dwInstance, uint dwFlags);

    [LibraryImport("winmm.dll", EntryPoint = "waveOutGetVolume", SetLastError = true)]
    public static partial MMRESULT WaveOutGetVolume(IntPtr hwo, out uint pdwVolume);
    [LibraryImport("winmm.dll", EntryPoint = "waveOutSetVolume", SetLastError = true)]
    public static partial MMRESULT WaveOutSetVolume(IntPtr hwo, uint pdwVolume);
    [LibraryImport("winmm.dll", EntryPoint = "waveOutClose", SetLastError = true)]
    public static partial MMRESULT WaveOutClose(IntPtr hwo);
}
//
// Summary:
//     WaveHeader interop structure (WAVEHDR) http://msdn.microsoft.com/en-us/library/dd743837%28VS.85%29.aspx
[StructLayout(LayoutKind.Sequential)]
public struct WaveHeader
{
    //
    // Summary:
    //     pointer to locked data buffer (lpData)
    public IntPtr dataBuffer;

    //
    // Summary:
    //     length of data buffer (dwBufferLength)
    public int bufferLength;

    //
    // Summary:
    //     used for input only (dwBytesRecorded)
    public int bytesRecorded;

    //
    // Summary:
    //     for client's use (dwUser)
    public IntPtr userData;

    //
    // Summary:
    //     assorted flags (dwFlags)
    public WaveHeaderFlags flags;

    //
    // Summary:
    //     loop control counter (dwLoops)
    public int loops;

    //
    // Summary:
    //     PWaveHdr, reserved for driver (lpNext)
    public IntPtr next;

    //
    // Summary:
    //     reserved for driver
    public IntPtr reserved;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WAVEFORMATEX
{
    public ushort wFormatTag;
    public ushort nChannels;
    public uint nSamplesPerSec;
    public uint nAvgBytesPerSec;
    public ushort nBlockAlign;
    public ushort wBitsPerSample;
    public ushort cbSize;
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WaveOutVolume
{
    // Cool thing, on systems with little endian vs big endian, left and right would be swapped
    // but idgaf which is which, so it doesnt matter to me ;)
    public ushort Left;
    public ushort Right;

    public static WaveOutVolume FromNative(in uint value) => new() { Left = (ushort)(value & 0xFFFF), Right = (ushort)((value >> 16) & 0xFFFF) };
    public static uint ToNative(in WaveOutVolume value) => (uint)((value.Right << 16) | value.Left);

    private static ushort AdjustSingleChannel(ushort channel, short delta) => (ushort)Math.Clamp(channel + delta, 0, ushort.MaxValue);
    private static ushort AdjustSingleChannel(ushort channel, VolumeAdjustmentAmount delta)
    {
        double vol0To1 = (double)channel / ushort.MaxValue;
        double volOutOf100 = vol0To1 * 100.0;
        double newVolOutOf100 = volOutOf100 + delta.Amount;
        return (ushort)Math.Clamp((int)double.Round((newVolOutOf100 / 100) * ushort.MaxValue), 0, ushort.MaxValue);
    }

    public static WaveOutVolume operator +(WaveOutVolume vol, VolumeAdjustmentAmount delta)
    {
        return new()
        {
            Left = AdjustSingleChannel(vol.Left, delta),
            Right = AdjustSingleChannel(vol.Right, delta),
        };
    }
    public static WaveOutVolume operator +(WaveOutVolume vol, short delta)
    {
        return new()
        {
            Left = AdjustSingleChannel(vol.Left, delta),
            Right = AdjustSingleChannel(vol.Right, delta),
        };
    }

    public override readonly string ToString()
    {
        if (Left == Right)
            return $"Volume: {Left} (0x{Left:X4})";
        else
            return $"Volume: Left={Left} (0x{Left:X4}), Right={Right} (0x{Right:X4})";
    }
}

//
// Summary:
//     Wave Header Flags enumeration
[Flags]
public enum WaveHeaderFlags
{
    //
    // Summary:
    //     WHDR_BEGINLOOP This buffer is the first buffer in a loop. This flag is used only
    //     with output buffers.
    BeginLoop = 4,
    //
    // Summary:
    //     WHDR_DONE Set by the device driver to indicate that it is finished with the buffer
    //     and is returning it to the application.
    Done = 1,
    //
    // Summary:
    //     WHDR_ENDLOOP This buffer is the last buffer in a loop. This flag is used only
    //     with output buffers.
    EndLoop = 8,
    //
    // Summary:
    //     WHDR_INQUEUE Set by Windows to indicate that the buffer is queued for playback.
    InQueue = 0x10,
    //
    // Summary:
    //     WHDR_PREPARED Set by Windows to indicate that the buffer has been prepared with
    //     the waveInPrepareHeader or waveOutPrepareHeader function.
    Prepared = 2
}
#endif