using FFmpegGUI.Network;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FFmpegGUI.Windows
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public static Authorization Current;

        private User _user;

        public Authorization()
        {
            InitializeComponent();
            TryMoveToMain();

            Current = this;

            KeySend.Click += (o, e) => { _user = new User(KeyInput.Text); };
        }

        public void TryMoveToMain()
        {
            RegistryKey user = Registry.CurrentUser;

            RegistryKey key = user.OpenSubKey("FFmpegGUI");

            if(key == null)
                key = user.CreateSubKey("FFmpegGUI");

            string applicationKey = (string)key.GetValue("applicationKey");
            
            if(applicationKey != null)
            {
                // _user = new User();
            }
        }

        public void MoveToMain()
        {
            MainWindow main = new MainWindow();
            main.Show();

            Close();
        }

        public void ProcessStatus(ushort status)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                    MoveToMain();
            });
        }
    }
}
