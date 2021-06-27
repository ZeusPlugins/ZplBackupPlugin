using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YoYoStudio.Core.Utils;
using YoYoStudio.Core.Utils.Preferences;
using YoYoStudio.FileAPI;
using Core.CoreOS.FileAPI;
using YoYoStudio.GUI.Gadgets;
using YoYoStudio.Resources;
using YoYoStudio.Core.Utils.SourceControl;
using YoYoStudio.GUI;

namespace YoYoStudio
{
    namespace Plugins
    {
        namespace ZplBackupPlugin
        {
            public class ZplBackupPluginCommand : IModule, IDisposable
            {
                public bool Stop { get; set; }
                public ZplBackupPluginPreferences Preferences { get; set; }
                public ModulePackage IdeInterface { get; set; }
                public Random Numbers { get; set; }

                public void OnChange(List<string> _changed)
                {
                    if (_changed.Any(_pref => _pref.Contains("ZplBackup")))
                    {
                        Preferences = PreferencesManager.Get<ZplBackupPluginPreferences>();
                        Log.WriteLine(eLog.Default, "[ZplBackup]: Preferences updated.");
                    }
                }

                public string GetCSVPath()
                {
                    string thePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Custom Plugins", "ZplBackupPluginStrings.csv");
                    return thePath;
                }

                public void CompressYYZ(bool _success)
                {
                    IDE.OnProjectSaved.RemoveThis();

                    ProjectInfo.ChangeProjectPath(ProjectInfo.Current.id, OriginalProjectPath);
                    ProjectInfo.Current.name = YoYoPath.GetFileNameWithoutExtension(OriginalProjectPath);

                    if (_success)
                    {
                        YYZStart();
                        new Task(YYZTask).Start();
                    }
                    else
                    {
                        MessageDialog.ShowWarning("ZPLB_DirectoryNotExist_Title", "ZPLB_Error", new string[] { "CompressYYZ/OnSave returned false." });
                    }
                }

                public string SaveProjectPath { get; set; }
                public string OriginalProjectPath { get; set; }
                public string OutYYZBackupPath { get; set; }

                public void YYZStart()
                {
                    IDE.BlockInput();
                    IdeInterface.WindowManager.UpdateProgressBar("BackingUp", 0f);
                }

                public void YYZOnProgress(float _progress)
                {
                    WindowManager.OnPreProcess += (TimeSpan _delta) =>
                    {
                        WindowManager.OnPreProcess.RemoveThis();
                        IdeInterface.WindowManager.UpdateProgressBar("BackingUp", _progress);
                    };
                }

                public void YYZEndOnPreProcess(TimeSpan _delta)
                {
                    WindowManager.OnPreProcess.RemoveThis();
                    IdeInterface.WindowManager.UpdateProgressBar("BackingUp", -1f);
                    IDE.UnblockInput();
                    SourceControl.UnPause();
                    Stop = false; // never forget this kids.

                    var fileErr = FileSystem.DeleteDirectory(YoYoPath.GetDirectoryName(SaveProjectPath), null, null, 0, null).wait();
                    if (fileErr != FileError.OK)
                    {
                        MessageDialog.ShowWarning("ZPLB_DirectoryNotExist_Title", "ZPLB_Error", new string[] { "Unable to delete temp project dir " + fileErr.ToString() });
                    }
                }

                public void YYZTask()
                {
                    string zipErrMsg = "";
                    ZipUtil.CompressDirectoryToZip(OutYYZBackupPath, YoYoPath.GetDirectoryName(SaveProjectPath), YYZOnProgress, ref zipErrMsg);
                    WindowManager.OnPreProcess += YYZEndOnPreProcess;
                }

                public void OnSavedMiddleman(bool _success)
                {
                    IDE.OnProjectSaved.RemoveThis();

                    if (_success)
                    {
                        IDE.OnProjectSaved += new Action<bool>(CompressYYZ);
                        Command.execute("save_project", SaveProjectPath, OriginalProjectPath, false);
                    }
                    else
                    {
                        MessageDialog.ShowWarning("ZPLB_DirectoryNotExist_Title", "ZPLB_Error", new string[] { "OnSavedMiddleman/OnSave returned false." });
                    }
                }

                public void BeginBackup()
                {
                    // fetch the pref value once, never reference it more than once just in case.
                    int max = Preferences.AmountOfBackups;

                    // backups are disabled?
                    if (max < 1)
                        return;

                    // idiot checks

                    string backupsDir = Preferences.BackupFolderPath;
                    if (string.IsNullOrWhiteSpace(backupsDir))
                    {
                        MessageDialog.ShowWarning("ZPLB_DirectoryNotExist_Title", "ZPLB_DirectoryNotExist_Message");
                        return;
                    }

                    if (FileSystem.DirectoryExists(backupsDir, null, null).wait() == FileError.DirectoryNotFound)
                    {
                        MessageDialog.ShowWarning("ZPLB_DirectoryNotExist_Title", "ZPLB_DirectoryNotExist_Message");
                        return;
                    }

                    int mynum = Numbers.Next();
                    string tempsaveDir = YoYoPath.Combine(backupsDir, "tempsave" + mynum.ToString());
                    string projectPath = ProjectInfo.GetProjectPath(ProjectInfo.Current.id);
                    string projectName = YoYoPath.GetFileNameWithoutExtension(projectPath);
                    string tempsavePath = YoYoPath.Combine(tempsaveDir, projectName + ".yyp");
                    string yyzformatpath = YoYoPath.Combine(backupsDir, projectName + "_backup{0}.yyz");
                    string yyzfinalpath = yyzformatpath;
                    SaveProjectPath = tempsavePath;
                    OriginalProjectPath = projectPath;

                    var fileErr = FileSystem.CreateDirectory(tempsaveDir, null, null).wait();
                    if (fileErr != FileError.OK)
                    {
                        MessageDialog.ShowWarning("ZPLB_DirectoryNotExist_Title", "ZPLB_Error", new string[] { "Unable to create temp project dir " + fileErr.ToString() });
                        return;
                    }

                    int i;
                    bool found = false;

                    // at first try to see if there are any free files we can use.
                    for (i = 1; i <= max; ++i)
                    {
                        yyzfinalpath = string.Format(yyzformatpath, i);
                        if (FileSystem.FileExists(yyzfinalpath, null, null).wait() == FileError.FileNotFound)
                        {
                            found = true;
                            break;
                        }
                    }

                    // if all files are taken, since we can't use more than max files
                    // delete last backup and move the files.
                    if (!found)
                    {
                        // delete the last file
                        FileSystem.DeleteFile(string.Format(yyzformatpath, max), null, null, 0, null).wait();

                        // move files
                        for (i = max - 1; i > 0; --i)
                        {
                            string curyyz = string.Format(yyzformatpath, i);
                            string beyondyyz = string.Format(yyzformatpath, i + 1);
                            FileSystem.MoveFile(curyyz, beyondyyz, null, null, 0, null).wait();
                        }

                        // set our final path to _backup1.yyz
                        yyzfinalpath = string.Format(yyzformatpath, 1);
                    }

                    // ensure the file does not exist so we can save there.
                    FileSystem.DeleteFile(yyzfinalpath, null, null, 0, null).wait();
                    OutYYZBackupPath = YoYoPath.Combine(backupsDir, yyzfinalpath);

                    // start the backup process yoo
                    SourceControl.Pause();
                    Stop = true;
                    IDE.SaveProjectIncremental(OnSavedMiddleman);
                }

                public void OnProjectSaved(bool _success)
                {
                    // update the RNG every time.
                    Numbers.Next();

                    if (_success)
                    {
                        if (!Stop)
                        {
                            BeginBackup();
                        }
                        // todo: do something?
                    }
                }

                public void OnIDEInitialised()
                {
                    Stop = false;

                    // rng used to generate random dir suffixes
                    Numbers = new Random();
                    Numbers.Next();

                    // subscribe us to the project save callback.
                    IDE.OnProjectSaved += OnProjectSaved;

                    // load our loc strings.
                    string langret = Language.Load(GetCSVPath());

                    // fetch prefs
                    Preferences = PreferencesManager.Get<ZplBackupPluginPreferences>();
                    PreferencesManager.OnChange += OnChange;

                    // we're done here
                    Log.WriteLine(eLog.Default, "[ZplBackup]: Initialised. {0}", langret);
                }

                public void Initialise(ModulePackage _ide)
                {
                    IdeInterface = _ide;
                    OnIDEInitialised();
                }

                #region IDisposable Support
                private bool disposed = false; // To detect redundant calls

                protected virtual void Dispose(bool disposing)
                {
                    if (!disposed)
                    {
                        if (disposing)
                        {
                            // TODO: dispose managed state (managed objects).
                        }

                        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                        // TODO: set large fields to null.

                        disposed = true;
                    }
                }

                // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
                ~ZplBackupPluginCommand()
                {
                    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                    Dispose(false);
                }

                // This code added to correctly implement the disposable pattern.
                public void Dispose()
                {
                    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                    Dispose(true);
                    // TODO: uncomment the following line if the finalizer is overridden above.
                    GC.SuppressFinalize(this);
                }
                #endregion
            }
        }
    }
}
