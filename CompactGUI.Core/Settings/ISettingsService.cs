using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactGUI.Core.Settings
{
    public interface ISettingsService
    {

        DirectoryInfo DataFolder { get; }
        FileInfo SettingsJSONFile { get; }

        Settings AppSettings { get; set; }

        decimal SettingsVersion { get; }

        void LoadSettings();
        void SaveSettings();
        void ScheduleSettingsSave();


    }


}
