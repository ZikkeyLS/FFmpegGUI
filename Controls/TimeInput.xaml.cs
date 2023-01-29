using System;
using System.Timers;
using System.Windows.Controls;

namespace FFmpegGUI
{
    /// <summary>
    /// Логика взаимодействия для IntInput.xaml
    /// </summary>
    public partial class TimeInput : UserControl
    {
        private bool _used = true;
        private Timer _timer;

        public TimeInput()
        {
            InitializeComponent();
            Time.TextChanged += TimeTextChanged;

            _timer = new Timer(100);
            _timer.Elapsed += OnElapsed;
            _timer.Start();
        }

        private void OnElapsed(object o, ElapsedEventArgs e)
        {
            _used = false;
        }

        private void TimeTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_used)
                return;

            _used = true;

            int doubleDots = 0;
            foreach (char c in Time.Text)
                if(c == ':')
                    doubleDots += 1;

            if (doubleDots != 2)
            {
                Time.Text = "00:00:00";
                return;
            }

            string[] timeParts = Time.Text.Split(':');

            if(timeParts.Length != 3)
            {
                Time.Text = "00:00:00";
                return;
            }

            for(int i = 0; i < timeParts.Length; i++)
                if(int.TryParse(timeParts[i], out int nothing) == false)
                {
                    Time.Text = "00:00:00";
                    return;
                }
        }

        public Grid GetGrid()
        {
            return Grid;
        }

        public TimeSpan GetSpan()
        {
            return TimeSpan.Parse(Time.Text);
        }
    }
}
