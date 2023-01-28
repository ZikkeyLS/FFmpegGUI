using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FFmpegGUI
{
    /// <summary>
    /// Логика взаимодействия для IntInput.xaml
    /// </summary>
    public partial class IntInput : UserControl
    {
        public IntInput()
        {
            InitializeComponent();
        }

        public void SetUnit(string value)
        {
            Unit.Content = value;
        }

        public Grid GetGrid()
        {
            return Grid;
        }
    }
}
