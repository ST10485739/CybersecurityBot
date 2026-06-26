using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CybersecurityBotWinForms.Data;
using CybersecurityBotWinForms.Helpers;

namespace CybersecurityBotWinForms.Forms
{
    // the Task Assistant tab — add, view, complete and delete cybersecurity tasks
    public class TaskPanel : Panel
    {
        private static readonly Color ColBg = Color.FromArgb(5, 9, 18);
        private static readonly Color ColPanel = Color.FromArgb(4, 10, 20);
        private static readonly Color ColGreen = Color.FromArgb(0, 255, 65);
        private static readonly Color ColText = Color.FromArgb(210, 235, 210);
        private static readonly Color ColCyan = Color.FromArgb(0, 190, 220);
        private static readonly Color ColRed = Color.FromArgb(200, 60, 60);

        private TextBox _titleBox;
        private TextBox _descBox;
        private CheckBox _reminderCheck;
        private DateTimePicker _datePicker;
        private Button _addBtn;
        private ListView _taskList;
        private Label _statusLabel;

        public TaskPanel()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ColBg;
            BuildLayout();
            LoadTasks();
        }

        private void BuildLayout()
        {
            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ColBg,
                Padding = new Padding(12)
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            split.Controls.Add(BuildAddForm(), 0, 0);
            split.Controls.Add(BuildTaskList(), 1, 0);
            this.Controls.Add(split);

            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                BackColor = ColPanel,
                ForeColor = ColGreen,
                Font = new Font("Consolas", 9F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Height = 28
            };
            this.Controls.Add(_statusLabel);
        }

        private Panel BuildAddForm()
        {
            var panel = new Panel
            {
                BackColor = ColPanel,
                Dock = DockStyle.Fill,
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 8, 0)
            };

            AddLabel(panel, "➕ ADD NEW TASK", ColGreen, 10, bold: true);
            AddLabel(panel, "Task Title:", ColCyan, 42);

            _titleBox = new TextBox
            {
                Top = 62,
                Left = 14,
                Width = 298,
                BackColor = Color.FromArgb(6, 14, 24),
                ForeColor = ColText,
                Font = new Font("Consolas", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(_titleBox);

            AddLabel(panel, "Description:", ColCyan, 96);

            _descBox = new TextBox
            {
                Top = 116,
                Left = 14,
                Width = 298,
                Height = 80,
                BackColor = Color.FromArgb(6, 14, 24),
                ForeColor = ColText,
                Font = new Font("Consolas", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            panel.Controls.Add(_descBox);

            _reminderCheck = new CheckBox
            {
                Text = "Set reminder date",
                Top = 210,
                Left = 14,
                ForeColor = ColText,
                Font = new Font("Consolas", 10F),
                AutoSize = true
            };
            _reminderCheck.CheckedChanged += (s, e) => _datePicker.Visible = _reminderCheck.Checked;
            panel.Controls.Add(_reminderCheck);

            _datePicker = new DateTimePicker
            {
                Top = 234,
                Left = 14,
                Width = 298,
                MinDate = DateTime.Today,
                Visible = false,
                Format = DateTimePickerFormat.Short
            };
            panel.Controls.Add(_datePicker);

            _addBtn = new Button
            {
                Text = "✔  ADD TASK",
                Top = 274,
                Left = 14,
                Width = 298,
                Height = 32,
                BackColor = Color.FromArgb(4, 10, 20),
                ForeColor = ColGreen,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 10F),
                Cursor = Cursors.Hand
            };
            _addBtn.Click += AddBtn_Click;
            panel.Controls.Add(_addBtn);

            AddLabel(panel, "Quick-add common tasks:", ColCyan, 318);

            var quickTasks = new (string label, string title, string desc)[]
            {
                ("Enable 2FA",        "Enable two-factor authentication",    "Set up 2FA on all important accounts."),
                ("Review privacy",    "Review account privacy settings",     "Check privacy settings on all social media accounts."),
                ("Update passwords",  "Update weak passwords",               "Replace old passwords with strong unique ones."),
                ("Install antivirus", "Install / update antivirus software", "Make sure antivirus is installed and up to date."),
            };

            int qTop = 340;
            foreach (var (lbl, t, d) in quickTasks)
            {
                string capturedTitle = t;
                string capturedDesc = d;
                var qBtn = new Button
                {
                    Text = $"⚡ {lbl}",
                    Top = qTop,
                    Left = 14,
                    Width = 298,
                    Height = 26,
                    BackColor = Color.FromArgb(0, 40, 15),
                    ForeColor = Color.FromArgb(0, 200, 70),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Consolas", 9.5F),
                    Cursor = Cursors.Hand
                };
                qBtn.Click += (s, e) => { _titleBox.Text = capturedTitle; _descBox.Text = capturedDesc; };
                panel.Controls.Add(qBtn);
                qTop += 30;
            }

            return panel;
        }

        private Control BuildTaskList()
        {
            var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };

            var header = new Label
            {
                Text = "📋  YOUR CYBERSECURITY TASKS",
                ForeColor = ColGreen,
                Font = new Font("Consolas", 10F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft
            };
            wrapper.Controls.Add(header);

            _taskList = new ListView
            {
                Dock = DockStyle.Fill,
                BackColor = ColPanel,
                ForeColor = ColText,
                Font = new Font("Consolas", 10F),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BorderStyle = BorderStyle.None
            };
            _taskList.Columns.Add("Title", 200);
            _taskList.Columns.Add("Description", 300);
            _taskList.Columns.Add("Reminder", 100);
            _taskList.Columns.Add("Status", 80);
            _taskList.Columns.Add("Added", 130);
            wrapper.Controls.Add(_taskList);

            var btnBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                BackColor = ColPanel,
                Padding = new Padding(4)
            };

            var completeBtn = MakeBtn("✔  Mark Complete", ColGreen, 160);
            completeBtn.Click += CompleteBtn_Click;

            var deleteBtn = MakeBtn("🗑  Delete", ColRed, 110);
            deleteBtn.Click += DeleteBtn_Click;

            var refreshBtn = MakeBtn("🔄  Refresh", ColCyan, 110);
            refreshBtn.Click += (s, e) => LoadTasks();

            btnBar.Controls.Add(completeBtn);
            btnBar.Controls.Add(deleteBtn);
            btnBar.Controls.Add(refreshBtn);
            wrapper.Controls.Add(btnBar);

            return wrapper;
        }

        public void LoadTasks()
        {
            try
            {
                _taskList.Items.Clear();
                foreach (var t in DatabaseHelper.GetAllTasks())
                {
                    var item = new ListViewItem(t.Title);
                    item.SubItems.Add(t.Description);
                    item.SubItems.Add(t.ReminderDate.HasValue ? t.ReminderDate.Value.ToString("dd MMM yyyy") : "None");
                    item.SubItems.Add(t.IsCompleted ? "✔ Done" : "Pending");
                    item.SubItems.Add(t.CreatedAt.ToString("dd MMM HH:mm"));
                    item.Tag = t.Id;
                    item.ForeColor = t.IsCompleted ? Color.FromArgb(60, 120, 60) : ColText;
                    _taskList.Items.Add(item);
                }
                SetStatus($"{_taskList.Items.Count} task(s) loaded.");
            }
            catch (Exception ex) { SetStatus($"⚠ Could not load tasks: {ex.Message}"); }
        }

        private void AddBtn_Click(object? sender, EventArgs e)
        {
            string title = _titleBox.Text.Trim();
            string desc = _descBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title)) { SetStatus("⚠ Please enter a task title."); return; }
            if (string.IsNullOrWhiteSpace(desc)) desc = $"Cybersecurity task: {title}";

            DateTime? reminder = _reminderCheck.Checked ? _datePicker.Value.Date : null;
            try
            {
                DatabaseHelper.AddTask(title, desc, reminder);
                string reminderText = reminder.HasValue ? $"Reminder: {reminder.Value:dd MMM yyyy}." : "No reminder.";
                ActivityLog.Add($"Task added: '{title}'. {reminderText}");
                SetStatus($"✔ Task '{title}' added. {reminderText}");
                _titleBox.Text = ""; _descBox.Text = ""; _reminderCheck.Checked = false;
                LoadTasks();
            }
            catch (Exception ex) { SetStatus($"⚠ Error: {ex.Message}"); }
        }

        private void CompleteBtn_Click(object? sender, EventArgs e)
        {
            if (_taskList.SelectedItems.Count == 0) { SetStatus("⚠ Select a task first."); return; }
            var item = _taskList.SelectedItems[0];
            try
            {
                DatabaseHelper.MarkCompleted((int)item.Tag);
                ActivityLog.Add($"Task marked as completed: '{item.Text}'.");
                SetStatus($"✔ '{item.Text}' marked as completed.");
                LoadTasks();
            }
            catch (Exception ex) { SetStatus($"⚠ Error: {ex.Message}"); }
        }

        private void DeleteBtn_Click(object? sender, EventArgs e)
        {
            if (_taskList.SelectedItems.Count == 0) { SetStatus("⚠ Select a task first."); return; }
            var item = _taskList.SelectedItems[0];
            if (MessageBox.Show($"Delete '{item.Text}'?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                DatabaseHelper.DeleteTask((int)item.Tag);
                ActivityLog.Add($"Task deleted: '{item.Text}'.");
                SetStatus($"🗑 '{item.Text}' deleted.");
                LoadTasks();
            }
            catch (Exception ex) { SetStatus($"⚠ Error: {ex.Message}"); }
        }

        private void SetStatus(string msg) => _statusLabel.Text = "  " + msg;

        private static void AddLabel(Panel panel, string text, Color color, int top, bool bold = false)
        {
            panel.Controls.Add(new Label
            {
                Text = text,
                ForeColor = color,
                Top = top,
                Left = 14,
                Font = new Font("Consolas", bold ? 10F : 9.5F, bold ? FontStyle.Bold : FontStyle.Regular),
                AutoSize = true,
                BackColor = Color.Transparent
            });
        }

        private static Button MakeBtn(string text, Color foreColor, int width) => new Button
        {
            Text = text,
            ForeColor = foreColor,
            BackColor = Color.FromArgb(4, 10, 20),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Consolas", 9.5F),
            Height = 28,
            Width = width,
            Cursor = Cursors.Hand
        };
    }
}