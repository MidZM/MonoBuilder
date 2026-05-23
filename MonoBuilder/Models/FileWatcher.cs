using MonoBuilder.Views;
using MonoBuilder.Views.ViewUtils;
using MonoBuilder.Models.character_management;
using MonoBuilder.Models.helpers;
using MonoBuilder.Models.image_management;
using MonoBuilder.Models.generics.enums;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MonoBuilder.Models.notification_management;
using MonoBuilder.ViewModels.NotifierModel;
using MonoBuilder.ViewModels.ImageModel;

namespace MonoBuilder.Models
{
	public interface ISynchronizable
	{
		void RunSynchronicityCheck();
	}

    static class FileWatcher
    {
        private static FileSystemWatcher? Watcher { get; set; }
        private static AppSettings? ApplicationSettings { get; set; }
        private static ScriptConversion? ScriptConverter { get; set; }
        private static Characters? CharacterData { get; set; }
        private static MonoImages? ImageData { get; set; }
        private static Notifications? NotificationData { get; set; }

        private static bool DialogIsOpen { get; set; } = false;
        private static Window? CurrentContext { get; set; }
        private static Dictionary<string, bool> ChangedFiles { get; set; } = new Dictionary<string, bool>();

        private static bool _isBeingWritten { get; set; } = false;
        private static System.Timers.Timer? _suppressionTimer { get; set; }

        private static readonly string[] TrackedFileTypes =
            ["Characters", "Images", "Scenes", "Gallery", "Messages", "Notifications", "Script"];

        public static void InitializeWatcher(
            FileSystemWatcher watcher,
            AppSettings settings,
            ScriptConversion converter,
            Characters characters,
            MonoImages images,
			Notifications notifications)
        {
            var masterPath = settings.GetFolderPath("Base");

            if (masterPath != null)
            {
                Watcher = watcher;
                ApplicationSettings = settings;
                ScriptConverter = converter;
                CharacterData = characters;
                ImageData = images;
				NotificationData = notifications;

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
                ShowChangesMadeSettings();
                ShowChangesMadeLoadLabels();
            }

            if (changed["Images"] || changed["Scenes"] || changed["Gallery"])
            {
                ShowChangedMadeImages();
            }

			if (changed["Messages"] || changed["Notifications"])
			{
				ShowChangesMadeNotifications();
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

				if (ScriptConverter != null &&
					ScriptConverter.CheckIsAutoSyncLabels() &&
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
                if (CurrentContext is ImageBuilder imageBuilder && (AnyFileChanged("Images") || AnyFileChanged("Scenes") || AnyFileChanged("Gallery")))
                {
                    var changedLabel = Helpers.FindVisualChild<Label>(CurrentContext, "ChangesMadeLabel");
                    if (changedLabel != null)
                    {
                        changedLabel.Visibility = Visibility.Visible;
                        ForciblyUpdateImagesList();
                    }
                }
            });
        }

		private static void ShowChangesMadeNotifications()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				if (CurrentContext is NotificationBuilder notificationBuilder && (AnyFileChanged("Messages") || AnyFileChanged("Notifications")))
				{
					var changedLabel = Helpers.FindVisualChild<Label>(CurrentContext, "ChangesMadeLabel");
					if (changedLabel != null)
					{
						changedLabel.Visibility = Visibility.Visible;
						ForciblyUpdateNotificationsList();
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
                    var characterData = CharacterData.SyncData(true);
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


                            Character newNormal = new(name, tag, color, directory);

                            newNormal.FileKey = fileKey;
                            newNormal.IsSynced = true;

                            CharacterData.UpdateData(characterId, newNormal);
                        }
                    }

                    foreach (var character in unsyncedCharacters)
                    {
                        if (character.IsSynced)
                        {
                            character.IsSynced = false;
                            CharacterData.UpdateData(character.EntityID, character);
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

					ScriptConverter!.UnsetCharacterList();

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
                        var imageData = ImageData.SyncData(true);
						var unsyncedImages = ImageData.DataMode.Collection;

                        if (imageData.Values.Count > 0)
                        {
                            foreach (var image in imageData.Values)
                            {
                                unsyncedImages.Remove(unsyncedImages.First(i => i.Name == image.Name));

								int imageId = ImageData.DataMode.Collection
									.First(i => i.Name == image.Name)
									.EntityID;

                                string name = image.Name;
                                string path = image.Path;
                                string fileKey = image.FileKey;

                                MonoImage newImage = new(name, path)
                                {
                                    EntityID = imageId,
                                    FileKey = fileKey,
                                    IsSynced = true
                                };

                                ImageData.UpdateData(imageId, newImage);
                            }
                        }

                        foreach (var image in unsyncedImages)
                        {
                            if (image.IsSynced)
                            {
                                image.IsSynced = false;
                                ImageData.UpdateData(image.EntityID, image);
                            }
                        }
					}

					var context = CurrentContext!.DataContext as ImageViewModel;
					context!.ClearSelectedEntities();

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

        public static void ForciblyUpdateNotificationsList(bool shouldShowMessages = true)
        {
            if (NotificationData != null)
            {
                if (shouldShowMessages)
                {
                    DialogBox.Show(
                    "Changes to a message or notification file was made!\nTo preserve synchronicity, existing content will be forcibly updated.",
                    "Changes Made",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
                }

                _isBeingWritten = true;

                try
                {
                    string[] modes = ["messages", "notifications"];
                    foreach (string mode in modes)
                    {
						NotificationData.SetDataMode(mode);
                        var notifierData = NotificationData.SyncData(true);
						var unsyncedNotifiers = NotificationData.DataMode.Collection;

                        if (notifierData.Values.Count > 0)
                        {
                            foreach (var notifier in notifierData.Values)
                            {
                                unsyncedNotifiers.Remove(unsyncedNotifiers.First(i => i.Name == notifier.Name));

								var type = mode == modes[0] ? NotifierType.Message : NotifierType.Notification;
								bool isMessage = type == NotifierType.Message;
								int notifierId = NotificationData.DataMode.Collection.First(i => i.Name == notifier.Name).EntityID;

                                string name = notifier.Name;
								string? title = notifier.Title;
								string? subtitle = notifier.Subtitle;
								string? icon = notifier.Icon;
								string? body = notifier.Body;
								string? actionString = notifier.CloseAction;
                                string fileKey = notifier.FileKey;

                                Notification newNotifier = new(name, title, isMessage ? null : icon, type)
                                {
                                    EntityID = notifierId,
									Body = body,
                                    FileKey = fileKey,
                                    IsSynced = true
                                };

								if (isMessage)
								{
									newNotifier.Subtitle = subtitle;
									newNotifier.CloseAction = actionString;
								}

								NotificationData.UpdateData(notifierId, newNotifier);
							}
						}

						foreach (var notifier in unsyncedNotifiers)
						{
							if (notifier.IsSynced)
							{
								notifier.IsSynced = false;
								NotificationData.UpdateData(notifier.EntityID, notifier);
							}
						}

					}

					var context = CurrentContext!.DataContext as NotifierViewModel;
					context!.ClearSelectedEntities();

                    if (shouldShowMessages)
                    {
                        DialogBox.Show(
                        "Sucessfully updated notification assets!",
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
                        $"An error occurred while updating notification assets!\n\n{error}",
                        "Error",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                }
            }
        }
    }
}
