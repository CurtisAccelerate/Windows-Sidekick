// path: ./WindowsSidekick/ResponseForm.cs
#nullable enable
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Markdig;
using Microsoft.Web.WebView2.WinForms;

namespace WindowsSidekick
{
    public class ResponseForm : Form
    {
        // Colors & transparency
        private readonly Color _bgColor      = Color.FromArgb(22, 22, 22);
        private readonly Color _textColor    = Color.FromArgb(230, 230, 230);
        private readonly Color _accentColor  = Color.MediumSeaGreen;
        private readonly Color _textAreaBg   = Color.FromArgb(30, 30, 30);
        private readonly float _opacityLevel = 0.95f;

        private DialogResult _userChoice = DialogResult.OK;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DialogResult UserChoice
        {
            get => _userChoice;
            private set => _userChoice = value;
        }

        public event EventHandler? ResendRequested;
        public string UserReply => _replyBox.Text;

        private string _explanation = "";
        private string _code = "";
        private WebView2 _webView = null!;
        private RichTextBox _codeBox = null!;
        private TextBox _replyBox = null!;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION       = 0x2;

        public ResponseForm(string responseText)
        {
            ExtractResponse(responseText);

            // Form styling
            Text            = "Windows Sidekick – Response";
            StartPosition   = FormStartPosition.CenterScreen;
            Size            = new Size(780, 640);
            BackColor       = _bgColor;
            ForeColor       = _textColor;
            Opacity         = _opacityLevel;
            FormBorderStyle = FormBorderStyle.None;
            TopMost         = true;
            KeyPreview      = true;

            // Title/drag bar
            var dragBar = new Panel { Height = 32, Dock = DockStyle.Top, BackColor = _bgColor };
            dragBar.MouseDown += Form_MouseDown;
            Controls.Add(dragBar);

            var lblTitle = new Label
            {
                Text      = Text,
                ForeColor = _textColor,
                BackColor = _bgColor,
                Font      = new Font("Cascadia Code", 12, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(8, 6)
            };
            dragBar.Controls.Add(lblTitle);

            var btnCloseX = new Button
            {
                Text      = "✕",
                ForeColor = _textColor,
                BackColor = Color.DarkRed,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Cascadia Code", 12, FontStyle.Bold),
                Size      = new Size(32, 28),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCloseX.FlatAppearance.BorderSize = 0;
            btnCloseX.Click += (_, _) => Close();
            dragBar.Controls.Add(btnCloseX);
            dragBar.Resize += (_, _) => btnCloseX.Location = new Point(dragBar.Width - btnCloseX.Width - 8, 2);

            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    UserChoice = DialogResult.OK;
                    Close();
                }
            };

            // Layout
            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                BackColor   = _bgColor,
                RowCount    = 6,
                ColumnCount = 1,
                Padding     = new Padding(0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            // Increased bottom row height to add spacing under buttons
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            Controls.Add(layout);

            // Title placeholder
            layout.Controls.Add(new Panel { BackColor = _bgColor }, 0, 0);

            // WebView2 for markdown rendering
            _webView = new WebView2 { Dock = DockStyle.Fill, BackColor = _bgColor };
            layout.Controls.Add(_webView, 0, 1);
            _webView.CoreWebView2InitializationCompleted += (s, e) =>
            {
                _webView.CoreWebView2.NavigateToString(BuildHtml(_explanation, _code));
            };
            _webView.EnsureCoreWebView2Async();

            // Copy panel
            var copyPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock          = DockStyle.Fill,
                BackColor     = _bgColor,
                Padding       = new Padding(10, 0, 0, 0)
            };
            layout.Controls.Add(copyPanel, 0, 2);

            var btnCopy = new Button
            {
                Text       = "📋 Copy",
                FlatStyle  = FlatStyle.Flat,
                ForeColor  = _textColor,
                BackColor  = Color.Transparent,
                Font       = new Font("Cascadia Code", 10),
                Size       = new Size(80, 28),
                Margin     = new Padding(0, 4, 0, 4),
                Region     = Region.FromHrgn(CreateRoundRectRgn(0,0,80,28,8,8))
            };
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.Click += (_, _) =>
            {
                var toCopy = string.IsNullOrEmpty(_codeBox.SelectedText)
                    ? _codeBox.Text
                    : _codeBox.SelectedText;
                Clipboard.SetText(toCopy);
                Close();
            };
            copyPanel.Controls.Add(btnCopy);

            // Code label
            var lblCode = new Label
            {
                Text      = "Code (edit if needed):",
                Dock      = DockStyle.Fill,
                ForeColor = _accentColor,
                Font      = new Font("Cascadia Code", 10, FontStyle.Bold),
                Padding   = new Padding(10, 0, 0, 0)
            };
            layout.Controls.Add(lblCode, 0, 3);

            // Code box
            _codeBox = new RichTextBox
            {
                ReadOnly    = false,
                Dock        = DockStyle.Fill,
                BackColor   = _textAreaBg,
                ForeColor   = _accentColor,
                Font        = new Font("Cascadia Code", 10),
                Text        = _code,
                BorderStyle = BorderStyle.None,
                Margin      = new Padding(10, 0, 10, 0)
            };
            layout.Controls.Add(_codeBox, 0, 4);

            // Bottom panel (reply + buttons)
            var bottomPanel = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                BackColor   = _bgColor,
                ColumnCount = 2,
                RowCount    = 1,
                Padding     = new Padding(10)
            };
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            layout.Controls.Add(bottomPanel, 0, 5);

            _replyBox = new TextBox
            {
                Multiline       = true,
                Dock            = DockStyle.Fill,
                BackColor       = _textAreaBg,
                ForeColor       = _textColor,
                Font            = new Font("Cascadia Code", 10),
                BorderStyle     = BorderStyle.None,
                PlaceholderText = "Type your follow-up here..."
            };
            bottomPanel.Controls.Add(_replyBox, 0, 0);

            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock          = DockStyle.Fill,
                BackColor     = _bgColor
            };
            bottomPanel.Controls.Add(btnPanel, 1, 0);

            var btnSend = new Button
            {
                Text       = "▶ Send",
                Size       = new Size(80, 32),
                BackColor  = Color.DodgerBlue,
                ForeColor  = _textColor,
                FlatStyle  = FlatStyle.Flat,
                Font       = new Font("Cascadia Code", 10),
                Margin     = new Padding(5),
                Region     = Region.FromHrgn(CreateRoundRectRgn(0,0,80,32,8,8))
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += (_, _) => ResendRequested?.Invoke(this, EventArgs.Empty);
            btnPanel.Controls.Add(btnSend);

            var btnClose = new Button
            {
                Text       = "✕ Close",
                Size       = new Size(80, 32),
                BackColor  = Color.Gray,
                ForeColor  = _textColor,
                FlatStyle  = FlatStyle.Flat,
                Font       = new Font("Cascadia Code", 10),
                Margin     = new Padding(5),
                Region     = Region.FromHrgn(CreateRoundRectRgn(0,0,80,32,8,8))
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, _) =>
            {
                UserChoice = DialogResult.OK;
                Close();
            };
            btnPanel.Controls.Add(btnClose);
        }

        private void ExtractResponse(string text)
        {
            const string fence = "```";
            int start = text.IndexOf(fence, StringComparison.Ordinal);
            int end   = start >= 0
                ? text.IndexOf(fence, start + fence.Length, StringComparison.Ordinal)
                : -1;

            if (start >= 0 && end > start + fence.Length)
            {
                _explanation = text.Substring(0, start).Trim();
                var block    = text.Substring(start + fence.Length, end - (start + fence.Length));
                var lines    = block.Split('\n');
                _code = (lines.Length > 1 && lines[0].Trim().All(c => char.IsLetter(c) || c == '#'))
                    ? string.Join("\n", lines.Skip(1)).TrimEnd()
                    : block.TrimEnd();
            }
            else
            {
                _explanation = text.Trim();
                _code = string.Empty;
            }
        }

        private string BuildHtml(string exp, string code)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            string combined = exp + (string.IsNullOrWhiteSpace(code) ? "" : "\n\n```" + code + "```");
            string bodyHtml = Markdown.ToHtml(combined, pipeline);

            // translate your form colors to CSS
            var bg = ColorTranslator.ToHtml(_bgColor);
            var text = ColorTranslator.ToHtml(_textColor);
            var areaBg = ColorTranslator.ToHtml(_textAreaBg);
            var accent = ColorTranslator.ToHtml(_accentColor);

            return $@"<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    html, body {{
      margin: 0; padding: 0;
      background: {bg};
      color: {text};
      font-family: 'Cascadia Code', monospace;
      overflow: hidden;           /* no scrollbars on body */
    }}
    pre, code {{
      background: {areaBg};
      padding: 8px;
      border-radius: 4px;
      white-space: pre-wrap;      /* wrap long lines */
      word-wrap: break-word;
      margin: 0;
    }}
    h1, h2, h3, h4 {{ color: {accent}; }}
    a {{ color: {accent}; }}
    /* hide any remaining scrollbars */
    ::-webkit-scrollbar {{ width: 0; height: 0; }}
    -ms-overflow-style: none;
    scrollbar-width: none;
  </style>
</head>
<body>
{bodyHtml}
</body>
</html>";
        }

        public void UpdateContent(string responseText)
        {
            ExtractResponse(responseText);
            if (_webView.CoreWebView2 != null)
                _webView.CoreWebView2.NavigateToString(BuildHtml(_explanation, _code));
            _codeBox.Text = _code;
            _replyBox.Clear();
        }

        private void Form_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        public new DialogResult ShowDialog()
        {
            base.ShowDialog();
            return UserChoice;
        }
    }
}
