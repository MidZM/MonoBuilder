using MonoBuilder.Views.ViewUtils;
using MonoBuilder.Models.generics.enums;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MonoBuilder.Models.image_management
{
    public class MonoImages : MonoSystem<MonoImage>
    {
        private readonly AssetStore<MonoImage> _images = new()
		{
			TypeName = "Images",
			MasterGuideContent = new(
				"Images",
				"// IMAGES_INSERTION_POINT",
				"// END_IMAGES_INSERTION_POINT")
		};

        private readonly AssetStore<MonoImage> _scenes = new()
		{
			TypeName = "Scenes",
			MasterGuideContent = new(
				"Scenes",
				"// SCENES_INSERTION_POINT",
				"// END_SCENES_INSERTION_POINT")
		};
        private readonly AssetStore<MonoImage> _gallery = new()
		{
			TypeName = "Gallery",
			MasterGuideContent = new(
				"Gallery",
				"// GALLERY_INSERTION_POINT",
				"// END_GALLERY_INSERTION_POINT")
		};

        public override AssetStore<MonoImage> DataMode { get; set; }
		protected override string SaveString { get; } = "images";


		private static Regex ImageRegex { get; set; } = new(@"^([""'`]?)(?<name>.*)\1.*[:]*[""'`](?<content>.*)\1[,]?$", RegexOptions.Compiled);
		
        public MonoImages()
        {
			AllDataModes = new()
			{
				[_images.TypeName.ToLower()] = _images,
				[_scenes.TypeName.ToLower()] = _scenes,
				[_gallery.TypeName.ToLower()] = _gallery
			};

            DataMode = _images;

            LoadData(SaveString);
        }

		#region Handle File Data

		#region Data Loading Utilities
		protected override void LoadElement(XElement image, ObservableCollection<MonoImage> collectionType, object? special = null)
        {
            string? name = (string?)image.Attribute("Name");
            string? path = (string?)image.Attribute("Path");
            string? fileKey = (string?)image.Attribute("FileKey") ?? string.Empty;
            _ = bool.TryParse((string?)image.Attribute("IsSynced"), out bool isSynced);

            if (name != null && path != null)
            {
                MonoImage newImage = new(name, path)
                {
                    EntityID = collectionType.Count,
                    FileKey = fileKey,
                    IsSynced = isSynced
                };

                collectionType.Add(newImage);
            }
        }

		protected override (AssetStore<MonoImage>, List<XElement>)[] GetLoadData()
			=> [(_images, SystemData.Descendants("Image").ToList()),
				(_scenes, SystemData.Descendants("Scene").ToList()),
				(_gallery, SystemData.Descendants("GalleryImage").ToList())];
		#endregion

		#region Data Saving Utilities
		private XElement SaveElement(string type, AssetStore<MonoImage> store)
		{
			return new XElement(store.TypeName,
				store.Collection.Select(element => new XElement(type,
					new XAttribute("EntityID", element.EntityID),
					new XAttribute("Name", element.Name),
					new XAttribute("Path", element.Path),
					!string.IsNullOrEmpty(element.FileKey) ? new XAttribute("FileKey", element.FileKey) : null,
					new XAttribute("IsSynced", element.IsSynced)
					))
				);
		}

		protected override XElement[] GetSaveData()
			=> [SaveElement("Image", (AssetStore<MonoImage>)GetDataMode("images")!),
				SaveElement("Scene", (AssetStore<MonoImage>)GetDataMode("scenes")!),
				SaveElement("GalleryImage", (AssetStore<MonoImage>)GetDataMode("gallery")!)];
		#endregion

        #endregion

        #region Sync images to the program
        public override Dictionary<string, MonoImage> SyncData(bool duplicatesOnly = false)
        {
            var images = new Dictionary<string, MonoImage>(StringComparer.Ordinal);
            var files = GetDataFiles();
            if (files.Count == 0)
            {
                DialogBox.Show(
                    "Something has gone wrong while building image data!\n\nNo image files are configured.",
                    "Failed to Compile Image Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception("Bad file data...\nNo image files are configured.");
            }

			var guide = DataMode.MasterGuideContent;
            foreach (var (fileKey, filePath) in files.OrderBy(entry => entry.Key))
            {
                string[] content = File.ReadAllLines(filePath);
                int start = Array.FindIndex(content, line => line.Trim() == guide.GuideStart);
                int end = Array.FindIndex(content, start + 1, line => line.Trim() == guide.GuideEnd);

                if (start <= -1 || end <= -1) continue;

                foreach (string rawData in content[(start + 1)..end])
                {
                    string line = rawData.Trim();
                    var result = ImageRegex.Match(line);
                    if (!result.Success) continue;

                    string name = result.Groups["name"].Value;
                    string path = result.Groups["content"].Value;

                    if (images.ContainsKey(name)) continue;

                    try
                    {
                        bool isDuplicate = ContainsName(name);

                        if (duplicatesOnly)
                        {
                            if (isDuplicate)
                            {
                                images[name] = new MonoImage(name, path) { FileKey = fileKey };
                            }
                        }
                        else
                        {
                            images[name] = new MonoImage(name, path) { FileKey = fileKey };
                        }
                    }
                    catch (Exception error)
                    {
                        DialogBox.Show(
                            $"Attempted to add image data without proper formatting.\n\nData: {line}\n\n{error}",
                            "Bad Image Data",
                            DialogButtonDefaults.OK,
                            DialogIcon.Error);
                    }
                }
            }

            return images;
        }
		#endregion

		#region Handle Image Data in File

		#region Entity Checking Methods
		public override Dictionary<string, bool> EntitiesExistInScript(HashSet<string> names)
        {
            var files = GetDataFiles();
            if (files.Count == 0)
            {
                DialogBox.Show("Attempted to check image existence without a proper file path!",
                    "No File Path", DialogButtonDefaults.OK, DialogIcon.Warning);
                return [];
            }

			var guide = DataMode.MasterGuideContent;
            Dictionary<string, bool> namesInScript = names.ToDictionary(
                name => name,
                name => false);

            foreach (var (_, filePath) in files)
            {
                string[] lines = File.ReadAllLines(filePath);

                bool inImageSection = false;
                foreach (string line in lines)
                {
                    string trimmed = line.Trim();

                    if (trimmed == guide.GuideStart)
                    {
                        inImageSection = true;
                        continue;
                    }
                    if (inImageSection && trimmed == guide.GuideEnd)
                        break;

                    if (inImageSection)
                    {
                        var match = ImageRegex.Match(trimmed);
                        if (match.Success)
                        {
                            var name = match.Groups["name"].Value;

                            if (names.Contains(name))
                                namesInScript[name] = true;
                        }
                    }
                }
            }

            return namesInScript;
        }

        public override bool EntityExistsInScript(string name, string? fileKey = null)
        {
            var files = GetDataFiles();
            if (files.Count == 0)
            {
                DialogBox.Show("Attempted to check image existence without a proper file path!",
                    "No File Path", DialogButtonDefaults.OK, DialogIcon.Warning);
                return false;
            }

			var guide = DataMode.MasterGuideContent;
            var filesToCheck = string.IsNullOrWhiteSpace(fileKey)
                ? files.OrderBy(entry => entry.Key)
                : files.Where(entry => entry.Key == ResolveDataFileKey(fileKey));

            foreach (var (_, filePath) in filesToCheck)
            {
                string[] lines = File.ReadAllLines(filePath);

                bool inImagesSection = false;
                foreach (string line in lines)
                {
                    string trimmed = line.Trim();

                    if (trimmed == guide.GuideStart)
                    {
                        inImagesSection = true;
                        continue;
                    }
                    if (inImagesSection && trimmed == guide.GuideEnd)
                        break;

                    if (inImagesSection)
                    {
                        var match = ImageRegex.Match(trimmed);
                        if (match.Success && match.Groups["name"].Value == name)
                            return true;
                    }
                }
            }
            return false;
		}

		public override Dictionary<string, bool> EntityContentMatches(List<string> names, string? fileKey = null)
		{
			var results = names.ToDictionary(n => n, _ => false);

			var filePath = ResolveDataFilePath(fileKey, out _);
			if (filePath == null || names.Count == 0)
				return results;

			var guide = DataMode.MasterGuideContent;
			string[] fileContent = File.ReadAllLines(filePath);
			int start = Array.FindIndex(fileContent, line => line.Trim() == guide.GuideStart);
			int end = Array.FindIndex(fileContent, start + 1, line => line.Trim() == guide.GuideEnd);

			if (start == -1 || end == -1)
				return results;

			var remaining = new HashSet<string>(names);   // Still useful for early exit

			string[] innerContent = fileContent[(start + 1)..end];

			foreach (string rawLine in innerContent)
			{
				if (remaining.Count == 0)
					break;

				string line = rawLine.Trim();
				var match = ImageRegex.Match(line);
				if (!match.Success)
					continue;

				string imageName = match.Groups["name"].Value;
				if (!remaining.Remove(imageName))   // Remove returns true only if it existed
					continue;

				string filePathValue = match.Groups["content"].Value;

				// Fast dictionary lookup instead of FirstOrDefault
				if (CheckData(imageName) is MonoImage image)
				{
					results[imageName] = image.Path == filePathValue;
				}
			}

			return results;
		}
		#endregion

		#region Conversion Methods
		public override Dictionary<string, string?> ConvertToScriptContent(MonoImage image)
        {
            return new Dictionary<string, string?>
            {
                ["name"] = image.Name,
                ["path"] = image.Path
            };
        }

        protected override string? ConvertToScriptContent(Dictionary<string, string?> content)
        {
            if (content.TryGetValue("name", out string? name) &&
                content.TryGetValue("path", out string? path) &&
                !string.IsNullOrEmpty(name) &&
                !string.IsNullOrEmpty(path))
            {
                return $"{AddIndentation()}\"{name}\": \"{path}\"";
            }

            return null;
        }
		#endregion

		#region Engine File Manipulation
		public override void AddEntityToScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null)
        {
			(string filePath, string resolvedFileKey) = CheckKeyResolutionInFile(fileKeyParam, name);
            string tempPath = Path.GetTempFileName();

            try
            {
				var guide = DataMode.MasterGuideContent;
				var lines = File.ReadAllLines(filePath).ToList();

				(int startIndex, int endIndex) = GetPositionIndexInFile(lines, guide);

                for (int i = endIndex - 1; i > startIndex; i--)
                {
                    string trimmed = lines[i].TrimEnd();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        if (ImageRegex.IsMatch(trimmed) && !trimmed.EndsWith(",", StringComparison.Ordinal))
                        {
                            lines[i] += ",";
                        }
                        break;
                    }
                }

                if (ConvertToScriptContent(content) is string newLine)
                {
                    lines.Insert(endIndex, newLine);

                    File.WriteAllLines(tempPath, lines);
                    FileWatcher.ReplaceFile(tempPath, filePath);

                    if (CheckData(name) is MonoImage image)
                    {
                        image.FileKey = resolvedFileKey;
                        image.IsSynced = true;
                        SaveData(SaveString);
                    }
                }
                else
                {
                    DialogBox.Show($"Failed to convert image data to script format for \"{name}\".",
                        "Conversion Error", DialogButtonDefaults.OK, DialogIcon.Error);
                }
            }
            catch (Exception ex)
            {
                DialogBox.Show($"Failed to add image \"{name}\" to script.\n\n{ex}",
                    "Add Failed", DialogButtonDefaults.OK, DialogIcon.Error);
                throw;
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        public override bool RemoveEntityFromScript(int imageId, bool shouldSave = true)
        {
            return RemoveEntitiesFromScript(new[] { imageId }, shouldSave);
        }

        public override bool RemoveEntitiesFromScript(int[] imageIds, bool shouldSave = true)
        {
            if (imageIds == null || imageIds.Length == 0)
                return false;

            var idsToRemove = new HashSet<int>(imageIds);
            var imagesToRemove = new List<MonoImage>();

            foreach (int id in idsToRemove)
            {
                if (CheckData(id) is MonoImage img)
                {
                    img.IsSynced = false;
                    imagesToRemove.Add(img);
                }
            }

            if (imagesToRemove.Count == 0)
                return false;

            var imagesByFile = imagesToRemove
                .GroupBy(img => ResolveDataFilePath(img, out _))
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!, g => g.ToList());

            try
            {
                foreach (var (filePath, imagesInFile) in imagesByFile)
                {
                    RemoveEntityFromSingleFile(filePath, imagesInFile);
                }

                if (shouldSave) SaveData(SaveString);

                return true;
            }
            catch (Exception ex)
            {
                DialogBox.Show($"Failed to remove image data from script.\n\n{ex}",
                    "Remove Failed", DialogButtonDefaults.OK, DialogIcon.Error);
                throw;
            }
        }

        protected override void RemoveEntityFromSingleFile(string filePath, List<MonoImage> imagesToRemove)
        {
            string tempPath = Path.GetTempFileName();
            var namesToRemove = new HashSet<string>(imagesToRemove.Select(i => i.Name));

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var writer = new StreamWriter(tempPath))
                {
					var guide = DataMode.MasterGuideContent;
					bool inImageSection = false;
                    string? line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string trimmed = line.Trim();

                        if (trimmed == guide.GuideStart)
                        {
                            inImageSection = true;
                            writer.WriteLine(line);
                            continue;
                        }

                        if (inImageSection && trimmed == guide.GuideEnd)
                        {
                            inImageSection = false;
                            writer.WriteLine(line);
                            continue;
                        }

                        if (inImageSection)
                        {
                            var match = ImageRegex.Match(trimmed);
                            if (match.Success && namesToRemove.Contains(match.Groups["name"].Value))
                            {
                                continue; // skip this line
                            }
                        }

                        writer.WriteLine(line);
                    }
                }

                FileWatcher.ReplaceFile(tempPath, filePath);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        public override bool UpdateEntityInScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null)
        {
			(string filePath, string resolvedFileKey) = CheckKeyResolutionInFile(fileKeyParam, name);
            string tempPath = Path.GetTempFileName();
            bool didUpdate = false;

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var writer = new StreamWriter(tempPath))
				{
					var guide = DataMode.MasterGuideContent;
					string? line;
                    bool inImagesSection = false;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string trimmed = line.Trim();

                        if (!inImagesSection && trimmed == guide.GuideStart)
                        {
                            inImagesSection = true;
                            writer.WriteLine(line);
                            continue;
                        }

                        if (inImagesSection && trimmed == guide.GuideEnd)
                        {
                            inImagesSection = false;
                            writer.WriteLine(line);
                            continue;
                        }

                        if (inImagesSection)
                        {
                            var match = ImageRegex.Match(trimmed);
                            if (match.Success && match.Groups["name"].Value == name)
                            {
                                if (ConvertToScriptContent(content) is string newLine)
                                {
                                    newLine += ',';
                                    writer.WriteLine(newLine);
                                    didUpdate = true;
                                    continue;
                                }
                            }
                        }

                        writer.WriteLine(line);
                    }

                    if (!didUpdate)
                        return false;
                }


                FileWatcher.ReplaceFile(tempPath, filePath);

                if (CheckData(name) is MonoImage image)
                {
                    image.FileKey = resolvedFileKey;
                    image.IsSynced = true;
                    SaveData(SaveString);
                }

                return true;
            }
            catch (Exception ex)
            {
                DialogBox.Show($"Failed to update image \"{name}\" in script.\n\n{ex}",
                    "Update Failed", DialogButtonDefaults.OK, DialogIcon.Error);
                throw;
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }
		#endregion

		#region Synchronicity Checking
		public override bool CheckSynchronicity(bool showMessage = true)
        {
            if (ApplicationSettings == null)
                return false;

			bool hasChanged = false;
			string[] modes = ["images", "scenes", "gallery"];
			foreach (var mode in modes)
			{
				SetDataMode(mode);
				var files = GetDataFiles();
				if (files.Count == 0)
					return false;

				foreach (var (fileKey, _) in files)
				{
					// Get only the relevant synced images for this file - using fast lookup
					var namesInFile = DataMode.Collection
						.Where(i => i.IsSynced &&
									(string.IsNullOrEmpty(i.FileKey)
										? fileKey == ResolveDataFileKey()
										: i.FileKey == fileKey))
						.Select(i => i.Name)
						.ToList();

					if (namesInFile.Count == 0)
						continue;

					var contentMatches = EntityContentMatches(namesInFile, fileKey);

					// Check for any mismatch
					foreach (string name in namesInFile)
					{
						if (contentMatches.TryGetValue(name, out bool matches) && !matches)
						{
							hasChanged = true;
							break;
						}
					}

					if (hasChanged)
						break;
				}

				if (hasChanged && showMessage)
				{
					DialogBox.Show(
						"It looks like something changed from the last time the program was opened.\n" +
						"Images that have been modified will appear as such when opening the image builder.",
						"Changes Have Been Made",
						DialogButtonDefaults.OK,
						DialogIcon.Warning);
				}
			}

            return hasChanged;
        }
		#endregion

		#endregion
	}
}
