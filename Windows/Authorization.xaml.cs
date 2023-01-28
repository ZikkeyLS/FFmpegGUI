using FFmpegGUI.Network;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace FFmpegGUI.Windows
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public static Authorization Current;

        private User _user = new User();
        private string _applicationKey;

        public Authorization()
        {
            Current = this;
            InitializeComponent();
            TryMoveToMain();

            Close.Click += (o, e) => { Close(); };
            Minimize.Click += (o, e) => { WindowState = WindowState.Minimized; };
            KeySend.Click += (o, e) => { _user.SendKey(KeyInput.Text); };
        }

        public void TryMoveToMain()
        {
            RegistryKey user = Registry.CurrentUser;

            RegistryKey key = user.OpenSubKey("FFmpegGUI");

            if(key == null)
                key = user.CreateSubKey("FFmpegGUI");

            _applicationKey = (string)key.GetValue("applicationKey");
            
            if(_applicationKey != null && _applicationKey != "")
            {
                _user.SendKey(_applicationKey);
            }
        }

        public void MoveToMain()
        {
            RegistryKey user = Registry.CurrentUser;

            RegistryKey key = user.OpenSubKey("FFmpegGUI", true);
            key.SetValue("applicationKey", _applicationKey);

            MainWindow main = new MainWindow();
            main.Show();

            Close();
        }

        public void ProcessStatus(ushort status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (status) 
                {
                    case 0:
                        MoveToMain();
                        break;
                    case 1:
                        Status.Content = "Неправильный ключ!";
                        break;
                    case 2:
                        Status.Content = "Ключ уже используется!";
                        break;
                    case 3:
                        Status.Content = "Ошибка на сервере!";
                        break;
                }
            });
        }

        private void OnLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
