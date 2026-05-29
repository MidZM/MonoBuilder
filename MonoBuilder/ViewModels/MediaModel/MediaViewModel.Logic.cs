using MonoBuilder.Commands;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.media_management;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace MonoBuilder.ViewModels.MediaModel
{
	public partial class MediaViewModel
	{
		public event Action<string>? MediaActionRequested;

		public RelayCommand PlayCommand { get; }
		public RelayCommand PauseCommand { get; }
		public RelayCommand StopCommand { get; }

		public RelayCommand ScrubberDragStart { get; }
		public RelayCommand ScrubberDragComplete { get; }

		#region Utility Commands

		#region Content Manipulation
		protected override string GetContentUniqueField()
			=> "Name";
		protected override List<string> GetContentNames()
			=> ["Name", "Path", "File"];
		protected override List<ContentBoxType> GetContentBoxTypes()
			=> [ContentBoxType.RequiredTextBox,
				ContentBoxType.RequiredFileButton,
				ContentBoxType.FileBox];
		protected override (string, string)[] GetPlaceholderValues()
			=> [("Name", "Enter Media Name"),
				("Path", "Enter Path to Media Assets")];
		protected override ObservableCollection<IEnumerable<string?>> GenerateContentFieldData(string folderPath)
			=> new(
				SelectedEntities.Select(m => new[]
				{
					m.Name,
					Path.Combine(folderPath, m.Path),
					m.FileKey
				}));

		protected override void ApplyContentChanges(ContentManipulator window, List<FrameworkElement> control, int index, Media[]? entities = null)
		{
			var elements = ExtractElements(control);

			elements.TryGetValue("Name", out string? name);
			elements.TryGetValue("Path", out string? path);
			elements.TryGetValue("File", out string? fileKey);

			if (name != null && path != null)
			{
				path = GetRelativePath(path, ApplicationSettings.GetFolderPath(Mode)!);
				var image = new Media(name, path);
				if (window.ModifyingContent?.Count > 0)
				{
					window.ModifyingContent[index].TryGetValue("Name", out string? nameValue);
					var modifiedImage = entities?.FirstOrDefault(c => c.Name == nameValue);

					if (modifiedImage != null)
					{
						if (fileKey != null)
						{
							image.FileKey = fileKey;
						}

						DataController.UpdateData(modifiedImage.EntityID, image);
					}
				}
				else
				{
					if (fileKey != null)
					{
						image.FileKey = fileKey;
					}

					DataController.AddData(image);
				}
			}
		}

		protected override void RunDataImportSetup(string entity, Dictionary<string, Media> entityData)
		{
			int imageId = DataController.DataMode.Collection.First(c => c.Name == entity).EntityID;
			string name = entityData[entity].Name;
			string path = entityData[entity].Path;
			string fileKey = entityData[entity].FileKey;

			Media newImage = new(name, path)
			{
				EntityID = imageId,
				FileKey = fileKey,
				IsSynced = true
			};

			DataController.UpdateData(imageId, newImage);

			entityData.Remove(entity);
		}
		#endregion

		#endregion

		#region Execution Commands
		private void ExecutePlayCommand(object? _) => MediaActionRequested?.Invoke("Play");
		private void ExecutePauseCommand(object? _) => MediaActionRequested?.Invoke("Pause");
		private void ExecuteStopCommand(object? _) => MediaActionRequested?.Invoke("Stop");
		private void ExecuteScrubberDragStarted(object? _)
		{
			MediaActionRequested?.Invoke("DragStart");
			IsUserDragging = true;
		}
		private void ExecuteScrubberDragCompleted(object? _)
		{
			IsUserDragging = false;
			MediaActionRequested?.Invoke("DragComplete");
		}
		#endregion
	}
}
