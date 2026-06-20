#nullable enable
using LoupixDeck.Models.Macros;
using LoupixDeck.Native;
using System.Diagnostics.CodeAnalysis;

namespace LoupixDeck.Services.Mouse;

/// <summary>
/// Linux implementation backed by a uinput virtual mouse device (relative axes +
/// buttons + wheel). Same P/Invoke pattern as <see cref="UInputKeyboard"/>.
/// Absolute positioning is not supported (would require an EV_ABS device) — see
/// <see cref="MoveAbsolute"/>.
/// </summary>
public sealed class UInputMouse : IVirtualMouse
{
    private static class Constants
    {
        public const ushort BTN_LEFT = 0x110;
        public const ushort BTN_RIGHT = 0x111;
        public const ushort BTN_MIDDLE = 0x112;
    }

    private readonly UInputFile? uinputNative;

    [MemberNotNullWhen(true, nameof(uinputNative))]
    public bool Connected => uinputNative?.Connected is true;

    public UInputMouse()
    {
        try
        {
            uinputNative = UInputFile.CreateMouse();
        }
        catch (Exception)
        {
            // Same policy as UInputKeyboard: no exception, just report unavailable.
            uinputNative = null;
            return;
        }
        uinputNative.Connect(static ctx =>
        {
            // Buttons
            ctx.SetupKeys()
                .SetKeyBit(Constants.BTN_LEFT)
                .SetKeyBit(Constants.BTN_RIGHT)
                .SetKeyBit(Constants.BTN_MIDDLE);

            // Relative axes + wheel
            ctx.SetupRelatives()
                .SetRelXBit()
                .SetRelYBit()
                .SetRelWheelBit();
        });
    }

    public void Click(MouseButton button)
    {
        if (!Connected) return;

        var code = ButtonCode(button);
        uinputNative.TapKey(code);
    }

    public void ButtonDown(MouseButton button)
    {
        if (!Connected) return;
        uinputNative.PressKey(ButtonCode(button));
    }

    public void ButtonUp(MouseButton button)
    {
        if (!Connected) return;

        uinputNative.ReleaseKey(ButtonCode(button));
    }

    public void MoveRelative(int dx, int dy)
    {
        if (!Connected) return;

        uinputNative.SendMouseMoveRelative(dx, dy);
    }

    public void MoveAbsolute(int x, int y)
    {
        // Would require an EV_ABS uinput device with ABS_X/ABS_Y absinfo — out of scope for v1.
        Console.Error.WriteLine("[UInputMouse] MoveAbsolute is not supported on Linux.");
    }

    public void Scroll(int amount)
    {
        if (!Connected) return;

        uinputNative.SendMouseScroll(amount);
    }

    public void Dispose() => uinputNative?.Close();

    private static ushort ButtonCode(MouseButton button) => button switch
    {
        MouseButton.Right => Constants.BTN_RIGHT,
        MouseButton.Middle => Constants.BTN_MIDDLE,
        _ => Constants.BTN_LEFT
    };
}
