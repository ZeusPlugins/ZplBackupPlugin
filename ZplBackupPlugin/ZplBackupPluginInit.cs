using YoYoStudio.Core.Utils.Preferences;

namespace YoYoStudio
{
    namespace Plugins
    {
        namespace ZplBackupPlugin
        {
            public class ZplBackupPluginInit : IPlugin
            {
                public PluginConfig Initialise()
                {
                    PluginConfig cfg = new PluginConfig("Backup for Zeus", "A plugin that brings back the project backup ability like in good old times.", false);
                    cfg.AddCommand("zplbackupplugin_command", "ide_loaded", "Adds a command that handles the backups", "create", typeof(ZplBackupPluginCommand));
                    PreferencesManager.Register(typeof(ZplBackupPluginPreferences));
                    return cfg;
                }
            }
        }
    }
}
