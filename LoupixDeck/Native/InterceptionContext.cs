using LoupixDeck.Native.Types.Windows;
using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LoupixDeck.Native;

public sealed partial class InterceptionContext : SafeHandleZeroOrMinusOneIsInvalid
{
    //keyboards are devices 1..10, mice 11..20.
    // INTERCEPTION_KEYBOARD(0)
    public const int KeyboardDevice = 1;
    // INTERCEPTION_MOUSE(0)
    public const int MouseDevice = 11;
    // interception.dll uses the C default calling convention (cdecl). On x64 there is only
    // one convention, but Cdecl keeps it correct should an x86 build ever exist.
    private static partial class InterceptionFunctions
    {

        [LibraryImport("interception.dll", EntryPoint = "interception_create_context")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        public static partial IntPtr InterceptionCreateContext();

        [LibraryImport("interception.dll", EntryPoint = "interception_destroy_context")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        public static partial void InterceptionDestroyContext(IntPtr context);

        [LibraryImport("interception.dll", EntryPoint = "interception_send")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        private static partial int InterceptionSend(IntPtr context, int device, ReadOnlySpan<InterceptionStroke> stroke, uint nstroke);
        public static int InterceptionSend(IntPtr context, int device, ReadOnlySpan<InterceptionStroke> stroke) => InterceptionSend(context, device, stroke, (uint)stroke.Length);
    }

    private readonly int device;

    public static InterceptionContext? Create(int device)
    {
        try
        {
            return new(device);
        }
        catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException or EntryPointNotFoundException)
        {
            return null;
        }
    }

    public InterceptionContext(int device) : base(true)
    {
        this.device = device;
        try
        {
            SetHandle(InterceptionFunctions.InterceptionCreateContext());
        }
        catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException
                                      or EntryPointNotFoundException)
        {
            SetHandle(IntPtr.Zero);
        }
    }

    public int Send(ReadOnlySpan<InterceptionKeyStroke> strokes) => Send(MemoryMarshal.Cast<InterceptionKeyStroke, InterceptionStroke>(strokes));
    public int Send(ReadOnlySpan<InterceptionMouseStroke> strokes) => Send(MemoryMarshal.Cast<InterceptionMouseStroke, InterceptionStroke>(strokes));
    public int Send(ReadOnlySpan<InterceptionStroke> strokes) => InterceptionFunctions.InterceptionSend(handle, device, strokes);

    protected override bool ReleaseHandle()
    {
        try
        {
            InterceptionFunctions.InterceptionDestroyContext(handle);
        }
        catch { /* DLL may be gone — nothing to clean up */ }
        return true;
    }
}
