﻿using FFmpegGUI.Profiles;
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
            Resolution.SelectionChanged += ResolutionSelectionChanged;

            IntegerInput bitRateInput = new IntegerInput(BitRate, RenderSettings.Instance.VideoBitRate, 1);
            BitRate.SetUnit("KB");
            bitRateInput.OnValueChanged += (value) => { RenderSettings.Instance.VideoBitRate = value; };

            IntegerInput cutFromStartInput = new IntegerInput(CutFromStart, RenderSettings.Instance.SecondsFromStart);
            CutFromStart.SetUnit("сек");
            cutFromStartInput.OnValueChanged += (value) => { RenderSettings.Instance.SecondsFromStart = value; };

            IntegerInput cutFromEndInput = new IntegerInput(CutFromEnd, RenderSettings.Instance.SecondsFromEnd);
            CutFromEnd.SetUnit("сек");
            cutFromEndInput.OnValueChanged += (value) => { RenderSettings.Instance.SecondsFromEnd = value; };
        }

        private void ResolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RenderSettings.Instance.ConvertVideoSizeOption(Resolution.SelectedIndex);
        }
    }
}
