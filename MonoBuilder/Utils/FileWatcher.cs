using Krypton.Toolkit;
using MonoBuilder.Screens;
using MonoBuilder.Screens.ScreenUtils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonoBuilder.Utils
{
    static class FileWatcher
    {
        private static FileSystemWatcher? Watcher { get; set; }
        private static AppSettings? ApplicationSettings { get; set; }
        private static ScriptConversion? Converter { get; set; }
        private static Characters? CharacterData { get; set; }

        private static bool DialogIsOpen { get; set; } = false;
        private static Form? CurrentContext { get; set; }
        private static Dictionary<string, bool> ChangedFiles { get; set; } = new Dictionary<string, bool>();

        private static bool _isBeingWritten { get; set; } = false;
        private static System.Timers.Timer? _suppressionTimer { get; set; }

        public static void InitializeWatcher(
            FileSystemWatcher watcher,
            AppSettings settings,
            ScriptConversion converter,
            Characters characters)
        {
            var masterPath = settings.GetFolderPath("Base");

            if (masterPath != null)
            {
                Watcher = watcher;
                ApplicationSettings = settings;
                Converter = converter;
                CharacterData = characters;

                watcher.Path = masterPath;
                watcher.NotifyFilter = NotifyFilters.LastWrite
                    | NotifyFilters.FileName
                    | NotifyFilters.DirectoryName;
                watcher.Filter = "*";
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;

                watcher.Changed += Watcher_Changed;
                watcher.Created += Watcher_Created;
                watcher.Deleted += Watcher_Deleted;
                watcher.Renamed += Watcher_Renamed;

                _suppressionTimer = new System.Timers.Timer(1000) { AutoReset = false };
                _suppressionTimer.Elapsed += (s, e) => _isBeingWritten = false;
            }
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (_isBeingWritten) return;

            string changedPath = e.FullPath;

            string? charactersPath = ApplicationSettings?.GetFilePath("Characters");
            string? scriptPath = ApplicationSettings?.GetFilePath("Script");

            bool charactersChanged = false;
            bool scriptChanged = false;

            if (charactersPath != null && string.Equals(changedPath, charactersPath, StringComparison.OrdinalIgnoreCase))
            {
                SetFileChanged("Characters", true);
                charactersChanged = true;
            }
            
            if (scriptPath != null && string.Equals(changedPath, scriptPath, StringComparison.OrdinalIgnoreCase))
            {
                SetFileChanged("Script", true);
                ApplicationSettings?.SetSynchronicityCheck(false);
                scriptChanged = true;
            }

            if (charactersChanged || scriptChanged)
            {
                ShowChangesMadeSettings();
            }

            if (scriptChanged)
            {
                ShowChangedMadeLoadLabels();
            }
        }

        private static void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            string? assetsPath = ApplicationSettings?.GetFolderPath("Assets");

            bool isInsideTrackedFolder =
                assetsPath != null &&
                e.FullPath.StartsWith(assetsPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

            if (isInsideTrackedFolder)
            {
                // Do something when the user adds a file to their assets directory.
            }
        }

        private static void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            bool changed = false;

            string? charactersPath = ApplicationSettings?.GetFilePath("Characters");
            string? scriptPath = ApplicationSettings?.GetFilePath("Script");

            string? assetsPath = ApplicationSettings?.GetFolderPath("Assets");

            // Use "if" instead of "if..else if" because some files share a similar path.
            changed = ReplaceProgramFile(charactersPath, e.FullPath, charactersPath, "Characters") || changed;
            changed = ReplaceProgramFile(scriptPath, e.FullPath, scriptPath, "Script") || changed;
            changed = ReplaceProgramFolder(assetsPath, e.FullPath, assetsPath, "Assets") || changed;

            if (changed)
            {
                ApplicationSettings?.SaveDirectories();
            }
        }

        private static void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            bool changed = false;
            string? charactersPath = ApplicationSettings?.GetFilePath("Characters");
            string? scriptPath = ApplicationSettings?.GetFilePath("Script");

            string? assetsPath = ApplicationSettings?.GetFolderPath("Assets");

            changed = ReplaceProgramFile(charactersPath, e.OldFullPath, charactersPath, "Characters") || changed;
            changed = ReplaceProgramFile(scriptPath, e.OldFullPath, scriptPath, "Script") || changed;
            changed = ReplaceProgramFolder(assetsPath, e.OldFullPath, assetsPath, "Assets") || changed;

            // Run if the base folder was changed for any reason.
            string oldPrefix = e.OldFullPath + Path.DirectorySeparatorChar;
            string newPrefix = e.FullPath + Path.DirectorySeparatorChar;

            if (charactersPath != null && charactersPath.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase))
            {
                ApplicationSettings?.AddReplaceFilePath("Characters", newPrefix + charactersPath[oldPrefix.Length..]);
                changed = true;
            }

            if (scriptPath != null && scriptPath.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase))
            {
                ApplicationSettings?.AddReplaceFilePath("Script", newPrefix + scriptPath[oldPrefix.Length..]);
                changed = true;
            }

            if (assetsPath != null && assetsPath.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase))
            {
                ApplicationSettings?.AddReplaceFolderPath("Assets", newPrefix + assetsPath[oldPrefix.Length..]);
                changed = true;
            }

            if (changed)
            {
                ApplicationSettings?.SaveDirectories();
            }
        }

        private static bool ReplaceProgramFile(string? filePath, string? oldFilePath, string? newFilePath, string asset)
        {
            if (filePath != null && string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
            {
                if (newFilePath != null)
                {
                    ApplicationSettings?.AddReplaceFilePath(asset, newFilePath);
                    SetFileChanged(asset, true);
                    return true;
                }
            }

            return false;
        }

        private static bool ReplaceProgramFolder(string? filePath, string? oldFilePath, string? newFilePath, string asset)
        {
            if (filePath != null && string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
            {
                if (newFilePath != null)
                {
                    ApplicationSettings?.AddReplaceFolderPath(asset, newFilePath);
                    SetFileChanged(asset, true);
                    return true;
                }
            }

            return false;
        }

        private static void ShowChangesMadeSettings()
        {
            if (CurrentContext is Settings && AnyFileChanged())
            {
                var changedLabel = CurrentContext.Controls.Find("ChangesMadeLabel", true)[0] as KryptonLabel;
                if (changedLabel != null)
                {
                    changedLabel.Visible = true;
                }
            }
        }

        private static void ShowChangedMadeLoadLabels()
        {
            if (CurrentContext is LoadScripts && IsFileChanged("Script"))
            {
                var changedLabel = CurrentContext.Controls.Find("ChangesMadeLabel", true)[0] as KryptonLabel;
                if (changedLabel != null)
                {
                    var btnSyncLabels = CurrentContext.Controls.Find("btnSyncLabels", true)[0] as KryptonButton;
                    changedLabel.Visible = true;

                    if (btnSyncLabels != null)
                    {
                        btnSyncLabels.Enabled = true;
                        btnSyncLabels.Text = "Sync Labels";
                    }
                }
            }

            if (!DialogIsOpen && (CurrentContext is LoadScripts ||
                CurrentContext is ScriptBuilder ||
                CurrentContext is ScriptBuilderOutput))
            {
                DialogIsOpen = true;
                var result = DialogBox.Show(
                    "It looks like changes were made to the script while the program was open.\nWould you like to run a synchronicity check to align current program and script content?",
                    "Changes Have Been Made",
                    DialogButtonDefaults.YesNo,
                    DialogIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    Type type = CurrentContext.GetType();
                    var CheckSynchronicity = type.GetMethod("RunSynchronicityCheck");

                    CheckSynchronicity?.Invoke(CurrentContext, null);
                }

                DialogIsOpen = false;
            }
        }

        public static void ReplaceFile(string tempPath, string filePath)
        {
            _isBeingWritten = true;

            try
            {
                _suppressionTimer?.Stop();
                _suppressionTimer?.Start();

                File.Replace(tempPath, filePath, filePath + ".bak");
            }
            catch (Exception error)
            {
                DialogBox.Show(
                    "An error occured during the file replacement process!\nNo need to worry, the process was stopped before the content saved.",
                    "An Error Occured",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new FileLoadException($"Something has gone wrong...\n\n{error}", filePath);
            }
        }

        public static void SetCurrentContext(Form context)
        {
            CurrentContext = context;
        }

        public static Form? GetCurrentContext()
        {
            return CurrentContext;
        }

        public static bool IsFileChanged(string fileName)
        {
            return ChangedFiles.TryGetValue(fileName, out bool value) && value;
        }

        public static bool AnyFileChanged()
        {
            foreach (bool value in ChangedFiles.Values)
            {
                if (value)
                {
                    return value;
                }
            }

            return false;
        }

        public static void SetFileChanged(string fileName, bool value = false)
        {
            ChangedFiles[fileName] = value;
        }
    }
}
