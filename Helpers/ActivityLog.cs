using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityBotWinForms.Helpers
{
    // keeps a record of everything the bot has done this session
    public static class ActivityLog
    {
        private static readonly List<string> _entries = new();

        // adds a new action with the current time
        public static void Add(string action)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            _entries.Add($"[{timestamp}] {action}");
        }

        // returns the most recent entries — newest first
        public static List<string> GetRecent(int count = 10)
        {
            return _entries
                .AsEnumerable()
                .Reverse()
                .Take(count)
                .ToList();
        }

        // returns everything in the log
        public static List<string> GetAll() => new List<string>(_entries);

        public static int TotalCount => _entries.Count;
    }
}