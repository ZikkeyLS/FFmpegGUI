using FFmpegGUI.Profiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FFmpegGUI.Modules
{
    internal class ConfigProfile
    {
        public readonly string Path;

        public ConfigProfile(string path)
        {
            Path = path;
        }
    }

    internal class ConfigWorker
    {
        private ComboBox _box;
        private Label _boxState;
        private ConfigProfile _currentProfile;
        private List<ConfigProfile> _profiles = new List<ConfigProfile>();
        private string _profilePath = "Profiles";

        private const string extension = "profile";

        public ConfigWorker(ComboBox box, Label boxState, string profilePath = "Profiles")
        {
            _boxState = boxState;
            _box = box;
            Initialize(profilePath);

            _box.SelectionChanged += (e, o) => { ReloadWithApply(_box.SelectedIndex); };
        }

        public void Initialize(string profilePath = "Profiles")
        {
            _profilePath = profilePath;
            UpdateProfiles();
        }

        public void UpdateProfiles()
        {
            if (!Directory.Exists(_profilePath))
                Directory.CreateDirectory(_profilePath);

            if (!File.Exists(_profilePath + "settings.profile"))
                File.Create(_profilePath + "settings.profile").Close();
            else
                ApplicationSettings.Instance = Newtonsoft.Json.JsonConvert.DeserializeObject<ApplicationSettings>(File.ReadAllText(_profilePath + "settings.profile"));

            string[] files = Directory.GetFiles(_profilePath, $"*.{extension}");
            foreach (string file in files)
            {
                _profiles.Add(new ConfigProfile(file));
                _box.Items.Add(new Label() { Content = Path.GetFileNameWithoutExtension(file) });
            }

            if(_profiles.Count != 0)
                ApplyCurrentProfile(_profiles[ApplicationSettings.Instance.ProfileIndex]);
        }

        public void ApplyCurrentProfile(int index) => ApplyCurrentProfile(_profiles[index]);

        public void ApplyCurrentProfile(ConfigProfile profile)
        {
            _currentProfile = profile;
            _boxState.Content = Path.GetFileNameWithoutExtension(profile.Path);

            RenderSettings profileData = Newtonsoft.Json.JsonConvert.DeserializeObject<RenderSettings>(File.ReadAllText(_currentProfile.Path));
            if (profileData != null)
            {
                RenderSettings.Instance = profileData;
            }
        }

        public ConfigProfile[] GetProfiles()
        {
            return _profiles.ToArray();
        }

        public void ChangeProfile(string path)
        {
            _currentProfile = _profiles.Find(x => x.Path == path);
        }

        public void SaveProfile()
        {
            string path = _profilePath + $@"\Profile{_profiles.Count}.{extension}";

            File.Create(path).Close();

            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(RenderSettings.Instance));

            ReloadWithApply(_profiles.Count);
        }

        public void ReloadWithApply(int index) 
        {
            ApplicationSettings.Instance.ProfileIndex = index;
            UpdateMainProfile();
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }


        public void UpdateMainProfile()
        {
            File.WriteAllText(_profilePath + "settings.profile", Newtonsoft.Json.JsonConvert.SerializeObject(ApplicationSettings.Instance));
        }

        public void UpdateProfile()
        {
            File.WriteAllText(_currentProfile.Path, Newtonsoft.Json.JsonConvert.SerializeObject(RenderSettings.Instance));
        }
    }
}
