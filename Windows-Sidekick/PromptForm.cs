// path: WindowsSidekick/PromptForm.cs
using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsSidekick
{
    public partial class PromptForm : Form
    {
        // Screenshot to display
        private readonly Image _screenshot;

        // Public property to access the user's input
        public string UserPrompt => promptInput.Text;

        // Colors for dark theme
        private readonly Color _bgColor          = Color.FromArgb(18, 18, 22);
        private readonly Color _textBoxBgColor   = Color.FromArgb(35, 35, 40);
        private readonly Color _buttonColor      = Color.FromArgb(50, 120, 230);
        private readonly Color _viewButtonColor  = Color.FromArgb(40, 40, 45);
        private readonly Color _closeButtonColor = Color.FromArgb(80, 80, 80);  // changed to gray
        private readonly Color _textColor        = Color.FromArgb(230, 230, 230);

        // DWM APIs for dark title bar
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE            = 20;

        public PromptForm(Image screenshot)
        {
            _screenshot = screenshot;
            InitializeComponent();

            Opacity = 0.85;
            EnableImmersiveDarkMode();
            ApplyStyles();

            // hook up actions
            sendButton.Click  += SendButton_Click;
            buttonClose.Click += CloseButton_Click;
            ViewButton.Click  += ViewButton_Click;

            AcceptButton   = sendButton;
            CancelButton   = buttonClose;
            TopMost        = true;
            ShowInTaskbar  = false;
        }

        private void EnableImmersiveDarkMode()
        {
            if (Environment.OSVersion.Version.Major >= 10)
            {
                int useDark = 1;
                DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
                DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDark, sizeof(int));
            }
        }

        private void ApplyStyles()
        {
            // Form background
            BackColor = _bgColor;

            // TextBox styling
            promptInput.BackColor   = _textBoxBgColor;
            promptInput.ForeColor   = _textColor;
            promptInput.BorderStyle = BorderStyle.None;  // remove white border
            promptInput.Font        = new Font("Cascadia Code", 18F, FontStyle.Regular, GraphicsUnit.Point);

            // Send button styling (kept blue)
            sendButton.FlatStyle                 = FlatStyle.Flat;
            sendButton.FlatAppearance.BorderSize = 0;
            sendButton.BackColor                 = _buttonColor;
            sendButton.ForeColor                 = _textColor;
            sendButton.Font                      = new Font("Cascadia Code", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            sendButton.Region                    = Region.FromHrgn(
                Win32.CreateRoundRectRgn(0, 0, sendButton.Width, sendButton.Height, 8, 8));
            sendButton.Cursor                    = Cursors.Hand;

            // View button styling
            ViewButton.FlatStyle                 = FlatStyle.Flat;
            ViewButton.FlatAppearance.BorderSize = 0;
            ViewButton.BackColor                 = _viewButtonColor;
            ViewButton.ForeColor                 = _textColor;
            ViewButton.Font                      = new Font("Segoe UI Emoji", 12F, FontStyle.Regular, GraphicsUnit.Point);
            ViewButton.Text                      = "🖼️";
            ViewButton.Region                    = Region.FromHrgn(
                Win32.CreateRoundRectRgn(0, 0, ViewButton.Width, ViewButton.Height, 8, 8));
            ViewButton.Cursor                    = Cursors.Hand;

            // Close button styling (now gray)
            buttonClose.FlatStyle                  = FlatStyle.Flat;
            buttonClose.FlatAppearance.BorderSize  = 0;
            buttonClose.BackColor                  = _closeButtonColor;
            buttonClose.ForeColor                  = _textColor;
            buttonClose.Font                       = new Font("Cascadia Code", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            buttonClose.Region                     = Region.FromHrgn(
                Win32.CreateRoundRectRgn(0, 0, buttonClose.Width, buttonClose.Height, 8, 8));
            buttonClose.Cursor                     = Cursors.Hand;
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ViewButton_Click(object sender, EventArgs e)
        {
            using var verifyForm = new Form
            {
                Text            = "Verify Screenshot",
                StartPosition   = FormStartPosition.CenterParent,
                Width           = 800,
                Height          = 600,
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                TopMost         = true
            };

            var pictureBox = new PictureBox
            {
                Dock     = DockStyle.Fill,
                Image    = _screenshot,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            verifyForm.Controls.Add(pictureBox);
            verifyForm.ShowDialog(this);
        }
    }

    internal static class Win32
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);
    }
}
