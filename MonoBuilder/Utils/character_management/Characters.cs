using MonoBuilder.Screens.ScreenUtils;
using MonoBuilder.Utils.character_management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MonoBuilder.Utils
{
    public class CharacterStructure
    {
        public string Name { get; set; } = "";
        public string Tag { get; set; } = "";
        public string? Color { get; set; }
        public string? Directory { get; set; }
        public object? Sprites { get; set; }
        public string FileKey { get; set; } = "";
        public bool IsSynced { get; set; } = false;
    }

    public partial class Characters
    {
        //public ObservableCollection<Character> AllCharactersSource { get; set; } = new ObservableCollection<Character>();
        public ObservableCollection<Character> AllCharacters { get; set; } = new ObservableCollection<Character>();
        private AppSettings? ApplicationSettings { get; set; }
        private XDocument data { get; set; } = new XDocument();

        [GeneratedRegex(@"^([""'`]?)(?<tag>.*)\1.*[:]*[\{] // START_CHARACTER$", RegexOptions.Singleline)]
        private static partial Regex CharacterRegex();
        [GeneratedRegex(@"^([""'`]?)(?<key>.*)\1[:] (?<attr>.*)$", RegexOptions.Singleline)]
        private static partial Regex AttributeRegex();
        private Dictionary<string, List<string>> ContentGuides { get; } = new Dictionary<string, List<string>>
        {
            {
                "Characters",
                [
                    "// CHARACTERS_INSERTION_POINT",
                    "// END_CHARACTERS_INSERTION_POINT"
                ]
            },
            {
                "CharactersLabel",
                [
                    "// START_CHARACTER",
                    "// END_CHARACTER"
                ]
            }
        };

        private readonly string Type = "Characters";
        private readonly string TypeLabel = "CharactersLabel";

        public Characters()
        {
            LoadCharacters();
        }

        public void LoadCharacters()
        {
            try
            {
                if (!Directory.Exists("data"))
                {
                    Directory.CreateDirectory("data");
                }

                if (!File.Exists("data/characters.xml"))
                {
                    SaveCharacters();
                    return;
                }

                data = XDocument.Load("data/characters.xml");
                AllCharacters.Clear();

                List<XElement> list = data.Descendants("Character").ToList();
                foreach (XElement character in list)
                {
                    string? name = (string?)character.Attribute("Name");
                    string? tag = (string?)character.Attribute("Tag");
                    string? color = (string?)character.Attribute("Color");
                    string? path = (string?)character.Attribute("Path");
                    string fileKey = (string?)character.Attribute("FileKey") ?? string.Empty;
                    _ = bool.TryParse((string?)character.Attribute("IsSynced"), out bool isSynced);

                    if (
                        name != null &&
                        tag != null
                    )
                    {
                        Normal normalCharcter = new Normal(name, tag, color ?? string.Empty, path ?? string.Empty)
                        {
                            EntityID = AllCharacters.Count,
                            FileKey = fileKey,
                            IsSynced = isSynced
                        };

                        AllCharacters.Add(normalCharcter);
                    }
                }
            }
			catch (FileNotFoundException error)
			{
				DialogBox.Show($"Save Data Reading Failure!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error);
			}
			catch (XmlException error)
			{
				DialogBox.Show($"Characters File Reading Failure!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error);
			}
			catch (Exception error)
			{
				DialogBox.Show($"Something went wrong!\r\n{error}", "Error", DialogButtonDefaults.OK, DialogIcon.Error);
			}
        }

        public void SaveCharacters()
        {
            data = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Root",
                    new XElement(Type,
                        AllCharacters.Select(c => new XElement("Character",
                            new XAttribute("CharacterID", c.EntityID),
                            new XAttribute("Name", c.Name),
                            new XAttribute("Tag", c.Tag),
                            !string.IsNullOrEmpty(c.FileKey) ? new XAttribute("FileKey", c.FileKey) : null,
                            c.Color != null ? new XAttribute("Color", c.Color) : null,
                            c.Directory != null ? new XAttribute("Path", c.Directory) : null,
                            new XAttribute("IsSynced", c.IsSynced)
                        ))
                    )
                )
            );

            data.Save("data/characters.xml");
        }

        public void LoadSettings(AppSettings settings)
        {
            ApplicationSettings = settings;
        }

        private Dictionary<string, string> GetCharacterFiles()
        {
            if (ApplicationSettings == null)
                return [];

            var files = ApplicationSettings.GetAllFilePaths(Type);
            if (files.Count == 0)
            {
                var legacyPath = ApplicationSettings.GetFilePath(Type);
                if (!string.IsNullOrEmpty(legacyPath))
                    files[Type] = legacyPath;
            }

            return files;
        }

        private string? ResolveCharacterFileKey(string? fileKey = null)
        {
            var files = GetCharacterFiles();
            if (!string.IsNullOrWhiteSpace(fileKey) && files.ContainsKey(fileKey))
                return fileKey;

            if (!string.IsNullOrWhiteSpace(fileKey))
            {
                var prefixedMatch = files.Keys.OrderBy(key => key).FirstOrDefault(key => key.StartsWith(fileKey + ":", StringComparison.Ordinal));
                if (!string.IsNullOrEmpty(prefixedMatch))
                    return prefixedMatch;
            }

            return files.Keys.OrderBy(key => key).FirstOrDefault();
        }

        private string? ResolveCharacterFilePath(string? fileKey, out string resolvedFileKey)
        {
            resolvedFileKey = ResolveCharacterFileKey(fileKey) ?? string.Empty;
            if (string.IsNullOrEmpty(resolvedFileKey))
                return null;

            return ApplicationSettings?.GetAllFilePaths().TryGetValue(resolvedFileKey, out string? filePath) == true
                ? filePath
                : null;
        }

        private string? ResolveCharacterFilePath(Character? character, out string resolvedFileKey)
        {
            return ResolveCharacterFilePath(character?.FileKey, out resolvedFileKey);
        }

        private Character? FindCharacter(string tag) => AllCharacters.FirstOrDefault(c => c.Tag == tag);

        #region Sync characters to the program
        public Dictionary<string, CharacterStructure> SyncCharacters(bool duplicatesOnly = false)
        {
            var characters = new Dictionary<string, CharacterStructure>();
            var files = GetCharacterFiles();
            if (files.Count == 0)
            {
                DialogBox.Show(
                    "Something has gone wrong while building character data!\n\nNo character files are configured.",
                    "Failed to Compile Character Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception("Bad file data...\nNo character files are configured.");
            }

            foreach (var (fileKey, filePath) in files.OrderBy(entry => entry.Key))
            {
                string[] content = File.ReadAllLines(filePath);
                int start = Array.FindIndex(content, line => line.Trim() == ContentGuides[Type][0]);
                int end = Array.FindIndex(content, start + 1, line => line.Trim() == ContentGuides[Type][1]);

                if (start <= -1 || end <= -1)
                    continue;

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
                            if (duplicatesOnly)
                            {
                                if (CheckedDuplicates(new CharacterStructure { Tag = tag }))
                                {
                                    characters[tag] = new CharacterStructure
                                    {
                                        Tag = tag,
                                        FileKey = fileKey
                                    };
                                }
                            }
                            else
                            {
                                characters[tag] = new CharacterStructure
                                {
                                    Tag = tag,
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
                        if (key == "Sprites") characters[tag].Sprites = attribute;
                    }
                }
            }

            return characters;
        }
        #endregion

        #region Handle character data in program
        public bool CheckedDuplicates(Character characterToCheck)
        {
            foreach (Character character in AllCharacters)
            {
                if (character.Tag == characterToCheck.Tag)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckedDuplicates(CharacterStructure characterToCheck)
        {
            foreach (Character character in AllCharacters)
            {
                if (character.Tag == characterToCheck.Tag)
                {
                    return true;
                }
            }

            return false;
        }

        private void ReorderCharacters()
        {
            for (int i = 0; i < AllCharacters.Count; i++)
            {
                Character character = AllCharacters[i];
                character.EntityID = i;
            }
        }

        private void ReorderCharacters(int startingIndex)
        {
            for (int i = startingIndex; i < AllCharacters.Count; i++)
            {
                Character character = AllCharacters[i];
                character.EntityID = i;
            }
        }

        public void AddCharacter(Character character)
        {
            if (string.IsNullOrEmpty(character.FileKey))
                character.FileKey = ResolveCharacterFileKey(Type) ?? string.Empty;

            character.EntityID = AllCharacters.Count;
            AllCharacters.Add(character);
            SaveCharacters();
        }

        public bool RemoveCharacter(int characterId)
        {
            Character? character = AllCharacters.FirstOrDefault(c => c.EntityID == characterId);
            if (character != null)
            {
                int index = AllCharacters.IndexOf(character);
                AllCharacters.RemoveAt(index);
                ReorderCharacters(index);
                SaveCharacters();
                return true;
            }

            return false;
        }

        public Character? CheckCharacter(int characterId)
        {
            Character? character = AllCharacters.FirstOrDefault(c => c.EntityID == characterId);
            return character;
        }

        public Character UpdateCharacter(int characterId, Character character)
        {
            Character? oldCharacter = AllCharacters.FirstOrDefault(c => c.EntityID == characterId);
            if (oldCharacter != null)
            {
                character.EntityID = oldCharacter.EntityID;
                if (string.IsNullOrEmpty(character.FileKey))
                    character.FileKey = oldCharacter.FileKey;
                if (!character.IsSynced)
                    character.IsSynced = oldCharacter.IsSynced;
                AllCharacters.Remove(oldCharacter);
                AllCharacters.Insert(characterId, character);
                SaveCharacters();
            }

            return AllCharacters[characterId];
        }
        #endregion

        #region Handle character data in file
        public bool CharacterExistsInScript(string tag, string? fileKey = null)
        {
            var files = GetCharacterFiles();
            if (files.Count == 0)
            {
                DialogBox.Show(
                    "Attempted to check character existance without a proper file path!",
                    "No File Path",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
                return false;
            }

            var filesToCheck = string.IsNullOrWhiteSpace(fileKey)
                ? files.OrderBy(entry => entry.Key)
                : files.Where(entry => entry.Key == ResolveCharacterFileKey(fileKey));

            foreach (var (_, filePath) in filesToCheck)
            {
                using var reader = new StreamReader(filePath);
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.EndsWith(ContentGuides[TypeLabel][0], StringComparison.Ordinal) && line.Contains(tag, StringComparison.Ordinal))
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

        public bool CharacterExistsInScript(int id)
        {
            var character = CheckCharacter(id);
            var tag = character?.Tag;
            if (tag == null) return false;

            return CharacterExistsInScript(tag, character?.FileKey);
        }

        private string AddIndentation()
        {
            return ApplicationSettings?.GetIndentationType() switch
            {
                "Tab"       => "\t",
                "Spaces"    => new string(' ', ApplicationSettings.GetIndentationAmount()),
                _           => new string(' ', 4)
            };
        }

        public Dictionary<string, string?> ConvertToScriptContent(Character character)
        {
            var content = new Dictionary<string, string?>();
            if (character is Normal normalCharacter)
            {
                content.Add("tag", normalCharacter.Tag);
                content.Add("name", normalCharacter.Name);
                content.Add("color", normalCharacter.Color);
                content.Add("directory", normalCharacter.Directory);

                // Not implemented yet.
                //content.Add("sprites", normalCharacter.Sprites);
            }
            else if (character is Expressive expresiveCharacter)
            {
                content.Add("tag", expresiveCharacter.Tag);
                content.Add("name", expresiveCharacter.Name);
                content.Add("color", expresiveCharacter.Color);
                content.Add("directory", expresiveCharacter.Directory);

                // Not implemented yet.
                //content.Add("sprites", expresiveCharacter.Sprites);
            }


            return content;
        }

        private string? ConvertToScriptContent(Dictionary<string, string?> content)
        {
            if (content.TryGetValue("tag", out string? characterTag))
            {
                string output = $"{AddIndentation()}\"{characterTag}\": {{ {ContentGuides[TypeLabel][0]}\n";

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

                        output += $"{AddIndentation()}}}, {ContentGuides[TypeLabel][1]}";
                        break;
                    }

                    if (tag == "tag" || data == null || data == string.Empty) continue;

                    output += $"{AddIndentation()}{AddIndentation()}{ProcessDictionaryTag(tag, data)},\n";
                }

                return output;
            }

            return null;
        }

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

        public void AddCharacterToScript(string tag, Dictionary<string, string?> content, string? fileKey = null)
        {
            fileKey ??= FindCharacter(tag)?.FileKey;
            var filePath = ResolveCharacterFilePath(fileKey, out string resolvedFileKey);
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
                var lines = File.ReadAllLines(filePath).ToList();
                int lastIndex = lines.FindLastIndex(line => line.EndsWith(ContentGuides[TypeLabel][1]));

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
                            SaveCharacters();
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
                    lastIndex = lines.FindLastIndex(line => line.Contains(ContentGuides[Type][0]));

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
                                SaveCharacters();
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
                            $"Missing Character formatting!\nIn order to use \"Add Characters to Script\" functionality, your character labels must be formatted with with opening and ending tags.\n\nExample:\n{ContentGuides[Type][0]}\n    \"YourLabel\": {{ {ContentGuides[TypeLabel][0]}\n        // Your content\n    }} {ContentGuides[TypeLabel][1]}\n{ContentGuides[Type][1]}",
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

        public bool RemoveCharacterFromScript(int characterId, bool save)
        {
            Character? character = AllCharacters.FirstOrDefault(c => c.EntityID == characterId);
            if (character != null)
            {
                var filePath = ResolveCharacterFilePath(character, out _);
                if (filePath == null)
                {
                    DialogBox.Show(
                        $"Something has gone wrong while attempting to remove character data for \"{character.Tag}\"!\n\nPath: {filePath ?? "null"}",
                        "Failed to Remove Character Data",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                    throw new Exception($"Bad file data...\n{filePath ?? "null"}");
                }

                string tempPath = Path.GetTempFileName();

                try
                {
                    using (var reader = new StreamReader(filePath))
                    using (var writer = new StreamWriter(tempPath))
                    {
                        string? line;
                        string? lastLine = null;
                        bool isRemoving = false;

                        while ((line = reader.ReadLine()) != null)
                        {
                            bool isStart = line.EndsWith(ContentGuides[TypeLabel][0]) && line.Contains(character.Tag);
                            bool isEnd = isRemoving && line.EndsWith(ContentGuides[TypeLabel][1]);

                            if (isStart)
                            {
                                string trimmed = line.Trim();
                                var result = CharacterRegex().Match(trimmed);
                                if (result.Success && result.Groups["tag"].Value == character.Tag)
                                {
                                    isRemoving = true;
                                    System.Diagnostics.Debug.WriteLine(lastLine);
                                    if (string.IsNullOrEmpty(lastLine))
                                        lastLine = null;
                                }
                            }

                            if (lastLine != null)
                            {
                                writer.WriteLine(lastLine);
                            }

                            if (isEnd)
                            {
                                isRemoving = false;
                                lastLine = null;
                                continue;
                            }

                            lastLine = isRemoving ? null : line;
                        }

                        if (lastLine != null)
                            writer.WriteLine(lastLine);

                        character.IsSynced = false;
                        if (save)
                        {
                            SaveCharacters();
                        }
                    }

                    FileWatcher.ReplaceFile(tempPath, filePath);
                    return true;
                }
                catch (Exception error)
                {
                    DialogBox.Show(
                        $"Something went wrong while removing character data for \"{character.Tag}\"!\nNo need to panic, the process was cut off before anything saved.\n\n{error}",
                        "Failed to Remove Character Data",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                    throw new Exception($"Bad removal data..\n{error}");
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
            }
            else
            {
                DialogBox.Show(
                    "Missing or Invalid \"Character\"...",
                    "Bad Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Invalid input data...");
            }
        }

        public bool UpdateCharacterInScript(string tag, Dictionary<string, string?> content, string? fileKey = null)
        {
            fileKey ??= FindCharacter(tag)?.FileKey;
            var filePath = ResolveCharacterFilePath(fileKey, out string resolvedFileKey);
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
            string indentUnit = indentationType == "Tab" ? "\t" : new string(' ', indentationAmount);

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var writer = new StreamWriter(tempPath))
                {
                    string? line;
                    bool startUpdating = false;
                    var writtenKeys = new HashSet<string>();

                    if (content.TryGetValue("directory", out string? dir) && dir != string.Empty)
                    {
                        CreateCharacterDirectory(tag, dir);
                    }

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.EndsWith(ContentGuides[TypeLabel][0], StringComparison.Ordinal) && line.Contains(tag, StringComparison.Ordinal))
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

                        if (startUpdating && line.EndsWith(ContentGuides[TypeLabel][1]))
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
                    SaveCharacters();
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

        public Dictionary<string, bool> CharacterContentMatches(List<string> tags, string? fileKey = null)
        {
            var results = tags.ToDictionary(t => t, _ => false);
            var remaining = new HashSet<string>(tags);

            var filePath = ResolveCharacterFilePath(fileKey, out _);
            if (filePath == null)
                return results;

            string[] fileContent = File.ReadAllLines(filePath);
            int start = Array.FindIndex(fileContent, line => line.Trim() == ContentGuides[Type][0]);
            int end = Array.FindIndex(fileContent, start + 1, line => line.Trim() == ContentGuides[Type][1]);

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
                        var character = AllCharacters.FirstOrDefault(c => c.Tag == currentTag);
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

        public bool CheckSynchronicity(bool showMessage = true)
        {
            if (ApplicationSettings == null) return false;

            bool charactersHaveChanged = false;
            var files = GetCharacterFiles();

            foreach (var (fileKey, _) in files)
            {
                var tagsInFile = AllCharacters
                    .Where(c => (string.IsNullOrEmpty(c.FileKey)
                        ? fileKey == ResolveCharacterFileKey()
                        : c.FileKey == fileKey) && c.IsSynced)
                    .Select(c => c.Tag)
                    .ToList();

                if (tagsInFile.Count == 0)
                    continue;

                var contentMatches = CharacterContentMatches(tagsInFile, fileKey);

                foreach (string tag in tagsInFile)
                {
                    contentMatches.TryGetValue(tag, out bool matches);
                    if (!matches)
                    {
                        charactersHaveChanged = true;
                        break;
                    }
                }

                if (charactersHaveChanged)
                    break;
            }

            if (charactersHaveChanged && showMessage)
            {
                DialogBox.Show(
                    $"It looks like something changed from the last time the program was opened.\nCharacters that have been modified will appear as such when opening the settings screen.\n\nDue to the fragile nature of characters, synchronicity checks and changes cannot be disabled. However, you can disable this message in the settings screen.",
                    "Changes Have Been Made",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
            }

            return charactersHaveChanged;
        }
        #endregion
    }
}
