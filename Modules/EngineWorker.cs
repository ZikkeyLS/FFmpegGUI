using FFmpeg.NET;
using FFmpegGUI.Profiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FFmpegGUI.Modules
{
    internal class EngineWorker
    {
        private Engine _engine;

        public string EnginePath { get; private set; }

        public Action<float> ProgressChanged; 

        public EngineWorker()
        {
            Initialize();
        }

        public void Initialize()
        {
            byte[] exeBytes = Properties.Resources.ffmpeg;
            EnginePath = Path.Combine(Path.GetTempPath(), "ffmpeg.exe");

            if (!File.Exists(EnginePath))
                using (FileStream exeFile = new FileStream(EnginePath, FileMode.CreateNew))
                    exeFile.Write(exeBytes, 0, exeBytes.Length);

            _engine = new Engine(EnginePath);

            _engine.Complete += ConvertComplete;
            _engine.Progress += ConvertProgress;
        }

        public async Task ConvertFilesWithSettings(string inputFolder, string outputFolder)
        {
            DirectoryInfo info = new DirectoryInfo(inputFolder);
            List<FileInfo> fileInfo = info.GetFiles().ToList();

            foreach (FileInfo file in fileInfo)
                if (file.Extension != ".mp4" && file.Extension != ".avi")
                    fileInfo.Remove(file);

            for(int i = 0; i < fileInfo.Count; i++)
            {
                FileInfo file = fileInfo[i];

                var inputFile = new MediaFile(file.FullName);
                var outputFile = new MediaFile(outputFolder + $"\\{file.Name}");

                MetaData data = await _engine.GetMetaDataAsync(inputFile);
                
                var options = new ConversionOptions();
                CompileVideoSettings(options, data);
                CompileAudioSettings(options, data);

                await _engine.ConvertAsync(inputFile, outputFile, options);
            }
        }

        private void CompileVideoSettings(ConversionOptions options, MetaData data)
        {
            TimeSpan secondsFromStart = TimeSpan.FromSeconds(RenderSettings.Instance.SecondsFromStart);
            TimeSpan secondsFromEnd = TimeSpan.FromSeconds(RenderSettings.Instance.SecondsFromEnd);

            options.CutMedia(secondsFromStart, data.Duration - secondsFromStart - secondsFromEnd); // 1) from start 2) length - from start - from end
            options.VideoSize = RenderSettings.Instance.VideoResolution;
            options.VideoBitRate = RenderSettings.Instance.PreferredVideoBitRate;
            options.VideoFps = (int)data.VideoData.Fps;
        }

        private void CompileAudioSettings(ConversionOptions options, MetaData data)
        {
            options.AudioBitRate = RenderSettings.Instance.PreferredAudioBitRate;
        }

        private void ConvertComplete(object sender, FFmpeg.NET.Events.ConversionCompleteEventArgs e)
        {
            string test = e.Input.FileInfo.Name.ToString();
            Console.Write(test);
        }

        private void ConvertProgress(object sender, FFmpeg.NET.Events.ConversionProgressEventArgs e)
        {
            if (!e.Frame.HasValue || !e.Fps.HasValue)
                return;

            ProgressChanged.Invoke(((float)e.ProcessedDuration.Seconds) / ((float)e.TotalDuration.Seconds));
        }
    }
}
