using FFmpegGUI.Profiles;
using System.Windows.Controls;

namespace FFmpegGUI.Pages
{
    /// <summary>
    /// Логика взаимодействия для BasePanel.xaml
    /// </summary>
    public partial class BasePanel : Page
    {
        public BasePanel()
        {
            InitializeComponent();

            Resolution.SelectedIndex = RenderSettings.Instance.GetConvertIndex();
            Resolution.SelectionChanged += ResolutionSelectionChanged;

            BitRate.Initialize(RenderSettings.Instance.VideoBitRate, 1);
            BitRate.SetUnit("KB");
            BitRate.OnValueChanged += (value) => { RenderSettings.Instance.VideoBitRate = value; };

            CutFromStart.Initialize(RenderSettings.Instance.SecondsFromStart);
            CutFromStart.SetUnit("сек");
            CutFromStart.OnValueChanged += (value) => { RenderSettings.Instance.SecondsFromStart = value; };

            CutFromEnd.Initialize(RenderSettings.Instance.SecondsFromEnd);
            CutFromEnd.SetUnit("сек");
            CutFromEnd.OnValueChanged += (value) => { RenderSettings.Instance.SecondsFromEnd = value; };
        }

        private void ResolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RenderSettings.Instance.ConvertVideoSizeOption(Resolution.SelectedIndex);
        }
    }
}
