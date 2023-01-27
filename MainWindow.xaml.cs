using FFmpegGUI.Modules;
using FFmpegGUI.Pages;
using FFmpegGUI.Profiles;
using System.IO;
using System.Windows;
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

            _config = new ConfigWorker(ConfigProfileSwitcher, ConfigProfileLabel);
            SaveConfig.Click += (o, e) => { _config.SaveProfile(); };
            UpdateConfig.Click += (o, e) => { _config.UpdateProfile(); };

            _swiper = new PageSwiper(MainFrame);
            _swiper.SetPanel(new BasePanel());

            _engine = new EngineWorker(LoadProgress, LoadLabel);

            InputPath.Text = RenderSettings.Instance.InputPath;
            InputPath.TextChanged += (e, o) => { RenderSettings.Instance.InputPath = InputPath.Text; };
            OutputPath.Text = RenderSettings.Instance.OutputPath;
            OutputPath.TextChanged += (e, o) => { RenderSettings.Instance.OutputPath = OutputPath.Text; };

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

        private void UpdateText(System.Windows.Controls.TextBox text)
        {
            var folderBrowser = new BetterFolderBrowser() { Multiselect = false, RootFolder = _previousFolder };

            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                text.Text = folderBrowser.SelectedFolder;
                _previousFolder = folderBrowser.SelectedFolder;
            }
        }

        public async void Run()
        {
            if (!Directory.Exists(InputPath.Text) || !Directory.Exists(OutputPath.Text))
            {
                MessageBox.Show("Пути заданы некоректно!");
                return;
            }

            await _engine.ConvertFilesWithSettings(InputPath.Text, OutputPath.Text);
        }

    }
}
