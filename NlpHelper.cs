using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CybersecurityBotWinForms.Helpers
{
    // figures out what the user is trying to do even when they phrase things differently
    public static class NlpHelper
    {
        private static readonly List<string> AddTaskPhrases = new()
        {
            "add task", "create task", "new task", "add a task",
            "create a task", "make a task", "set task", "add reminder",
            "set a reminder", "remind me", "set reminder", "create reminder",
            "i need to", "i want to", "schedule", "don't forget"
        };

        private static readonly List<string> ViewTaskPhrases = new()
        {
            "view tasks", "show tasks", "list tasks", "my tasks",
            "what tasks", "see tasks", "show my tasks", "view my tasks",
            "pending tasks", "current tasks", "open tasks", "all tasks"
        };

        private static readonly List<string> QuizPhrases = new()
        {
            "quiz", "mini game", "mini-game", "game", "test my knowledge",
            "test me", "question", "play", "challenge", "start quiz",
            "cybersecurity quiz", "start the quiz"
        };

        private static readonly List<string> LogPhrases = new()
        {
            "activity log", "show log", "what have you done",
            "recent actions", "show history", "action log",
            "what did you do", "log", "show activity", "history"
        };

        // detects what the user wants to do
        public static UserIntent DetectIntent(string input)
        {
            input = input.ToLower().Trim();

            if (ContainsAny(input, AddTaskPhrases)) return UserIntent.AddTask;
            if (ContainsAny(input, ViewTaskPhrases)) return UserIntent.ViewTasks;
            if (ContainsAny(input, QuizPhrases)) return UserIntent.StartQuiz;
            if (ContainsAny(input, LogPhrases)) return UserIntent.ShowLog;

            return UserIntent.Unknown;
        }

        // pulls the task title out of a message like "add task to enable 2FA"
        public static string ExtractTaskTitle(string input)
        {
            input = input.Trim();

            string[] stripPhrases = {
                "add task", "create task", "new task", "add a task", "create a task",
                "add reminder", "set reminder", "set a reminder", "remind me to",
                "remind me", "i need to", "i want to", "don't forget to",
                "make a task", "make task", "schedule"
            };

            foreach (var phrase in stripPhrases)
            {
                if (input.ToLower().Contains(phrase))
                {
                    int idx = input.ToLower().IndexOf(phrase);
                    input = input.Substring(idx + phrase.Length).Trim();
                    break;
                }
            }

            string[] fillerStarts = { "to ", "for ", "about ", "that ", "the " };
            foreach (var filler in fillerStarts)
                if (input.ToLower().StartsWith(filler))
                    input = input.Substring(filler.Length).Trim();

            if (string.IsNullOrWhiteSpace(input))
                return "Cybersecurity task";

            return char.ToUpper(input[0]) + input.Substring(1);
        }

        // tries to pull a number of days from things like "in 3 days" or "tomorrow"
        public static int? ExtractDays(string input)
        {
            var match = Regex.Match(input, @"\bin\s+(\d+)\s+days?\b", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
                return days;

            if (input.Contains("tomorrow")) return 1;
            if (input.Contains("next week") || input.Contains("a week")) return 7;
            if (input.Contains("next month")) return 30;

            return null;
        }

        private static bool ContainsAny(string input, List<string> phrases)
        {
            foreach (var phrase in phrases)
                if (input.Contains(phrase)) return true;
            return false;
        }
    }

    // the possible things a user might be trying to do
    public enum UserIntent
    {
        Unknown,
        AddTask,
        ViewTasks,
        StartQuiz,
        ShowLog
    }
}