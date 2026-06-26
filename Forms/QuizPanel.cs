using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CybersecurityBotWinForms.Helpers;
using CybersecurityBotWinForms.Models;

namespace CybersecurityBotWinForms.Forms
{
    // the quiz mini-game — 12 questions, one at a time, with score tracking
    public class QuizPanel : Panel
    {
        private static readonly Color ColBg = Color.FromArgb(5, 9, 18);
        private static readonly Color ColPanel = Color.FromArgb(4, 10, 20);
        private static readonly Color ColGreen = Color.FromArgb(0, 255, 65);
        private static readonly Color ColText = Color.FromArgb(210, 235, 210);
        private static readonly Color ColCyan = Color.FromArgb(0, 190, 220);
        private static readonly Color ColRed = Color.FromArgb(220, 60, 60);
        private static readonly Color ColYellow = Color.FromArgb(255, 200, 50);

        private List<QuizQuestion> _questions = new();
        private int _currentIndex = 0;
        private int _score = 0;
        private bool _answered = false;

        private Label _progressLabel;
        private Label _scoreLabel;
        private Label _questionLabel;
        private Panel _optionsPanel;
        private Label _feedbackLabel;
        private Button _nextBtn;
        private Panel _startScreen;
        private Panel _quizScreen;
        private Panel _endScreen;
        private Label _endResultLabel;
        private Label _endTipLabel;

        public QuizPanel()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ColBg;
            _questions = new List<QuizQuestion>(QuizBank.Questions);
            BuildLayout();
            ShowStartScreen();
        }

        private void BuildLayout()
        {
            _startScreen = BuildStartScreen();
            this.Controls.Add(_startScreen);

            _quizScreen = BuildQuizScreen();
            _quizScreen.Visible = false;
            this.Controls.Add(_quizScreen);

            _endScreen = BuildEndScreen();
            _endScreen.Visible = false;
            this.Controls.Add(_endScreen);
        }

        private Panel BuildStartScreen()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };

            var box = new Panel { Width = 560, Height = 380, BackColor = ColPanel };

            box.Controls.Add(MakeLabel("🛡  CYBERSECURITY QUIZ", ColGreen, 16F, FontStyle.Bold, top: 24));
            box.Controls.Add(MakeLabel(
                $"Test your cybersecurity knowledge!\n\n" +
                $"• {QuizBank.Questions.Count} questions — passwords, phishing, malware, 2FA.\n" +
                "• Multiple-choice and true/false.\n" +
                "• Immediate feedback after each answer.\n" +
                "• Final score and rating at the end.",
                ColText, 11F, top: 72, height: 180));

            var startBtn = MakeButton("▶  START QUIZ", ColGreen, 140, 36, top: 295);
            startBtn.Click += (s, e) => StartQuiz();
            box.Controls.Add(startBtn);

            panel.Controls.Add(box);
            panel.Resize += (s, e) => { box.Left = (panel.Width - box.Width) / 2; box.Top = (panel.Height - box.Height) / 2; };
            return panel;
        }

        private Panel BuildQuizScreen()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = ColBg, Padding = new Padding(40, 20, 40, 20) };

            var topBar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ColPanel,
                Padding = new Padding(10, 4, 10, 4)
            };
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _progressLabel = new Label { Text = "Question 1 / 12", ForeColor = ColCyan, Font = new Font("Consolas", 10F), Dock = DockStyle.Fill };
            _scoreLabel = new Label { Text = "Score: 0 / 0", ForeColor = ColGreen, Font = new Font("Consolas", 10F), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
            topBar.Controls.Add(_progressLabel, 0, 0);
            topBar.Controls.Add(_scoreLabel, 1, 0);
            panel.Controls.Add(topBar);

            _questionLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 90,
                ForeColor = ColText,
                Font = new Font("Consolas", 13F),
                Padding = new Padding(0, 16, 0, 0),
                AutoSize = false
            };
            panel.Controls.Add(_questionLabel);

            _optionsPanel = new Panel { Dock = DockStyle.Top, Height = 200, BackColor = ColBg, Padding = new Padding(0, 8, 0, 0) };
            panel.Controls.Add(_optionsPanel);

            _feedbackLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(6, 18, 6),
                Font = new Font("Consolas", 10F),
                Padding = new Padding(10, 8, 10, 8),
                Visible = false,
                AutoSize = false
            };
            panel.Controls.Add(_feedbackLabel);

            _nextBtn = MakeButton("NEXT  ▶", ColCyan, 130, 34, top: 0);
            _nextBtn.Visible = false;
            _nextBtn.Click += NextBtn_Click;
            panel.Controls.Add(_nextBtn);

            return panel;
        }

        private Panel BuildEndScreen()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };
            var box = new Panel { Width = 560, Height = 380, BackColor = ColPanel };

            box.Controls.Add(MakeLabel("🏆  QUIZ COMPLETE!", ColGreen, 15F, FontStyle.Bold, top: 24));

            _endResultLabel = new Label { Top = 68, Left = 30, Width = 500, Height = 80, ForeColor = ColYellow, Font = new Font("Consolas", 13F), AutoSize = false };
            box.Controls.Add(_endResultLabel);

            _endTipLabel = new Label { Top = 148, Left = 30, Width = 500, Height = 120, ForeColor = ColText, Font = new Font("Consolas", 10.5F), AutoSize = false };
            box.Controls.Add(_endTipLabel);

            var restartBtn = MakeButton("🔄  Play Again", ColGreen, 140, 34, top: 300, left: 30);
            restartBtn.Click += (s, e) => StartQuiz();
            box.Controls.Add(restartBtn);

            var homeBtn = MakeButton("🏠  Back to Chat", ColCyan, 150, 34, top: 300, left: 184);
            homeBtn.Click += (s, e) => { if (this.Parent is TabPage tp && tp.Parent is TabControl tc) tc.SelectedIndex = 0; };
            box.Controls.Add(homeBtn);

            panel.Controls.Add(box);
            panel.Resize += (s, e) => { box.Left = (panel.Width - box.Width) / 2; box.Top = (panel.Height - box.Height) / 2; };
            return panel;
        }

        private void ShowStartScreen()
        {
            _startScreen.Visible = true;
            _quizScreen.Visible = false;
            _endScreen.Visible = false;
        }

        private void StartQuiz()
        {
            _questions = new List<QuizQuestion>(QuizBank.Questions);
            ShuffleList(_questions);
            _currentIndex = 0;
            _score = 0;
            _answered = false;

            ActivityLog.Add("Quiz started.");
            _startScreen.Visible = false;
            _endScreen.Visible = false;
            _quizScreen.Visible = true;
            ShowQuestion();
        }

        private void ShowQuestion()
        {
            if (_currentIndex >= _questions.Count) { ShowEndScreen(); return; }

            _answered = false;
            var q = _questions[_currentIndex];

            _progressLabel.Text = $"Question {_currentIndex + 1} / {_questions.Count}";
            _scoreLabel.Text = $"Score: {_score} / {_currentIndex}";
            _questionLabel.Text = q.Question;
            _feedbackLabel.Visible = false;
            _nextBtn.Visible = false;
            _optionsPanel.Controls.Clear();

            int top = 0;
            for (int i = 0; i < q.Options.Count; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text = $"{(char)('A' + i)})  {q.Options[i]}",
                    Top = top,
                    Left = 0,
                    Width = _optionsPanel.Width > 0 ? _optionsPanel.Width - 4 : 600,
                    Height = 40,
                    BackColor = Color.FromArgb(6, 14, 24),
                    ForeColor = ColText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Consolas", 10.5F),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(8, 0, 0, 0),
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(20, 55, 20);
                btn.Click += (s, e) => HandleAnswer(idx, q);
                _optionsPanel.Controls.Add(btn);
                top += 44;
            }
        }

        private void HandleAnswer(int selectedIndex, QuizQuestion q)
        {
            if (_answered) return;
            _answered = true;

            bool correct = selectedIndex == q.CorrectIndex;
            if (correct) _score++;

            for (int i = 0; i < _optionsPanel.Controls.Count; i++)
            {
                var btn = (Button)_optionsPanel.Controls[i];
                if (i == q.CorrectIndex) btn.BackColor = Color.FromArgb(0, 60, 20);
                else if (i == selectedIndex && !correct) btn.BackColor = Color.FromArgb(70, 10, 10);
            }

            _feedbackLabel.Text = correct ? $"✔  Correct!\n\n💡 {q.Explanation}" : $"✗  Incorrect.\n\n💡 {q.Explanation}";
            _feedbackLabel.ForeColor = correct ? ColGreen : ColRed;
            _feedbackLabel.Visible = true;
            _scoreLabel.Text = $"Score: {_score} / {_currentIndex + 1}";
            _nextBtn.Visible = true;
            _nextBtn.Text = _currentIndex + 1 < _questions.Count ? "NEXT  ▶" : "RESULTS  ▶";
        }

        private void NextBtn_Click(object? sender, EventArgs e) { _currentIndex++; ShowQuestion(); }

        private void ShowEndScreen()
        {
            _quizScreen.Visible = false;
            _endScreen.Visible = true;

            double pct = (double)_score / _questions.Count * 100;
            string rating;
            Color ratingColor;

            if (pct >= 90) { rating = "🏆 Outstanding! You're a cybersecurity pro!"; ratingColor = ColGreen; }
            else if (pct >= 70) { rating = "👍 Great job! Solid cybersecurity knowledge."; ratingColor = ColCyan; }
            else if (pct >= 50) { rating = "📖 Not bad, but keep learning!"; ratingColor = ColYellow; }
            else { rating = "⚠ Keep studying — knowledge keeps you safe!"; ratingColor = ColRed; }

            _endResultLabel.Text = $"You scored  {_score} / {_questions.Count}  ({pct:0}%)\n\n{rating}";
            _endResultLabel.ForeColor = ratingColor;
            _endTipLabel.Text =
                "Key takeaways:\n" +
                "  🔑 Use strong, unique passwords\n" +
                "  🎣 Treat every unexpected email with suspicion\n" +
                "  🔐 Enable 2FA on all important accounts\n" +
                "  🛡 Keep your software up to date\n" +
                "  🌐 Use a VPN on public Wi-Fi";

            ActivityLog.Add($"Quiz completed — score: {_score}/{_questions.Count} ({pct:0}%).");
        }

        private static void ShuffleList<T>(List<T> list)
        {
            var rng = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static Label MakeLabel(string text, Color color, float size = 10F, FontStyle style = FontStyle.Regular, int top = 0, int height = 24) =>
            new Label { Text = text, ForeColor = color, Font = new Font("Consolas", size, style), Top = top, Left = 30, Width = 500, Height = height, AutoSize = false, BackColor = Color.Transparent };

        private static Button MakeButton(string text, Color foreColor, int width, int height, int top = 0, int left = 30) =>
            new Button { Text = text, ForeColor = foreColor, BackColor = Color.FromArgb(4, 10, 20), FlatStyle = FlatStyle.Flat, Font = new Font("Consolas", 10F), Width = width, Height = height, Top = top, Left = left, Cursor = Cursors.Hand };
    }
}