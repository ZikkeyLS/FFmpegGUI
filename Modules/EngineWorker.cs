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

        private float _percentage = -1;
        private int _currentFile;
        private int _allFiles;
        private string _name;

        private DateTime _startTime;
        private TimeSpan _currentTime;
        private TimeSpan _finalTime;

        private const int TimerMSDelay = 50;
        private const int KB = 1024;

        private bool _restart = false;
        private CancellationTokenSource _renderToken = new CancellationTokenSource();

        public EngineWorker(ProgressBar bar, Label data)
        {
            _bar = bar;
            _data = data;

            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, TimerMSDelay) };
            dispatcherTimer.Tick += new EventHandler((o, e) => {
                if (_percentage >= 0 && _percentage <= 100)
                {
                    TimeSpan currentTime = TimeSpan.FromTicks(DateTime.Now.Ticks - _startTime.Ticks);

                    _bar.Visibility = Visibility.Visible;
                    _data.Visibility = Visibility.Visible;

                    _bar.Value = _percentage;
                    _data.Content = $"{_currentFile}/{_allFiles} файлов \n {_percentage}% - {_name} \n " +
                    $"{_currentTime}/" +
                    $"{_finalTime} " + 
                    $"({currentTime.ToString("hh':'mm':'ss")})";
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

                string fullName = outputFolder + $"\\{file.Name}";
                _name = Path.GetFileName(fullName);

                if (File.Exists(fullName))
                    File.Delete(fullName);

                var data = await FFmpeg.GetMediaInfo(file.FullName);
                var snippet = await FFmpeg.Conversions.FromSnippet.Convert(file.FullName, fullName);
                CompileVideoSettings(snippet, data);
                CompileAudioSettings(snippet);


                switch ((RenderType)RenderSettings.Instance.RenderPrototype)
                {
                    case RenderType.CPU:
                        break;
                    case RenderType.Intel:
                        snippet.UseHardwareAcceleration(HardwareAccelerator.d3d11va, VideoCodec.h264, VideoCodec.h264);
                        break;
                    case RenderType.AMD:
                        snippet.UseHardwareAcceleration(HardwareAccelerator.d3d11va, VideoCodec.h264, VideoCodec.h264);
                        break;
                    case RenderType.Nvidia:
                        snippet.UseHardwareAcceleration(HardwareAccelerator.cuvid, VideoCodec.h264_cuvid, VideoCodec.h264_cuvid);
                        break;
                }

                snippet.UseMultiThread(true);
                snippet.UseMultiThread(RenderSettings.Instance.Threads);

                try
                {
                    snippet.OnProgress += SnippetOnProgress;
                    _startTime = DateTime.Now;
                    IConversionResult result = await snippet.Start(_renderToken.Token);
                }
                catch
                {
                    File.Delete(outputFolder + $"\\{file.Name}");

                    MessageBox.Show("Ошибка при конвертации.(возможно проблема в типе рендера)");

                    if (_restart)
                    {
                        System.Windows.Forms.Application.Restart();
                        Application.Current.Shutdown();
                    }
                }
                finally
                {
                    _percentage = -1;
                    _bar.Visibility = Visibility.Hidden;
                    _data.Visibility = Visibility.Hidden;
                }
            }
        }

        private void SnippetOnProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
        {
            _percentage = (float)args.Percent;
            _currentTime = args.Duration;
            _finalTime = args.TotalLength;
        }

        public void Cancel(bool restart = true)
        {
            _restart = restart;
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
    }
}
