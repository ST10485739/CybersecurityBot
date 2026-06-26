using System;
using System.Drawing;
using System.Windows.Forms;
using CybersecurityBotWinForms.Helpers;

namespace CybersecurityBotWinForms.Forms
{
    // shows the activity log — everything the bot has done this session
    public class ActivityLogPanel : Panel
    {
        private static readonly Color ColBg = Color.FromArgb(5, 9, 18);
        private static readonly Color ColPanel = Color.FromArgb(4, 10, 20);
        private static readonly Color ColGreen = Color.FromArgb(0, 255, 65);
        private static readonly Color ColText = Color.FromArgb(210, 235, 210);
        private static readonly Color ColCyan = Color.FromArgb(0, 190, 220);
        private static readonly Color ColDim = Color.FromArgb(60, 100, 60);

        private RichTextBox _logDisplay;
        private Label _countLabel;
        private bool _showingAll = false;

        public ActivityLogPanel()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ColBg;
            BuildLayout();
        }

        private void BuildLayout()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = ColPanel, Padding = new Padding(14, 0, 14, 0) };
            header.Controls.Add(new Label { Text = "📋  ACTIVITY LOG", ForeColor = ColGreen, Font = new Font("Consolas", 11F, FontStyle.Bold), Dock = DockStyle.Left, Width = 260, TextAlign = ContentAlignment.MiddleLeft });
            _countLabel = new Label { ForeColor = ColDim, Font = new Font("Consolas", 9F), Dock = DockStyle.Right, Width = 200, TextAlign = ContentAlignment.MiddleRight };
            header.Controls.Add(_countLabel);
            this.Controls.Add(header);

            _logDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = ColPanel,
                ForeColor = ColText,
                Font = new Font("Consolas", 10.5F),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(14, 10, 14, 10)
            };
            this.Controls.Add(_logDisplay);

            var btnBar = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = ColPanel, Padding = new Padding(14, 6, 14, 6) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = ColPanel };

            var refreshBtn = MakeBtn("🔄  Refresh", ColGreen);
            refreshBtn.Click += (s, e) => RefreshLog();

            Button toggleBtn = MakeBtn("📂  Show More", ColCyan);
            toggleBtn.Click += (s, e) =>
            {
                _showingAll = !_showingAll;
                toggleBtn.Text = _showingAll ? "📁  Show Less" : "📂  Show More";
                RefreshLog();
            };

            flow.Controls.Add(refreshBtn);
            flow.Controls.Add(toggleBtn);
            btnBar.Controls.Add(flow);
            this.Controls.Add(btnBar);

            RefreshLog();
        }

        public void RefreshLog()
        {
            _logDisplay.Clear();
            var entries = _showingAll ? ActivityLog.GetAll() : ActivityLog.GetRecent(10);

            if (entries.Count == 0)
            {
                Append("No actions recorded yet.\n", ColDim);
                Append("Start chatting, add a task, or play the quiz to see activity here.", ColDim);
                _countLabel.Text = "0 entries";
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                string entry = entries[i];
                if (entry.StartsWith("["))
                {
                    int close = entry.IndexOf(']');
                    if (close > 0)
                    {
                        Append($"{i + 1:D2}.  ", ColDim);
                        Append(entry.Substring(0, close + 1) + " ", ColCyan);
                        Append(entry.Substring(close + 1) + "\n", ColText);
                        continue;
                    }
                }
                Append($"{i + 1:D2}.  {entry}\n", ColText);
            }

            int total = ActivityLog.TotalCount;
            _countLabel.Text = _showingAll ? $"Showing all {total} entries" : $"Showing {Math.Min(10, total)} of {total} entries";
        }

        private void Append(string text, Color color)
        {
            _logDisplay.SelectionStart = _logDisplay.TextLength;
            _logDisplay.SelectionLength = 0;
            _logDisplay.SelectionColor = color;
            _logDisplay.AppendText(text);
        }

        private static Button MakeBtn(string text, Color foreColor) => new Button
        {
            Text = text,
            ForeColor = foreColor,
            BackColor = Color.FromArgb(6, 14, 24),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9.5F),
            Height = 30,
            Width = 140,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };
    }
}