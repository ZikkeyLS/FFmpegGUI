using FFmpegGUI.Modules;
using FFmpegGUI.Pages;
using FFmpegGUI.Profiles;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WK.Libraries.BetterFolderBrowserNS;

namespace FFmpegGUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private PageSwiper _swiper;
        private EngineWorker _engine;
        private ConfigWorker _config;

        private string _previousFolder;

        public MainWindow()
        {
            Icon = BitmapSource.Create(1, 1, 0, 0, PixelFormats.Bgra32, null, new byte[4], 4);
            InitializeComponent();
            Title = $"FFmpegGUI / Лицензия до {ApplicationSettings.Instance.ExpirationTime.ToString("dd.MM.yyyy")}";

            _config = new ConfigWorker(ConfigProfileSwitcher);
            SaveConfig.Click += (o, e) => { _config.SaveProfile(); };
            UpdateConfig.Click += (o, e) => { _config.UpdateProfile(); };

            _swiper = new PageSwiper(MainFrame);
            _swiper.SetPanel(new BasePanel());
            _engine = new EngineWorker(LoadProgress, LoadLabel);

            Threads.Initialize(RenderSettings.Instance.Threads, 1, 16);
            Threads.OnValueChanged += (value) => { RenderSettings.Instance.Threads = value; };

            InputPath.Text = RenderSettings.Instance.InputPath;
            InputPath.TextChanged += (e, o) => { RenderSettings.Instance.InputPath = InputPath.Text; };
            OutputPath.Text = RenderSettings.Instance.OutputPath;
            OutputPath.TextChanged += (e, o) => { RenderSettings.Instance.OutputPath = OutputPath.Text; };

            RenderType.SelectedIndex = RenderSettings.Instance.RenderPrototype;
            RenderType.SelectionChanged += (e, o) => 
            {
                RenderSettings.Instance.RenderPrototype = RenderType.SelectedIndex;
            };

            BaseButton.Click += (o, e) => { _swiper.SetPanel(new BasePanel()); };
            ColorButton.Click += (o, e) => { _swiper.SetPanel(new ColorPanel()); };
            ZoomButton.Click += (o, e) => { _swiper.SetPanel(new ZoomPanel()); };
            BorderButton.Click += (o, e) => { _swiper.SetPanel(new BorderPanel()); };
            OtherButton.Click += (o, e) => { _swiper.SetPanel(new OtherPanel()); };

            InputPathButton.Click += (o, e) => { UpdateText(InputPath); };
            OutputPathButton.Click += (o, e) => { UpdateText(OutputPath); };

            Render.Click += (o, e) => { Run(); };
            Cancel.Click += (o, e) => { _engine.Cancel(); };
        }

        private void UpdateText(TextBox text)
        {
            var folderBrowser = new BetterFolderBrowser() { Multiselect = false, RootFolder = _previousFolder };

            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                text.Text = folderBrowser.SelectedFolder;
                _previousFolder = folderBrowser.SelectedFolder;
            }
        }

        public void Run()
        {
            if (!Directory.Exists(InputPath.Text) || !Directory.Exists(OutputPath.Text))
            {
                MessageBox.Show("Пути заданы некоректно!");
                return;
            }

            if (_engine.Working == false)
                return;

            _engine.ConvertFilesWithSettings(InputPath.Text, OutputPath.Text);
        }

        private void OnClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _engine.Cancel(false);
        }
    }
}
