using System;
using CybersecurityBotWinForms.Helpers;

namespace CybersecurityBotWinForms.Models
{
    // main chatbot class — stores the user's info and handles the conversation
    public class ChatBot
    {
        // stores the user's name and favourite topic
        public string? UserName { get; private set; }
        public string? FavouriteTopic { get; private set; }

        public bool SessionEnded { get; private set; }

        // tracks the last topic discussed and how many follow-ups were asked
        private string? _lastTopic;
        private int _followUpCount;

        // delegate that points to the response method
        private readonly Func<string, string, string?, string> _responseSelector;

        public ChatBot()
        {
            // connect the delegate to the response helper method
            _responseSelector = (input, userName, lastTopic) =>
                ResponseHelper.GetResponse(input, userName, lastTopic);
        }

        // saves and formats the user's name
        public void SetUserName(string raw)
        {
            raw = raw.Trim();
            UserName = char.ToUpper(raw[0]) + raw.Substring(1).ToLower();
        }

        // returns the first message shown after the user enters their name
        public string GetWelcomeMessage()
        {
            return $"Great to meet you, {UserName}! 👋\n\n" +
                   "I am your Cybersecurity Awareness Bot — here to help you stay safe online.\n\n" +
                   "You can ask me about:\n" +
                   "  🔑 Password safety\n" +
                   "  🎣 Phishing and scams\n" +
                   "  🔒 Privacy and social engineering\n" +
                   "  🛡 Malware and viruses\n" +
                   "  🔐 Two-Factor Authentication (2FA)\n" +
                   "  🌐 Safe browsing\n\n" +
                   "Type 'help' to see all topics, or use the quick chips below!";
        }

        // processes each message the user sends after their name is captured
        public string GetResponse(string userInput)
        {
            string cleaned = userInput.Trim().ToLower();

            // check for goodbye first
            if (IsGoodbye(cleaned))
            {
                SessionEnded = true;
                return BuildGoodbyeMessage();
            }

            // detect the user's mood and get an empathetic opener if needed
            string? sentimentPrefix = SentimentHelper.DetectSentiment(cleaned);

            // check if the user is asking for more info on the last topic
            if (IsFollowUp(cleaned))
            {
                _followUpCount++;
                string followUp = _lastTopic != null
                    ? ResponseHelper.GetFollowUp(_lastTopic, _followUpCount, UserName!)
                    : $"I don't have a previous topic to continue on, {UserName}. " +
                      "What cybersecurity topic would you like to explore?";
                return Combine(sentimentPrefix, followUp);
            }

            // check if the user mentioned a topic they're interested in and save it
            TryExtractInterest(cleaned);

            // add a personalised line if the user's saved interest comes up
            string? personalPrefix = BuildPersonalPrefix(cleaned);

            // detect the topic and route to the right response
            string? topic = ResponseHelper.DetectTopic(cleaned);
            if (topic != null)
            {
                _lastTopic = topic;
                _followUpCount = 0;
            }

            string mainResponse = _responseSelector(cleaned, UserName!, _lastTopic);

            return Combine(sentimentPrefix, Combine(personalPrefix, mainResponse));
        }

        private static bool IsGoodbye(string s) =>
            s is "exit" or "quit" or "bye" or "goodbye" ||
            s.StartsWith("bye ") || s.StartsWith("goodbye ");

        private static bool IsFollowUp(string s) =>
            s.Contains("tell me more") || s.Contains("explain more") ||
            s.Contains("give me another") || s.Contains("more info") ||
            s.Contains("more details") || s == "more" || s == "continue" ||
            s.Contains("go on") || s.Contains("another tip") ||
            s.Contains("what else");

        // looks for phrases like "I'm interested in X" and saves the topic
        private void TryExtractInterest(string s)
        {
            string[] patterns = { "interested in ", "i like ", "i love ",
                                   "i care about ", "focus on " };
            foreach (var p in patterns)
            {
                int idx = s.IndexOf(p, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    string after = s[(idx + p.Length)..].Trim(' ', '.', '!', '?');
                    if (!string.IsNullOrWhiteSpace(after))
                        FavouriteTopic = after.Length > 30 ? after[..30] : after;
                }
            }
        }

        // adds a personalised line if the user's saved interest is mentioned
        private string? BuildPersonalPrefix(string s)
        {
            if (FavouriteTopic == null) return null;
            if (s.Contains(FavouriteTopic.ToLower()))
                return $"As someone interested in {FavouriteTopic}, this is especially relevant for you.";
            return null;
        }

        private string BuildGoodbyeMessage() =>
            $"Goodbye, {UserName}! 👋 Stay safe online. Remember:\n\n" +
            "  🔑 Use strong, unique passwords\n" +
            "  🎣 Watch out for phishing emails\n" +
            "  🌐 Always browse safely\n" +
            "  🔐 Enable 2FA on important accounts\n\n" +
            "Until next time — stay cyber-safe! 🛡";

        // joins a prefix and a body with a blank line between them
        private static string Combine(string? prefix, string body)
        {
            if (string.IsNullOrEmpty(prefix)) return body;
            return prefix.TrimEnd() + "\n\n" + body;
        }
    }
}