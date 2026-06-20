#nullable enable
using LoupixDeck.Native.Types.Windows;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LoupixDeck.Native;

internal static partial class User32
{
    /// <summary>Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowthreadprocessid#parameters">Read more on learn.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lpdwProcessId">
    /// <para>A pointer to a variable that receives the process identifier. If this parameter is not <b>NULL</b>, <b>GetWindowThreadProcessId</b> copies the identifier of the process to the variable; otherwise, it does not. If the function fails, the value of the variable is unchanged.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowthreadprocessid#parameters">Read more on learn.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>DWORD</b> If the function succeeds, the return value is the identifier of the thread that created the window. If the window handle is invalid, the return value is zero. To get extended error information, call <a href="https://docs.microsoft.com/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowthreadprocessid">Learn more about this API from learn.microsoft.com</see>.</para>
    /// </remarks>
    [LibraryImport("USER32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial uint GetWindowThreadProcessId(IntPtr hWnd, [Optional] out uint lpdwProcessId);
    public static bool TryGetWindowThreadIdAndProcessId(WindowHandle hWnd, out uint threadId, out uint processId) => (threadId = GetWindowThreadProcessId(hWnd, out processId)) != 0;

    public static string? GetWindowThreadProcessName(WindowHandle hWnd)
    {

        try
        {
            if (!TryGetWindowThreadIdAndProcessId(hWnd, out uint threadId, out uint processId) || processId == 0)
                return null;
            using var process = Process.GetProcessById((int)processId);
            return process.ProcessName; // already without path or ".exe"
        }
        catch
        {
            // Protected / exited process, or pid no longer valid.
            return null;
        }
    }

    /// <summary>Retrieves the length, in characters, of the specified window's title bar text (if the window has a title bar). (Unicode)</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window or control.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowtextlengthw#parameters">Read more on learn.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>int</b> If the function succeeds, the return value is the length, in characters, of the text. Under certain conditions, this value might be greater than the length of the text (see Remarks). If the window has no text, the return value is zero. Function failure is indicated by a return value of zero and a <a href="https://docs.microsoft.com/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a> result that is nonzero. > [!NOTE] > This function does not clear the most recent error information. To determine success or failure, clear the most recent error information by calling <a href="https://docs.microsoft.com/windows/desktop/api/errhandlingapi/nf-errhandlingapi-setlasterror">SetLastError</a> with 0, then call <a href="https://docs.microsoft.com/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
    /// </returns>
    /// <remarks>
    /// <para>If the target window is owned by the current process, <b>GetWindowTextLength</b> causes a <a href="https://docs.microsoft.com/windows/desktop/winmsg/wm-gettextlength">WM_GETTEXTLENGTH</a> message to be sent to the specified window or control. Under certain conditions, the <b>GetWindowTextLength</b> function may return a value that is larger than the actual length of the text. This occurs with certain mixtures of ANSI and Unicode, and is due to the system allowing for the possible existence of double-byte character set (DBCS) characters within the text. The return value, however, will always be at least as large as the actual length of the text; you can thus always use it to guide buffer allocation. This behavior can occur when an application uses both ANSI functions and common dialogs, which use Unicode. It can also occur when an application uses the ANSI version of <b>GetWindowTextLength</b> with a window whose window procedure is Unicode, or the Unicode version of <b>GetWindowTextLength</b> with a window whose window procedure is ANSI. For more information on ANSI and ANSI functions, see <a href="https://docs.microsoft.com/windows/desktop/Intl/conventions-for-function-prototypes">Conventions for Function Prototypes</a>. To obtain the exact length of the text, use the <a href="https://docs.microsoft.com/windows/desktop/winmsg/wm-gettext">WM_GETTEXT</a>, <a href="https://docs.microsoft.com/windows/desktop/Controls/lb-gettext">LB_GETTEXT</a>, or <a href="https://docs.microsoft.com/windows/desktop/Controls/cb-getlbtext">CB_GETLBTEXT</a> messages, or the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getwindowtexta">GetWindowText</a> function.</para>
    /// <para>> [!NOTE] > The winuser.h header defines GetWindowTextLength as an alias which automatically selects the ANSI or Unicode version of this function based on the definition of the UNICODE preprocessor constant. Mixing usage of the encoding-neutral alias with code that not encoding-neutral can lead to mismatches that result in compilation or runtime errors. For more information, see [Conventions for Function Prototypes](/windows/win32/intl/conventions-for-function-prototypes).</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowtextlengthw#">Read more on learn.microsoft.com</see>.</para>
    /// </remarks>
    [LibraryImport("USER32.dll", EntryPoint = "GetWindowTextLengthW", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int GetWindowTextLength(IntPtr hWnd);

    /// <summary>
    /// Copies the text of the specified window's title bar (if it has one) into a buffer.
    /// If the specified window is a control, the text of the control is copied.
    /// However, GetWindowText cannot retrieve the text of a control in another application. (Unicode)
    /// </summary>
    /// <param name="hWnd">A handle to the window or control containing the text.</param>
    /// <param name="lpString">
    /// <para>Type: <b>LPTSTR</b> The buffer that will receive the text. If the string is as long or longer than the buffer, the string is truncated and terminated with a null character.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowtextw#parameters">Read more on learn.microsoft.com</see>.</para>
    /// </param>
    /// <param name="nMaxCount">
    /// <para>Type: <b>int</b> The maximum number of characters to copy to the buffer, including the null character. If the text exceeds this limit, it is truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowtextw#parameters">Read more on learn.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>int</b> If the function succeeds, the return value is the length, in characters, of the copied string, not including the terminating null character. If the window has no title bar or text, if the title bar is empty, or if the window or control handle is invalid, the return value is zero. To get extended error information, call <a href="https://docs.microsoft.com/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>. This function cannot retrieve the text of an edit control in another application.</para>
    /// </returns>
    /// <remarks>
    /// <para>If the target window is owned by the current process, <b>GetWindowText</b> causes a <a href="https://docs.microsoft.com/windows/desktop/winmsg/wm-gettext">WM_GETTEXT</a> message to be sent to the specified window or control. If the target window is owned by another process and has a caption, <b>GetWindowText</b> retrieves the window caption text. If the window does not have a caption, the return value is a null string. This behavior is by design. It allows applications to call <b>GetWindowText</b> without becoming unresponsive if the process that owns the target window is not responding. However, if the target window is not responding and it belongs to the calling application, <b>GetWindowText</b> will cause the calling application to become unresponsive. To retrieve the text of a control in another process, send a <a href="https://docs.microsoft.com/windows/desktop/winmsg/wm-gettext">WM_GETTEXT</a> message directly instead of calling <b>GetWindowText</b>.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowtextw#">Read more on learn.microsoft.com</see>.</para>
    /// </remarks>
    [LibraryImport("USER32.dll", EntryPoint = "GetWindowTextW", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static unsafe partial int GetWindowText(IntPtr hWnd, char* lpString, int nMaxCount);
    private static unsafe int GetWindowText(IntPtr hWnd, Span<char> lpString)
    {
        fixed (char* lpStringPtr = lpString)
            return GetWindowText(hWnd, lpStringPtr, lpString.Length);
    }

    public static string GetWindowText(WindowHandle hWnd)
    {
        int length = GetWindowTextLength(hWnd);
        if (length <= 0) return string.Empty;
        char[]? rented = null;
        int lengthWithTerminator = length + 1; // could tack on extra buffer
        Span<char> buffer = length < 64 ? stackalloc char[lengthWithTerminator] : ((rented = ArrayPool<char>.Shared.Rent(lengthWithTerminator)).AsSpan(0, lengthWithTerminator));
        try
        {

            int resultLength = GetWindowText(hWnd, buffer);
            return buffer[..resultLength].ToString();
        }
        finally
        {
            if (rented is not null)
                ArrayPool<char>.Shared.Return(rented);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "Matches Windows API")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1234:Duplicate enum value", Justification = "Matches Windows API")]
    public enum VIRTUAL_KEY : ushort
    {
        VK_0 = 48,
        VK_1 = 49,
        VK_2 = 50,
        VK_3 = 51,
        VK_4 = 52,
        VK_5 = 53,
        VK_6 = 54,
        VK_7 = 55,
        VK_8 = 56,
        VK_9 = 57,
        VK_A = 65,
        VK_B = 66,
        VK_C = 67,
        VK_D = 68,
        VK_E = 69,
        VK_F = 70,
        VK_G = 71,
        VK_H = 72,
        VK_I = 73,
        VK_J = 74,
        VK_K = 75,
        VK_L = 76,
        VK_M = 77,
        VK_N = 78,
        VK_O = 79,
        VK_P = 80,
        VK_Q = 81,
        VK_R = 82,
        VK_S = 83,
        VK_T = 84,
        VK_U = 85,
        VK_V = 86,
        VK_W = 87,
        VK_X = 88,
        VK_Y = 89,
        VK_Z = 90,
        VK_ABNT_C1 = 193,
        VK_ABNT_C2 = 194,
        VK_DBE_ALPHANUMERIC = 240,
        VK_DBE_CODEINPUT = 250,
        VK_DBE_DBCSCHAR = 244,
        VK_DBE_DETERMINESTRING = 252,
        VK_DBE_ENTERDLGCONVERSIONMODE = 253,
        VK_DBE_ENTERIMECONFIGMODE = 248,
        VK_DBE_ENTERWORDREGISTERMODE = 247,
        VK_DBE_FLUSHSTRING = 249,
        VK_DBE_HIRAGANA = 242,
        VK_DBE_KATAKANA = 241,
        VK_DBE_NOCODEINPUT = 251,
        VK_DBE_NOROMAN = 246,
        VK_DBE_ROMAN = 245,
        VK_DBE_SBCSCHAR = 243,
        VK__none_ = 255,
        VK_LBUTTON = 1,
        VK_RBUTTON = 2,
        VK_CANCEL = 3,
        VK_MBUTTON = 4,
        VK_XBUTTON1 = 5,
        VK_XBUTTON2 = 6,
        VK_BACK = 8,
        VK_TAB = 9,
        VK_CLEAR = 12,
        VK_RETURN = 13,
        VK_SHIFT = 16,
        VK_CONTROL = 17,
        VK_MENU = 18,
        VK_PAUSE = 19,
        VK_CAPITAL = 20,
        VK_KANA = 21,
        VK_HANGEUL = 21,
        VK_HANGUL = 21,
        VK_IME_ON = 22,
        VK_JUNJA = 23,
        VK_FINAL = 24,
        VK_HANJA = 25,
        VK_KANJI = 25,
        VK_IME_OFF = 26,
        VK_ESCAPE = 27,
        VK_CONVERT = 28,
        VK_NONCONVERT = 29,
        VK_ACCEPT = 30,
        VK_MODECHANGE = 31,
        VK_SPACE = 32,
        VK_PRIOR = 33,
        VK_NEXT = 34,
        VK_END = 35,
        VK_HOME = 36,
        VK_LEFT = 37,
        VK_UP = 38,
        VK_RIGHT = 39,
        VK_DOWN = 40,
        VK_SELECT = 41,
        VK_PRINT = 42,
        VK_EXECUTE = 43,
        VK_SNAPSHOT = 44,
        VK_INSERT = 45,
        VK_DELETE = 46,
        VK_HELP = 47,
        VK_LWIN = 91,
        VK_RWIN = 92,
        VK_APPS = 93,
        VK_SLEEP = 95,
        VK_NUMPAD0 = 96,
        VK_NUMPAD1 = 97,
        VK_NUMPAD2 = 98,
        VK_NUMPAD3 = 99,
        VK_NUMPAD4 = 100,
        VK_NUMPAD5 = 101,
        VK_NUMPAD6 = 102,
        VK_NUMPAD7 = 103,
        VK_NUMPAD8 = 104,
        VK_NUMPAD9 = 105,
        VK_MULTIPLY = 106,
        VK_ADD = 107,
        VK_SEPARATOR = 108,
        VK_SUBTRACT = 109,
        VK_DECIMAL = 110,
        VK_DIVIDE = 111,
        VK_F1 = 112,
        VK_F2 = 113,
        VK_F3 = 114,
        VK_F4 = 115,
        VK_F5 = 116,
        VK_F6 = 117,
        VK_F7 = 118,
        VK_F8 = 119,
        VK_F9 = 120,
        VK_F10 = 121,
        VK_F11 = 122,
        VK_F12 = 123,
        VK_F13 = 124,
        VK_F14 = 125,
        VK_F15 = 126,
        VK_F16 = 127,
        VK_F17 = 128,
        VK_F18 = 129,
        VK_F19 = 130,
        VK_F20 = 131,
        VK_F21 = 132,
        VK_F22 = 133,
        VK_F23 = 134,
        VK_F24 = 135,
        VK_NAVIGATION_VIEW = 136,
        VK_NAVIGATION_MENU = 137,
        VK_NAVIGATION_UP = 138,
        VK_NAVIGATION_DOWN = 139,
        VK_NAVIGATION_LEFT = 140,
        VK_NAVIGATION_RIGHT = 141,
        VK_NAVIGATION_ACCEPT = 142,
        VK_NAVIGATION_CANCEL = 143,
        VK_NUMLOCK = 144,
        VK_SCROLL = 145,
        VK_OEM_NEC_EQUAL = 146,
        VK_OEM_FJ_JISHO = 146,
        VK_OEM_FJ_MASSHOU = 147,
        VK_OEM_FJ_TOUROKU = 148,
        VK_OEM_FJ_LOYA = 149,
        VK_OEM_FJ_ROYA = 150,
        VK_LSHIFT = 160,
        VK_RSHIFT = 161,
        VK_LCONTROL = 162,
        VK_RCONTROL = 163,
        VK_LMENU = 164,
        VK_RMENU = 165,
        VK_BROWSER_BACK = 166,
        VK_BROWSER_FORWARD = 167,
        VK_BROWSER_REFRESH = 168,
        VK_BROWSER_STOP = 169,
        VK_BROWSER_SEARCH = 170,
        VK_BROWSER_FAVORITES = 171,
        VK_BROWSER_HOME = 172,
        VK_VOLUME_MUTE = 173,
        VK_VOLUME_DOWN = 174,
        VK_VOLUME_UP = 175,
        VK_MEDIA_NEXT_TRACK = 176,
        VK_MEDIA_PREV_TRACK = 177,
        VK_MEDIA_STOP = 178,
        VK_MEDIA_PLAY_PAUSE = 179,
        VK_LAUNCH_MAIL = 180,
        VK_LAUNCH_MEDIA_SELECT = 181,
        VK_LAUNCH_APP1 = 182,
        VK_LAUNCH_APP2 = 183,
        VK_OEM_1 = 186,
        VK_OEM_PLUS = 187,
        VK_OEM_COMMA = 188,
        VK_OEM_MINUS = 189,
        VK_OEM_PERIOD = 190,
        VK_OEM_2 = 191,
        VK_OEM_3 = 192,
        VK_GAMEPAD_A = 195,
        VK_GAMEPAD_B = 196,
        VK_GAMEPAD_X = 197,
        VK_GAMEPAD_Y = 198,
        VK_GAMEPAD_RIGHT_SHOULDER = 199,
        VK_GAMEPAD_LEFT_SHOULDER = 200,
        VK_GAMEPAD_LEFT_TRIGGER = 201,
        VK_GAMEPAD_RIGHT_TRIGGER = 202,
        VK_GAMEPAD_DPAD_UP = 203,
        VK_GAMEPAD_DPAD_DOWN = 204,
        VK_GAMEPAD_DPAD_LEFT = 205,
        VK_GAMEPAD_DPAD_RIGHT = 206,
        VK_GAMEPAD_MENU = 207,
        VK_GAMEPAD_VIEW = 208,
        VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON = 209,
        VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON = 210,
        VK_GAMEPAD_LEFT_THUMBSTICK_UP = 211,
        VK_GAMEPAD_LEFT_THUMBSTICK_DOWN = 212,
        VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT = 213,
        VK_GAMEPAD_LEFT_THUMBSTICK_LEFT = 214,
        VK_GAMEPAD_RIGHT_THUMBSTICK_UP = 215,
        VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN = 216,
        VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT = 217,
        VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT = 218,
        VK_OEM_4 = 219,
        VK_OEM_5 = 220,
        VK_OEM_6 = 221,
        VK_OEM_7 = 222,
        VK_OEM_8 = 223,
        VK_OEM_AX = 225,
        VK_OEM_102 = 226,
        VK_ICO_HELP = 227,
        VK_ICO_00 = 228,
        VK_PROCESSKEY = 229,
        VK_ICO_CLEAR = 230,
        VK_PACKET = 231,
        VK_OEM_RESET = 233,
        VK_OEM_JUMP = 234,
        VK_OEM_PA1 = 235,
        VK_OEM_PA2 = 236,
        VK_OEM_PA3 = 237,
        VK_OEM_WSCTRL = 238,
        VK_OEM_CUSEL = 239,
        VK_OEM_ATTN = 240,
        VK_OEM_FINISH = 241,
        VK_OEM_COPY = 242,
        VK_OEM_AUTO = 243,
        VK_OEM_ENLW = 244,
        VK_OEM_BACKTAB = 245,
        VK_ATTN = 246,
        VK_CRSEL = 247,
        VK_EXSEL = 248,
        VK_EREOF = 249,
        VK_PLAY = 250,
        VK_ZOOM = 251,
        VK_NONAME = 252,
        VK_PA1 = 253,
        VK_OEM_CLEAR = 254,
    }

    /// <summary>
    /// Determines whether a key is up or down at the time the function is called, and whether the key was pressed after a previous call to GetAsyncKeyState.
    /// </summary>
    /// <param name="vKey">
    /// <para>Type: <b>int</b> The virtual-key code. For more information, see <a href="https://docs.microsoft.com/windows/desktop/inputdev/virtual-key-codes">Virtual Key Codes</a>. You can use left- and right-distinguishing constants to specify certain keys.
    /// See the Remarks section for further information.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getasynckeystate#parameters">Read more on learn.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>SHORT</b> If the function succeeds, the return value specifies whether the key was pressed since the last call to <b>GetAsyncKeyState</b>, and whether the key is currently up or down. If the most significant bit is set, the key is down, and if the least significant bit is set, the key was pressed after the previous call to <b>GetAsyncKeyState</b>. However, you should not rely on this last behavior; for more information, see the Remarks. The return value is zero for the following cases: </para>
    /// <para>This doc was truncated.</para>
    /// </returns>
    /// <remarks>
    /// <para>The <b>GetAsyncKeyState</b> function works with mouse buttons. However, it checks on the state of the physical mouse buttons, not on the logical mouse buttons that the physical buttons are mapped to. For example, the call <b>GetAsyncKeyState</b>(VK_LBUTTON) always returns the state of the left physical mouse button, regardless of whether it is mapped to the left or right logical mouse button. You can determine the system's current mapping of physical mouse buttons to logical mouse buttons by calling <c>GetSystemMetrics(SM_SWAPBUTTON)</c>. which returns TRUE if the mouse buttons have been swapped. Although the least significant bit of the return value indicates whether the key has been pressed since the last query, due to the preemptive multitasking nature of Windows, another application can call <b>GetAsyncKeyState</b> and receive the "recently pressed" bit instead of your application. The behavior of the least significant bit of the return value is retained strictly for compatibility with 16-bit Windows applications (which are non-preemptive) and should not be relied upon. You can use the virtual-key code constants <b>VK_SHIFT</b>, <b>VK_CONTROL</b>, and <b>VK_MENU</b> as values for the <i>vKey</i> parameter. This gives the state of the SHIFT, CTRL, or ALT keys without distinguishing between left and right. You can use the following virtual-key code constants as values for <i>vKey</i> to distinguish between the left and right instances of those keys. </para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getasynckeystate#">Read more on learn.microsoft.com</see>.</para>
    /// </remarks>
    [LibraryImport("USER32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial short GetAsyncKeyState(int vKey);
    public static short GetAsyncKeyState(VIRTUAL_KEY vKey) => GetAsyncKeyState((int)vKey);

    public enum MAP_VIRTUAL_KEY_TYPE : uint
    {
        MAPVK_VK_TO_VSC = 0U,
        MAPVK_VSC_TO_VK = 1U,
        MAPVK_VK_TO_CHAR = 2U,
        // MapVirtualKey translation type: scan code → virtual key, distinguishing left/right
        // modifier keys and honouring the extended-key (E0) prefix.
        MAPVK_VSC_TO_VK_EX = 3U,
        MAPVK_VK_TO_VSC_EX = 4U,
    }

    /// <summary>
    /// Translates (maps) a virtual-key code into a scan code or character value, or translates a scan code into a virtual-key code. (Unicode)
    /// </summary>
    /// <param name="uCode">
    /// <para>Type: **UINT** The [virtual key code](/windows/desktop/inputdev/virtual-key-codes) or scan code for a key. How this value is interpreted depends on the value of the *uMapType* parameter. **Starting with Windows Vista**, the high byte of the *uCode* value can contain either 0xe0 or 0xe1 to specify the extended scan code.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-mapvirtualkeyw#parameters">Read more on learn.microsoft.com</see>.</para>
    /// </param>
    /// <param name="uMapType">
    /// <para>Type: **UINT** The translation to be performed. The value of this parameter depends on the value of the *uCode* parameter. | Value | Meaning | |-------|---------| | **MAPVK\_VK\_TO\_VSC**<br>0 | The *uCode* parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no translation, the function returns 0. | | **MAPVK\_VSC\_TO\_VK**<br>1 | The *uCode* parameter is a scan code and is translated into a virtual-key code that does not distinguish between left- and right-hand keys. If there is no translation, the function returns 0. | | **MAPVK\_VK\_TO\_CHAR**<br>2 | The *uCode* parameter is a virtual-key code and is translated into an unshifted character value in the low order word of the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If there is no translation, the function returns 0. See Remarks. | | **MAPVK\_VSC\_TO\_VK\_EX**<br>3 | The *uCode* parameter is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys. If there is no translation, the function returns 0. | | **MAPVK\_VK\_TO\_VSC\_EX**<br>4 | **Windows Vista and later:** The *uCode* parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned. If the scan code is an extended scan code, the high byte of the *uCode* value can contain either 0xe0 or 0xe1 to specify the extended scan code. If there is no translation, the function returns 0. |</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-mapvirtualkeyw#parameters">Read more on learn.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: **UINT** The return value is either a scan code, a virtual-key code, or a character value, depending on the value of *uCode* and *uMapType*. If there is no translation, the return value is zero.</para>
    /// </returns>
    /// <remarks>
    /// <para>To specify a handle to the keyboard layout to use for translating the specified code, use the [MapVirtualKeyEx](nf-winuser-mapvirtualkeyexw.md) function. An application can use **MapVirtualKey** to translate scan codes to the virtual-key code constants **VK_SHIFT**, **VK_CONTROL**, and **VK_MENU**, and vice versa. These translations do not distinguish between the left and right instances of the SHIFT, CTRL, or ALT keys. An application can get the scan code corresponding to the left or right instance of one of these keys by calling **MapVirtualKey** with *uCode* set to one of the following virtual-key code constants: - **VK\_LSHIFT** - **VK\_RSHIFT** - **VK\_LCONTROL** - **VK\_RCONTROL** - **VK\_LMENU** - **VK\_RMENU** These left- and right-distinguishing constants are available to an application only through the [GetKeyboardState](nf-winuser-getkeyboardstate.md), [SetKeyboardState](nf-winuser-setkeyboardstate.md), [GetAsyncKeyState](nf-winuser-getasynckeystate.md), [GetKeyState](nf-winuser-getkeystate.md), [MapVirtualKey](nf-winuser-mapvirtualkeyw.md), and **MapVirtualKeyEx** functions. For list complete table of virtual key codes, see [Virtual Key Codes](/windows/win32/inputdev/virtual-key-codes). In **MAPVK\_VK\_TO\_CHAR** mode [virtual-key codes](/windows/win32/inputdev/virtual-key-codes), the 'A'..'Z' keys are translated to upper-case 'A'..'Z' characters regardless of current keyboard layout. If you want to translate a virtual-key code to the corresponding character, use the [ToUnicode](/windows/win32/api/winuser/nf-winuser-tounicode) function. > [!NOTE] > The winuser.h header defines MapVirtualKey as an alias which automatically selects the ANSI or Unicode version of this function based on the definition of the UNICODE preprocessor constant. Mixing usage of the encoding-neutral alias with code that not encoding-neutral can lead to mismatches that result in compilation or runtime errors. For more information, see [Conventions for Function Prototypes](/windows/win32/intl/conventions-for-function-prototypes).</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-mapvirtualkeyw#">Read more on learn.microsoft.com</see>.</para>
    /// </remarks>
    [LibraryImport("USER32.dll", EntryPoint = "MapVirtualKeyW"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial uint MapVirtualKey(uint uCode, MAP_VIRTUAL_KEY_TYPE uMapType);

    public static uint MapVirtualScanCodeToVirtualKeyEx(uint uCode) => MapVirtualKey(uCode, MAP_VIRTUAL_KEY_TYPE.MAPVK_VSC_TO_VK_EX);
    public static ushort MapVirtualKeyToToVirtualScanCode(uint uCode) => (ushort)MapVirtualKey(uCode, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_VSC);

    /// <summary>Retrieves the active input locale identifier (formerly called the keyboard layout).</summary>
    /// <param name="idThread">
    /// <para>Type: <b>DWORD</b> The identifier of the thread to query, or 0 for the current thread.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getkeyboardlayout#parameters">Read more on learn.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>HKL</b> The return value is the input locale identifier for the thread. The low word contains a <a href="https://docs.microsoft.com/windows/desktop/Intl/language-identifiers">Language Identifier</a> for the input language and the high word contains a device handle to the physical layout of the keyboard.</para>
    /// </returns>
    /// <remarks>
    /// <para>The input locale identifier is a broader concept than a keyboard layout, since it can also encompass a speech-to-text converter, an Input Method Editor (IME), or any other form of input. Since the keyboard layout can be dynamically changed, applications that cache information about the current keyboard layout should process the <a href="https://docs.microsoft.com/windows/desktop/winmsg/wm-inputlangchange">WM_INPUTLANGCHANGE</a> message to be informed of changes in the input language. To get the KLID (keyboard layout ID) of the currently active HKL, call the  <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getkeyboardlayoutnamea">GetKeyboardLayoutName</a>. <b>Beginning in Windows 8:</b> The preferred method to retrieve the language associated with the current keyboard layout or input method is a call to <a href="https://docs.microsoft.com/uwp/api/windows.globalization.language.currentinputmethodlanguagetag">Windows.Globalization.Language.CurrentInputMethodLanguageTag</a>. If your app passes language tags from <b>CurrentInputMethodLanguageTag</b> to any <a href="https://docs.microsoft.com/windows/desktop/Intl/national-language-support-functions">National Language Support</a> functions, it must first convert the tags by calling <a href="https://docs.microsoft.com/windows/desktop/api/winnls/nf-winnls-resolvelocalename">ResolveLocaleName</a>.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getkeyboardlayout#">Read more on learn.microsoft.com</see>.</para>
    /// </remarks>
    [LibraryImport("USER32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial UIntPtr GetKeyboardLayout(uint idThread);

    public static ushort GetKeyboardLayoutLanguageId(uint idThread = 0)
    {
        nuint fullValue = GetKeyboardLayout(idThread);
        // Low word of the HKL is the LANGID; primary language id lives in its low 10 bits.
        ushort langId = (ushort)(fullValue & 0xFF_FFU);
        return langId;
    }

    public static ushort GetPrimaryKeyboardLayoutLanguageId(uint idThread = 0) => (ushort)(GetKeyboardLayoutLanguageId(idThread) & 0x3FFU);
}
