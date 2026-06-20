using LoupixDeck.Models;
using LoupixDeck.Native;
using LoupixDeck.Native.Types.Windows;
using LoupixDeck.Utils;

namespace LoupixDeck.Services;

/// <inheritdoc/>
public class InterceptionKeyboard() : InterceptionBase(InterceptionContext.KeyboardDevice), IUInputKeyboard
{
    // InterceptionKeyState flags. (Named State* to avoid colliding with the
    // IUInputKeyboard.KeyDown/KeyUp methods.)
    private const ushort StateKeyDown = 0x00;
    private const ushort StateKeyUp = 0x01;
    private const ushort KeyE0 = 0x02;

    // Left Shift make code (PS/2 set 1) — used to type shifted characters in SendText.
    private const ushort ScanLeftShift = 0x2A;

    private readonly KeyboardLayout _layout = KeyboardLayouts.GetLayout(GetCurrentKeyboardLayout());

    void IUInputKeyboard.SendKey(uint keyCode)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(keyCode, ushort.MaxValue);
        SendKey((ushort)keyCode);
    }

    public void SendKey(ushort keyCode)
    {
        // keyCode is a PS/2 set-1 scan code (non-extended) for interface compatibility.
        lock (_lock)
        {
            if (!EnsureContext()) return;
            SendStrokeVerified(keyCode, false, down: true);
            SendStrokeVerified(keyCode, false, down: false);
        }
    }

    public void SendText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        lock (_lock)
        {
            if (!EnsureContext()) return;

            // One stroke at a time, each acknowledged via the async key state before the
            // next is injected. The driver gives no backpressure signal (it reports strokes
            // as written even when the stack above drops them), so this handshake is the
            // fastest loss-free rate possible.
            foreach (var c in text)
            {
                if (!_layout.KeyMap.TryGetValue(c, out var key)) continue;

                if (key.shift) SendStrokeVerified(ScanLeftShift, false, down: true);
                SendStrokeVerified(key.keycode, false, down: true);
                SendStrokeVerified(key.keycode, false, down: false);
                if (key.shift) SendStrokeVerified(ScanLeftShift, false, down: false);
            }
        }
    }

    public void SendKeyCombination(IReadOnlyList<string> keyNames)
    {
        if (keyNames == null || keyNames.Count == 0) return;

        var keys = new List<(ushort scanCode, bool e0)>(keyNames.Count);
        foreach (var name in keyNames)
        {
            if (KeyNames.TryGetInterception(name, out var scanCode, out var e0))
                keys.Add((scanCode, e0));
            else
                Console.Error.WriteLine($"[InterceptionKeyboard] Unknown key name: '{name}'");
        }

        if (keys.Count == 0) return;

        lock (_lock)
        {
            if (!EnsureContext()) return;

            // Press all in order, then release in reverse order. Each stroke is acknowledged
            // before the next, so modifier state is guaranteed when the main key arrives.
            foreach (var (scanCode, e0) in keys)
                SendStrokeVerified(scanCode, e0, true);
            for (var k = keys.Count - 1; k >= 0; k--)
                SendStrokeVerified(keys[k].scanCode, keys[k].e0, false);
        }
    }

    public void KeyDown(string keyName) => SendSingle(keyName, down: true);

    public void KeyUp(string keyName) => SendSingle(keyName, down: false);

    private void SendSingle(string keyName, bool down)
    {
        if (!KeyNames.TryGetInterception(keyName, out var scanCode, out var e0))
        {
            Console.Error.WriteLine($"[InterceptionKeyboard] Unknown key name: '{keyName}'");
            return;
        }

        lock (_lock)
        {
            if (!EnsureContext()) return;
            SendStrokeVerified(scanCode, e0, down);
        }
    }

    // Sends a single stroke and waits (spin, sub-millisecond granularity) until win32k's
    // async key state reflects it. The interception driver reports strokes as written even
    // when the input stack above it silently drops them (queue overflow), so the key state
    // actually changing is the only reliable delivery signal — and at the same time the
    // fastest possible pacing: injection proceeds the moment the previous stroke is through.
    // Caller must hold _lock and have ensured the context exists.
    private void SendStrokeVerified(ushort code, bool e0, bool down)
    {
        // Resolve the VK that win32k will track for this scan code. 0 = no mapping.
        var vk = User32.MapVirtualKey(e0 ? 0xE000u | code : code, User32.MAP_VIRTUAL_KEY_TYPE.MAPVK_VSC_TO_VK_EX);

        ushort state = down ? StateKeyDown : StateKeyUp;
        if (e0) state |= KeyE0;
        InterceptionStroke stroke = new()
        {
            Key = {
                Code = code,
                State = state,
                Information = 0
            }
        };
        if (vk == 0 || _ackFailures >= ConfigConstants.MaxConsecutiveAckFailures)
        {
            // Cannot (or should not) verify — inject and pace with a fixed spin delay.
            InjectWithDelay(stroke);
            return;
        }

        Send(stroke);

        if (WaitForKeyButtonState((int)vk, down))
        {
            _ackFailures = 0;
        }
        else
        {
            // Not resent on purpose: a timeout can also mean "processed but not observable"
            // (e.g. per-window layout mismatch) — resending would duplicate the keystroke.
            _ackFailures++;
            Console.Error.WriteLine(
                $"[InterceptionKeyboard] Stroke 0x{code:X2} ({(down ? "down" : "up")}) not acknowledged within {ConfigConstants.StrokeAckTimeoutMs} ms.");
        }
    }

    private static string GetCurrentKeyboardLayout()
    {
        try
        {
            // Low word of the HKL is the LANGID; primary language id lives in its low 10 bits.
            var primarylangId = User32.GetPrimaryKeyboardLayoutLanguageId();
            return primarylangId == 0x07U ? "de" : "us"; // 0x07 = German, fall back to US otherwise
        }
        catch
        {
            return "us";
        }
    }
}
