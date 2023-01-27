using Xabe.FFmpeg;
using FFmpegGUI.Profiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading;

namespace FFmpegGUI.Modules
{
    internal class EngineWorker
    {
        public string EnginePath { get; private set; }
        public string EnginePathProbe { get; private set; }

        private ProgressBar _bar;
        private Label _data;

        private float _percentage;
        private int _currentFile;
        private int _allFiles;
        private string _name;

        private TimeSpan _currentTime;
        private TimeSpan _finalTime;

        private const int TimerMSDelay = 50;
        private const int KB = 1024;

        private CancellationTokenSource _renderToken = new CancellationTokenSource();

        public EngineWorker(ProgressBar bar, Label data)
        {
            _bar = bar;
            _data = data;

            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, TimerMSDelay) };
            dispatcherTimer.Tick += new EventHandler((o, e) => {
                if (_percentage >= 0 && _percentage <= 100)
                {
                    _bar.Value = _percentage;
                    _data.Content = $"{_currentFile}/{_allFiles} процессов \n {_percentage}% - {_name} \n " +
                    $"{_currentTime.Hours}:{_currentTime.Minutes}:{_currentTime.Seconds}/" +
                    $"{_finalTime.Hours}:{_finalTime.Minutes}:{_finalTime.Seconds}";
                }
            });
            dispatcherTimer.Start();
        }

        public async Task ConvertFilesWithSettings(string inputFolder, string outputFolder)
        {
            DirectoryInfo info = new DirectoryInfo(inputFolder);
            List<FileInfo> fileInfo = info.GetFiles().ToList();

            foreach (FileInfo file in fileInfo)
                if (file.Extension != ".mp4" && file.Extension != ".avi")
                    fileInfo.Remove(file);

            _allFiles = fileInfo.Count;

            for (int i = 0; i < fileInfo.Count; i++)
            {
                FileInfo file = fileInfo[i];

                _currentFile = i;
                _bar.Visibility = Visibility.Visible;
                _data.Visibility = Visibility.Visible;
                var data = await FFmpeg.GetMediaInfo(file.FullName);
                var snippet = await FFmpeg.Conversions.FromSnippet.Convert(file.FullName, outputFolder + $"\\{file.Name}");
                CompileVideoSettings(snippet, data);
                CompileAudioSettings(snippet);

                snippet.SetOverwriteOutput(true);

                try
                {
                    IConversionResult result = await snippet.Start(_renderToken.Token);
                }
                catch 
                {
                    File.Delete(outputFolder + $"\\{file.Name}");
                    System.Windows.Forms.Application.Restart();
                    Application.Current.Shutdown();
                }
            }
        }

        public void Cancel()
        {
            _renderToken.Cancel();
        }


        private void CompileVideoSettings(IConversion options, IMediaInfo data)
        {
            TimeSpan secondsFromStart = TimeSpan.FromSeconds(RenderSettings.Instance.SecondsFromStart);
            TimeSpan secondsFromEnd = TimeSpan.FromSeconds(RenderSettings.Instance.SecondsFromEnd);

            TimeSpan mainDuration = data.Duration - secondsFromEnd;

            options.AddParameter($"-ss {secondsFromStart} -to {mainDuration}"); // skip from start and end
            options.AddParameter($"-s {RenderSettings.Instance.VideoResolution}");
            options.SetVideoBitrate(RenderSettings.Instance.PreferredVideoBitRate * KB);
            options.SetFrameRate(data.VideoStreams.FirstOrDefault().Framerate);
        }

        private void CompileAudioSettings(IConversion options)
        {
            options.SetAudioBitrate(RenderSettings.Instance.PreferredAudioBitRate * KB);
        }

        /*
        private void ConvertComplete(object sender, FFmpeg.NET.Events.ConversionCompleteEventArgs e)
        {
            _percentage = -1;
            _bar.Visibility = Visibility.Hidden;
            _data.Visibility = Visibility.Hidden;
        }

        private void ConvertProgress(object sender, FFmpeg.NET.Events.ConversionProgressEventArgs e)
        {
            if (!e.Frame.HasValue || !e.Fps.HasValue)
                return;

            _percentage = (float)Math.Round((e.ProcessedDuration.TotalSeconds / e.TotalDuration.TotalSeconds * 100));
            _name = e.Output.FileInfo.Name;
            _currentTime = e.ProcessedDuration;
            _finalTime = e.TotalDuration;
        }
        */
    }
}
