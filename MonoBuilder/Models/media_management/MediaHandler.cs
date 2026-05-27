using MonoBuilder.Models.generics.enums;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MonoBuilder.Models.media_management
{
	public class MediaHandler : MonoSystem<Media>
	{
		private readonly AssetStore<Media> _music = new()
		{
			TypeName = "Music",
			MasterGuideContent = new(
				"Music",
				"// MUSIC_INSERTION_POINT",
				"// END_MUSIC_INSERTION_POINT")
		};

		private readonly AssetStore<Media> _sounds = new()
		{
			TypeName = "Sounds",
			MasterGuideContent = new(
				"Sounds",
				"// SOUNDS_INSERTION_POINT",
				"// END_SOUNDS_INSERTION_POINT")
		};

		private readonly AssetStore<Media> _voices = new()
		{
			TypeName = "Voices",
			MasterGuideContent = new(
				"Voices",
				"// VOICES_INSERTION_POINT",
				"// END_VOICES_INSERTION_POINT")
		};

		private readonly AssetStore<Media> _videos = new()
		{
			TypeName = "Videos",
			MasterGuideContent = new(
				"Videos",
				"// VIDEOS_INSERTION_POINT",
				"// END_VIDEOS_INSERTION_POINT")
		};

		public override AssetStore<Media> DataMode { get; set; }
		protected override string SaveString { get; } = "media";

		private static Regex MediaRegex { get; set; } = new(@"^([""'`]?)(?<name>.*)\1.*[:]*[""'`](?<content>.*)\1[,]?$", RegexOptions.Compiled);

		public MediaHandler()
		{
			AllDataModes = new()
			{
				[_music.TypeName.ToLower()] = _music,
				[_sounds.TypeName.ToLower()] = _sounds,
				[_voices.TypeName.ToLower()] = _voices,
				[_videos.TypeName.ToLower()] = _videos
			};

			DataMode = _music;

			LoadData(SaveString);
		}

		#region Handle File Data

		#region Data Loading Utilities
		protected override void LoadElement(XElement media, ObservableCollection<Media> collectionType, object? special = null)
		{
			string? name = (string?)media.Attribute("Name");
			string? path = (string?)media.Attribute("Path");
			string? fileKey = (string?)media.Attribute("FileKey") ?? string.Empty;
			_ = bool.TryParse((string?)media.Attribute("IsSynced"), out bool isSynced);

			if (name != null && path != null)
			{
				Media newMedia = new(name, path)
				{
					EntityID = collectionType.Count,
					FileKey = fileKey,
					IsSynced = isSynced
				};

				collectionType.Add(newMedia);
			}
		}

		protected override (AssetStore<Media>, List<XElement>)[] GetLoadData()
			=> [(_music, SystemData.Descendants("Song").ToList()),
				(_sounds, SystemData.Descendants("Sound").ToList()),
				(_voices, SystemData.Descendants("Voice").ToList()),
				(_videos, SystemData.Descendants("Video").ToList())];
		#endregion

		#region Data Saving Utilities
		private XElement SaveElement(string type, AssetStore<Media> store)
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
			=> [SaveElement("Song", (AssetStore<Media>)GetDataMode("music")!),
				SaveElement("Sound", (AssetStore<Media>)GetDataMode("sounds")!),
				SaveElement("Voice", (AssetStore<Media>)GetDataMode("voices")!),
				SaveElement("Video", (AssetStore<Media>)GetDataMode("videos")!)];
		#endregion

		#endregion

		#region Sync Media to the Program
		public override Dictionary<string, Media> SyncData(bool duplicatesOnly = false)
		{
			var media = new Dictionary<string, Media>(StringComparer.Ordinal);
			var files = GetDataFiles();
			if (files.Count == 0)
			{
				DialogBox.Show(
					"Something has gone wrong while building media data!\n\nNo media files are configured.",
					"Failed to Compile Media Data",
					DialogButtonDefaults.OK,
					DialogIcon.Error);
				throw new Exception("Bad file data...\nNo media files are configured.");
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
					var result = MediaRegex.Match(line);
					if (!result.Success) continue;

					string name = result.Groups["name"].Value;
					string path = result.Groups["content"].Value;

					if (media.ContainsKey(name)) continue;

					try
					{
						bool isDuplicate = ContainsName(name);

						if (duplicatesOnly)
						{
							if (isDuplicate)
							{
								media[name] = new Media(name, path) { FileKey = fileKey };
							}
						}
						else
						{
							media[name] = new Media(name, path) { FileKey = fileKey };
						}
					}
					catch (Exception error)
					{
						DialogBox.Show(
							$"Attempted to add media data without proper formatting.\n\nData: {line}\n\n{error}",
							"Bad Image Data",
							DialogButtonDefaults.OK,
							DialogIcon.Error);
					}
				}
			}

			return media;
		}
		#endregion

		#region Handle Image Data in File

		#region Entity Checking Methods
		public override Dictionary<string, bool> EntitiesExistInScript(HashSet<string> names)
		{
			var files = GetDataFiles();
			if (files.Count == 0)
			{
				DialogBox.Show("Attempted to check media existence without a proper file path!",
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

				bool inMediaSection = false;
				foreach (string line in lines)
				{
					string trimmed = line.Trim();

					if (trimmed == guide.GuideStart)
					{
						inMediaSection = true;
						continue;
					}
					if (inMediaSection && trimmed == guide.GuideEnd)
						break;

					if (inMediaSection)
					{
						var match = MediaRegex.Match(trimmed);
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
				DialogBox.Show("Attempted to check media existence without a proper file path!",
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

				bool inMediaSection = false;
				foreach (string line in lines)
				{
					string trimmed = line.Trim();

					if (trimmed == guide.GuideStart)
					{
						inMediaSection = true;
						continue;
					}
					if (inMediaSection && trimmed == guide.GuideEnd)
						break;

					if (inMediaSection)
					{
						var match = MediaRegex.Match(trimmed);
						if (match.Success && match.Groups["name"].Value == name)
							return true;
					}
				}
			}
			return false;
		}

		public override Dictionary<string, bool> EntityContentMatches(List<string> names, string? fileKey = null)
		{
			var results = names.ToDictionary(n => n, _ => true);

			var filePath = ResolveDataFilePath(fileKey, out _);
			if (filePath == null || names.Count == 0)
				return results;

			var guide = DataMode.MasterGuideContent;
			string[] fileContent = File.ReadAllLines(filePath);
			int start = Array.FindIndex(fileContent, line => line.Trim() == guide.GuideStart);
			int end = Array.FindIndex(fileContent, start + 1, line => line.Trim() == guide.GuideEnd);

			if (start == -1 || end == -1)
				return results;

			var remaining = new HashSet<string>(names);

			string[] innerContent = fileContent[(start + 1)..end];

			foreach (string rawLine in innerContent)
			{
				if (remaining.Count == 0)
					break;

				string line = rawLine.Trim();
				var match = MediaRegex.Match(line);
				if (!match.Success)
					continue;

				string meidaName = match.Groups["name"].Value;
				if (!remaining.Remove(meidaName))
					continue;

				string filePathValue = match.Groups["content"].Value;

				if (CheckData(meidaName) is Media media)
				{
					results[meidaName] = media.Path == filePathValue;
				}
			}

			return results;
		}
		#endregion

		#region Conversion Methods
		public override Dictionary<string, string?> ConvertToScriptContent(Media type)
		{
			return new Dictionary<string, string?>
			{
				["name"] = type.Name,
				["path"] = type.Path
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
						if (MediaRegex.IsMatch(trimmed) && !trimmed.EndsWith(",", StringComparison.Ordinal))
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

					if (CheckData(name) is Media image)
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

		public override bool RemoveEntityFromScript(int entityId, bool shouldSave = true)
		{
			return RemoveEntitiesFromScript([entityId], shouldSave);
		}

		public override bool RemoveEntitiesFromScript(int[] entityIds, bool shouldSave = true)
		{
			if (entityIds.Length == 0)
				return false;

			var idsToRemove = new HashSet<int>(entityIds);
			var mediaToRemove = new List<Media>();

			foreach (int id in idsToRemove)
			{
				if (CheckData(id) is Media img)
				{
					img.IsSynced = false;
					mediaToRemove.Add(img);
				}
			}

			if (mediaToRemove.Count == 0)
				return false;

			var mediaByFile = mediaToRemove
				.GroupBy(media => ResolveDataFilePath(media, out _))
				.Where(g => g.Key != null)
				.ToDictionary(g => g.Key!, g => g.ToList());

			try
			{
				foreach (var (filePath, mediaInFile) in mediaByFile)
				{
					RemoveEntityFromSingleFile(filePath, mediaInFile);
				}

				if (shouldSave) SaveData(SaveString);

				return true;
			}
			catch (Exception ex)
			{
				DialogBox.Show($"Failed to remove media data from script.\n\n{ex}",
					"Remove Failed", DialogButtonDefaults.OK, DialogIcon.Error);
				throw;
			}
		}

		protected override void RemoveEntityFromSingleFile(string filePath, List<Media> entityToRemove)
		{
			string tempPath = Path.GetTempFileName();
			var namesToRemove = new HashSet<string>(entityToRemove.Select(i => i.Name));

			try
			{
				using (var reader = new StreamReader(filePath))
				using (var writer = new StreamWriter(tempPath))
				{
					var guide = DataMode.MasterGuideContent;
					bool inMediaSection = false;
					string? line;

					while ((line = reader.ReadLine()) != null)
					{
						string trimmed = line.Trim();

						if (trimmed == guide.GuideStart)
						{
							inMediaSection = true;
							writer.WriteLine(line);
							continue;
						}

						if (inMediaSection && trimmed == guide.GuideEnd)
						{
							inMediaSection = false;
							writer.WriteLine(line);
							continue;
						}

						if (inMediaSection)
						{
							var match = MediaRegex.Match(trimmed);
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
					bool inMediaSection = false;

					while ((line = reader.ReadLine()) != null)
					{
						string trimmed = line.Trim();

						if (!inMediaSection && trimmed == guide.GuideStart)
						{
							inMediaSection = true;
							writer.WriteLine(line);
							continue;
						}

						if (inMediaSection && trimmed == guide.GuideEnd)
						{
							inMediaSection = false;
							writer.WriteLine(line);
							continue;
						}

						if (inMediaSection)
						{
							var match = MediaRegex.Match(trimmed);
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

				if (CheckData(name) is Media media)
				{
					media.FileKey = resolvedFileKey;
					media.IsSynced = true;
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

		#endregion
	}
}
