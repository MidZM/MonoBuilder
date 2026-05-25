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
		#region Utility Commands
		protected override string GetContentUniqueField()
			=> "Name";

		protected override List<string> GetContentNames()
			=> ["Name", "Path", "File"];
		protected override List<ContentBoxType> GetContentBoxTypes()
			=> [ContentBoxType.RequiredTextBox,
				ContentBoxType.RequiredFileButton,
				ContentBoxType.FileBox];
		protected override (string, string)[] GetPlaceholderValues()
			=> [("Name", "Enter Image Name"),
				("Path", "Path to Image Assets")];
		protected override ObservableCollection<IEnumerable<string?>> GenerateContentFieldData(string folderPath)
			=> new(
				SelectedEntities.Select(i => new[]
				{
					i.Name,
					Path.Combine(folderPath, i.Path),
					i.FileKey
				}));

		protected override void ApplyContentChanges(ContentManipulator window, List<FrameworkElement> control, int index, MonoImage[]? entities = null)
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

		protected override void RunDataImportSetup(string image, Dictionary<string, MonoImage> imageData)
		{
			int imageId = DataController.DataMode.Collection.First(c => c.Name == image).EntityID;
			string name = imageData[image].Name;
			string path = imageData[image].Path;
			string fileKey = imageData[image].FileKey;

			MonoImage newImage = new(name, path)
			{
				EntityID = imageId,
				FileKey = fileKey,
				IsSynced = true
			};

			DataController.UpdateData(imageId, newImage);

			imageData.Remove(image);
		}
		#endregion
	}
}
