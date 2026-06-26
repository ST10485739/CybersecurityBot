using System;
using System.IO;
using System.Media;

namespace CybersecurityBotWinForms.Helpers
{
    // plays the wav greeting when the app starts
    public static class AudioHelper
    {
        private static readonly string AudioFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Media", "greeting.wav");

        // looks for the wav file in the Media folder and plays it
        // if the file is missing the app still runs fine
        public static void PlayGreeting()
        {
            try
            {
                if (File.Exists(AudioFilePath))
                {
                    using var player = new SoundPlayer(AudioFilePath);
                    player.Play(); // async — does not block the UI thread
                }
            }
            catch (Exception)
            {
                // audio failure is non-fatal so we just skip it
            }
        }
    }
}