using System;
using System.Collections.Generic;

namespace FFmpegGUI.Profiles
{
    [Serializable]
    public class RenderSettings
    {
        public static RenderSettings Instance { get; set; } = new RenderSettings();

        public int SecondsFromStart = 0;
        public int SecondsFromEnd = 0;
        public int PreferredVideoBitRate = 800;
        public int PreferredAudioBitRate = 157;
        public int Threads = 1;

        public int RenderPrototype = 0;

        public string InputPath;
        public string OutputPath;
        public string VideoResolution = "1920x1080";

        private Dictionary<int, string> ResolutionConverter = new Dictionary<int, string>()
        {
            { 0,  "1920x1080" },
            { 1,  "3840x2160" },
            { 2,  "2560x1440" },
            { 3,  "1280x720" },
            { 4,  "848x480" }
        };

        private Dictionary<string, int> ResolutionDeconverter = new Dictionary<string, int>()
        {
            { "1920x1080", 0 },
            { "3840x2160", 1 },
            { "2560x1440", 2 },
            { "1280x720", 3 },
            { "848x480", 4 }
        };


        public void ConvertVideoSizeOption(int index)
        {
            VideoResolution = ResolutionConverter[index];
        }

        public int GetConvertIndex()
        {
            return ResolutionDeconverter[VideoResolution];
        }
    }

    public enum RenderType : int
    {
        CPU = 0,
        Intel,
        AMD,
        Nvidia
    }
}
