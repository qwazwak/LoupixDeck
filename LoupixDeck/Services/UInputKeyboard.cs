#nullable enable
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LoupixDeck.Models;
using LoupixDeck.Native;
using LoupixDeck.Native.Types.Windows;
using LoupixDeck.Utils;
// ReSharper disable CollectionNeverQueried.Local
// ReSharper disable UnusedMember.Local

namespace LoupixDeck.Services;

public interface IUInputKeyboard : IDisposable
{
    public bool Connected { get; }

#if IUInputKeyboard_SEND_KEY
    /// <summary>
    /// Sends a single keycode as a key press and release.
    /// </summary>
    /// <param name="keyCode">Linux key code (e.g. 30 = KEY_A).</param>
#if IUInputKeyboard_SEND_KEY_UINT
    void SendKey(uint keyCode);
#else
    void SendKey(ushort keyCode);
#endif
#endif

    /// <summary>
    /// Sends a complete text, letter by letter.
    /// Currently only supports single a-z, A-Z and spaces.
    /// </summary>
    /// <param name="text">Text to be sent</param>
    void SendText(string text);

    /// <summary>
    /// Sends a key combination (e.g. ["Ctrl","C"]): all keys are pressed in order and
    /// released in reverse order. Key names are resolved via <see cref="Utils.KeyNames"/>.
    /// </summary>
    /// <param name="keyNames">Ordered list of key names making up the combination.</param>
    void SendKeyCombination(IReadOnlyList<string> keyNames);

    /// <summary>
    /// Presses a key and keeps it held down until <see cref="KeyUp"/> is called for the
    /// same key. Key names are resolved via <see cref="Utils.KeyNames"/>.
    /// </summary>
    void KeyDown(string keyName);

    /// <summary>
    /// Releases a key previously held down by <see cref="KeyDown"/>.
    /// </summary>
    void KeyUp(string keyName);
}

public sealed class UInputKeyboard : IUInputKeyboard
{
    private readonly KeyboardLayout _layout;

    // Shift key
    private const int KEY_LEFTSHIFT = 42;

    private readonly UInputFile? uinputNative;

    [MemberNotNullWhen(true, nameof(uinputNative))]
    public bool Connected => uinputNative?.Connected is true;

    public UInputKeyboard()
    {
        var localLayout = GetCurrentKeyboardLayout();
        _layout = KeyboardLayouts.GetLayout(localLayout);

        // Step 1: open /dev/uinput
        try
        {
            uinputNative = UInputFile.CreateKeyboard();
        }
        catch (Exception)
        {
            // Don´t throw an Exception.
            // Just set a value, that this won´t work and get out.
            //throw new IOException("Could not open /dev/uinput. Is uinput running and are the permissions set?");
            uinputNative = null;
            return;
        }

        uinputNative.Connect((_layout.KeyMap.Values, KeyNames.AllLinuxKeyCodes), static (ctx, state) =>
        {
            var (keyMap, allKeyCodes) = state;

            // Step 2: Activate Events
            UInputFile.SetupContextKeys keys = ctx.SetupKeys();

            // Set keybits for the letters + SHIFT
            foreach (var i in keyMap)
                keys.SetKeyBit(i.keycode);

            // SHIFT
            keys.SetLeftShift();
            // Keys usable in key combinations (modifiers, function keys, navigation, ...).
            // uinput only emits events for keys whose keybit was registered before UI_DEV_CREATE.
            foreach (var keyCode in KeyNames.AllLinuxKeyCodes)
                keys.SetKeyBit(keyCode);
        });
    }

#if IUInputKeyboard_SEND_KEY
#if IUInputKeyboard_SEND_KEY_UINT
    void IUInputKeyboard.SendKey(uint keyCode)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(keyCode, ushort.MaxValue);
        SendKey((ushort)keyCode);
    }
#endif

    /// <summary>
    /// Sends a single keycode (press + release).
    /// </summary>
    public void SendKey(ushort keyCode)
    {
        if (!Connected)
            return;

        uinputNative.PressKey(keyCode);
        uinputNative.ReleaseKey(keyCode);
    }
#endif

    /// <summary>
    /// Sends a complete text (simplified, only a-z, A-Z, spaces).
    /// </summary>
    public void SendText(string text)
    {
        if (!Connected)
            return;

        foreach (var c in text)
        {
            if (!_layout.KeyMap.TryGetValue(c, out var keyCode))
            {
                // Optional: log or skip unsupported characters
                continue;
            }

            if (keyCode.shift)
                uinputNative.PressKey(KEY_LEFTSHIFT);

            uinputNative.PressKey(keyCode.keycode);
            uinputNative.ReleaseKey(keyCode.keycode);

            if (keyCode.shift)
                uinputNative.ReleaseKey(KEY_LEFTSHIFT);

            Thread.Sleep(1); // Small delay between keystrokes
        }
    }

    /// <summary>
    /// Presses every key of the combination in order, then releases them in reverse order.
    /// </summary>
    public void SendKeyCombination(IReadOnlyList<string> keyNames)
    {
        if (!Connected || keyNames == null || keyNames.Count == 0)
            return;

        var codes = new List<ushort>(keyNames.Count);
        foreach (var name in keyNames)
        {
            if (KeyNames.TryGetLinux(name, out var code))
                codes.Add(code);
            else
                Console.Error.WriteLine($"[UInputKeyboard] Unknown key name: '{name}'");
        }

        if (codes.Count == 0)
            return;

        foreach (var code in codes)
            uinputNative.PressKey(code);

        for (var i = codes.Count - 1; i >= 0; i--)
            uinputNative.ReleaseKey(codes[i]);
    }

    public void KeyDown(string keyName)
    {
        if (!Connected)
            return;

        if (KeyNames.TryGetLinux(keyName, out var code))
            uinputNative.PressKey(code);
        else
            Console.Error.WriteLine($"[UInputKeyboard] Unknown key name: '{keyName}'");
    }

    public void KeyUp(string keyName)
    {
        if (!Connected)
            return;

        if (KeyNames.TryGetLinux(keyName, out var code))
            uinputNative.ReleaseKey(code);
        else
            Console.Error.WriteLine($"[UInputKeyboard] Unknown key name: '{keyName}'");
    }

    public void Dispose() => uinputNative?.Dispose();

    private static string GetCurrentKeyboardLayout()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "localectl",
                    Arguments = "status",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Layout:"))
                {
                    return line.Split(':')[1].Trim();
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[KeyboardLayout] Error with localectl: {ex.Message}");
        }

        // Fallback:
        return "us";
    }
}

/// <summary>
/// Windows implementation backed by the Win32 <c>SendInput</c> API (user32.dll).
/// No kernel driver, no admin rights and no third-party dependency: input is injected
/// into the session input stream and delivered to the focused window, like a normal
/// keyboard. Text is sent layout-independently via Unicode injection; key combinations
/// use virtual-key codes.
/// </summary>
/// <remarks>
/// Note: injected events carry the LLKHF_INJECTED flag, so apps reading raw input
/// (some games / anti-cheat) may ignore them — that is a fundamental limit of any
/// user-mode injection and cannot be bypassed without a kernel driver.
/// </remarks>
public sealed class WindowsUInputKeyboard : IUInputKeyboard
{
    private const int MaxStackAllocLengthHalf = 64;

    // SendInput requires no setup, so the backend is always available on Windows.
    public bool Connected => true;

#if IUInputKeyboard_SEND_KEY
#if IUInputKeyboard_SEND_KEY_UINT
    void IUInputKeyboard.SendKey(uint keyCode)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(keyCode, ushort.MaxValue);
        SendKey((ushort)keyCode);
    }
#endif

    public void SendKey(ushort keyCode)
    {
        if (!Connected)
            return;

        // keyCode is treated as a virtual-key code (interface compatibility).
        User32.Input.SendInput([
            KEYBDINPUT.Create.KeyInput(keyCode, false, false),
            KEYBDINPUT.Create.KeyInput(keyCode, false, true)
        ]);
    }
#endif

    public void SendText(string text)
    {
        if (!Connected || string.IsNullOrEmpty(text))
            return;

        // Unicode injection: send each UTF-16 code unit directly, independent of the
        // active keyboard layout (handles umlauts, accents, emoji, ...).

        INPUT[]? rented = null;
        Span<INPUT> inputs = text.Length <= MaxStackAllocLengthHalf ? stackalloc INPUT[text.Length * 2] : (rented = ArrayPool<INPUT>.Shared.Rent(text.Length * 2)).AsSpan(0, text.Length * 2);
        try
        {
            var i = 0;
            foreach (var c in text)
            {
                inputs[i++] = KEYBDINPUT.Create.UnicodeInput(c, false);
                inputs[i++] = KEYBDINPUT.Create.UnicodeInput(c, true);
            }

            User32.Input.SendInput(inputs);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<INPUT>.Shared.Return(rented);
        }
    }

    public void SendKeyCombination(IReadOnlyList<string> keyNames)
    {
        if (!Connected || keyNames == null || keyNames.Count == 0)
            return;

        var keys = new List<(ushort virtualKey, bool extended)>(keyNames.Count);
        foreach (var name in keyNames)
        {
            if (KeyNames.TryGetWindows(name, out var virtualKey, out var extended))
                keys.Add((virtualKey, extended));
            else
                Console.Error.WriteLine($"[WindowsUInputKeyboard] Unknown key name: '{name}'");
        }

        if (keys.Count == 0)
            return;

        // Press all keys in order, then release them in reverse order.
        INPUT[]? rented = null;
        Span<INPUT> inputs = keys.Count <= MaxStackAllocLengthHalf ? stackalloc INPUT[keys.Count * 2] : (rented = ArrayPool<INPUT>.Shared.Rent(keys.Count * 2)).AsSpan(0, keys.Count * 2);
        try
        {
            var i = 0;
            foreach (var (virtualKey, extended) in keys)
                inputs[i++] = KEYBDINPUT.Create.KeyInput(virtualKey, extended, false);

            for (var k = keys.Count - 1; k >= 0; k--)
                inputs[i++] = KEYBDINPUT.Create.KeyInput(keys[k].virtualKey, keys[k].extended, true);

            User32.Input.SendInput(inputs);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<INPUT>.Shared.Return(rented);
        }
    }

    public void KeyDown(string keyName)
    {
        SendSingle(keyName, up: false);
    }

    public void KeyUp(string keyName)
    {
        SendSingle(keyName, up: true);
    }

    private void SendSingle(string keyName, bool up)
    {
        if (!Connected)
            return;

        if (KeyNames.TryGetWindows(keyName, out var virtualKey, out var extended))
            User32.Input.SendInput([KEYBDINPUT.Create.KeyInput(virtualKey, extended, up)]);
        else
            Console.Error.WriteLine($"[WindowsUInputKeyboard] Unknown key name: '{keyName}'");
    }

    public void Dispose()
    {
        // Nothing to dispose — SendInput holds no resources.
    }
}
