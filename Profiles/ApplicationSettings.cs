using System;

namespace FFmpegGUI.Profiles
{
    [Serializable]
    internal class ApplicationSettings
    {
        public static ApplicationSettings Instance = new ApplicationSettings();

        public int ProfileIndex { get; set; }
        [NonSerialized] public DateTime ExpirationTime;
    }
}
