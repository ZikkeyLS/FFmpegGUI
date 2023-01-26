using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpegGUI.Profiles
{
    [Serializable]
    internal class ApplicationSettings
    {
        public static ApplicationSettings Instance = new ApplicationSettings();

        public int ProfileIndex { get; set; } = 0;
    }
}
