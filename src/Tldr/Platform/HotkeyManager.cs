using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Tldr.Platform;

public sealed class HotkeyManager : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_OPEN = 1;
    private const int HOTKEY_STOP = 2;

    private IntPtr _hwnd;
    private HwndSource? _source;

    public event Action? OpenRequested;
    public event Action? StopRequested;

    public void Register(System.Windows.Window window)
    {
        var helper = new WindowInteropHelper(window);
        _hwnd = helper.Handle;
        _source = HwndSource.FromHwnd(_hwnd);
        _source?.AddHook(WndProc);

        // Ctrl+Shift+S = open
        if (!RegisterHotKey(_hwnd, HOTKEY_OPEN, MOD_CONTROL | MOD_SHIFT, VK_S))
            System.Diagnostics.Debug.WriteLine("[Hotkey] Ctrl+Shift+S registration failed (conflict)");

        // Ctrl+Shift+X = stop reading
        if (!RegisterHotKey(_hwnd, HOTKEY_STOP, MOD_CONTROL | MOD_SHIFT, VK_X))
            System.Diagnostics.Debug.WriteLine("[Hotkey] Ctrl+Shift+X registration failed (conflict)");
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (id == HOTKEY_OPEN) OpenRequested?.Invoke();
            else if (id == HOTKEY_STOP) StopRequested?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        UnregisterHotKey(_hwnd, HOTKEY_OPEN);
        UnregisterHotKey(_hwnd, HOTKEY_STOP);
        _source?.RemoveHook(WndProc);
    }

    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_S = 0x53;
    private const uint VK_X = 0x58;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
