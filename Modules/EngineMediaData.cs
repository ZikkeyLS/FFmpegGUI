using System;

namespace FFmpegGUI.Modules
{
    internal class EngineMediaData
    {
        public TimeSpan Duration;
        public int FPS;

        private string _videoPath;
        private string _ffprobe;

        public EngineMediaData(string videoPath, string ffprobe = "ffprobe.exe")
        {
            _videoPath = videoPath;
            _ffprobe = ffprobe;
        }

        public void PerformRequest()
        {
            var ffProbe = new NReco.VideoInfo.FFProbe();
            ffProbe.FFProbeExeName = _ffprobe;

            var videoInfo = ffProbe.GetMediaInfo(_videoPath);

            Duration = videoInfo.Duration;
            FPS = (int)videoInfo.Streams[0].FrameRate;
        }
    }
}