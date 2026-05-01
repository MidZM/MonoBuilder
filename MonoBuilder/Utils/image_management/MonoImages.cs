using MonoBuilder.Screens.ScreenUtils;
using MonoBuilder.Utils.character_management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MonoBuilder.Utils.image_management
{
    public class MonoImages
    {
        private class AssetStore
        {
            public ObservableCollection<MonoImage> Collection { get; } = new();
            public Dictionary<string, MonoImage> ByName { get; } = new(StringComparer.Ordinal);
            public Dictionary<int, MonoImage> ById { get; } = new();
            public int NextId { get; set; } = 0;
            public required string TypeName { get; init; }
        }

        private readonly AssetStore _images = new() { TypeName = "Images" };
        private readonly AssetStore _scenes = new() { TypeName = "Scenes" };
        private readonly AssetStore _gallery = new() { TypeName = "Gallery" };

        private AssetStore DataMode { get; set; }

        public ObservableCollection<MonoImage> AllImages { get; private set; }
        public ObservableCollection<MonoImage> AllScenes { get; private set; }
        public ObservableCollection<MonoImage> AllGalleryImages { get; private set; }

        private AppSettings? ApplicationSettings { get; set; }
        private XDocument Data { get; set; } = new();

        private static Regex ImageRegex { get; set; } = new(@"^([""'`]?)(?<name>.*)\1.*[:]*[""'`](?<content>.*)\1[,]?$", RegexOptions.Compiled);
        //private static Regex AttributeRegex { get; set; } = new(@"^");
        private Dictionary<string, List<string>> ContentGuides { get; } = new()
        {
            {
                "Images",
                [
                    "// IMAGES_INSERTION_POINT",
                    "// END_IMAGES_INSERTION_POINT"
                ]
            },
            {
                "Scenes",
                [
                    "// SCENES_INSERTION_POINT",
                    "// END_SCENES_INSERTION_POINT"
                ]
            },
            {
                "Gallery",
                [
                    "// GALLERY_INSERTION_POINT",
                    "// END_GALLERY_INSERTION_POINT"
                ]
            }
        };

        public MonoImages()
        {
            AllImages = _images.Collection;
            AllScenes = _scenes.Collection;
            AllGalleryImages = _gallery.Collection;

            DataMode = _images;

            LoadImages();
        }

        #region Mode Management
        public void SetDataMode(string mode)
        {
            switch (mode)
            {
                case "images": DataMode = _images; break;
                case "scenes": DataMode = _scenes; break;
                case "gallery": DataMode = _gallery; break;
            }
        }

        private AssetStore GetDataMode(string mode)
        {
            return mode switch
            {
                "images" => _images,
                "scenes" => _scenes,
                "gallery" => _gallery,
                _ => _images
            };
        }
        #endregion

        #region Handle File Data
        private void LoadImage(XElement image, ObservableCollection<MonoImage> collectionType)
        {
            string? name = (string?)image.Attribute("Name");
            string? path = (string?)image.Attribute("Path");
            string? fileKey = (string?)image.Attribute("FileKey") ?? string.Empty;
            _ = bool.TryParse((string?)image.Attribute("IsSynced"), out bool isSynced);

            if (name != null &&
                path != null)
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

        public void LoadImages()
        {
            try
            {
                if (!Directory.Exists("data"))
                {
                    Directory.CreateDirectory("data");
                }

                if (!File.Exists("data/images.xml"))
                {
                    SaveImages();
                    return;
                }

                Data = XDocument.Load("data/images.xml");
                _images.Collection.Clear();
                _scenes.Collection.Clear();
                _gallery.Collection.Clear();

                List<XElement> imageList = Data.Descendants("Image").ToList();
                List<XElement> sceneList = Data.Descendants("Scene").ToList();
                List<XElement> galleryList = Data.Descendants("GalleryImage").ToList();

                foreach (XElement image in imageList)
                {
                    LoadImage(image, _images.Collection);
                }

                foreach(XElement scene in sceneList)
                {
                    LoadImage(scene, _scenes.Collection);
                }

                foreach (XElement image in galleryList)
                {
                    LoadImage(image, _gallery.Collection);
                }

                if (_images.Collection.Any())
                    _images.NextId = _images.Collection.Max(i => i.EntityID) + 1;

                if (_scenes.Collection.Any())
                    _scenes.NextId = _scenes.Collection.Max(s => s.EntityID) + 1;

                if (_gallery.Collection.Any())
                    _gallery.NextId = _gallery.Collection.Max(i => i.EntityID) + 1;
                    

                RebuildLookups("images");
                RebuildLookups("scenes");
                RebuildLookups("gallery");
            }
            catch (FileNotFoundException error)
            {
                DialogBox.Show($"Save Data Reading Failure!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error);
            }
            catch (XmlException error)
            {
                DialogBox.Show($"Images File Reading Failure!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error);
            }
            catch (Exception error)
            {
                DialogBox.Show($"Something went wrong!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error);
            }
        }

        public void SaveImages()
        {
            Data = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Root",
                    new XElement(GetDataMode("images").TypeName,
                        _images.Collection.Select(i => new XElement("Image",
                            new XAttribute("ImageID", i.EntityID),
                            new XAttribute("Name", i.Name),
                            new XAttribute("Path", i.Path),
                            !string.IsNullOrEmpty(i.FileKey) ? new XAttribute("FileKey", i.FileKey) : null,
                            new XAttribute("IsSynced", i.IsSynced)
                        ))
                    ),
                    new XElement(GetDataMode("scenes").TypeName,
                        _scenes.Collection.Select(s => new XElement("Scene",
                            new XAttribute("SceneID", s.EntityID),
                            new XAttribute("Name", s.Name),
                            new XAttribute("Path", s.Path),
                            !string.IsNullOrEmpty(s.FileKey) ? new XAttribute("FileKey", s.FileKey) : null,
                            new XAttribute("IsSynced", s.IsSynced)
                        ))
                    ),
                    new XElement(GetDataMode("gallery").TypeName,
                        _gallery.Collection.Select(s => new XElement("GalleryImage",
                            new XAttribute("SceneID", s.EntityID),
                            new XAttribute("Name", s.Name),
                            new XAttribute("Path", s.Path),
                            !string.IsNullOrEmpty(s.FileKey) ? new XAttribute("FileKey", s.FileKey) : null,
                            new XAttribute("IsSynced", s.IsSynced)
                        ))
                    )
                )
            );

            Data.Save("data/images.xml");
        }

        public void LoadSettings(AppSettings settings)
        {
            ApplicationSettings = settings;
        }

        private Dictionary<string, string> GetImageFiles()
        {
            if (ApplicationSettings == null)
                return [];

            var files = ApplicationSettings.GetAllFilePaths(DataMode.TypeName);
            if (files.Count == 0)
            {
                var legacyPath = ApplicationSettings.GetFilePath(DataMode.TypeName);
                if (!string.IsNullOrEmpty(legacyPath))
                    files[DataMode.TypeName] = legacyPath;
            }

            return files;
        }

        private string? ResolveImageFileKey(string? fileKey = null)
        {
            var files = GetImageFiles();
            if (files.Count == 0)
                return null;

            if (!string.IsNullOrWhiteSpace(fileKey) && files.ContainsKey(fileKey))
                return fileKey;

            if (!string.IsNullOrWhiteSpace(fileKey))
            {
                var prefixed = files.Keys
                    .OrderBy(key => key)
                    .FirstOrDefault(key => key.StartsWith(fileKey + ":", StringComparison.Ordinal));

                if (prefixed != null)
                    return prefixed;
            }

            return files.Keys.OrderBy(key => key).FirstOrDefault();
        }

        private string? ResolveImageFilePath(string? fileKey, out string resolvedFileKey)
        {
            resolvedFileKey = ResolveImageFileKey(fileKey) ?? string.Empty;
            if (string.IsNullOrEmpty(resolvedFileKey))
                return null;

            return ApplicationSettings?.GetAllFilePaths().TryGetValue(resolvedFileKey, out string? filePath) == true
                ? filePath
                : null;
        }

        private string? ResolveImageFilePath(MonoImage? image, out string resolvedFileKey)
        {
            return ResolveImageFilePath(image?.FileKey, out resolvedFileKey);
        }
        #endregion

        private MonoImage? FindImage(string name) => DataMode.Collection.FirstOrDefault(i => i.Name == name);
        private MonoImage? FindImage(int index) => DataMode.Collection.FirstOrDefault(i => i.EntityID == index);
                        
        #region Sync images to the program
        public Dictionary<string, MonoImage> SyncImages(bool duplicatesOnly = false)
        {
            var images = new Dictionary<string, MonoImage>(StringComparer.Ordinal);
            var files = GetImageFiles();
            if (files.Count == 0)
            {
                DialogBox.Show(
                    "Something has gone wrong while building image data!\n\nNo image files are configured.",
                    "Failed to Compile Image Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception("Bad file data...\nNo image files are configured.");
            }

            var type = DataMode.TypeName;
            foreach (var (fileKey, filePath) in files.OrderBy(entry => entry.Key))
            {
                string[] content = File.ReadAllLines(filePath);
                int start = Array.FindIndex(content, line => line.Trim() == ContentGuides[type][0]);
                int end = Array.FindIndex(content, start + 1, line => line.Trim() == ContentGuides[type][1]);

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

        #region Handle image data in program
        public bool CheckForDuplicates(MonoImage imageToCheck, bool checkScene = false)
        {
            foreach (MonoImage image in DataMode.Collection)
            {
                if (image.Name == imageToCheck.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public List<string> CheckForDuplicates(HashSet<MonoImage> imagesToCheck, bool checkScene = false)
        {
            List<string> list = new();
            foreach (MonoImage image in DataMode.Collection)
            {
                if (imagesToCheck.Contains(image))
                {
                    list.Add(image.Name);
                }
            }

            return list;
        }

        public void RebuildLookups()
        {
            DataMode.ByName.Clear();
            DataMode.ById.Clear();

            foreach (var img in DataMode.Collection)
            {
                DataMode.ByName[img.Name] = img;
                DataMode.ById[img.EntityID] = img;
            }
        }

        private void RebuildLookups(string type)
        {
            var dataMode = GetDataMode(type);

            dataMode.ByName.Clear();
            dataMode.ById.Clear();

            foreach (var img in dataMode.Collection)
            {
                dataMode.ByName[img.Name] = img;
                dataMode.ById[img.EntityID] = img;
            }
        }

        public void AddImage(MonoImage image, bool shouldSave = true, bool isScene = false)
        {
            if (string.IsNullOrEmpty(image.FileKey))
                image.FileKey = ResolveImageFileKey(DataMode.TypeName) ?? string.Empty;

            image.EntityID = DataMode.NextId++;
            DataMode.Collection.Add(image);

            DataMode.ByName[image.Name] = image;
            DataMode.ById[image.EntityID] = image;

            if (shouldSave) SaveImages();
        }

        public bool RemoveImage(int imageId, bool shouldSave = true, bool isScene = false)
        {
            if (!DataMode.ById.TryGetValue(imageId, out var image))
                return false;

            DataMode.ById.Remove(imageId);
            DataMode.ByName.Remove(image.Name);
            DataMode.Collection.RemoveAt(DataMode.Collection.IndexOf(image));
            RebuildLookups();

            if (shouldSave) SaveImages();
            return true;
        }

        public void RemoveImages(int[] imageIds, bool shouldSave = true, bool isScene = false)
        {
            if (imageIds == null || imageIds.Length == 0)
                return;

            var idsToRemove = new HashSet<int>(imageIds);

            foreach (int id in idsToRemove)
            {
                if (DataMode.ById.TryGetValue(id, out var img))
                {
                    DataMode.ByName.Remove(img.Name);
                    DataMode.ById.Remove(id);
                }
            }

            DataMode.Collection.RemoveByIds(idsToRemove);

            if (shouldSave) SaveImages();
        }

        public bool ContainsName(string name) => DataMode.ByName.ContainsKey(name);
        public MonoImage? CheckImage(string imageName) => DataMode.ByName.TryGetValue(imageName, out var img) ? img : null;
        public MonoImage? CheckImage(int imageId) => DataMode.ById.TryGetValue(imageId, out var img) ? img : null;

        public MonoImage UpdateImage(int imageId, MonoImage newData, bool shouldSave = true, bool isScene = false)
        {
            if (!DataMode.ById.TryGetValue(imageId, out var existing))
                throw new KeyNotFoundException($"Image ID {imageId} not found");

            existing.Name = newData.Name;
            existing.Path = newData.Path;
            existing.FileKey = newData.FileKey ?? existing.FileKey;
            existing.IsSynced = newData.IsSynced;

            if (existing.Name != newData.Name)
            {
                DataMode.ByName.Remove(existing.Name);
                DataMode.ByName[newData.Name] = existing;
            }

            if (shouldSave) SaveImages();

            return existing;
        }
        #endregion

        #region Handle image data in file
        public Dictionary<string, bool> ImagesExistsInScript(HashSet<string> names)
        {
            var files = GetImageFiles();
            if (files.Count == 0)
            {
                DialogBox.Show("Attempted to check image existence without a proper file path!",
                    "No File Path", DialogButtonDefaults.OK, DialogIcon.Warning);
                return [];
            }

            var type = DataMode.TypeName;
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

                    if (trimmed == ContentGuides[type][0])
                    {
                        inImageSection = true;
                        continue;
                    }
                    if (inImageSection && trimmed == ContentGuides[type][1])
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

        public bool ImageExistsInScript(string name, string? fileKey = null)
        {
            var files = GetImageFiles();
            if (files.Count == 0)
            {
                DialogBox.Show("Attempted to check image existence without a proper file path!",
                    "No File Path", DialogButtonDefaults.OK, DialogIcon.Warning);
                return false;
            }

            var type = DataMode.TypeName;
            var filesToCheck = string.IsNullOrWhiteSpace(fileKey)
                ? files.OrderBy(entry => entry.Key)
                : files.Where(entry => entry.Key == ResolveImageFileKey(fileKey));

            foreach (var (_, filePath) in filesToCheck)
            {
                string[] lines = File.ReadAllLines(filePath);

                bool inImagesSection = false;
                foreach (string line in lines)
                {
                    string trimmed = line.Trim();

                    if (trimmed == ContentGuides[type][0])
                    {
                        inImagesSection = true;
                        continue;
                    }
                    if (inImagesSection && trimmed == ContentGuides[type][1])
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

        public bool ImageExistsInScript(int imageId)
        {
            if (CheckImage(imageId) is not MonoImage image)
                return false;

            return ImageExistsInScript(image.Name, image.FileKey);
        }

        private string AddIndentation()
        {
            return ApplicationSettings?.GetIndentationType() switch
            {
                "Tab" => "\t",
                "Spaces" => new string(' ', ApplicationSettings.GetIndentationAmount()),
                _ => new string(' ', 4)
            };
        }

        public Dictionary<string, string?> ConvertToScriptContent(MonoImage image)
        {
            return new Dictionary<string, string?>
            {
                ["name"] = image.Name,
                ["path"] = image.Path
            };
        }

        private string? ConvertToScriptContent(Dictionary<string, string?> content)
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

        public void AddImageToScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null)
        {
            string? fileKey = fileKeyParam ?? CheckImage(name)?.FileKey;
            var filePath = ResolveImageFilePath(fileKey, out string resolvedFileKey);

            if (filePath == null)
            {
                DialogBox.Show($"Failed to resolve file path for image \"{name}\".",
                    "Bad File Path", DialogButtonDefaults.OK, DialogIcon.Error);
                throw new Exception($"Bad file data for {name}");
            }

            string tempPath = Path.GetTempFileName();

            try
            {
                var type = DataMode.TypeName;
                var lines = File.ReadAllLines(filePath).ToList();

                int startIndex = lines.FindIndex(l => l.Trim() == ContentGuides[type][0]);
                int endIndex = lines.FindIndex(startIndex + 1, l => l.Trim() == ContentGuides[type][1]);

                if (startIndex == -1 || endIndex == -1)
                {
                    DialogBox.Show("Missing proper image section markers in script file.\n\n" +
                        "You need opening and closing tags like:\n" +
                        $"{ContentGuides[type][0]}\n    \"Example\": \"path.jpg\",\n{ContentGuides[type][1]}",
                        "Missing Image Section", DialogButtonDefaults.OK, DialogIcon.Error);
                    return;
                }

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

                    if (CheckImage(name) is MonoImage image)
                    {
                        image.FileKey = resolvedFileKey;
                        image.IsSynced = true;
                        SaveImages();
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

        public bool RemoveImageFromScript(int imageId, bool shouldSave = true)
        {
            return RemoveImagesFromScript(new[] { imageId }, shouldSave);
        }

        public bool RemoveImagesFromScript(int[] imageIds, bool shouldSave = true)
        {
            if (imageIds == null || imageIds.Length == 0)
                return false;

            var idsToRemove = new HashSet<int>(imageIds);
            var imagesToRemove = new List<MonoImage>();

            foreach (int id in idsToRemove)
            {
                if (CheckImage(id) is MonoImage img)
                {
                    img.IsSynced = false;
                    imagesToRemove.Add(img);
                }
            }

            if (imagesToRemove.Count == 0)
                return false;

            var imagesByFile = imagesToRemove
                .GroupBy(img => ResolveImageFilePath(img, out _))
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!, g => g.ToList());

            try
            {
                foreach (var (filePath, imagesInFile) in imagesByFile)
                {
                    RemoveImagesFromSingleFile(filePath, imagesInFile);
                }

                if (shouldSave) SaveImages();

                return true;
            }
            catch (Exception ex)
            {
                DialogBox.Show($"Failed to remove image data from script.\n\n{ex}",
                    "Remove Failed", DialogButtonDefaults.OK, DialogIcon.Error);
                throw;
            }
        }

        private void RemoveImagesFromSingleFile(string filePath, List<MonoImage> imagesToRemove)
        {
            string tempPath = Path.GetTempFileName();
            var namesToRemove = new HashSet<string>(imagesToRemove.Select(i => i.Name));

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var writer = new StreamWriter(tempPath))
                {
                    var type = DataMode.TypeName;
                    bool inImageSection = false;
                    string? line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string trimmed = line.Trim();

                        if (trimmed == ContentGuides[type][0])
                        {
                            inImageSection = true;
                            writer.WriteLine(line);
                            continue;
                        }

                        if (inImageSection && trimmed == ContentGuides[type][1])
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

        public bool UpdateImageInScript(string name, Dictionary<string, string?> content, string? fileKeyParam = null)
        {
            string? fileKey = fileKeyParam ?? CheckImage(name)?.FileKey;
            var filePath = ResolveImageFilePath(fileKey, out string resolvedFileKey);

            if (filePath == null)
            {
                DialogBox.Show($"Failed to resolve file path for image \"{name}\".",
                    "Bad File Path", DialogButtonDefaults.OK, DialogIcon.Error);
                throw new Exception($"Bad file data for {name}");
            }

            string tempPath = Path.GetTempFileName();
            bool didUpdate = false;

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var writer = new StreamWriter(tempPath))
                {
                    var type = DataMode.TypeName;
                    string? line;
                    bool inImagesSection = false;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string trimmed = line.Trim();

                        if (!inImagesSection && trimmed == ContentGuides[type][0])
                        {
                            inImagesSection = true;
                            writer.WriteLine(line);
                            continue;
                        }

                        if (inImagesSection && trimmed == ContentGuides[type][1])
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

                if (CheckImage(name) is MonoImage image)
                {
                    image.FileKey = resolvedFileKey;
                    image.IsSynced = true;
                    SaveImages();
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

        public Dictionary<string, bool> ImageContentMatches(List<string> names, string? fileKey = null)
        {
            var results = names.ToDictionary(n => n, _ => false);

            var filePath = ResolveImageFilePath(fileKey, out _);
            if (filePath == null || names.Count == 0)
                return results;

            var type = DataMode.TypeName;
            string[] fileContent = File.ReadAllLines(filePath);
            int start = Array.FindIndex(fileContent, line => line.Trim() == ContentGuides[type][0]);
            int end = Array.FindIndex(fileContent, start + 1, line => line.Trim() == ContentGuides[type][1]);

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
                if (CheckImage(imageName) is MonoImage image)
                {
                    results[imageName] = image.Path == filePathValue;
                }
            }

            return results;
        }

        public bool CheckSynchronicity(bool showMessage = true)
        {
            if (ApplicationSettings == null)
                return false;

            var files = GetImageFiles();
            if (files.Count == 0)
                return false;

            bool imagesHaveChanged = false;

            foreach (var (fileKey, _) in files)
            {
                // Get only the relevant synced images for this file - using fast lookup
                var namesInFile = DataMode.Collection
                    .Where(i => i.IsSynced &&
                                (string.IsNullOrEmpty(i.FileKey)
                                    ? fileKey == ResolveImageFileKey()
                                    : i.FileKey == fileKey))
                    .Select(i => i.Name)
                    .ToList();

                if (namesInFile.Count == 0)
                    continue;

                var contentMatches = ImageContentMatches(namesInFile, fileKey);

                // Check for any mismatch
                foreach (string name in namesInFile)
                {
                    if (contentMatches.TryGetValue(name, out bool matches) && !matches)
                    {
                        imagesHaveChanged = true;
                        break;
                    }
                }

                if (imagesHaveChanged)
                    break;
            }

            if (imagesHaveChanged && showMessage)
            {
                DialogBox.Show(
                    "It looks like something changed from the last time the program was opened.\n" +
                    "Images that have been modified will appear as such when opening the image builder.",
                    "Changes Have Been Made",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
            }

            return imagesHaveChanged;
        }
        #endregion
    }
}
