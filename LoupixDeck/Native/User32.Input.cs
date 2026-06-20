using LoupixDeck.Native.Types.Windows;
using System.Runtime.InteropServices;

namespace LoupixDeck.Native;

internal static partial class User32
{
    public static partial class Input
    {

        private static readonly int InputSize = Marshal.SizeOf<INPUT>();

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial uint SendInput(uint nInputs, ReadOnlySpan<INPUT> inputs, int cbSize);
        public static uint SendInput(ReadOnlySpan<INPUT> inputs)
        {
            if (inputs.IsEmpty)
                return 0;
            return SendInput((uint)inputs.Length, inputs, InputSize);
        }

        public static uint SendInput(INPUT input) => SendInput(1, MemoryMarshal.CreateReadOnlySpan(ref input, 1), InputSize);
    }
}
