using System;
using System.Collections.Generic;

namespace CybersecurityBotWinForms.Helpers
{
    // handles all chatbot responses and keyword matching
    // uses dictionaries and lists to organise topics and their responses
    public static class ResponseHelper
    {
        private static readonly Random Rng = new();

        // random response pools — one list of responses per topic
        // a random one is picked each time so the bot doesn't repeat itself
        private static readonly Dictionary<string, List<string>> RandomPools = new(StringComparer.OrdinalIgnoreCase)
        {
            ["phishing"] = new()
            {
                "🎣 Be cautious of emails asking for personal information — scammers often disguise themselves as trusted organisations.",
                "🎣 Hover over links before clicking. If the URL looks misspelled or strange, do NOT click it.",
                "🎣 Legitimate banks and companies will NEVER ask for your password via email or SMS.",
                "🎣 Check the sender's actual email address, not just the display name — scammers copy display names easily.",
                "🎣 If an offer sounds too good to be true, it almost certainly is. Report it and delete it."
            },
            ["password"] = new()
            {
                "🔑 Use at least 12 characters mixing uppercase, lowercase, numbers, and symbols.",
                "🔑 Never reuse the same password across multiple sites — one breach exposes all accounts.",
                "🔑 A passphrase like 'Coffee!Rainy#Monday7' is both strong and memorable.",
                "🔑 Use a password manager like Bitwarden or KeePass to generate and store unique passwords.",
                "🔑 Change passwords immediately if you suspect any of your accounts have been compromised."
            },
            ["malware"] = new()
            {
                "🛡 Keep your OS and all apps updated — most malware exploits unpatched vulnerabilities.",
                "🛡 Never download software from unofficial sources or pop-up windows.",
                "🛡 Back up your files regularly — ransomware cannot touch your backups.",
                "🛡 Be careful with USB drives you did not personally format — they can carry malware.",
                "🛡 Install a reputable antivirus (Windows Defender is solid) and keep it active."
            },
            ["privacy"] = new()
            {
                "🔒 Review the privacy settings on ALL your social media accounts at least once a month.",
                "🔒 Limit what personal info you share publicly — your birthdate and phone number can be weaponised.",
                "🔒 Use a VPN on public Wi-Fi to encrypt your internet traffic.",
                "🔒 Browse in private/incognito mode when using shared or public computers.",
                "🔒 Be selective about which apps you grant location, microphone, and camera permissions to."
            },
            ["2fa"] = new()
            {
                "🔐 Even if your password is stolen, 2FA stops attackers from getting in.",
                "🔐 Use an authenticator app like Google Authenticator or Authy — SMS-based 2FA can be bypassed via SIM-swapping.",
                "🔐 Enable 2FA on your email first — it is the master key to all your other accounts.",
                "🔐 Hardware security keys like YubiKey are the most phishing-resistant 2FA option available.",
                "🔐 Most banks, email providers, and social platforms now support 2FA — enable it on all of them."
            },
            ["browsing"] = new()
            {
                "🌐 Always look for HTTPS and the padlock icon before entering any personal information.",
                "🌐 Use a privacy-focused browser like Firefox or Brave with uBlock Origin installed.",
                "🌐 Avoid banking or shopping on public Wi-Fi without a VPN.",
                "🌐 Keep your browser updated — outdated browsers are a prime target for drive-by attacks.",
                "🌐 Only install browser extensions from trusted developers with minimal required permissions."
            },
            ["social"] = new()
            {
                "🎭 Attackers research you on social media to craft convincing personalised scams.",
                "🎭 'Pretexting' means creating a fake story to gain your trust — be sceptical of all unsolicited contact.",
                "🎭 Always verify someone's identity before sharing sensitive information, even if they seem familiar.",
                "🎭 Urgency is a red flag — scammers create panic on purpose to stop you thinking clearly.",
                "🎭 When in doubt, hang up and call back on an official number you looked up yourself."
            }
        };

        // maps each topic name to the keywords that trigger it
        private static readonly Dictionary<string, string[]> TopicKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["phishing"] = new[] { "phishing", "scam", "email", "spam", "smishing", "vishing", "fake email" },
            ["password"] = new[] { "password", "passphrase", "credentials", "login", "passwords" },
            ["malware"] = new[] { "malware", "virus", "ransomware", "trojan", "spyware", "hack", "hacker", "infected" },
            ["privacy"] = new[] { "privacy", "personal info", "data", "tracking", "surveillance", "social media" },
            ["2fa"] = new[] { "two factor", "2fa", "authentication", "authenticator", "mfa", "otp" },
            ["browsing"] = new[] { "browsing", "browser", "internet", "website", "vpn", "https", "wifi", "wi-fi", "safe browsing" },
            ["social"] = new[] { "social engineering", "pretexting", "manipulation", "impersonation", "social eng" },
        };

        // extra tips shown when the user asks for more info on a topic
        // cycles through so each follow-up gives something new
        private static readonly Dictionary<string, List<string>> FollowUpTips = new(StringComparer.OrdinalIgnoreCase)
        {
            ["phishing"] = new() {
                "Report phishing emails to your IT team or mark them as spam straight away.",
                "Never call a phone number shown in a suspicious email — look up the official number yourself.",
                "Enable email filtering and anti-phishing tools in your email client or provider settings."
            },
            ["password"] = new() {
                "A password manager means you only need to remember one strong master password.",
                "Check if your email has been in a data breach at haveibeenpwned.com — it is free.",
                "Avoid security questions based on publicly available info like your mother's maiden name."
            },
            ["malware"] = new() {
                "Disconnect from Wi-Fi immediately if you suspect your device is infected.",
                "Use Malwarebytes for a second-opinion scan alongside your main antivirus.",
                "Ransomware is often spread via phishing emails — the two topics are closely linked!"
            },
            ["privacy"] = new() {
                "Use DuckDuckGo or Startpage instead of Google to reduce search tracking.",
                "Review your Google account's 'My Activity' page — you might be surprised what is stored.",
                "Regularly delete apps you no longer use — they may still collect data in the background."
            },
            ["2fa"] = new() {
                "Print and store backup codes in a safe place in case you lose access to your phone.",
                "Authy lets you back up your 2FA tokens — Google Authenticator does not by default.",
                "Check which of your accounts support 2FA at twofactorauth.org."
            },
            ["browsing"] = new() {
                "Firefox with uBlock Origin gives you strong ad and tracker blocking with no setup.",
                "Brave browser blocks ads and trackers by default — great for privacy out of the box.",
                "Check a website's safety at virustotal.com before downloading anything from it."
            },
            ["social"] = new() {
                "Train your colleagues too — social engineering attacks often target whole organisations.",
                "Be suspicious of any unsolicited contact requesting access or sensitive information.",
                "Never allow 'tech support' remote access to your computer unless you initiated the call."
            },
        };

        // checks the input and returns the matching topic name, or null if nothing matches
        public static string? DetectTopic(string input)
        {
            foreach (var (topic, keywords) in TopicKeywords)
                foreach (var kw in keywords)
                    if (input.Contains(kw))
                        return topic;
            return null;
        }

        // returns a response based on what the user typed
        // checks greetings and help first, then keywords, then falls back to a default message
        public static string GetResponse(string input, string userName, string? lastTopic)
        {
            // greetings
            if (input is "hello" or "hi" or "hey" ||
                input.StartsWith("hi ") ||
                input.StartsWith("hello ") ||
                input.StartsWith("hey "))
                return $"Hey {userName}! Great to see you. 👋 Ask me anything about staying safe online!";

            if (input.Contains("how are you"))
                return "I'm doing great, thanks for asking! 😊 Ready to help you with cybersecurity tips.";

            // purpose / what does this bot do
            if (input.Contains("purpose") || input.Contains("what do you do") || input.Contains("what are you"))
                return $"I'm the Cybersecurity Awareness Bot, {userName}! 🤖\n\n" +
                       "I help you understand how to stay safe in the digital world.\n" +
                       "Ask me about passwords, phishing, malware, 2FA, privacy, and safe browsing!";

            // help menu
            if (input.Contains("help") || input.Contains("topics") || input.Contains("what can i ask"))
                return "Here are all the topics I can help with:\n\n" +
                       "  🔑 password / passphrase / credentials\n" +
                       "  🎣 phishing / scam / email\n" +
                       "  🔒 privacy / personal info / data\n" +
                       "  🛡 malware / virus / ransomware / hack\n" +
                       "  🔐 two factor / 2fa / authentication\n" +
                       "  🌐 browsing / browser / VPN / Wi-Fi\n" +
                       "  🎭 social engineering\n\n" +
                       "You can also click the quick chips at the bottom of the screen!";

            // random tip request
            if (input.Contains("tip") || input.Contains("random"))
            {
                var allPools = new List<List<string>>(RandomPools.Values);
                var pool = allPools[Rng.Next(allPools.Count)];
                return "💡 Here's a random cybersecurity tip:\n\n" + pool[Rng.Next(pool.Count)];
            }

            // thank you
            if (input.Contains("thank"))
                return $"You're very welcome, {userName}! 😊 " +
                       "Cybersecurity is everyone's responsibility — stay curious and stay safe!";

            // match a keyword and return a random response from that topic's pool
            string? topic = DetectTopic(input);
            if (topic != null && RandomPools.TryGetValue(topic, out var matched))
                return matched[Rng.Next(matched.Count)];

            // default fallback if nothing matched
            return $"I didn't quite understand that, {userName}. 🤔\n\n" +
                   "Could you try rephrasing? You can ask about:\n" +
                   "  passwords · phishing · malware · privacy · 2FA · safe browsing\n\n" +
                   "Or type 'help' to see all topics!";
        }

        // returns the next follow-up tip for the topic being discussed
        public static string GetFollowUp(string topic, int followUpCount, string userName)
        {
            if (FollowUpTips.TryGetValue(topic, out var tips) && tips.Count > 0)
            {
                int idx = (followUpCount - 1) % tips.Count;
                return $"Here's another tip about {topic} for you, {userName}:\n\n" +
                       $"  💡 {tips[idx]}\n\n" +
                       "Say 'tell me more' for another tip, or ask about a different topic!";
            }
            return $"I've shared everything I know about {topic} for now, {userName}! " +
                   "Feel free to ask about a different topic.";
        }
    }
}