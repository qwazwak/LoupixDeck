using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace LoupixDeck.Native;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Name matching (with C)")]
public sealed class UInputUserDev : SafeHandleZeroOrMinusOneIsInvalid
{
    [StructLayout(LayoutKind.Explicit, Size = Offsets.absflat + Sizes.absflat)]
    public struct Data
    {
        private static class Sizes
        {
            public const int name = sizeof(char) * 80;
            public const int id_bustype = sizeof(short);
            public const int id_vendor = sizeof(short);
            public const int id_product = sizeof(short);
            public const int id_version = sizeof(short);
            public const int ff_effects_max = sizeof(int);
            public const int absmax = sizeof(int) * 64;
            public const int absmin = sizeof(int) * 64;
            public const int absfuzz = sizeof(int) * 64;
            public const int absflat = sizeof(int) * 64;
        }
        private static class Offsets
        {
            public const int name = 0;
            public const int id_bustype = name + Sizes.name;
            public const int id_vendor = id_bustype + Sizes.id_bustype;
            public const int id_product = id_vendor + Sizes.id_vendor;
            public const int id_version = id_product + Sizes.id_product;
            public const int ff_effects_max = id_version + Sizes.id_version;
            public const int absmax = ff_effects_max + Sizes.ff_effects_max;
            public const int absmin = absmax + Sizes.absmax;
            public const int absfuzz = absmin + Sizes.absmin;
            public const int absflat = absfuzz + Sizes.absfuzz;
        }

        public static Data Default() => new()
        {
            Name = "LoupixVirtualKeyboard",
            id_bustype = 0,
            id_vendor = 0x1234,
            id_product = 0x5678,
            id_version = 1,
        };

        [FieldOffset(Offsets.name)]
        private char name;
        [FieldOffset(Offsets.id_bustype)]
        public ushort id_bustype;
        [FieldOffset(Offsets.id_vendor)]
        public ushort id_vendor;
        [FieldOffset(Offsets.id_product)]
        public ushort id_product;
        [FieldOffset(Offsets.id_version)]
        public ushort id_version;
        [FieldOffset(Offsets.ff_effects_max)]
        public int ff_effects_max;
        [FieldOffset(Offsets.absmax)]
        private int absmax;
        [FieldOffset(Offsets.absmin)]
        private int absmin;
        [FieldOffset(Offsets.absfuzz)]
        private int absfuzz;
        [FieldOffset(Offsets.absflat)]
        private int absflat;

        private Span<char> NameSpan => MemoryMarshal.CreateSpan(ref name, 80);
        public string Name
        {
            get => new(NameSpan);
            set
            {
                if (value.Length > 80)
                    throw new ArgumentException("Device name cannot exceed 80 characters.", nameof(value));
                value.CopyTo(NameSpan);
            }
        }
        public Span<int> AbsMaxSpan => MemoryMarshal.CreateSpan(ref absmax, 64);
        public Span<int> AbsMinSpan => MemoryMarshal.CreateSpan(ref absmin, 64);
        public Span<int> AbsFuzzSpan => MemoryMarshal.CreateSpan(ref absfuzz, 64);
        public Span<int> AbsFlatSpan => MemoryMarshal.CreateSpan(ref absflat, 64);
    }

    private static int SizeOfUInputUserDev => Marshal.SizeOf<Data>();

    public UInputUserDev() : this(Data.Default()) { }
    public UInputUserDev(Data device) : base(true)
    {
        SetHandle(Marshal.AllocHGlobal(SizeOfUInputUserDev));
        // Copy Struct to unmanaged memory
        Marshal.StructureToPtr(device, handle, false);
    }

    public unsafe ReadOnlySpan<byte> GetDeviceBytes() => new((void*)handle, SizeOfUInputUserDev);

    protected override bool ReleaseHandle()
    {
        Marshal.FreeHGlobal(handle);
        return true;
    }
}
