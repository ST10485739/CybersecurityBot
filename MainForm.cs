using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CybersecurityBotWinForms.Helpers;
using CybersecurityBotWinForms.Models;

namespace CybersecurityBotWinForms
{
    public class MainForm : Form
    {
        // UI controls
        private RichTextBox _chatDisplay;
        private TextBox _inputBox;
        private Button _sendButton;
        private Label _memoryLabel;
        private Label _userPromptLabel;

        // chatbot instance and name tracking
        private readonly ChatBot _bot = new ChatBot();
        private bool _nameEntered = false;

        // colour theme for the dark UI
        private static readonly Color ColBgMain = Color.FromArgb(8, 16, 30);
        private static readonly Color ColBgPanel = Color.FromArgb(4, 10, 20);
        private static readonly Color ColBgInput = Color.FromArgb(6, 14, 24);
        private static readonly Color ColBgChat = Color.FromArgb(5, 9, 18);
        private static readonly Color ColGreen = Color.FromArgb(0, 255, 65);
        private static readonly Color ColCyan = Color.FromArgb(0, 190, 220);
        private static readonly Color ColGreenDim = Color.FromArgb(0, 100, 35);
        private static readonly Color ColText = Color.FromArgb(210, 235, 210);
        private static readonly Color ColBotLabel = Color.FromArgb(0, 200, 70);
        private static readonly Color ColUserLbl = Color.FromArgb(0, 170, 200);

        public MainForm()
        {
            this.SuspendLayout();
            SetupForm();
            BuildLayout();
            this.ResumeLayout(false);

            // using Shown instead of Load so the window handle exists before audio plays
            this.Shown += OnFormShown;
        }

        // sets the basic form properties
        private void SetupForm()
        {
            this.Text = "CyberGuard Bot — Cybersecurity Awareness Assistant";
            this.Size = new Size(1100, 760);
            this.MinimumSize = new Size(860, 620);
            this.BackColor = ColBgMain;
            this.ForeColor = ColGreen;
            this.Font = new Font("Consolas", 10F);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // builds the main layout with 3 rows: header, chat area, input bar
        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = ColBgMain,
                Padding = new Padding(0),
                Margin = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 115)); // header
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // content
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));  // input

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildContent(), 0, 1);
            root.Controls.Add(BuildInputBar(), 0, 2);

            this.Controls.Add(root);
        }

        // builds the header panel with the ASCII art banner
        private Panel BuildHeader()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBgPanel,
                Padding = new Padding(10, 6, 10, 0)
            };

            var ascii = new Label
            {
                Text = GetAsciiArt(),
                ForeColor = Color.FromArgb(0, 80, 25),
                Font = new Font("Consolas", 7F),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // draws a bottom border line on the header panel
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(0, 55, 18), 1);
                e.Graphics.DrawLine(pen, 0, panel.Height - 1, panel.Width, panel.Height - 1);
            };

            panel.Controls.Add(ascii);
            return panel;
        }

        // splits the content area into the chat display and the sidebar
        private Control BuildContent()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = ColBgMain,
                Padding = new Padding(0),
                Margin = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 215));

            layout.Controls.Add(BuildChatArea(), 0, 0);
            layout.Controls.Add(BuildSidebar(), 1, 0);
            return layout;
        }

        private RichTextBox BuildChatArea()
        {
            _chatDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = ColBgChat,
                ForeColor = ColText,
                Font = new Font("Consolas", 11F),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Padding = new Padding(14, 10, 14, 10),
                WordWrap = true
            };
            return _chatDisplay;
        }

        // builds the right sidebar with topic buttons and the memory panel
        private Panel BuildSidebar()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBgPanel,
                Padding = new Padding(10, 12, 10, 12),
                AutoScroll = true
            };

            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(0, 55, 18), 1);
                e.Graphics.DrawLine(pen, 0, 0, 0, panel.Height);
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = ColBgPanel,
                Padding = new Padding(0)
            };

            flow.Controls.Add(MakeSectionLabel("TOPICS"));

            var topics = new (string label, string tag)[]
            {
                ("🔑 Passwords",    "password"),
                ("🎣 Phishing",     "phishing"),
                ("⚠ Scams",        "scam"),
                ("🔒 Privacy",      "privacy"),
                ("🛡 Malware",      "malware"),
                ("🔐 2FA",          "2fa"),
                ("🌐 Safe Browsing","browsing"),
                ("🎭 Social Eng.",  "social engineering"),
            };

            foreach (var (lbl, tag) in topics)
                flow.Controls.Add(MakeTopicButton(lbl, tag));

            flow.Controls.Add(MakeSectionLabel("ACTIONS"));
            flow.Controls.Add(MakeTopicButton("💡 Random Tip", "give me a tip"));
            flow.Controls.Add(MakeTopicButton("❓ Help", "help"));
            flow.Controls.Add(MakeTopicButton("➕ Tell Me More", "tell me more"));

            var clearBtn = MakeTopicButton("🗑 Clear Chat", "__clear__");
            clearBtn.ForeColor = Color.FromArgb(180, 60, 60);
            flow.Controls.Add(clearBtn);

            flow.Controls.Add(MakeSectionLabel("MEMORY"));

            _memoryLabel = new Label
            {
                Text = "No session data yet.",
                ForeColor = Color.FromArgb(40, 90, 40),
                Font = new Font("Consolas", 9F),
                AutoSize = false,
                Width = 186,
                Height = 90,
                TextAlign = ContentAlignment.TopLeft
            };
            flow.Controls.Add(_memoryLabel);

            panel.Controls.Add(flow);
            return panel;
        }

        // builds the input bar at the bottom with the text box and send button
        private Panel BuildInputBar()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBgPanel,
                Padding = new Padding(14, 8, 14, 8)
            };

            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(0, 55, 18), 1);
                e.Graphics.DrawLine(pen, 0, 0, panel.Width, 0);
            };

            var inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = ColBgPanel,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // prompt label
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // text box
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); // send button

            _userPromptLabel = new Label
            {
                Text = "[ YOU ]: ",
                ForeColor = ColCyan,
                Font = new Font("Consolas", 11F, FontStyle.Bold),
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _inputBox = new TextBox
            {
                BackColor = ColBgInput,
                ForeColor = ColText,
                Font = new Font("Consolas", 12F),
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill
            };
            _inputBox.KeyDown += InputBox_KeyDown;

            _sendButton = new Button
            {
                Text = "SEND ▶",
                BackColor = ColBgInput,
                ForeColor = ColGreen,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 10F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand
            };
            _sendButton.FlatAppearance.BorderColor = ColGreenDim;
            _sendButton.FlatAppearance.MouseOverBackColor = ColGreenDim;
            _sendButton.Click += SendButton_Click;

            inputLayout.Controls.Add(_userPromptLabel, 0, 0);
            inputLayout.Controls.Add(_inputBox, 1, 0);
            inputLayout.Controls.Add(_sendButton, 2, 0);

            panel.Controls.Add(inputLayout);
            return panel;
        }

        // creates a section label for the sidebar
        private Label MakeSectionLabel(string text) => new Label
        {
            Text = text,
            ForeColor = ColGreen,
            Font = new Font("Consolas", 9F, FontStyle.Bold),
            AutoSize = false,
            Width = 186,
            Height = 24,
            TextAlign = ContentAlignment.BottomLeft,
            Margin = new Padding(0, 8, 0, 4)
        };

        // creates a styled button for the sidebar
        private Button MakeTopicButton(string label, string tag)
        {
            var btn = new Button
            {
                Text = label,
                Tag = tag,
                BackColor = ColBgPanel,
                ForeColor = Color.FromArgb(155, 200, 155),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 10F),
                Width = 186,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 2)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(20, 55, 20);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(8, 25, 8);
            btn.Click += TopicButton_Click;
            return btn;
        }

        // plays the greeting audio and shows the opening message when the form loads
        private void OnFormShown(object? sender, EventArgs e)
        {
            var thread = new Thread(() => AudioHelper.PlayGreeting());
            thread.IsBackground = true;
            thread.Start();

            AppendBotMessage(
                "Welcome to the Cybersecurity Awareness Bot!\n\n" +
                "Your personal cybersecurity awareness assistant.\n" +
                "Please type your name to get started:");

            _inputBox.Focus();
        }

        private void SendButton_Click(object? sender, EventArgs e) => ProcessInput();

        private void InputBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // stops the ding sound on Enter
                ProcessInput();
            }
        }

        // handles clicks on the sidebar topic buttons
        private void TopicButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            string tag = btn.Tag?.ToString() ?? "";

            if (tag == "__clear__")
            {
                _chatDisplay.Clear();
                if (_nameEntered)
                    AppendBotMessage($"Chat cleared! What would you like to know, {_bot.UserName}?");
                return;
            }

            if (!_nameEntered)
            {
                AppendBotMessage("Please enter your name first to start chatting!");
                return;
            }

            _inputBox.Text = tag;
            ProcessInput();
        }

        // handles all user input — collects the name first then routes normal messages
        private void ProcessInput()
        {
            string input = _inputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            _inputBox.Clear();
            _inputBox.Focus();

            // first input is always the user's name
            if (!_nameEntered)
            {
                AppendUserMessage(input);

                if (input.Length < 2)
                {
                    AppendBotMessage("That name seems too short. Please enter your full name:");
                    return;
                }

                _bot.SetUserName(input);
                _nameEntered = true;
                _userPromptLabel.Text = $"[ {_bot.UserName!.ToUpper()} ]: ";
                AppendBotMessage(_bot.GetWelcomeMessage());
                UpdateMemoryPanel();
                return;
            }

            // normal conversation after name is set
            AppendUserMessage(input);
            string response = _bot.GetResponse(input);
            AppendBotMessage(response);
            UpdateMemoryPanel();

            if (_bot.SessionEnded)
            {
                _inputBox.Enabled = false;
                _sendButton.Enabled = false;
                _userPromptLabel.Text = "[ SESSION ENDED ]";
                _userPromptLabel.ForeColor = Color.FromArgb(70, 70, 70);
            }
        }

        // adds a bot message to the chat display in green
        private void AppendBotMessage(string text)
        {
            AppendColored("[ CYBER GUARD ]\n", ColBotLabel, bold: true);
            AppendColored(text + "\n\n", ColText);
            _chatDisplay.ScrollToCaret();
        }

        // adds a user message to the chat display in cyan
        private void AppendUserMessage(string text)
        {
            string name = _nameEntered ? _bot.UserName!.ToUpper() : "YOU";
            AppendColored($"[ {name} ]\n", ColUserLbl, bold: true);
            AppendColored(text + "\n\n", Color.FromArgb(195, 215, 240));
            _chatDisplay.ScrollToCaret();
        }

        // appends coloured and optionally bold text to the chat display
        private void AppendColored(string text, Color color, bool bold = false)
        {
            _chatDisplay.SelectionStart = _chatDisplay.TextLength;
            _chatDisplay.SelectionLength = 0;
            _chatDisplay.SelectionColor = color;
            _chatDisplay.SelectionFont = bold
                ? new Font("Consolas", 10F, FontStyle.Bold)
                : new Font("Consolas", 11F);
            _chatDisplay.AppendText(text);
        }

        // updates the memory panel in the sidebar with the user's name and interests
        private void UpdateMemoryPanel()
        {
            string text = $"Name: {_bot.UserName}\n";
            text += _bot.FavouriteTopic != null
                ? $"Interested in:\n  {_bot.FavouriteTopic}"
                : "Interests: none yet";

            _memoryLabel.Text = text;
            _memoryLabel.ForeColor = Color.FromArgb(80, 160, 80);
        }

        private static string GetAsciiArt() =>
            "  ██████╗██╗   ██╗██████╗ ███████╗██████╗      ██████╗ ██╗   ██╗ █████╗ ██████╗ ██████╗ \n" +
            " ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗    ██╔════╝ ██║   ██║██╔══██╗██╔══██╗██╔══██╗\n" +
            " ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝    ██║  ███╗██║   ██║███████║██████╔╝██║  ██║\n" +
            " ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗    ██║   ██║██║   ██║██╔══██║██╔══██╗██║  ██║\n" +
            " ╚██████╗   ██║   ██████╔╝███████╗██║  ██║    ╚██████╔╝╚██████╔╝██║  ██║██║  ██║██████╔╝\n" +
            "  ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝     ╚═════╝  ╚═════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═════╝  \n" +
            "                           --- Keeping You Safe Online ---                                ";
    }
}