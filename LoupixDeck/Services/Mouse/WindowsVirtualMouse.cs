using LoupixDeck.Models.Macros;
using LoupixDeck.Native;
using LoupixDeck.Native.Types.Windows;

namespace LoupixDeck.Services.Mouse;

/// <summary>
/// Windows implementation backed by the Win32 <c>SendInput</c> API (user32.dll).
/// Injected mouse events enter the session input stream like a real mouse. Note:
/// they carry the injected flag, so raw-input apps may ignore them (same limit as
/// <see cref="WindowsUInputKeyboard"/>) — <see cref="InterceptionMouse"/> covers
/// that case; <see cref="WindowsMouseRouter"/> picks between the two per call.
/// </summary>
public class WindowsVirtualMouse : IVirtualMouse
{

    // SendInput requires no setup, so the backend is always available on Windows.
    public bool Connected => true;

    public void Click(MouseButton button) => User32.Input.SendInput([MOUSEINPUT.Create.ButtonDown(button), MOUSEINPUT.Create.ButtonUp(button)]);

    public void ButtonDown(MouseButton button) => User32.Input.SendInput(MOUSEINPUT.Create.ButtonDown(button));

    public void ButtonUp(MouseButton button) => User32.Input.SendInput(MOUSEINPUT.Create.ButtonUp(button));

    public void MoveRelative(int dx, int dy) => User32.Input.SendInput(MOUSEINPUT.Create.Move(dx, dy: dy));

    public void MoveAbsolute(int x, int y)
    {
        // Absolute coordinates are normalized to 0..65535 across the virtual desktop.
        var left = User32.SystemMetrics.VirtualScreenLeft;
        var top = User32.SystemMetrics.VirtualScreenTop;
        var width = User32.SystemMetrics.VirtualScreenWidth;
        var height = User32.SystemMetrics.VirtualScreenHeight;

        if (width <= 0 || height <= 0)
            return;

        var nx = (int)Math.Round((x - left) * 65535.0 / width);
        var ny = (int)Math.Round((y - top) * 65535.0 / height);

        User32.Input.SendInput(MOUSEINPUT.Create.AbsoluteMovement(nx, ny));
    }

    public void Scroll(int amount) => User32.Input.SendInput(MOUSEINPUT.Create.Scroll(amount));

    public void Dispose()
    {
        // Nothing to dispose — SendInput holds no resources.
    }
}
