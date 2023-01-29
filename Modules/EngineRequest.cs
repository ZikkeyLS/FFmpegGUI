using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FFmpegGUI.Modules
{
    internal class EngineRequest
    {
        public Action<ResponseOutput> OnDataReceived;

        private string _ffmpeg;
        private Process _ffmpegProcess;

        public List<string> _parameters = new List<string>();

        public TimeSpan Duration { get; private set; }
        public TimeSpan Current { get; private set; }

        public EngineRequest(string ffmpeg = "ffmpeg.exe")
        {
            _ffmpeg = ffmpeg;
        }

        public async Task Start()
        {
            await Task.Run(() =>
            {
                string arguments = BuildArguments();

                _ffmpegProcess = new Process();
                _ffmpegProcess.StartInfo.FileName = _ffmpeg;
                _ffmpegProcess.StartInfo.Arguments = arguments;
                _ffmpegProcess.StartInfo.UseShellExecute = false;
                _ffmpegProcess.StartInfo.CreateNoWindow = true;
                _ffmpegProcess.StartInfo.RedirectStandardError = true;
                _ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                _ffmpegProcess.OutputDataReceived += OnData;
                _ffmpegProcess.ErrorDataReceived += OnData;
                _ffmpegProcess.Start();
                _ffmpegProcess.BeginOutputReadLine();
                _ffmpegProcess.BeginErrorReadLine();
                _ffmpegProcess.WaitForExit();
                _ffmpegProcess.Dispose();
                _ffmpegProcess = null;
            });
        }

        public void AddParameter(string parameter)
        {
            _parameters.Add(parameter);
        }

        public void Stop()
        {
            if (_ffmpegProcess == null)
                return;

            _ffmpegProcess.Kill();
        }

        private void OnData(object o, DataReceivedEventArgs e)
        {
            TryInvoke(OnDataReceived, CollectOutputData(e.Data));
        }

        private string BuildArguments()
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < _parameters.Count; i++)
            {
                string notFirstSpace = i != 0 ? " " : "";
                builder.Append(notFirstSpace + _parameters[i]);
            }

            return builder.ToString();
        }

        private ResponseOutput CollectOutputData(string data)
        {
            if (data == null || data == "")
                return null;

            string loweredData = data.ToLower();

            if (loweredData.Contains("duration"))
            {
                string[] parsedData = data.Split(',')[0].Split(' ');
                string durationData = parsedData[parsedData.Length - 1];
                Duration = TimeSpan.Parse(durationData);
            }
            else if (loweredData.Contains("frame") && loweredData.Contains("time"))
            {
                string cell = FindCell(loweredData.Split(' '), "time");
                Current = TimeSpan.Parse(cell.Split('=')[1]);
            }

            return new ResponseOutput() { duration = Duration, current = Current, percents = (float)(Current.TotalSeconds / Duration.TotalSeconds * 100) };
        }

        private string FindCell(string[] data, string contains)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Contains(contains))
                    return data[i];
            }

            return "";
        }

        private void TryInvoke(Action<ResponseOutput> action, ResponseOutput data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (action != null && data != null)
                    action.Invoke(data);
            });
        }
    }

    internal class ResponseOutput
    {
        public TimeSpan current;
        public TimeSpan duration;
        public float percents;
    }
}
