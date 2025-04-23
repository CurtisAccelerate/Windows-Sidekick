// path: WindowsSidekick/HotkeyListener.cs
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WindowsSidekick.Settings;

namespace WindowsSidekick
{
    /// <summary>
    /// Listens for a global hotkey and captures the last foreground window handle.
    /// </summary>
    public class HotkeyListener : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY    = 0x0312;
        private const uint MOD_ALT     = 0x0001;
        private const uint MOD_CTRL    = 0x0002;
        private const uint MOD_SHIFT   = 0x0004;
        private const uint MOD_WIN     = 0x0008;

        private readonly int _id;
        public static IntPtr LastForegroundWindow { get; private set; }
        public event EventHandler HotkeyPressed;

        public HotkeyListener(HotkeySettings settings)
        {
            CreateHandle(new CreateParams());
            _id = GetType().GetHashCode() & 0xFFFF;

            uint mods = 0;
            if (settings.Ctrl)  mods |= MOD_CTRL;
            if (settings.Alt)   mods |= MOD_ALT;
            if (settings.Shift) mods |= MOD_SHIFT;
            if (settings.Win)   mods |= MOD_WIN;

            if (!Enum.TryParse(settings.Key, out Keys keyEnum))
                throw new ArgumentException($"Invalid hotkey key name: {settings.Key}");
            uint vk = (uint)keyEnum;

            if (!RegisterHotKey(Handle, _id, mods, vk))
                throw new InvalidOperationException("Could not register hotkey.");
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == _id)
            {
                LastForegroundWindow = GetForegroundWindow();
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            UnregisterHotKey(Handle, _id);
            DestroyHandle();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
    }
}
