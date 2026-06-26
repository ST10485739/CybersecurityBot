using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CybersecurityBotWinForms.Data;
using CybersecurityBotWinForms.Forms;
using CybersecurityBotWinForms.Helpers;
using CybersecurityBotWinForms.Models;

namespace CybersecurityBotWinForms
{
    public class MainForm : Form
    {
        // UI controls for the chat tab
        private RichTextBox _chatDisplay;
        private TextBox _inputBox;
        private Button _sendButton;
        private Label _memoryLabel;
        private Label _userPromptLabel;

        // the other panels — they live in their own tabs
        private TaskPanel _taskPanel;
        private QuizPanel _quizPanel;
        private ActivityLogPanel _logPanel;
        private TabControl _tabs;

        // the chatbot brain from part 2
        private readonly ChatBot _bot = new ChatBot();
        private bool _nameEntered = false;

        // whether we're waiting for the user to confirm adding a task via chat
        // these hold the pending task details while we wait for a reminder response
        private bool _awaitingReminderReply = false;
        private string? _pendingTaskTitle = null;
        private string? _pendingTaskDesc = null;

        // colour theme — same dark green palette as parts 1 and 2
        private static readonly Color ColBgMain = Color.FromArgb(8, 16, 30);
        private static readonly Color ColBgPanel = Color.FromArgb(4, 10, 20);
        private static readonly Color ColBgInput = Color.FromArgb(6, 14, 24);
        private static readonly Color ColBgChat = Color.FromArgb(5, 9, 18);
        private static readonly Color ColGreen = Color.FromArgb(0, 255, 65);
        private static readonly Color ColCyan = Color.FromArgb(0, 190, 220);
        private static readonly Color ColGreenDim = Color.FromArgb(0, 100, 35);
        private static readonly Color ColText = Color.FromArgb(210, 235, 210);
        private static readonly Color ColBotLbl = Color.FromArgb(0, 200, 70);
        private static readonly Color ColUserLbl = Color.FromArgb(0, 170, 200);

        public MainForm()
        {
            this.SuspendLayout();
            SetupForm();
            BuildLayout();
            this.ResumeLayout(false);
            this.Shown += OnFormShown;
        }

        private void SetupForm()
        {
            this.Text = "CyberGuard Bot — Cybersecurity Awareness Assistant";
            this.Size = new Size(1200, 820);
            this.MinimumSize = new Size(900, 660);
            this.BackColor = ColBgMain;
            this.ForeColor = ColGreen;
            this.Font = new Font("Consolas", 10F);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // the overall layout: header on top, then a tab control for all the sections
        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = ColBgMain,
                Padding = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 115)); // header
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // tabs

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildTabs(), 0, 1);

            this.Controls.Add(root);
        }

        // the ASCII art header — same as part 2
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

            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(0, 55, 18), 1);
                e.Graphics.DrawLine(pen, 0, panel.Height - 1, panel.Width, panel.Height - 1);
            };

            panel.Controls.Add(ascii);
            return panel;
        }

        // builds the tab control with 4 tabs: Chat, Tasks, Quiz, Activity Log
        private TabControl BuildTabs()
        {
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = ColBgMain,
                Font = new Font("Consolas", 10F)
            };

            // style the tab pages to match the dark theme
            _tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            _tabs.DrawItem += Tabs_DrawItem;

            //  Chat tab 
            var chatTab = new TabPage("💬  Chat")
            {
                BackColor = ColBgMain,
                Padding = new Padding(0)
            };
            chatTab.Controls.Add(BuildChatContent());
            _tabs.TabPages.Add(chatTab);

            // Tasks tab 
            var tasksTab = new TabPage("✔  Tasks")
            {
                BackColor = ColBgMain,
                Padding = new Padding(0)
            };
            _taskPanel = new TaskPanel();
            tasksTab.Controls.Add(_taskPanel);
            _tabs.TabPages.Add(tasksTab);

            // Quiz tab 
            var quizTab = new TabPage("🎮  Quiz")
            {
                BackColor = ColBgMain,
                Padding = new Padding(0)
            };
            _quizPanel = new QuizPanel();
            quizTab.Controls.Add(_quizPanel);
            _tabs.TabPages.Add(quizTab);

            // Activity Log tab 
            var logTab = new TabPage("📋  Activity Log")
            {
                BackColor = ColBgMain,
                Padding = new Padding(0)
            };
            _logPanel = new ActivityLogPanel();
            logTab.Controls.Add(_logPanel);
            _tabs.TabPages.Add(logTab);

            // refresh the log every time the user switches to that tab
            _tabs.SelectedIndexChanged += (s, e) =>
            {
                if (_tabs.SelectedIndex == 3)
                    _logPanel.RefreshLog();
            };

            return _tabs;
        }

        // custom draw for the tab headers so they match the dark theme
        private void Tabs_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var tab = _tabs.TabPages[e.Index];
            var rect = _tabs.GetTabRect(e.Index);
            bool selected = (e.Index == _tabs.SelectedIndex);

            using var bgBrush = new SolidBrush(selected ? ColBgChat : ColBgPanel);
            e.Graphics.FillRectangle(bgBrush, rect);

            using var fgBrush = new SolidBrush(selected ? ColGreen : Color.FromArgb(80, 140, 80));
            using var font = new Font("Consolas", 9.5F, selected ? FontStyle.Bold : FontStyle.Regular);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(tab.Text, font, fgBrush, rect, sf);
        }

        // builds the chat tab content — same layout as part 2 with the sidebar
        private Control BuildChatContent()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 2,
                BackColor = ColBgMain,
                Padding = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 215));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

            var chatArea = BuildChatArea();
            var sidebar = BuildSidebar();
            var inputBar = BuildInputBar();

            layout.Controls.Add(chatArea, 0, 0);
            layout.Controls.Add(sidebar, 1, 0);
            layout.Controls.Add(inputBar, 0, 1);
            layout.SetColumnSpan(inputBar, 2);

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

        // the right sidebar with topic chips and the memory panel
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
                ("🔑 Passwords",     "password"),
                ("🎣 Phishing",      "phishing"),
                ("⚠ Scams",         "scam"),
                ("🔒 Privacy",       "privacy"),
                ("🛡 Malware",       "malware"),
                ("🔐 2FA",           "2fa"),
                ("🌐 Safe Browsing", "safe browsing"),
                ("🎭 Social Eng.",   "social engineering"),
            };

            foreach (var (lbl, tag) in topics)
                flow.Controls.Add(MakeTopicButton(lbl, tag));

            flow.Controls.Add(MakeSectionLabel("ACTIONS"));
            flow.Controls.Add(MakeTopicButton("💡 Random Tip", "give me a tip"));
            flow.Controls.Add(MakeTopicButton("❓ Help", "help"));
            flow.Controls.Add(MakeTopicButton("➕ Tell Me More", "tell me more"));
            flow.Controls.Add(MakeTopicButton("📋 Activity Log", "show activity log"));
            flow.Controls.Add(MakeTopicButton("✔  Add Task", "add task"));
            flow.Controls.Add(MakeTopicButton("🎮 Start Quiz", "start quiz"));

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

        // the input bar at the bottom of the chat tab
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
                Padding = new Padding(0)
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

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

        // plays the greeting audio and shows the opening message
        private void OnFormShown(object? sender, EventArgs e)
        {
            // try to set up the database — if it fails we warn the user but carry on
            try
            {
                DatabaseHelper.Initialise();
                ActivityLog.Add("Application started. Database connected.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not connect to MySQL:\n{ex.Message}\n\n" +
                    "Tasks will not be saved this session. Make sure MySQL is running with password '1234'.",
                    "Database Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                ActivityLog.Add("Application started. Database connection failed.");
            }

            var thread = new Thread(() => AudioHelper.PlayGreeting());
            thread.IsBackground = true;
            thread.Start();

            AppendBotMessage(
                "Welcome to CyberGuard Bot — your cybersecurity awareness assistant!\n\n" +
                "New in this version:\n" +
                "  ✔  Task Assistant — manage your cybersecurity to-do list\n" +
                "  🎮  Quiz Mini-Game — test your knowledge\n" +
                "  📋  Activity Log — see everything I've done for you\n\n" +
                "Type your name to get started:");

            _inputBox.Focus();
        }

        private void SendButton_Click(object? sender, EventArgs e) => ProcessInput();

        private void InputBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ProcessInput();
            }
        }

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

        // the main message handler — runs on every user submission
        // combines the old Part 2 logic with the new NLP routing from Part 3
        private void ProcessInput()
        {
            string input = _inputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            _inputBox.Clear();
            _inputBox.Focus();

            // first message is always the user's name
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
                AppendBotMessage(_bot.GetWelcomeMessage() +
                    "\n\n🆕 New: Use the tabs above for Tasks, Quiz, and Activity Log!");
                UpdateMemoryPanel();
                ActivityLog.Add($"Session started — user: {_bot.UserName}.");
                return;
            }

            AppendUserMessage(input);
            string cleaned = input.ToLower().Trim();

            // if we're mid-flow waiting for a reminder response 
            if (_awaitingReminderReply)
            {
                HandleReminderReply(cleaned, input);
                return;
            }

            //  NLP intent detection 
            // check what the user is trying to do before falling through to the regular response engine
            UserIntent intent = NlpHelper.DetectIntent(cleaned);

            switch (intent)
            {
                case UserIntent.AddTask:
                    HandleAddTaskIntent(cleaned, input);
                    return;

                case UserIntent.ViewTasks:
                    AppendBotMessage($"Opening the Tasks tab for you, {_bot.UserName}! You can view, add, and manage all your cybersecurity tasks there.");
                    ActivityLog.Add("NLP: User asked to view tasks — navigated to Tasks tab.");
                    _tabs.SelectedIndex = 1; // switch to the Tasks tab
                    return;

                case UserIntent.StartQuiz:
                    AppendBotMessage($"Let's test your cybersecurity knowledge, {_bot.UserName}! Opening the Quiz tab now. Good luck! 🎮");
                    ActivityLog.Add("NLP: User asked to start quiz — navigated to Quiz tab.");
                    _tabs.SelectedIndex = 2; // switch to the Quiz tab
                    return;

                case UserIntent.ShowLog:
                    HandleShowLogIntent();
                    return;
            }

            //  fall back to the Part 2 response engine 
            string response = _bot.GetResponse(input);
            AppendBotMessage(response);
            UpdateMemoryPanel();

            // log NLP interactions where the bot detected a specific topic
            string? topic = ResponseHelper.DetectTopic(cleaned);
            if (topic != null)
                ActivityLog.Add($"NLP: User asked about '{topic}' — response given.");

            if (_bot.SessionEnded)
            {
                _inputBox.Enabled = false;
                _sendButton.Enabled = false;
                _userPromptLabel.Text = "[ SESSION ENDED ]";
                _userPromptLabel.ForeColor = Color.FromArgb(70, 70, 70);
                ActivityLog.Add("Session ended by user.");
            }
        }

        // handles when NLP detects the user wants to add a task via chat
        // extracts the title and asks about a reminder before saving
        private void HandleAddTaskIntent(string cleaned, string raw)
        {
            string title = NlpHelper.ExtractTaskTitle(raw);
            string desc = $"Cybersecurity task added via chat: {title}";

            // check if they also mentioned a time frame in the same message
            int? days = NlpHelper.ExtractDays(cleaned);
            if (days.HasValue)
            {
                // they gave us everything in one message — save it straight away
                DateTime reminder = DateTime.Today.AddDays(days.Value);
                try
                {
                    DatabaseHelper.AddTask(title, desc, reminder);
                    ActivityLog.Add($"Task added (via chat NLP): '{title}'. Reminder: {reminder:dd MMM yyyy}.");
                    AppendBotMessage(
                        $"✔ Task added: '{title}'\n\n" +
                        $"📅 Reminder set for {reminder:dd MMMM yyyy} ({days} day{(days == 1 ? "" : "s")} from today).\n\n" +
                        "You can view and manage all your tasks in the Tasks tab.");
                    _taskPanel.LoadTasks();
                }
                catch (Exception ex)
                {
                    AppendBotMessage($"⚠ Task could not be saved: {ex.Message}");
                }
            }
            else
            {
                // save the task title temporarily and ask about a reminder
                _pendingTaskTitle = title;
                _pendingTaskDesc = desc;
                _awaitingReminderReply = true;
                AppendBotMessage(
                    $"Task noted: '{title}'\n\n" +
                    "Would you like a reminder? If yes, say something like 'remind me in 7 days' or 'tomorrow'. " +
                    "Otherwise just say 'no'.");
            }
        }

        // called when we're waiting for the user's reminder reply after adding a task via chat
        private void HandleReminderReply(string cleaned, string raw)
        {
            _awaitingReminderReply = false;

            // user said no reminder
            if (cleaned is "no" or "nope" or "no thanks" or "none" || cleaned.Contains("no reminder"))
            {
                try
                {
                    DatabaseHelper.AddTask(_pendingTaskTitle!, _pendingTaskDesc!, null);
                    ActivityLog.Add($"Task added (via chat NLP): '{_pendingTaskTitle}'. No reminder.");
                    AppendBotMessage(
                        $"✔ Task '{_pendingTaskTitle}' saved with no reminder.\n\n" +
                        "You can view it in the Tasks tab anytime.");
                    _taskPanel.LoadTasks();
                }
                catch (Exception ex)
                {
                    AppendBotMessage($"⚠ Task could not be saved: {ex.Message}");
                }
            }
            else
            {
                // try to extract a number of days from their reply
                int? days = NlpHelper.ExtractDays(cleaned);
                DateTime reminder = days.HasValue
                    ? DateTime.Today.AddDays(days.Value)
                    : DateTime.Today.AddDays(7); // default to 7 days if we couldn't parse

                try
                {
                    DatabaseHelper.AddTask(_pendingTaskTitle!, _pendingTaskDesc!, reminder);
                    ActivityLog.Add($"Reminder set for task '{_pendingTaskTitle}': {reminder:dd MMM yyyy}.");
                    AppendBotMessage(
                        $"✔ Got it! Task '{_pendingTaskTitle}' saved.\n\n" +
                        $"📅 I'll remind you on {reminder:dd MMMM yyyy}.\n\n" +
                        "You can see it in the Tasks tab.");
                    _taskPanel.LoadTasks();
                }
                catch (Exception ex)
                {
                    AppendBotMessage($"⚠ Task could not be saved: {ex.Message}");
                }
            }

            _pendingTaskTitle = null;
            _pendingTaskDesc = null;
        }

        // shows the last 5 log entries inline in the chat and switches to the log tab
        private void HandleShowLogIntent()
        {
            var entries = ActivityLog.GetRecent(5);
            if (entries.Count == 0)
            {
                AppendBotMessage("No activity has been logged yet this session. Try adding a task or playing the quiz!");
                return;
            }

            string summary = $"Here's a summary of recent actions for you, {_bot.UserName}:\n\n";
            for (int i = 0; i < entries.Count; i++)
                summary += $"  {i + 1}. {entries[i]}\n";
            summary += "\nSwitch to the Activity Log tab to see the full history.";

            AppendBotMessage(summary);
            ActivityLog.Add("User requested activity log summary.");
            _logPanel.RefreshLog();
        }

        //  UI helpers from Part 2 (unchanged) 

        private void AppendBotMessage(string text)
        {
            AppendColored("[ CYBER GUARD ]\n", ColBotLbl, bold: true);
            AppendColored(text + "\n\n", ColText);
            _chatDisplay.ScrollToCaret();
        }

        private void AppendUserMessage(string text)
        {
            string name = _nameEntered ? _bot.UserName!.ToUpper() : "YOU";
            AppendColored($"[ {name} ]\n", ColUserLbl, bold: true);
            AppendColored(text + "\n\n", Color.FromArgb(195, 215, 240));
            _chatDisplay.ScrollToCaret();
        }

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

        private void UpdateMemoryPanel()
        {
            string text = $"Name: {_bot.UserName}\n";
            text += _bot.FavouriteTopic != null
                ? $"Interested in:\n  {_bot.FavouriteTopic}"
                : "Interests: none yet";
            _memoryLabel.Text = text;
            _memoryLabel.ForeColor = Color.FromArgb(80, 160, 80);
        }

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

        private static string GetAsciiArt() =>
            "  ██████╗██╗   ██╗██████╗ ███████╗██████╗      ██████╗ ██╗   ██╗ █████╗ ██████╗ ██████╗ \n" +
            " ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗    ██╔════╝ ██║   ██║██╔══██╗██╔══██╗██╔══██╗\n" +
            " ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝    ██║  ███╗██║   ██║███████║██████╔╝██║  ██║\n" +
            " ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗    ██║   ██║██║   ██║██╔══██║██╔══██╗██║  ██║\n" +
            " ╚██████╗   ██║   ██████╔╝███████╗██║  ██║    ╚██████╔╝╚██████╔╝██║  ██║██║  ██║██████╔╝\n" +
            "  ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝     ╚═════╝  ╚═════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═════╝  \n" +
            "                            --- Keeping You Safe Online ---                                ";
    }
}
