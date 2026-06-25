using System.Runtime.InteropServices;

namespace CoreAudioApi.Interfaces;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public class WAVEFORMATEX
{
    public short wFormatTag;
    public short nChannels;
    public int nSamplesPerSec;
    public int nAvgBytesPerSec;
    public short nBlockAlign;
    public short wBitsPerSample;
    public short cbSize;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public class WAVEFORMATEXTENSIBLE : WAVEFORMATEX
{
    [FieldOffset(0)]
    public short wValidBitsPerSample;
    [FieldOffset(0)]
    public short wSamplesPerBlock;
    [FieldOffset(0)]
    public short wReserved;
    [FieldOffset(2)]
    public WaveMask dwChannelMask;
    [FieldOffset(6)]
    public Guid SubFormat;
}
public enum ERole
{
    eConsole = 0,
    eMultimedia = 1,
    eCommunications = 2,
    ERole_enum_count = 3
}


[Flags]
public enum WaveMask
{
    None = 0x0,
    FrontLeft = 0x1,
    FrontRight = 0x2,
    FrontCenter = 0x4,
    LowFrequency = 0x8,
    BackLeft = 0x10,
    BackRight = 0x20,
    FrontLeftOfCenter = 0x40,
    FrontRightOfCenter = 0x80,
    BackCenter = 0x100,
    SideLeft = 0x200,
    SideRight = 0x400,
    TopCenter = 0x800,
    TopFrontLeft = 0x1000,
    TopFrontCenter = 0x2000,
    TopFrontRight = 0x4000,
    TopBackLeft = 0x8000,
    TopBackCenter = 0x10000,
    TopBackRight = 0x20000
}

public enum DeviceShareMode
{
    Shared,
    Exclusive
}