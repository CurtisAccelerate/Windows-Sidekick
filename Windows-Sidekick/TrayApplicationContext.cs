// path: ./WindowsSidekick/TrayApplicationContext.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsSidekick.Settings;
using WindowsSidekick.Utils;

namespace WindowsSidekick
{
    class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly HotkeyListener _hotkeyListener;
        private readonly HotkeyListener _altHotkeyListener;
        private readonly HotkeyListener _exitHotkeyListener;
        private readonly GemmaClient _gemmaClient;
        private readonly AssistantSettings _settings;
        private Point? _lastPromptLocation;
        private Point? _lastResponseLocation;

        public TrayApplicationContext()
        {
            _settings = new AssistantSettings();
            _gemmaClient = new GemmaClient(_settings);

            _trayIcon = new NotifyIcon
            {
                Icon = new Icon("Assets/karate_icon_orange.ico"),  // custom orange karate icon
                Text = "WindowsSidekick",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, Exit);

            _hotkeyListener = new HotkeyListener(_settings.Hotkey);
            _hotkeyListener.HotkeyPressed += OnHotkey;

            var rebuildHotkey = new HotkeySettings { Ctrl = true, Shift = true, Key = "Oem3" };
            _altHotkeyListener = new HotkeyListener(rebuildHotkey);
            _altHotkeyListener.HotkeyPressed += OnHotkey;

            var exitHotkey = new HotkeySettings { Ctrl = true, Key = "D1" };
            _exitHotkeyListener = new HotkeyListener(exitHotkey);
            _exitHotkeyListener.HotkeyPressed += (_, _) => Exit(this, EventArgs.Empty);

            _trayIcon.BalloonTipTitle = "WindowsSidekick";
            _trayIcon.BalloonTipText = $"Hotkeys: {GetHotkeyString()}, Ctrl+Shift+`, Ctrl+1";
            _trayIcon.ShowBalloonTip(3000);
        }

        private string GetHotkeyString()
        {
            var h = _settings.Hotkey;
            return $"{(h.Ctrl ? "Ctrl+" : "")}{(h.Alt ? "Alt+" : "")}{(h.Shift ? "Shift+" : "")}{h.Key}";
        }

        private async void OnHotkey(object? sender, EventArgs e)
        {
            IntPtr hwnd = HotkeyListener.LastForegroundWindow;
            using Bitmap screenshot = WindowCapture.Capture(hwnd);
            var screen = Screen.FromHandle(hwnd);

            // Prompt form
            using var promptForm = new PromptForm(screenshot);
            promptForm.StartPosition = FormStartPosition.Manual;
            if (_lastPromptLocation.HasValue)
            {
                promptForm.Location = _lastPromptLocation.Value;
            }
            else
            {
                promptForm.Location = new Point(
                    screen.WorkingArea.X + (screen.WorkingArea.Width - promptForm.Width) / 2,
                    screen.WorkingArea.Y + (screen.WorkingArea.Height - promptForm.Height) / 2
                );
            }
            promptForm.Move += (_, _) => _lastPromptLocation = promptForm.Location;
            var promptResult = promptForm.ShowDialog();
            if (promptResult != DialogResult.OK)
                return;

            string userPrompt = promptForm.UserPrompt;
            // Query assistant
            string response = await QueryAssistantAsync(userPrompt, screenshot);

            // Response form
            var responseForm = new ResponseForm(response);
            responseForm.StartPosition = FormStartPosition.Manual;
            if (_lastResponseLocation.HasValue)
            {
                responseForm.Location = _lastResponseLocation.Value;
            }
            else
            {
                responseForm.Location = new Point(
                    screen.WorkingArea.X + (screen.WorkingArea.Width - responseForm.Width) / 2,
                    screen.WorkingArea.Y + (screen.WorkingArea.Height - responseForm.Height) / 2
                );
            }
            responseForm.Move += (_, _) => _lastResponseLocation = responseForm.Location;
            responseForm.ResendRequested += async (_, _) =>
            {
                string followUp = responseForm.UserReply;
                string nextResp = await QueryAssistantAsync(followUp, screenshot);
                responseForm.UpdateContent(nextResp);
            };
            responseForm.ShowDialog();
        }

        private async System.Threading.Tasks.Task<string> QueryAssistantAsync(string prompt, Bitmap screenshot)
        {
            _trayIcon.BalloonTipTitle = "WindowsSidekick";
            _trayIcon.BalloonTipText = "Querying assistant…";
            _trayIcon.ShowBalloonTip(1000);
            try
            {
                return await _gemmaClient.ChatWithImageAsync(
                    _settings.SystemPrompt,
                    prompt,
                    screenshot
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error calling assistant: {ex.Message}",
                    "WindowsSidekick",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return string.Empty;
            }
        }

        private void Exit(object? sender, EventArgs e)
        {
            _hotkeyListener.Dispose();
            _altHotkeyListener.Dispose();
            _exitHotkeyListener.Dispose();
            _trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
