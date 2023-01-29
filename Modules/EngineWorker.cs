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
using System.ComponentModel;

namespace FFmpegGUI.Modules
{
    internal class EngineWorker
    {
        public string EnginePath { get; private set; }
        public string EnginePathProbe { get; private set; }

        private ProgressBar _bar;
        private Label _data;

        private int _currentFile;
        private int _allFiles;
        private string _name;

        private DateTime _startTime;

        private bool _restart = false;
        private CancellationTokenSource _renderToken = new CancellationTokenSource();

        private const int TimerMSDelay = 100;
        private const int KB = 1024;

        public EngineWorker(ProgressBar bar, Label data)
        {
            _bar = bar;
            _data = data;
        }

        public async void ConvertFilesWithSettings(string inputFolder, string outputFolder)
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

                EngineMediaData media = new EngineMediaData(file.FullName);
                media.PerformRequest();

                EngineRequest request = new EngineRequest();
                request.AddParameter($"-i \"{file.FullName}\"");
                CompileVideoSettings(request, media);
                CompileAudioSettings(request);
                request.AddParameter($"\"{fullName}\"");

                switch ((RenderType)RenderSettings.Instance.RenderPrototype)
                {
                    case RenderType.CPU:
                        break;
                    case RenderType.Intel:
                        //  snippet.UseHardwareAcceleration(HardwareAccelerator.d3d11va, VideoCodec.h264, VideoCodec.h264);
                        break;
                    case RenderType.AMD:
                        //  snippet.UseHardwareAcceleration(HardwareAccelerator.d3d11va, VideoCodec.h264, VideoCodec.h264);
                        break;
                    case RenderType.Nvidia:
                        //  snippet.UseHardwareAcceleration(HardwareAccelerator.cuvid, VideoCodec.h264_cuvid, VideoCodec.h264_cuvid);
                        break;
                }

                try
                {
                    _startTime = DateTime.Now;
                    request.OnDataReceived += OnProgress;
                    await request.Start();
                }
                catch (Exception ex)
                {
                    File.Delete(outputFolder + $"\\{file.Name}");

                    MessageBox.Show("Ошибка при конвертации. (возможно проблема в типе рендера)");
                    MessageBox.Show(ex.Message);

                    if (_restart)
                    {
                        System.Windows.Forms.Application.Restart();
                        Application.Current.Shutdown();
                    }
                }
                finally
                {
                    _bar.Visibility = Visibility.Hidden;
                    _data.Visibility = Visibility.Hidden;
                }
            }
        }

        private void CompileVideoSettings(EngineRequest request, EngineMediaData media)
        {
            TimeSpan secondsFromStart = TimeSpan.FromSeconds(RenderSettings.Instance.SecondsFromStart);
            TimeSpan secondsFromEnd = TimeSpan.FromSeconds(RenderSettings.Instance.SecondsFromEnd);

            TimeSpan mainDuration = media.Duration - secondsFromEnd;

            request.AddParameter($"-ss {secondsFromStart} -to {mainDuration}"); // skip from start and end
            request.AddParameter($"-s {RenderSettings.Instance.VideoResolution}");
            request.AddParameter($"-b:v {RenderSettings.Instance.VideoBitRate}k");
            request.AddParameter($"-r {media.FPS}");
        }
        
        private void CompileAudioSettings(EngineRequest request)
        {
            request.AddParameter($"-b:a {RenderSettings.Instance.AudioBitRate}k");
        }

        private void OnProgress(ResponseOutput output)
        {
            if (output.current.TotalSeconds == 0)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                TimeSpan currentTime = TimeSpan.FromTicks(DateTime.Now.Ticks - _startTime.Ticks);

                _bar.Visibility = Visibility.Visible;
                _data.Visibility = Visibility.Visible;

                _bar.Value = output.percents;
                _data.Content = $"{_currentFile}/{_allFiles} файлов \n {(int)output.percents}% - {_name} \n " +
                $"{output.current.ToString("hh':'mm':'ss")}/" +
                $"{output.duration.ToString("hh':'mm':'ss")} " +
                $"({currentTime.ToString("hh':'mm':'ss")})";
            });
        }

        public void Cancel(bool restart = true)
        {
            _restart = restart;
        }
    }
}
