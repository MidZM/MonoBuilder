using MonoBuilder.Models.character_management;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MonoBuilder.Models
{
	public partial class Characters : MonoSystem<Character>
    {
		private readonly AssetStore<Character> _characters = new()
		{
			TypeName = "Characters",
			TypeBody = "charactersLabel",
			MasterGuideContent = new(
				"Characters",
				"// CHARACTERS_INSERTION_POINT",
				"// END_CHARACTERS_INSERTION_POINT"),
			ChildGuideContent = new(
				"CharactersLabel",
				"// START_CHARACTER",
				"// END_CHARACTER")
		};

		public override AssetStore<Character> DataMode { get; set; }
		protected override string SaveString { get; } = "characters";

        public ObservableCollection<Character> AllCharacters { get; private set; }

        [GeneratedRegex(@"^([""'`]?)(?<tag>.*)\1.*[:]*[\{] // START_CHARACTER$", RegexOptions.Singleline)]
        private static partial Regex CharacterRegex();
        [GeneratedRegex(@"^([""'`]?)(?<key>.*)\1[:] (?<attr>.*)$", RegexOptions.Singleline)]
        private static partial Regex AttributeRegex();

        public Characters()
        {
			AllCharacters = _characters.Collection;

			AllDataModes = new()
			{
				[_characters.TypeName.ToLower()] = _characters
			};

			DataMode = _characters;

            LoadData(SaveString);
		}

		#region Handle File Data

		#region Data Loading Utilities
		protected override void LoadElement(XElement element, ObservableCollection<Character> collection, object? special = null)
		{
			string? name = (string?)element.Attribute("Name");
			string? tag = (string?)element.Attribute("Tag");
			string? color = (string?)element.Attribute("Color");
			string? path = (string?)element.Attribute("Path");
			string fileKey = (string?)element.Attribute("FileKey") ?? string.Empty;
			_ = bool.TryParse((string?)element.Attribute("IsSynced"), out bool isSynced);

			if (name != null && tag != null)
			{
				Character character = new Character(name, tag, color, path)
				{
					EntityID = collection.Count,
					FileKey = fileKey,
					IsSynced = isSynced
				};

				collection.Add(character);
			}
		}

		protected override (AssetStore<Character>, List<XElement>)[] GetLoadData()
			=> [(_characters, SystemData.Descendants("Character").ToList())];
		#endregion

		#region Data Saving Utilities
		private XElement SaveElement(string type, AssetStore<Character> store)
		{
			return new XElement(store.TypeName,
				store.Collection.Select(element => new XElement(type,
					new XAttribute("EntityID", element.EntityID),
					new XAttribute("Name", element.Name),
					new XAttribute("Tag", element.Tag),
					!string.IsNullOrEmpty(element.FileKey) ? new XAttribute("FileKey", element.FileKey) : null,
					element.Color != null ? new XAttribute("Color", element.Color) : null,
					element.Directory != null ? new XAttribute("Path", element.Directory) : null,
					new XAttribute("IsSynced", element.IsSynced)
					))
				);
		}

		protected override XElement[] GetSaveData()
			=> [SaveElement("Character", DataMode)];
		#endregion

		#endregion

		private Character? FindCharacter(string tag) => AllCharacters.FirstOrDefault(c => c.Tag == tag);

        #region Sync Characters to the Program
        public override Dictionary<string, Character> SyncData(bool duplicatesOnly = false)
        {
            var characters = new Dictionary<string, Character>();
            var files = GetDataFiles();

            if (files.Count == 0)
            {
                DialogBox.Show(
                    "Something has gone wrong while building character data!\n\nNo character files are configured.",
                    "Failed to Compile Character Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception("Bad file data...\nNo character files are configured.");
            }

			var guide = DataMode.MasterGuideContent;
			foreach (var (fileKey, filePath) in files.OrderBy(entry => entry.Key))
            {
                string[] content = File.ReadAllLines(filePath);
                int start = Array.FindIndex(content, line => line.Trim() == guide.GuideStart);
                int end = Array.FindIndex(content, start + 1, line => line.Trim() == guide.GuideEnd);

                if (start <= -1 || end <= -1) continue;

                string[] innerContent = content[(start + 1)..end];
                string tag = string.Empty;

                foreach (string rawData in innerContent)
                {
                    string line = rawData.Trim();
                    var result = CharacterRegex().Match(line);
                    bool isEndCharacter = line.StartsWith("} // END_CHARACTER") || line.StartsWith("}, // END_CHARACTER");

                    if (result.Success)
                    {
                        tag = result.Groups["tag"].Value;
                        try
						{
							bool isDuplicate = ContainsName(tag);
							if (!duplicatesOnly || isDuplicate)
							{
								characters[tag] = new Character(string.Empty, tag)
								{
									FileKey = fileKey
								};
							}
						}
                        catch (Exception error)
                        {
                            tag = string.Empty;
                            DialogBox.Show(
                                $"Attempted to add a character without a proper \"tag.\"\nTags are a required part of building a character. Without it, a character cannot be called in-game, thus it cannot be added here.\n\n{error}",
                                "Bad Character Data",
                                DialogButtonDefaults.OK,
                                DialogIcon.Error);
                        }
                        continue;
                    }

                    if (tag != string.Empty)
                    {
                        if (isEndCharacter)
                        {
                            tag = string.Empty;
                            continue;
                        }

                        if (line.EndsWith(","))
                            line = line[..^1];

                        var attrRes = AttributeRegex().Match(line);
                        string key = attrRes.Groups["key"].Value;
                        string attribute = attrRes.Groups["attr"].Value;

                        if (
                            attribute.StartsWith("\"") && attribute.EndsWith("\"") ||
                            attribute.StartsWith("'") && attribute.EndsWith("'") ||
                            attribute.StartsWith("`") && attribute.EndsWith("`"))
                        {
                            attribute = attribute.Substring(1, attribute.Length - 2);
                        }

                        if (key == "name") characters[tag].Name = attribute;
                        if (key == "color") characters[tag].Color = attribute;
                        if (key == "directory") characters[tag].Directory = attribute;
                        //if (key == "Sprites") characters[tag].Sprites = attribute;
                    }
                }
            }

            return characters;
        }
        #endregion

		#region Handle Character Data in Files

		#region Utility Methods
		private void CreateCharacterDirectory(string tag, string? data)
		{
			var assets = ApplicationSettings?.GetFolderPath("Assets");
			if (assets != null && data != null)
			{
				if (Path.Exists($"{assets}/characters") &&
					!Path.Exists($"{assets}/characters/{data}"))
				{
					if (DialogBox.Show(
						$"It appears this character's directory does not exist.\nWould you like to generate the missing directory?\n\nCharacter: {tag}\nDirectory: {data}",
						"Missing Directory",
						DialogButtonDefaults.YesNo,
						DialogIcon.Question) == DialogBoxResult.Yes)
					{
						Directory.CreateDirectory($"{assets}/characters/{data}");
					}
				}
			}
		}

		private string ProcessDictionaryTag(string key, object value)
		{
			if (value is string stringValue)
			{
				return $"\"{key}\": \"{stringValue.Replace("\"", "\\\"")}\"";
			}

			return $"\"{key}\": {value}";
		}

		private int FindFirstNonWhitespaceIndex(string input)
		{
			for (int i = 0; i < input.Length; i++)
			{
				if (!char.IsWhiteSpace(input[i]))
				{
					return i;
				}
			}
			return -1;
		}
		#endregion

		#region Entity Checking Methods
		public override Dictionary<string, bool> EntitiesExistInScript(HashSet<string> tags)
		{
			var files = GetDataFiles();
			if (files.Count == 0)
			{
				DialogBox.Show("Attempted to check character existence without a proper file path!",
					"No File Path", DialogButtonDefaults.OK, DialogIcon.Warning);
				return [];
			}

			var masterGuide = DataMode.MasterGuideContent;
			var tagsInScript = tags.ToDictionary(tag => tag, _ => false);

			foreach (var (_, filePath) in files)
			{
				string[] lines = File.ReadAllLines(filePath);
				bool inSection = false;

				foreach (string line in lines)
				{
					string trimmed = line.Trim();

					if (trimmed == masterGuide.GuideStart) { inSection = true; continue; }
					if (inSection && trimmed == masterGuide.GuideEnd) break;

					if (inSection)
					{
						var match = CharacterRegex().Match(trimmed);
						if (match.Success && tags.Contains(match.Groups["tag"].Value))
							tagsInScript[match.Groups["tag"].Value] = true;
					}
				}
			}

			return tagsInScript;
		}

        public override bool EntityExistsInScript(string tag, string? fileKey = null)
        {
            var files = GetDataFiles();
            if (files.Count == 0)
            {
                DialogBox.Show(
                    "Attempted to check character existence without a proper file path!",
                    "No File Path",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
                return false;
            }

			var childGuide = DataMode.ChildGuideContent!;
            var filesToCheck = string.IsNullOrWhiteSpace(fileKey)
                ? files.OrderBy(entry => entry.Key)
                : files.Where(entry => entry.Key == ResolveDataFileKey(fileKey));

            foreach (var (_, filePath) in filesToCheck)
            {
                using var reader = new StreamReader(filePath);
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.EndsWith(childGuide.GuideStart, StringComparison.Ordinal) && line.Contains(tag, StringComparison.Ordinal))
                    {
                        string trimmed = line.Trim();
                        var result = CharacterRegex().Match(trimmed);
                        if (result.Success && result.Groups["tag"].Value == tag)
                            return true;
                    }
                }
            }

            return false;
		}

		public override Dictionary<string, bool> EntityContentMatches(List<string> tags, string? fileKey = null)
		{
			var results = tags.ToDictionary(t => t, _ => true);
			var remaining = new HashSet<string>(tags);

			var masterGuide = DataMode.MasterGuideContent;
			var filePath = ResolveDataFilePath(fileKey, out _);
			if (filePath == null)
				return results;

			string[] fileContent = File.ReadAllLines(filePath);
			int start = Array.FindIndex(fileContent, line => line.Trim() == masterGuide.GuideStart);
			int end = Array.FindIndex(fileContent, start + 1, line => line.Trim() == masterGuide.GuideEnd);

			if (start == -1 || end == -1)
				return results;

			string[] innerContent = fileContent[(start + 1)..end];
			string currentTag = string.Empty;
			var parsedAttributes = new Dictionary<string, string>();

			foreach (string rawLine in innerContent)
			{
				if (remaining.Count == 0) break;

				string line = rawLine.Trim();
				var charResult = CharacterRegex().Match(line);
				bool isEndCharacter = line.StartsWith("} // END_CHARACTER") || line.StartsWith("}, // END_CHARACTER");

				if (charResult.Success)
				{
					currentTag = charResult.Groups["tag"].Value;
					parsedAttributes.Clear();
					continue;
				}

				if (currentTag != string.Empty && remaining.Contains(currentTag))
				{
					if (isEndCharacter)
					{
						var character = DataMode.Collection.FirstOrDefault(c => c.Tag == currentTag);
						if (character != null)
						{
							parsedAttributes.TryGetValue("name", out string? scriptName);
							parsedAttributes.TryGetValue("color", out string? scriptColor);
							parsedAttributes.TryGetValue("directory", out string? scriptDirectory);

							results[currentTag] =
								scriptName == character.Name &&
								(scriptColor ?? string.Empty) == (character.Color ?? string.Empty) &&
								(scriptDirectory ?? string.Empty) == (character.Directory ?? string.Empty);
						}

						remaining.Remove(currentTag);
						currentTag = string.Empty;
						parsedAttributes.Clear();
						continue;
					}

					string attrLine = line.EndsWith(",") ? line[..^1] : line;
					var attrRes = AttributeRegex().Match(attrLine);
					if (attrRes.Success)
					{
						string key = attrRes.Groups["key"].Value;
						string attribute = attrRes.Groups["attr"].Value;

						if (attribute.Length >= 2 &&
							(attribute.StartsWith("\"") && attribute.EndsWith("\"") ||
							 attribute.StartsWith("'") && attribute.EndsWith("'") ||
							 attribute.StartsWith("`") && attribute.EndsWith("`")))
						{
							attribute = attribute[1..^1];
						}

						parsedAttributes[key] = attribute;
					}
				}
			}

			return results;
		}
		#endregion

		#region Conversion Methods
		public override Dictionary<string, string?> ConvertToScriptContent(Character character)
        {
			var dict = new Dictionary<string, string?>()
			{
				["tag"] = character.Tag,
				["name"] = character.Name,
				["color"] = character.Color,
				["directory"] = character.Directory
				// Sprites...?
			};

            return dict;
        }

        protected override string? ConvertToScriptContent(Dictionary<string, string?> content)
        {
            if (content.TryGetValue("tag", out string? characterTag))
            {
				var guide = DataMode.ChildGuideContent!;
                string output = $"{AddIndentation()}\"{characterTag}\": {{ {guide.GuideStart}\n";

                var keys = content.Keys.ToList();
                for (int i = 0; i < content.Count; i++)
                {
                    string tag = keys[i];
                    string? data = content[tag];

                    if (i == content.Count - 1)
                    {
                        if (data != null && data != string.Empty)
                        {
                            output += $"{AddIndentation()}{AddIndentation()}{ProcessDictionaryTag(tag, data)},\n";
                        }

                        output += $"{AddIndentation()}}}, {guide.GuideEnd}";
                        break;
                    }

                    if (tag == "tag" || data == null || data == string.Empty) continue;

                    output += $"{AddIndentation()}{AddIndentation()}{ProcessDictionaryTag(tag, data)},\n";
                }

                return output;
            }

            return null;
        }
		#endregion

		#region Engine File Manipulation
		public override void AddEntityToScript(string tag, Dictionary<string, string?> content, string? fileKey = null)
        {
            fileKey ??= FindCharacter(tag)?.FileKey;
            var filePath = ResolveDataFilePath(fileKey, out string resolvedFileKey);
            if (filePath == null)
            {
                DialogBox.Show(
                    $"Something has gone wrong while attempting to add new character data for \"{tag}\"!\n\nPath: {filePath ?? "null"}",
                    "Failed to Update Character Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Bad file data...\n{filePath ?? "null"}");
            }

            string tempPath = Path.GetTempFileName();

            try
            {
				var masterGuide = DataMode.MasterGuideContent;
				var childGuide = DataMode.ChildGuideContent!;
                var lines = File.ReadAllLines(filePath).ToList();
                int lastIndex = lines.FindLastIndex(line => line.EndsWith(childGuide.GuideEnd));

                if (lastIndex != -1)
                {
                    if (lines[lastIndex].Trim().StartsWith("} "))
                    {
                        lines[lastIndex] = lines[lastIndex].Replace("} ", "}, ");
                    }

                    string? stringContent = ConvertToScriptContent(content);
                    if (stringContent != null)
                    {
                        string makeReadable = "\n" + stringContent;

                        lines.Insert(lastIndex + 1, makeReadable);

                        if (content.TryGetValue("directory", out string? dir) && dir != string.Empty)
                        {
                            CreateCharacterDirectory(tag, dir);
                        }

                        File.WriteAllLines(tempPath, lines);
                        FileWatcher.ReplaceFile(tempPath, filePath);

                        var character = FindCharacter(tag);
                        if (character != null)
                        {
                            character.FileKey = resolvedFileKey;
                            character.IsSynced = true;
                            SaveData(SaveString);
                        }
                    }
                    else
                    {
                        DialogBox.Show(
                            $"Something went wrong when converting character data into a script format!\n\nCharacter: {content["tag"]}",
                            "Failed to Convert Character Data",
                            DialogButtonDefaults.OK,
                            DialogIcon.Error);
                    }
                }
                else
                {
                    lastIndex = lines.FindLastIndex(line => line.Contains(masterGuide.GuideStart));

                    if (lastIndex != -1)
                    {
                        string? stringContent = ConvertToScriptContent(content);
                        if (stringContent != null)
                        {
                            string makeReadable = "\n" + stringContent;

                            lines.Insert(lastIndex, makeReadable);

                            if (content.TryGetValue("directory", out string? dir) && dir != string.Empty)
                            {
                                CreateCharacterDirectory(tag, dir);
                            }

                            File.WriteAllLines(tempPath, lines);
                            FileWatcher.ReplaceFile(tempPath, filePath);

                            var character = FindCharacter(tag);
                            if (character != null)
                            {
                                character.FileKey = resolvedFileKey;
                                character.IsSynced = true;
                                SaveData(SaveString);
                            }
                        }
                        else
                        {
                            DialogBox.Show(
                                $"Something went wrong when convert character data into a script format!\n\nCharacter: {content["tag"]}",
                                "Failed to Convert Character Data",
                                DialogButtonDefaults.OK,
                                DialogIcon.Error);
                        }
                    }
                    else
                    {
                        DialogBox.Show(
                            $"Missing Character formatting!\nSyncing your characters reuiqres labels to be formatted with with opening and ending tags.\n\n" +
							$"Example:\n{masterGuide.GuideStart}\n" +
							$"{AddIndentation()}\"YourTag\": {{ {childGuide.GuideStart}\n" +
							$"{AddIndentation()}{AddIndentation()}// Your content\n" +
							$"{AddIndentation()}}} {childGuide.GuideEnd}\n{masterGuide.GuideEnd}",
                            "Missing Characters");
                    }
                }
            }
            catch (Exception error)
            {
                DialogBox.Show(
                    $"Something went wrong while adding new character data for \"{content["tag"]}\"!\nNo need to panic, the process was cut off before anything saved.\n\n{error}",
                    "Failed to Append Character Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Bad merge data...\n{error}");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        public override bool RemoveEntityFromScript(int characterId, bool shouldSave = true)
        {
			return RemoveEntitiesFromScript([characterId], shouldSave);
      //      Character? character = AllCharacters.FirstOrDefault(c => c.EntityID == characterId);
      //      if (character != null)
      //      {
      //          var filePath = ResolveDataFilePath(character, out _);
      //          if (filePath == null)
      //          {
      //              DialogBox.Show(
      //                  $"Something has gone wrong while attempting to remove character data for \"{character.Tag}\"!\n\nPath: {filePath ?? "null"}",
      //                  "Failed to Remove Character Data",
      //                  DialogButtonDefaults.OK,
      //                  DialogIcon.Error);
      //              throw new Exception($"Bad file data...\n{filePath ?? "null"}");
      //          }

      //          string tempPath = Path.GetTempFileName();

      //          try
      //          {
      //              using (var reader = new StreamReader(filePath))
      //              using (var writer = new StreamWriter(tempPath))
      //              {
						//var masterGuider = DataMode.MasterGuideContent;
      //                  string? line;
      //                  string? lastLine = null;
      //                  bool isRemoving = false;

      //                  while ((line = reader.ReadLine()) != null)
      //                  {
      //                      bool isStart = line.EndsWith(masterGuider.GuideStart) && line.Contains(character.Tag);
      //                      bool isEnd = isRemoving && line.EndsWith(masterGuider.GuideEnd);

      //                      if (isStart)
      //                      {
      //                          string trimmed = line.Trim();
      //                          var result = CharacterRegex().Match(trimmed);
      //                          if (result.Success && result.Groups["tag"].Value == character.Tag)
      //                          {
      //                              isRemoving = true;
      //                              if (string.IsNullOrEmpty(lastLine))
      //                                  lastLine = null;
      //                          }
      //                      }

      //                      if (lastLine != null)
      //                      {
      //                          writer.WriteLine(lastLine);
      //                      }

      //                      if (isEnd)
      //                      {
      //                          isRemoving = false;
      //                          lastLine = null;
      //                          continue;
      //                      }

      //                      lastLine = isRemoving ? null : line;
      //                  }

      //                  if (lastLine != null)
      //                      writer.WriteLine(lastLine);

      //                  character.IsSynced = false;
      //                  if (shouldSave)
      //                  {
      //                      SaveData();
      //                  }
      //              }

      //              FileWatcher.ReplaceFile(tempPath, filePath);
      //              return true;
      //          }
      //          catch (Exception error)
      //          {
      //              DialogBox.Show(
      //                  $"Something went wrong while removing character data for \"{character.Tag}\"!\nNo need to panic, the process was cut off before anything saved.\n\n{error}",
      //                  "Failed to Remove Character Data",
      //                  DialogButtonDefaults.OK,
      //                  DialogIcon.Error);
      //              throw new Exception($"Bad removal data..\n{error}");
      //          }
      //          finally
      //          {
      //              if (File.Exists(tempPath))
      //                  File.Delete(tempPath);
      //          }
      //      }
      //      else
      //      {
      //          DialogBox.Show(
      //              "Missing or Invalid \"Character\"...",
      //              "Bad Data",
      //              DialogButtonDefaults.OK,
      //              DialogIcon.Error);
      //          throw new Exception($"Invalid input data...");
      //      }
		}

		public override bool RemoveEntitiesFromScript(int[] characterIds, bool shouldSave = true)
		{
			if (characterIds.Length == 0)
				return false;

			var idsToRemove = new HashSet<int>(characterIds);
			var charactersToRemove = new List<Character>();

			foreach (int id in idsToRemove)
			{
				if (CheckData(id) is Character notif)
				{
					notif.IsSynced = false;
					charactersToRemove.Add(notif);
				}
			}

			if (charactersToRemove.Count == 0)
				return false;

			var charactersByFile = charactersToRemove
				.GroupBy(notif => ResolveDataFilePath(notif, out _))
				.Where(g => g.Key != null)
				.ToDictionary(g => g.Key!, g => g.ToList());

			try
			{
				foreach (var (filePath, notificationsInFile) in charactersByFile)
					RemoveEntityFromSingleFile(filePath, notificationsInFile);

				if (shouldSave) SaveData(SaveString);
				return true;
			}
			catch (Exception ex)
			{
				DialogBox.Show($"Failed to remove notification data from script.\n\n{ex}",
					"Remove Failed", DialogButtonDefaults.OK, DialogIcon.Error);
				throw;
			}
		}

		protected override void RemoveEntityFromSingleFile(string filePath, List<Character> charactersToRemove)
		{
			string tempPath = Path.GetTempFileName();
			var namesToRemove = new HashSet<string>(charactersToRemove.Select(n => n.Tag));

			try
			{
				using (var reader = new StreamReader(filePath))
				using (var writer = new StreamWriter(tempPath))
				{
					var masterGuide = DataMode.MasterGuideContent;
					var childGuide = DataMode.ChildGuideContent!;
					bool inSection = false;
					bool isRemoving = false;
					string? line;

					while ((line = reader.ReadLine()) != null)
					{
						string trimmed = line.Trim();

						if (!inSection && trimmed == masterGuide.GuideStart)
						{
							inSection = true;
							writer.WriteLine(line);
							continue;
						}

						if (inSection && trimmed == masterGuide.GuideEnd)
						{
							inSection = false;
							writer.WriteLine(line);
							continue;
						}

						if (inSection)
						{
							var match = CharacterRegex().Match(trimmed);
							bool isBlockEnd = trimmed.EndsWith(childGuide.GuideEnd);

							if (!isRemoving && match.Success && namesToRemove.Contains(match.Groups["tag"].Value))
							{
								isRemoving = true;
								continue;
							}

							if (isRemoving && isBlockEnd)
							{
								isRemoving = false;
								continue;
							}

							if (isRemoving)
								continue;
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

		public override bool UpdateEntityInScript(string tag, Dictionary<string, string?> content, string? fileKey = null)
        {
            fileKey ??= FindCharacter(tag)?.FileKey;
            var filePath = ResolveDataFilePath(fileKey, out string resolvedFileKey);
            if (filePath == null)
            {
                DialogBox.Show(
                    $"Something has gone wrong while attempting to update character data for \"{tag}\"!\n\nPath: {filePath ?? "null"}",
                    "Failed to Update Character Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Bad file data...\n{filePath ?? "null"}");
            }

            string tempPath = Path.GetTempFileName();
            var tagsToCheck = content.Keys.ToList();

            var indentationType = ApplicationSettings?.GetIndentationType() ?? "Spaces";
            var indentationAmount = ApplicationSettings?.GetIndentationAmount() ?? 4;
            string indentUnit = indentationType == "Tabs" ? "\t" : new string(' ', indentationAmount);

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var writer = new StreamWriter(tempPath))
                {
					var masterGuider = DataMode.MasterGuideContent;
					var childGuide = DataMode.ChildGuideContent!;
                    string? line;
                    bool startUpdating = false;
                    var writtenKeys = new HashSet<string>();

                    if (content.TryGetValue("directory", out string? dir) && dir != string.Empty)
                    {
                        CreateCharacterDirectory(tag, dir);
                    }

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.EndsWith(childGuide.GuideStart, StringComparison.Ordinal) && line.Contains(tag, StringComparison.Ordinal))
                        {
                            string trimmed = line.Trim();
                            var result = CharacterRegex().Match(trimmed);
                            if (result.Success && result.Groups["tag"].Value == tag)
                            {
                                startUpdating = true;
                                writer.WriteLine(line);
                                continue;
                            }
                        }

                        if (startUpdating && line.EndsWith(childGuide.GuideEnd))
                        {
                            var blockIndent = line.Substring(0, FindFirstNonWhitespaceIndex(line));
                            foreach (string newKey in tagsToCheck)
                            {
                                if (writtenKeys.Contains(newKey) || newKey == "tag") continue;
                                string? newValue = content[newKey];
                                if (newValue == null || newValue == string.Empty) continue;
                                string newLine = blockIndent + indentUnit + ProcessDictionaryTag(newKey, newValue) + ",";
                                writer.WriteLine(newLine);
                            }

                            startUpdating = false;
                            writer.WriteLine(line);
                            continue;
                        }

                        if (startUpdating)
                        {
                            var whitespaceIndex = FindFirstNonWhitespaceIndex(line);
                            if (whitespaceIndex < 0)
                            {
                                writer.WriteLine(line);
                                continue;
                            }

                            var trimmed = line.Trim();

                            var strippedTrimmed = trimmed.Replace("\"", "").Replace("'", "").Replace("`", "");

                            string? matchedKey = null;
                            foreach (string objectTag in tagsToCheck)
                            {
                                if (strippedTrimmed.StartsWith(objectTag + ":") || strippedTrimmed.StartsWith(objectTag + " :"))
                                {
                                    matchedKey = objectTag;
                                    break;
                                }
                            }

                            if (matchedKey == null)
                            {
                                if (!trimmed.EndsWith("{") &&
                                    !trimmed.EndsWith("[") &&
                                    !line.EndsWith(","))
                                {
                                    line += ",";
                                }

                                writer.WriteLine(line);
                                continue;
                            }

                            string? updatedValue = content[matchedKey];
                            if (updatedValue == null)
                            {
                                continue;
                            }

                            var whitespace = line.Substring(0, whitespaceIndex);
                            var processedLine = ProcessDictionaryTag(matchedKey, updatedValue);
                            writer.WriteLine(whitespace + processedLine + ",");
                            writtenKeys.Add(matchedKey);
                            continue;
                        }

                        writer.WriteLine(line);
                    }
                }

                FileWatcher.ReplaceFile(tempPath, filePath);

                var character = FindCharacter(tag);
                if (character != null)
                {
                    character.FileKey = resolvedFileKey;
                    character.IsSynced = true;
                    SaveData(SaveString);
                }

                return true;
            }
            catch (Exception error)
            {
                DialogBox.Show(
                    $"Something went wrong while updating character data for \"{tag}\"!\nNo need to panic, the process was cut off before anything saved.\n\n{error}",
                    "Failed to Update Character Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Bad update data..\n{error}");
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
		#endregion

		#endregion
	}
}
