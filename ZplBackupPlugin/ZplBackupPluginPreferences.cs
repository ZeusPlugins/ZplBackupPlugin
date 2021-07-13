using System.ComponentModel;
using System.Runtime.CompilerServices;
using YoYoStudio.Core.Utils.Preferences;

namespace YoYoStudio
{
    namespace Plugins
    {
        namespace ZplBackupPlugin
        {
            public class ZplBackupPluginPreferences : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;

                private int _AmountOfBackups;
                private string _BackupFolderPath;

                [Prefs("machine.Plugins.ZplBackup.AmountOfBackups", 0, "Amount of backup files produced", "ZPLB_AmountOfBackups", ePrefType.text_int, new object[] { /* minimum value */ 0,  /* maximum value */ 1000, /* tooltip:[tooltip CSV id] */ "tooltip:ZPLB_AmountOfBackups_tooltip" })]
                public int AmountOfBackups { get { return _AmountOfBackups; } set { SetProperty(ref _AmountOfBackups, value); } }

                [Prefs("machine.Plugins.ZplBackup.BackupFolderPath", 10, "Where to place the backups", "ZPLB_BackupFolderPath", ePrefType.text_path, new object[] { /* gadget mode */ "selectFolder" })]
                public string BackupFolderPath { get { return _BackupFolderPath; } set { SetProperty(ref _BackupFolderPath, value); } }

                public ZplBackupPluginPreferences()
                {
                    // default values, the IDE will call the constructor if you click 'Restore Defaults'
                    AmountOfBackups = 0;
                    BackupFolderPath = "";
                }

                private void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
                {
                    property = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
