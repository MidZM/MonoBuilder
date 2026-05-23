using MonoBuilder.Commands;
using MonoBuilder.Models;
using MonoBuilder.Models.character_management;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.image_management;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static System.Resources.ResXFileRef;

namespace MonoBuilder.ViewModels.ImageModel
{
    public partial class ImageViewModel
    {
		public RelayCommand MoveImagesCommand { get; }

		public RelayCommand AddImagesCommand { get; }
		public RelayCommand ModifyImagesCommand { get; }
		public RelayCommand RemoveImagesCommand { get; }
		public RelayCommand SaveToScriptCommand { get; }
		public RelayCommand ImportImagesCommand { get; }

		public RelayCommand ExitCommand { get; }

		#region Utility Commands
		private ContentManipulator GetDefinedManipulator(ObservableCollection<IEnumerable<string?>>? data = null)
		{
			return GetManipulator(ApplicationSettings, Mode,
				new _generic_models.ContentBuilderTemplate()
				{
					Names = new List<string>(["Name", "Path", "File"]),
					BoxTypes = new List<ContentBoxType>([
						ContentBoxType.RequiredTextBox,
						ContentBoxType.RequiredFileButton,
						ContentBoxType.FileBox]),
					Placeholders =
						[
							("Name", "Enter Image Name"),
							("Path", "Path to Image Assets")
						]
				}, data);
		}

		protected override void ApplyContentChanges(ContentManipulator window, List<FrameworkElement> control, int index, MonoImage[]? notifications = null)
		{
			var elements = ExtractElements(control);

			elements.TryGetValue("Name", out string? name);
			elements.TryGetValue("Path", out string? path);
			elements.TryGetValue("File", out string? fileKey);

			if (name != null && path != null)
			{
				path = GetRelativePath(path, ApplicationSettings.GetFolderPath(Mode)!);
				var image = new MonoImage(name, path);
				if (window.ModifyingContent?.Count > 0)
				{
					window.ModifyingContent[index].TryGetValue("Name", out string? nameValue);
					var modifiedImage = notifications?.FirstOrDefault(c => c.Name == nameValue);

					if (modifiedImage != null)
					{
						if (fileKey != null)
						{
							image.FileKey = fileKey;
						}

						ImageData.UpdateData(modifiedImage.EntityID, image);
					}
				}
				else
				{
					if (fileKey != null)
					{
						image.FileKey = fileKey;
					}

					ImageData.AddData(image);
				}
			}
		}
		#endregion

		#region Execution Methods
		private void ExecuteMoveImagesCommand(object? element)
		{
			if (element is not MenuItem item) return;

			string? selectedItem = item.Header.ToString() ?? null;

			if (SelectedEntities.Count > 0 && selectedItem != null)
			{
				var match = GetFileKey.Match(selectedItem);
				if (match.Success)
				{
					var targetFileKey = match.Groups[1].Value;
					if (targetFileKey != null)
					{
						foreach (MonoImage notification in SelectedEntities.ToArray())
						{
							notification.FileKey = targetFileKey;
							ImageData.UpdateData(notification.EntityID, notification);
						}
					}
					else
					{
						DialogBox.Show(
							$"The selected file path for {selectedItem} is not set. Please set it before moving images.",
							"File Path Not Set",
							DialogButtonDefaults.OK,
							DialogIcon.Warning);
					}
				}
			}
		}

		private void ExecuteAddImagesCommand(object? _)
		{
			var manipulator = GetDefinedManipulator();
			manipulator.Owner = Owner;
			manipulator.ShowDialog();

			if (manipulator.DidCloseWithSave == false) return;

			SaveData(manipulator, ImageData, "Name");
		}

		private void ExecuteModifyImagesCommand(object? _)
		{
			var folderPath = ApplicationSettings.GetFolderPath(Mode);
			var filePath = ApplicationSettings.GetAllFilePaths(Mode);
			if (folderPath == null ||
				filePath.Count == 0 ||
				SelectedEntities.Count == 0) return;

			var collection = new ObservableCollection<IEnumerable<string?>>(
				SelectedEntities.Select(i => new[]
				{
					i.Name,
					Path.Combine(folderPath, i.Path),
					i.FileKey
				}));

			var manipulator = GetDefinedManipulator(collection);
			manipulator.Owner = Owner;
			manipulator.ShowDialog();

			if (manipulator.DidCloseWithSave == false) return;

			SaveData(manipulator, ImageData, "Name", SelectedEntities);
		}

		private void ExecuteRemoveImagesCommand(object? _)
		{
			if (SelectedEntities.Count == 0) return;

			var selectedItems = SelectedEntities;
			int maxAmount = 10;
			string phrasing = selectedItems.Count > 1 ? "these images" : "this image";
			var imageNames = string.Join("\n", selectedItems
				.Cast<MonoImage>()
				.Take(maxAmount)
				.Select(image => $"- {image.Name}"));

			if (selectedItems.Count > maxAmount)
			{
				imageNames += $"\n- And {selectedItems.Count - maxAmount} more...";
			}

			var question = DialogBox.Show($"Are you sure you wish to remove {phrasing}?\r\n{imageNames}",
				"Confirm Removal",
				600,
				DialogIcon.Question,
				new DialogButton("From Program", DialogBoxResult.Continue),
				new DialogButton("From Script", DialogBoxResult.Retry),
				new DialogButton("From Both", DialogBoxResult.Yes, "ErrorButton"),
				new DialogButton("Cancel", DialogBoxResult.No, "ErrorButton"));

			if (question == DialogBoxResult.Continue || question == DialogBoxResult.Retry || question == DialogBoxResult.Yes)
			{
				try
				{
					List<MonoImage> imagesToRemove = new();
					foreach (MonoImage image in selectedItems)
					{
						imagesToRemove.Add(image);
					}

					foreach (MonoImage image in imagesToRemove)
					{
						if (question == DialogBoxResult.Retry ||
							question == DialogBoxResult.Yes)
						{
							if (ImageData.EntityExistsInScript(image.EntityID))
							{
								ImageData.RemoveEntityFromScript(image.EntityID, false);
							}
						}

						if (question == DialogBoxResult.Continue ||
							question == DialogBoxResult.Yes)
						{
							ImageData.RemoveData(image.EntityID);
						}
					}

					ImageData.SaveData();
				}
				catch (Exception error)
				{
					DialogBox.Show(
						$"Something went wrong when removing characters!\n\n{error}",
						"Failed to Remove Characters",
						DialogButtonDefaults.OK,
						DialogIcon.Error);
				}
			}
		}

		private void ExecuteSaveToScriptCommand(object? _)
		{
			if (SelectedEntities.Count == 0) return;

			var result = DialogBox.Show(
				$"Would you like to update the selected images, or all images?",
				"Save Images To Script",
				DialogIcon.Question,
				new DialogButton("Selected", DialogBoxResult.Continue),
				new DialogButton("All", DialogBoxResult.Yes),
				new DialogButton("Cancel", DialogBoxResult.No, "ErrorButton"));
			if (result == DialogBoxResult.Continue || result == DialogBoxResult.Yes)
			{
				try
				{
					List<(MonoImage, bool)> tags = new();
					int maxImageMerge = 10;
					int currentImageMerge = 0;
					string mergeDialog = "";
					int index = 0;

					foreach (MonoImage row in SelectedEntities)
					{
						var inScript = ImageData.EntityExistsInScript(row.Name, row.FileKey);

						if (inScript)
						{
							if (currentImageMerge < maxImageMerge)
							{
								mergeDialog += $"- {row.Name}\n";
							}
							currentImageMerge++;
						}

						tags.Add((row, inScript));

						if (index == SelectedEntities.Count - 1)
						{
							if (currentImageMerge > maxImageMerge)
							{
								mergeDialog += $"- And {currentImageMerge - maxImageMerge} more...";
							}
						}

						index++;
					}

					if (currentImageMerge > 0)
					{
						if (DialogBox.Show(
							$"Detected images that already exist within the script.\nWould you like to merge existing images?\n(Regardless of the answer, any new images will be added)\n\n{mergeDialog}",
							"Existing Images Detected",
							DialogButtonDefaults.YesNo,
							DialogIcon.Warning) == DialogBoxResult.No)
						{
							Predicate<(MonoImage, bool)> value = tuple => tuple.Item2;
							tags.RemoveAll(value);
						}
					}

					foreach (var (image, inScript) in tags)
					{
						var content = ImageData.ConvertToScriptContent(image);
						if (inScript)
						{
							ImageData.UpdateEntityInScript(image.Name, content);
						}
						else
						{
							ImageData.AddEntityToScript(image.Name, content);
						}
					}

					DialogBox.Show(
						"Images successfully saved to script!",
						"Success",
						DialogButtonDefaults.OK,
						DialogIcon.Information);
				}
				catch (Exception error)
				{
					DialogBox.Show(
						$"Something went wrong when adding images!\n\n{error}",
						"Failed to Add Images",
						DialogButtonDefaults.OK,
						DialogIcon.Error);
				}
			}
		}

		private void ExecuteImportImagesCommand(object? _)
		{
			var imageData = ImageData.SyncData();
			var duplicates = imageData.Keys
				.Where(name => ImageData.ContainsName(name))
				.ToList();

			if (duplicates.Count > 0)
			{
				int selectedCount = duplicates.Count;
				int maxAmount = 10;
				string phrasing = duplicates.Count > 1 ? "multiple images" : "an image";
				string names = string.Join("\n", duplicates.Take(maxAmount).Select(c => $"- {c}"));

				if (selectedCount > maxAmount)
				{
					names += $"\n- And {selectedCount - maxAmount} more...";
				}

				var result = DialogBox.Show(
					$"Found {phrasing} with a similar name that already exist...\nDo you want to merge them?\r\n{names}",
					"Confirm Merge Status",
					DialogButtonDefaults.YesNo,
					DialogIcon.Question);

				if (result == DialogBoxResult.Yes)
				{
					foreach (string image in duplicates)
					{
						int imageId = ImageData.DataMode.Collection.First(c => c.Name == image).EntityID;
						string name = imageData[image].Name;
						string path = imageData[image].Path;
						string fileKey = imageData[image].FileKey;

						MonoImage newImage = new(name, path)
						{
							FileKey = fileKey,
							IsSynced = true
						};

						ImageData.UpdateData(imageId, newImage);

						imageData.Remove(image);
					}
				}
				else
				{
					foreach (string image in duplicates)
					{
						imageData.Remove(image);
					}
				}
			}

			foreach (MonoImage image in imageData.Values)
			{
				image.IsSynced = true;
				ImageData.AddData(image);
			}

			//Converter.UnsetCharacterList();
		}

		private void ExecuteExitCommand(object? _)
		{
			Owner.Close();
		}
		#endregion
	}
}
