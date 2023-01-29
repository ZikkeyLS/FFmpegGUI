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

        private int _currentFile = 0;
        private int _allFiles = 1;
        private string _name;

        private DateTime _startTime;

        private bool _restart = false;
        private EngineRequest _request;

        public bool Working => _currentFile != _allFiles;

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

                _request = new EngineRequest();
                _request.AddParameter($"-i \"{file.FullName}\""); // initial file

                _request.AddParameter($"-threads {RenderSettings.Instance.Threads}");

                switch ((RenderType)RenderSettings.Instance.RenderPrototype)
                {
                    case RenderType.CPU:
                        break;
                    case RenderType.Intel:
                        _request.AddParameter($"-hwaccel qsv");
                        break;
                    case RenderType.AMD:
                        _request.AddParameter($"-hwaccel d3d11va");
                        break;
                    case RenderType.Nvidia:
                        _request.AddParameter($"-hwaccel cuda");
                        break;
                }

                if((RenderType)RenderSettings.Instance.RenderPrototype != RenderType.CPU)
                    MessageBox.Show("Для рендеринга через GPU требуются специальные драйвера в зависимости от карты. \nПерезапустите программу в случае сбоя.");

                CompileVideoSettings(_request, media);
                CompileAudioSettings(_request);

                try
                {
                    _startTime = DateTime.Now;
                    _request.AddParameter($"\"{fullName}\""); // final file
                    _request.OnDataReceived += OnProgress;
                    await _request.Start();
                }
                catch (Exception ex)
                {
                    _request.Stop();
                    File.Delete(outputFolder + $"\\{file.Name}");

                    MessageBox.Show("Ошибка при конвертации. (возможно проблема в типе рендера)");
                    MessageBox.Show(ex.Message);

                    System.Windows.Forms.Application.Restart();
                    Application.Current.Shutdown();
                }
                finally
                {
                    _bar.Visibility = Visibility.Hidden;
                    _data.Visibility = Visibility.Hidden;
                    _currentFile = _allFiles;
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

            if(_request != null)
                _request.Stop();

            if (_restart)
            {
                System.Windows.Forms.Application.Restart();
                Application.Current.Shutdown();
            }
        }
    }
}
