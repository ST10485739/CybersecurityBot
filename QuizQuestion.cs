using System.Collections.Generic;

namespace CybersecurityBotWinForms.Models
{
    // holds one quiz question
    public class QuizQuestion
    {
        public string Question { get; set; } = "";
        public List<string> Options { get; set; } = new();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = "";
        public bool IsTrueFalse { get; set; }
    }

    // all 12 quiz questions
    public static class QuizBank
    {
        public static readonly List<QuizQuestion> Questions = new()
        {
            new QuizQuestion
            {
                Question     = "What should you do if you receive an email asking for your password?",
                Options      = new() { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                CorrectIndex = 2,
                Explanation  = "Reporting phishing emails helps protect you and others. Legitimate organisations will never ask for your password via email."
            },
            new QuizQuestion
            {
                Question     = "Which of the following is the strongest password?",
                Options      = new() { "password123", "MyDog2020", "C0ff3e!Rain#Monday7", "123456" },
                CorrectIndex = 2,
                Explanation  = "A strong password uses uppercase, lowercase, numbers AND symbols — and is at least 12 characters long."
            },
            new QuizQuestion
            {
                Question     = "What does '2FA' stand for?",
                Options      = new() { "Two-Factor Authentication", "Two-File Access", "Trusted Firewall Application", "Two-Faced Attacker" },
                CorrectIndex = 0,
                Explanation  = "Two-Factor Authentication adds a second layer of security on top of your password."
            },
            new QuizQuestion
            {
                Question     = "Which is the safest way to store your passwords?",
                Options      = new() { "Write them in a notebook", "Use the same password everywhere", "Use a password manager", "Save them in a text file on your desktop" },
                CorrectIndex = 2,
                Explanation  = "A password manager generates and stores unique passwords securely. You only need to remember one strong master password."
            },
            new QuizQuestion
            {
                Question     = "What is 'ransomware'?",
                Options      = new() { "A type of antivirus software", "Malware that encrypts your files and demands payment", "A secure messaging app", "A government cybersecurity program" },
                CorrectIndex = 1,
                Explanation  = "Ransomware locks your files and demands money. Regular backups are your best protection."
            },
            new QuizQuestion
            {
                Question     = "Which habit keeps you most secure on public Wi-Fi?",
                Options      = new() { "Using incognito mode", "Using a VPN", "Clearing your cookies", "Using a different browser" },
                CorrectIndex = 1,
                Explanation  = "A VPN encrypts all your internet traffic, protecting you from anyone else on the same network."
            },
            new QuizQuestion
            {
                Question     = "What is 'social engineering' in cybersecurity?",
                Options      = new() { "Building secure social media profiles", "Manipulating people into revealing confidential information", "Engineering social media algorithms", "A type of network firewall" },
                CorrectIndex = 1,
                Explanation  = "Social engineering tricks people rather than hacking systems directly."
            },
            new QuizQuestion
            {
                Question     = "Which 2FA method is the MOST secure?",
                Options      = new() { "SMS text message code", "Email verification code", "Hardware security key (e.g. YubiKey)", "Security question" },
                CorrectIndex = 2,
                Explanation  = "Hardware security keys are the most phishing-resistant 2FA option available."
            },
            new QuizQuestion
            {
                Question     = "True or False: You should use the same password for every account.",
                Options      = new() { "True", "False" },
                CorrectIndex = 1,
                Explanation  = "FALSE — one breach exposes ALL your accounts. Always use unique passwords.",
                IsTrueFalse  = true
            },
            new QuizQuestion
            {
                Question     = "True or False: HTTPS means a website is 100% safe and trustworthy.",
                Options      = new() { "True", "False" },
                CorrectIndex = 1,
                Explanation  = "FALSE — HTTPS only means the connection is encrypted. Phishing sites also use HTTPS.",
                IsTrueFalse  = true
            },
            new QuizQuestion
            {
                Question     = "True or False: Hovering over a link before clicking helps spot phishing.",
                Options      = new() { "True", "False" },
                CorrectIndex = 0,
                Explanation  = "TRUE — hovering reveals the actual URL. If it looks suspicious, don't click.",
                IsTrueFalse  = true
            },
            new QuizQuestion
            {
                Question     = "True or False: You should enable automatic updates on your OS and apps.",
                Options      = new() { "True", "False" },
                CorrectIndex = 0,
                Explanation  = "TRUE — most malware exploits unpatched software. Updates close those gaps.",
                IsTrueFalse  = true
            },
        };
    }
}