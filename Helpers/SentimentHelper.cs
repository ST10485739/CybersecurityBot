using System;
using System.Collections.Generic;

namespace CybersecurityBotWinForms.Helpers
{
    // detects the user's mood and returns a fitting response opener
    // kept in its own class so it doesn't clutter the response logic
    public static class SentimentHelper
    {
        private static readonly Random Rng = new();

        // keywords grouped by mood
        private static readonly List<string> WorriedKeywords = new()
        {
            "worried", "scared", "afraid", "anxious", "nervous",
            "stressed", "overwhelmed", "panic", "terrified", "unsafe", "fear"
        };

        private static readonly List<string> FrustratedKeywords = new()
        {
            "frustrated", "angry", "annoyed", "confused", "lost",
            "don't understand", "dont understand", "difficult",
            "hard", "complicated", "hate", "useless"
        };

        private static readonly List<string> CuriousKeywords = new()
        {
            "curious", "interested", "wondering", "want to know",
            "tell me", "how does", "what is", "explain", "why", "learn", "teach me"
        };

        private static readonly List<string> HappyKeywords = new()
        {
            "great", "awesome", "love this", "amazing", "helpful",
            "thank", "excited", "happy", "fantastic", "brilliant"
        };

        // response lines that match each mood
        private static readonly List<string> WorriedPrefixes = new()
        {
            "It's completely understandable to feel that way — cybersecurity threats are real. Let me help you.",
            "I hear you. Many people feel the same way. The good news is there are clear steps you can take.",
            "Your concern is valid and shows you're taking this seriously. Here's what you can do:"
        };

        private static readonly List<string> FrustratedPrefixes = new()
        {
            "Don't worry — cybersecurity can feel overwhelming at first. Let's break it down simply.",
            "I get it, this stuff can be confusing. Let me explain it as clearly as possible.",
            "No judgment here — let's take it one step at a time and make this easy to understand."
        };

        private static readonly List<string> CuriousPrefixes = new()
        {
            "Great question! Curiosity is the first step to staying safe. Here's what you need to know:",
            "Love the enthusiasm! The more you know, the safer you'll be. Here you go:",
            "Excellent! Let's dive in:"
        };

        private static readonly List<string> HappyPrefixes = new()
        {
            "Glad to hear it! 😊",
            "That's the spirit! 🙌 Here's more:",
            "Wonderful! Here you go:"
        };

        // checks the input for mood keywords and returns an empathetic opening line
        // returns null if no mood is detected
        public static string? DetectSentiment(string input)
        {
            if (ContainsAny(input, WorriedKeywords)) return Pick(WorriedPrefixes);
            if (ContainsAny(input, FrustratedKeywords)) return Pick(FrustratedPrefixes);
            if (ContainsAny(input, CuriousKeywords)) return Pick(CuriousPrefixes);
            if (ContainsAny(input, HappyKeywords)) return Pick(HappyPrefixes);
            return null;
        }

        private static bool ContainsAny(string input, List<string> keywords)
        {
            foreach (var kw in keywords)
                if (input.Contains(kw)) return true;
            return false;
        }

        private static string Pick(List<string> list) => list[Rng.Next(list.Count)];
    }
}