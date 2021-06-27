using YoYoStudio.Core.Utils.Preferences;

namespace YoYoStudio
{
    namespace Plugins
    {
        namespace ZplBackupPlugin
        {
            public class ZplBackupPluginPreferences
            {
                [Prefs("machine.Plugins.ZplBackup.AmountOfBackups", 0, "Amount of backup files produced", "ZPLB_AmountOfBackups", ePrefType.text_int, new object[] { /* minimum value */ 0,  /* maximum value */ 1000, /* tooltip:[tooltip CSV id] */ "tooltip:ZPLB_AmountOfBackups_tooltip" })]
                public int AmountOfBackups { get; set; }

                [Prefs("machine.Plugins.ZplBackup.BackupFolderPath", 10, "Where to place the backups", "ZPLB_BackupFolderPath", ePrefType.text_path, new object[] { /* gadget mode */ "selectFolder" })]
                public string BackupFolderPath { get; set; }

                public ZplBackupPluginPreferences()
                {
                    // default values, the IDE will call the constructor if you click 'Restore Defaults'
                    AmountOfBackups = 0;
                    BackupFolderPath = "";
                }
            }
        }
    }
}
