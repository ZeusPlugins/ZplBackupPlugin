using System;
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

                [Prefs("machine.Plugins.ZplBackup.AmountOfBackups", 0, "Amount of backup files produced", "ZPLB_AmountOfBackups", ePrefType.text_int, new object[] { /* minimum value */ 0,  /* maximum value */ 9000, /* tooltip:[tooltip CSV id] */ "tooltip:ZPLB_AmountOfBackups_tooltip" })]
                public int AmountOfBackups { get { return _AmountOfBackups; } set { SetPropertyIfChanged(ref _AmountOfBackups, value); } }

                [Prefs("machine.Plugins.ZplBackup.BackupFolderPath", 10, "Where to place the backups", "ZPLB_BackupFolderPath", ePrefType.text_path, new object[] { /* gadget mode */ "selectFolder" })]
                public string BackupFolderPath { get { return _BackupFolderPath; } set { SetPropertyIfChanged(ref _BackupFolderPath, value); } }

                public ZplBackupPluginPreferences()
                {
                    // default values, the IDE will call the constructor if you click 'Restore Defaults'
                    AmountOfBackups = 0;
                    BackupFolderPath = "";
                }

                private void SetPropertyIfChanged<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
                {
                    var isEqual = property != null && ((IEquatable<T>)property).Equals(value);

                    // only update if value is not equal to property.
                    if (!isEqual)
                    {
                        property = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }
                }
            }
        }
    }
}
