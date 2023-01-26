using FFmpeg.NET.Enums;
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
        public string InputPath;
        public string OutputPath;
        public VideoSize VideoResolution = VideoSize.Hd1080;

        private Dictionary<int, VideoSize> ResolutionConverter = new Dictionary<int, VideoSize>()
        {
            { 0,  VideoSize.Hd1080 },
            { 1,  VideoSize._4K },
            { 2,  VideoSize._2K },
            { 3,  VideoSize.Hd720 },
            { 4,  VideoSize.Hd480 }
        };

        private Dictionary<VideoSize, int> ResolutionDeconverter = new Dictionary<VideoSize, int>()
        {
            { VideoSize.Hd1080, 0 },
            { VideoSize._4K, 1 },
            { VideoSize._2K, 2 },
            { VideoSize.Hd720, 3 },
            { VideoSize.Hd480, 4 }
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
}
