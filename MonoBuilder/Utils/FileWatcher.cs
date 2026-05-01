using MonoBuilder.Screens;
using MonoBuilder.Screens.ScreenUtils;
using MonoBuilder.Utils.character_management;
using MonoBuilder.Utils.image_management;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MonoBuilder.Utils
{
	public interface ISynchronizable
	{
		void RunSynchronicityCheck();
	}

    static class FileWatcher
    {
        private static FileSystemWatcher? Watcher { get; set; }
        private static AppSettings? ApplicationSettings { get; set; }
        private static ScriptConversion? Converter { get; set; }
        private static Characters? CharacterData { get; set; }
        private static MonoImages? ImageData { get; set; }

        private static bool DialogIsOpen { get; set; } = false;
        private static Window? CurrentContext { get; set; }
        private static Dictionary<string, bool> ChangedFiles { get; set; } = new Dictionary<string, bool>();

        private static bool _isBeingWritten { get; set; } = false;
        private static System.Timers.Timer? _suppressionTimer { get; set; }

        private static readonly string[] TrackedFileTypes =
            ["Characters", "Images", "Scenes", "Gallery", "Script"];

        public static void InitializeWatcher(
            FileSystemWatcher watcher,
            AppSettings settings,
            ScriptConversion converter,
            Characters characters,
            MonoImages images)
        {
            var masterPath = settings.GetFolderPath("Base");

            if (masterPath != null)
            {
                Watcher = watcher;
                ApplicationSettings = settings;
                Converter = converter;
                CharacterData = characters;
                ImageData = images;

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

        private static Dictionary<string, string> GetTrackedFiles(string type) =>
            ApplicationSettings?.GetAllFilePaths(type) ?? [];

        private static Dictionary<string, string> GetTrackedFolders(string type) =>
            ApplicationSettings?.GetAllFolderPaths(type) ?? [];

        private static bool MatchesTrackedPath(string changedPath, string trackedPath) =>
            string.Equals(changedPath, trackedPath, StringComparison.OrdinalIgnoreCase);

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (_isBeingWritten) return;

            string changedPath = e.FullPath;
            Dictionary<string, bool> changed = TrackedFileTypes
                .ToDictionary(type => type, type => false);

            foreach (string type in TrackedFileTypes)
            {
                foreach (var (fileKey, filePath) in GetTrackedFiles(type))
                {
                    if (MatchesTrackedPath(changedPath, filePath))
                    {
                        SetFileChanged(fileKey, true);
                        if (type == "Script")
                        {
                            ApplicationSettings?.SetSynchronicityCheck(false);
                        }

                        changed[type] = true;
                    }
                }
            }

            if (changed["Characters"])
            {
                ShowChangesMadeCharacters();
            }

            if (changed["Script"])
            {
                System.Diagnostics.Debug.WriteLine("I changed - Script");
                ShowChangesMadeSettings();
                ShowChangesMadeLoadLabels();
            }

            if (changed["Images"] || changed["Scenes"] || changed["Gallery"])
            {
                System.Diagnostics.Debug.WriteLine("I changed - Image Assets");
                ShowChangedMadeImages();
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

            foreach (string type in TrackedFileTypes)
            {
                foreach (var (fileKey, filePath) in GetTrackedFiles(type).ToList())
                {
                    changed = ReplaceProgramFile(fileKey, filePath, e.FullPath, filePath) || changed;
                }
            }

            foreach (var (folderKey, folderPath) in GetTrackedFolders("Assets"))
            {
                changed = ReplaceProgramFolder(folderKey, folderPath, e.FullPath, folderPath) || changed;
            }

            if (changed)
            {
                ApplicationSettings?.SaveDirectories();
            }
        }

        private static void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            bool changed = false;

            foreach (string type in TrackedFileTypes)
            {
                foreach (var (fileKey, filePath) in GetTrackedFiles(type).ToList())
                {
                    changed = ReplaceProgramFile(fileKey, filePath, e.OldFullPath, e.FullPath) || changed;
                }
            }

            foreach (var (folderKey, folderPath) in GetTrackedFolders("Assets").ToList())
            {
                changed = ReplaceProgramFolder(folderKey, folderPath, e.OldFullPath, e.FullPath) || changed;
            }

            string oldPrefix = e.OldFullPath + Path.DirectorySeparatorChar;
            string newPrefix = e.FullPath + Path.DirectorySeparatorChar;

            if (ApplicationSettings != null)
            {
                foreach (var (fileKey, filePath) in ApplicationSettings.GetAllFilePaths().ToList())
                {
                    if (filePath.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        ApplicationSettings.AddReplaceFilePath(fileKey, newPrefix + filePath[oldPrefix.Length..]);
                        changed = true;
                    }
                }

                foreach (var (folderKey, folderPath) in ApplicationSettings.GetAllFolderPaths().ToList())
                {
                    if (folderPath.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        ApplicationSettings.AddReplaceFolderPath(folderKey, newPrefix + folderPath[oldPrefix.Length..]);
                        changed = true;
                    }
                }

                if (changed)
                {
                    ApplicationSettings.SaveDirectories();
                }
            }
        }

		private static bool ReplaceProgramFile(string key, string? trackedPath, string? oldFilePath, string? newFilePath)
		{
			if (!_isBeingWritten && trackedPath != null && string.Equals(trackedPath, oldFilePath, StringComparison.OrdinalIgnoreCase))
			{
				if (newFilePath != null)
				{
					ApplicationSettings?.AddReplaceFilePath(key, newFilePath);
					SetFileChanged(key, true);
					return true;
				}
			}
			return false;
		}

		private static bool ReplaceProgramFolder(string key, string? trackedPath, string? oldFilePath, string? newFilePath)
		{
			if (trackedPath != null && string.Equals(trackedPath, oldFilePath, StringComparison.OrdinalIgnoreCase))
			{
				if (newFilePath != null)
				{
					ApplicationSettings?.AddReplaceFolderPath(key, newFilePath);
					SetFileChanged(key, true);
					return true;
				}
			}
			return false;
		}

		private static void ShowChangesMadeSettings()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				if (CurrentContext is Settings settings && AnyFileChanged())
				{
					var changedLabel = Helpers.FindVisualChild<Label>(settings, "ChangesMadeLabel");
					if (changedLabel != null)
					{
						changedLabel.Visibility = Visibility.Visible;
					}
				}
			});
		}

		private static void ShowChangesMadeLoadLabels()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				if (CurrentContext is LoadScripts scripts && AnyFileChanged("Script"))
				{
					var changedLabel = Helpers.FindVisualChild<Label>(CurrentContext, "ChangesMadeLabel");
					if (changedLabel != null)
					{
						var btnSyncLabels = Helpers.FindVisualChild<Button>(CurrentContext, "btnSyncLabels");
						changedLabel.Visibility = Visibility.Visible;

						if (btnSyncLabels != null)
						{
							btnSyncLabels.Content = "Sync Labels";
						}
					}
				}

				if (Converter != null &&
					Converter.CheckIsAutoSyncLabels() &&
					!DialogIsOpen &&
					(CurrentContext is LoadScripts ||
					CurrentContext is ScriptBuilder ||
					CurrentContext is ScriptBuilderOutput))
				{
					DialogIsOpen = true;
					var result = DialogBox.Show(
						"It looks like changes were made to the script while the program was open.\nWould you like to run a synchronicity check to align current program and script content?",
						"Changes Have Been Made",
						DialogButtonDefaults.YesNo,
						DialogIcon.Warning);

					if (result == DialogBoxResult.Yes)
					{
						(CurrentContext as ISynchronizable)?.RunSynchronicityCheck();
					}

					DialogIsOpen = false;
				}
			});
		}

        private static void ShowChangesMadeCharacters()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (CurrentContext is Settings characters && AnyFileChanged("Characters"))
                {
                    var changedLabel = Helpers.FindVisualChild<Label>(CurrentContext, "ChangesMadeLabel");
                    if (changedLabel != null)
                    {
                        changedLabel.Visibility = Visibility.Visible;
                        ForciblyUpdateCharactersList();
                    }
                }
            });
        }

        private static void ShowChangedMadeImages()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine(CurrentContext);
                if (CurrentContext is ImageBuilder imageBuilder && (AnyFileChanged("Images") || AnyFileChanged("Scenes") || AnyFileChanged("Gallery")))
                {
                    System.Diagnostics.Debug.WriteLine("Changes Made");
                    var changedLabel = Helpers.FindVisualChild<Label>(CurrentContext, "ChangesMadeLabel");
                    if (changedLabel != null)
                    {
                        changedLabel.Visibility = Visibility.Visible;
                        ForciblyUpdateImagesList();
                    }
                }
            });
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

        public static void SetCurrentContext(Window context)
        {
            CurrentContext = context;
        }

        public static Window? GetCurrentContext()
        {
            return CurrentContext;
        }

        public static bool IsFileChanged(string fileName)
        {
            return ChangedFiles.TryGetValue(fileName, out bool value) && value;
        }

        public static bool AnyFileChanged(string? typePrefix = null)
        {
            foreach (var pair in ChangedFiles)
            {
                if (!pair.Value)
                    continue;

                if (string.IsNullOrWhiteSpace(typePrefix) ||
                    pair.Key.Equals(typePrefix, StringComparison.OrdinalIgnoreCase) ||
                    pair.Key.StartsWith(typePrefix + ":", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetFileChanged(string fileName, bool value = false)
        {
            ChangedFiles[fileName] = value;
        }

        public static void ForciblyUpdateCharactersList(bool shouldShowMessages = true)
        {
            if (CharacterData != null)
            {
                if (shouldShowMessages)
                {
                    DialogBox.Show(
                    "Changes to a character file were made!\nTo preserve synchronicity, existing characters will be forcibly updated.",
                    "Changes Made",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
                }

                _isBeingWritten = true;

                try
                {
                    var characterData = CharacterData.SyncCharacters(true);
                    var unsyncedCharacters = CharacterData.AllCharacters.ToList();

                    if (characterData.Values.Count > 0)
                    {
                        foreach (var character in characterData.Values)
                        {
                            unsyncedCharacters.Remove(unsyncedCharacters.First(c => c.Tag == character.Tag));

                            int characterId = CharacterData.AllCharacters.First(c => c.Tag == character.Tag).EntityID;
                            string name = character.Name;
                            string tag = character.Tag;
                            string fileKey = character.FileKey;
                            string? color = character.Color;
                            string? directory = character.Directory;


                            Normal newNormal = new(name, tag, color, directory);

                            newNormal.FileKey = fileKey;
                            newNormal.IsSynced = true;

                            CharacterData.UpdateCharacter(characterId, newNormal);
                        }
                    }

                    foreach (var character in unsyncedCharacters)
                    {
                        if (character.IsSynced)
                        {
                            character.IsSynced = false;
                            CharacterData.UpdateCharacter(character.EntityID, character);
                        }
                    }

                    if (shouldShowMessages)
                    {
                        DialogBox.Show(
                        "Sucessfully updated characters!",
                        "Success",
                        DialogButtonDefaults.OK,
                        DialogIcon.Information);
                    }

                    _suppressionTimer?.Stop();
                    _suppressionTimer?.Start();
                } catch (Exception error)
                {
                    DialogBox.Show(
                        $"An error occurred while updating characters!\n\n{error}",
                        "Error",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                }

            }
        }

        public static void ForciblyUpdateImagesList(bool shouldShowMessages = true)
        {
            if (ImageData != null)
            {
                if (shouldShowMessages)
                {
                    DialogBox.Show(
                    "Changes to an image, scene, or gallery file were made!\nTo preserve synchronicity, existing assets will be forcibly updated.",
                    "Changes Made",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
                }

                _isBeingWritten = true;

                try
                {
                    string[] modes = ["images", "scenes", "gallery"];
                    foreach (string mode in modes)
                    {
                        ImageData.SetDataMode(mode);
                        var imageData = ImageData.SyncImages(true);
                        var unsyncedImages = mode == modes[0]
                            ? ImageData.AllImages.ToList()
                                : mode == modes[1]
                            ? ImageData.AllScenes.ToList()
                            : ImageData.AllGalleryImages.ToList();

                        if (imageData.Values.Count > 0)
                        {
                            foreach (var image in imageData.Values)
                            {
                                unsyncedImages.Remove(unsyncedImages.First(i => i.Name == image.Name));

                                int imageId = mode == modes[0]
                                    ? ImageData.AllImages.First(i => i.Name == image.Name).EntityID
                                        : mode == modes[1]
                                    ? ImageData.AllScenes.First(i => i.Name == image.Name).EntityID
                                    : ImageData.AllGalleryImages.First(i => i.Name == image.Name).EntityID;
                                string name = image.Name;
                                string path = image.Path;
                                string fileKey = image.FileKey;

                                MonoImage newImage = new(name, path)
                                {
                                    EntityID = imageId,
                                    FileKey = fileKey,
                                    IsSynced = true
                                };

                                ImageData.UpdateImage(imageId, newImage);
                            }
                        }

                        foreach (var image in unsyncedImages)
                        {
                            if (image.IsSynced)
                            {
                                image.IsSynced = false;
                                ImageData.UpdateImage(image.EntityID, image);
                            }
                        }
                    }

                    if (shouldShowMessages)
                    {
                        DialogBox.Show(
                        "Sucessfully updated image assets!",
                        "Success",
                        DialogButtonDefaults.OK,
                        DialogIcon.Information);
                    }

                    _suppressionTimer?.Stop();
                    _suppressionTimer?.Start();
                }
                catch (Exception error)
                {
                    DialogBox.Show(
                        $"An error occurred while updating image assets!\n\n{error}",
                        "Error",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                }
            }
        }
    }
}
