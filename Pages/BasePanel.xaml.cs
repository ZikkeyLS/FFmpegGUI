using FFmpegGUI.Profiles;
using FFmpegGUI.ScriptableUI;
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
            UpdateResolutionName();
            Resolution.SelectionChanged += ResolutionSelectionChanged;

            IntegerInput bitRateInput = new IntegerInput(BitRate, RenderSettings.Instance.PreferredVideoBitRate, 1);
            bitRateInput.OnValueChanged += (value) => { RenderSettings.Instance.PreferredVideoBitRate = value; };

            IntegerInput cutFromStartInput = new IntegerInput(CutFromStart, RenderSettings.Instance.SecondsFromStart);
            cutFromStartInput.OnValueChanged += (value) => { RenderSettings.Instance.SecondsFromStart = value; };

            IntegerInput cutFromEndInput = new IntegerInput(CutFromEnd, RenderSettings.Instance.SecondsFromEnd);
            cutFromEndInput.OnValueChanged += (value) => { RenderSettings.Instance.SecondsFromEnd = value; };
        }

        private void ResolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateResolutionName();
            RenderSettings.Instance.ConvertVideoSizeOption(Resolution.SelectedIndex);
        }

        private void UpdateResolutionName()
        {
            ResolutionName.Content = ((Label)Resolution.SelectedItem).Content;
        }
    }
}
