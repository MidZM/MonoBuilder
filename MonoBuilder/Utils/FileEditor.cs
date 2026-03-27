using MonoBuilder.Screens;
using MonoBuilder.Screens.ScreenUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MonoBuilder.Utils
{
    /// <summary>Represents a collection of key/value pairs of strings and LoadedLabel objects that preserves insertion order.</summary>
    public class TypedOrderedDictionary
    {
        /// <summary>Stores key/value pairs in the order in which they were added.</summary>
        private readonly OrderedDictionary _inner = new OrderedDictionary();

        /// <summary>Gets the number of elements contained in the collection.</summary>
        public int Count => _inner.Count;
        /// <summary>Gets a collection of loaded labels contained in the inner collection.</summary>
        public IEnumerable<LoadedLabel> Values => _inner.Values.Cast<LoadedLabel>();
        /// <summary>Gets a collection containing the keys in the collection.</summary>
        public IEnumerable<string> Keys => _inner.Keys.Cast<string>();

        /// <summary>
        /// Gets or sets the loaded label associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the label to get or set.</param>
        /// <returns>The loaded label associated with the specified key, or null if the key does not exist.</returns>
        public LoadedLabel? this[string key]
        {
            get => _inner.Contains(key) ? (LoadedLabel)_inner[key]! : null;
            set
            {
                if (value is null) _inner.Remove(key);
                else _inner[key] = value;
            }
        }

        /// <summary>
        /// Gets the label at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the label to retrieve.</param>
        /// <returns>The label at the specified index, or null if the entry is not a LoadedLabel.</returns>
        public LoadedLabel? this[int index] => (LoadedLabel?)_inner[index];

        /// <summary>
        /// Determines whether the collection contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the collection.</param>
        /// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key) => _inner.Contains(key);

        /// <summary>
        /// Attempts to retrieve the LoadedLabel associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to retrieve.</param>
        /// <param name="value">When this method returns, contains the LoadedLabel associated with the specified key, if the key is found;
        /// otherwise, null.</param>
        /// <returns>true if the key was found; otherwise, false.</returns>
        public bool TryGetValue(string key, out LoadedLabel? value)
        {
            if (_inner.Contains(key))
            {
                value = (LoadedLabel)_inner[key]!;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        public void Remove(string key) => _inner.Remove(key);
        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index) => _inner.RemoveAt(index);
        /// <summary>
        /// Inserts a key and value into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the key and value should be inserted.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        public void Insert(int index, string key, LoadedLabel value) => _inner.Insert(index, key, value);
        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear() => _inner.Clear();
    }

    public partial class FileEditor
    {
        /// <summary>Gets or sets a collection of file names and their associated contents.</summary>
        private Dictionary<string, string> Files { get; set; }
        /// <summary>Gets or sets a collection of folder names and their corresponding paths.</summary>
        private Dictionary<string, string> Folders { get; set; }
        /// <summary>Gets or sets the type of indentation used, such as spaces or tabs.</summary>
        private string IndentationType { get; set; }
        /// <summary>Gets or sets the number of spaces used for indentation.</summary>
        private int IndentationAmount { get; set; }

        /// <summary>
        /// Gets a regular expression that matches and captures a labeled section in a string.
        /// </summary>
        /// <returns>A Regex instance for matching labeled sections.</returns>
        [GeneratedRegex(@"^([""'`])(?<label>.*)\1.*[:]*[\[] // START_LABEL$")]
        private static partial Regex LabelRegex();

        /// <summary>Gets or sets the collection of labels that have not been synchronized.</summary>
        private TypedOrderedDictionary UnsyncedLabels { get; set; } = new();
        /// <summary>Gets or sets the collection of labels synchronized with the current state.</summary>
        private TypedOrderedDictionary SyncedLabels { get; set; } = new();
        /// <summary>Indicates whether scripts have been initialized.</summary>
        private bool _scriptsInitialized = false;

        /// <summary>Gets or sets a collection of key-value pairs representing labels.</summary>
        private Dictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
        /// <summary>Gets or sets the XML document used to store application data.</summary>
        private XDocument SaveData { get; set; } = new XDocument();

        /// <summary>Gets a mapping of content guide categories to their associated marker strings.</summary>
        private Dictionary<string, List<string>> ContentGuides { get; } = new Dictionary<string, List<string>>
        {
            {
                "Script",
                [
                    "// SCRIPT_INSERTION_POINT",
                    "// END_SCRIPT_INSERTION_POINT"
                ]
            },
            {
                "ScriptLabel",
                [
                    "// START_LABEL",
                    "// END_LABEL"
                ]
            }
        };

        /// <summary>Specifies the content guide type as "Script".</summary>
        private readonly string Type = "Script";
        /// <summary>Specifies the label content guide of the type.</summary>
        private readonly string TypeLabel = "ScriptLabel";

        /// <summary>
        /// Initializes a new instance of the FileEditor class with the specified files, folders, indentation type, and
        /// indentation amount.
        /// </summary>
        /// <param name="files">A dictionary containing file names and their contents.</param>
        /// <param name="folders">A dictionary containing folder names and their descriptions.</param>
        /// <param name="indentType">The type of indentation to use, such as spaces or tabs.</param>
        /// <param name="indentAmount">The number of indentation units to apply.</param>
        public FileEditor(Dictionary<string, string> files, Dictionary<string, string> folders, string indentType, int indentAmount)
        {
            Files = files;
            Folders = folders;
            IndentationType = indentType;
            IndentationAmount = indentAmount;
        }

        #region Utilities
        /// <summary>
        /// Gets the collection of labels that have not been synchronized.
        /// </summary>
        /// <returns>A TypedOrderedDictionary containing unsynced labels.</returns>
        public TypedOrderedDictionary GetUnsyncedLabels() => UnsyncedLabels;

        /// <summary>
        /// Gets the collection of synchronized labels.
        /// </summary>
        /// <returns>A TypedOrderedDictionary containing the synchronized labels.</returns>
        public TypedOrderedDictionary GetSyncedLabels() => SyncedLabels;

        /// <summary>
        /// Creates a binding list of label grid rows representing unsynced labels.
        /// </summary>
        /// <returns>A BindingList containing LabelGridRow objects for each unsynced label.</returns>
        public BindingList<LabelGridRow> GetUnsyncedBindingSource()
        {
            var list = new BindingList<LabelGridRow>();
            foreach (LoadedLabel l in UnsyncedLabels.Values)
                list.Add(new LabelGridRow(l.Name, l.WordCount, l.Synced, l.InScript));
            return list;
        }

        /// <summary>
        /// Creates a new binding list containing label grid rows synchronized with the current labels.
        /// </summary>
        /// <returns>A BindingList of LabelGridRow objects representing the synchronized labels.</returns>
        public BindingList<LabelGridRow> GetSyncedBindingSource()
        {
            var list = new BindingList<LabelGridRow>();
            foreach (LoadedLabel l in SyncedLabels.Values)
                list.Add(new LabelGridRow(l.Name, l.WordCount, l.Synced, l.InScript));
            return list;
        }

        /// <summary>
        /// Indicates whether the scripts have been initialized.
        /// </summary>
        /// <returns>true if the scripts are initialized; otherwise, false.</returns>
        public bool ScriptInitialized() => _scriptsInitialized;

        /// <summary>
        /// Counts the number of words in the specified text.
        /// </summary>
        /// <param name="text">The text to analyze.</param>
        /// <returns>The number of words found in the text.</returns>
        private static int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return text.Split((char[])[' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>
        /// Retrieves a loaded label by name from either the unsynced or synced label collection.
        /// </summary>
        /// <param name="labelName">The name of the label to retrieve.</param>
        /// <param name="isUnsynced">true to search in the unsynced labels; false to search in the synced labels. Defaults to true.</param>
        /// <returns>The loaded label if found; otherwise, null.</returns>
        public LoadedLabel? GetLabel(string labelName, bool isUnsynced = true)
        {
            var labels = isUnsynced ? UnsyncedLabels : SyncedLabels;
            labels.TryGetValue(labelName, out var label);
            return label;
        }

        /// <summary>
        /// Determines whether a label exists in the synchronized label collection.
        /// </summary>
        /// <param name="label">The label to locate in the collection.</param>
        /// <returns>true if the label exists; otherwise, false.</returns>
        private bool LabelExists(string label) => SyncedLabels.ContainsKey(label);

        /// <summary>
        /// Determines whether a script label matching the specified name exists in the script file.
        /// </summary>
        /// <param name="label">The name of the script label to search for.</param>
        /// <returns>true if the script label exists; otherwise, false.</returns>
        /// <exception cref="Exception">Thrown when the script file path is missing or invalid.</exception>
        public bool ScriptLabelExists(string label)
        {
            if (!Files.TryGetValue(Type, out string? filePath) || filePath == null)
            {
                DialogBox.Show(
                    $"Something has gone wrong while attempting to search for a script label!\n\nPath: {filePath}",
                    "Failed to Search Script Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Bad file data...\n{filePath}");
            }

            string[] content = File.ReadAllLines(filePath);
            int start = Array.FindIndex(content, line => line.Trim() == ContentGuides[Type][0]);
            int end = Array.FindIndex(content, start + 1, line => line.Trim() == ContentGuides[Type][1]);

            if (start != -1 && end != -1)
            {
                string[] innerContent = content[(start + 1)..end];
                string startLabel = ContentGuides[TypeLabel][0];

                string endPattern1 = "[ " + startLabel;
                string endPattern2 = "[" + startLabel;
                string endPattern3 = ": [";
                string endPattern4 = ":[";

                foreach (string line in innerContent)
                {
                    bool isLabel =
                        line.EndsWith(endPattern1, StringComparison.Ordinal) ||
                        line.EndsWith(endPattern2, StringComparison.Ordinal) ||
                        line.EndsWith(endPattern3, StringComparison.Ordinal) ||
                        line.EndsWith(endPattern4, StringComparison.Ordinal);

                    if (isLabel && line.Contains(label))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Dictionary<string, bool> ScriptLabelExists(List<string> labels)
        {
            if (!Files.TryGetValue(Type, out string? filePath) || filePath == null)
            {
                DialogBox.Show(
                    $"Something has gone wrong while attempting to search for multiple script labels!\n\nPath: {filePath}",
                    "Failed to Search Script Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Bad file data...\n{filePath}");
            }

            var checkedLabels = labels.ToDictionary(l => l, _ => false);
            var remaining = new HashSet<string>(labels);

            string[] content = File.ReadAllLines(filePath);
            int start = Array.FindIndex(content, line => line.Trim() == ContentGuides[Type][0]);
            int end = Array.FindIndex(content, start + 1, line => line.Trim() == ContentGuides[Type][1]);

            if (start != -1 && end != -1)
            {
                string[] innerContent = content[(start + 1)..end];
                string startLabel = ContentGuides[TypeLabel][0];

                string endPattern1 = "[ " + startLabel;
                string endPattern2 = "[" + startLabel;
                string endPattern3 = ": [";
                string endPattern4 = ":[";

                foreach (string line in innerContent)
                {
                    if (remaining.Count == 0) break;

                    bool isLabelLine =
                        line.EndsWith(endPattern1, StringComparison.Ordinal) ||
                        line.EndsWith(endPattern2, StringComparison.Ordinal) ||
                        line.EndsWith(endPattern3, StringComparison.Ordinal) ||
                        line.EndsWith(endPattern4, StringComparison.Ordinal);

                    if (isLabelLine)
                    {
                        foreach (string label in remaining)
                        {
                            if (line.Contains(label))
                            {
                                checkedLabels[label] = true;
                                remaining.Remove(label);
                                break;
                            }
                        }
                    }
                }
            }

            return checkedLabels;
        }

        /// <summary>
        /// Determines whether the script content in the file for the specified label matches the content
        /// currently stored in <see cref="SyncedLabels"/>.
        /// </summary>
        /// <param name="label">The label whose content to compare.</param>
        /// <param name="content">The content to compare against the script file.</param>
        /// <returns>true if the content matches; false if it differs or the label is not found in the file.</returns>
        /// <exception cref="Exception">Thrown when the script file path is missing or invalid.</exception>
        public bool ScriptContentMatches(string label, string content)
        {
            if (!Files.TryGetValue(Type, out string? filePath) || filePath == null)
            {
                DialogBox.Show(
                    $"Something has gone wrong while attempting to compare script content for \"{label}\"!\n\nPath: {filePath}",
                    "Failed to Compare Script Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Bad file data...\n{filePath}");
            }

            string[] fileContent = File.ReadAllLines(filePath);
            int start = Array.FindIndex(fileContent, line => line.Trim() == ContentGuides[Type][0]);
            int end = Array.FindIndex(fileContent, start + 1, line => line.Trim() == ContentGuides[Type][1]);

            if (start == -1 || end == -1) return false;

            string[] innerContent = fileContent[(start + 1)..end];
            string baseline = DetectBaseline(innerContent);
            string endLabelTag = ContentGuides[TypeLabel][1];

            bool capturing = false;
            var scriptContent = new StringBuilder();

            foreach (string rawLine in innerContent)
            {
                string line = StripBaselineIndent(rawLine, baseline);
                var labelRes = LabelRegex().Match(line);

                if (labelRes.Success && labelRes.Groups["label"].Value == label)
                {
                    capturing = true;
                    continue;
                }

                if (capturing)
                {
                    bool isEndLabel = line.StartsWith("] " + endLabelTag) || line.StartsWith("], " + endLabelTag);
                    if (isEndLabel)
                    {
                        break;
                    }

                    scriptContent.Append(line);
                    scriptContent.Append('\n');
                }
            }

            var deformatted = DeformatFromScript(scriptContent.ToString());
            return deformatted == content;
        }

        /// <summary>
        /// Determines whether the script content in the file matches the content currently stored in
        /// <see cref="SyncedLabels"/> for each of the specified labels.
        /// </summary>
        /// <param name="labels">The labels whose content to compare.</param>
        /// <returns>A dictionary mapping each label to true if the content matches; false if it differs or the label is not found.</returns>
        /// <exception cref="Exception">Thrown when the script file path is missing or invalid.</exception>
        public Dictionary<string, bool> ScriptContentMatches(List<string> labels)
        {
            if (!Files.TryGetValue(Type, out string? filePath) || filePath == null)
            {
                DialogBox.Show(
                    $"Something has gone wrong while attempting to compare script content!\n\nPath: {filePath}",
                    "Failed to Compare Script Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Bad file data...\n{filePath}");
            }

            var results = labels.ToDictionary(l => l, _ => false);
            var remaining = new HashSet<string>(labels);

            string[] fileContent = File.ReadAllLines(filePath);
            int start = Array.FindIndex(fileContent, line => line.Trim() == ContentGuides[Type][0]);
            int end = Array.FindIndex(fileContent, start + 1, line => line.Trim() == ContentGuides[Type][1]);

            if (start == -1 || end == -1) return results;

            string[] innerContent = fileContent[(start + 1)..end];
            string baseline = DetectBaseline(innerContent);
            string endLabelTag = ContentGuides[TypeLabel][1];

            string currentLabel = string.Empty;
            var scriptContent = new StringBuilder();

            foreach (string rawLine in innerContent)
            {
                if (remaining.Count == 0) break;

                string line = StripBaselineIndent(rawLine, baseline);
                var labelRes = LabelRegex().Match(line);

                if (labelRes.Success)
                {
                    currentLabel = labelRes.Groups["label"].Value;
                    scriptContent.Clear();
                    continue;
                }

                if (currentLabel != string.Empty && remaining.Contains(currentLabel))
                {
                    bool isEndLabel = line.StartsWith("] " + endLabelTag) || line.StartsWith("], " + endLabelTag);
                    if (isEndLabel)
                    {
                        SyncedLabels.TryGetValue(currentLabel, out var syncedLabel);
                        var syncedContent = syncedLabel?.Content?.ToString() ?? string.Empty;
                        var deformatted = DeformatFromScript(scriptContent.ToString());

                        results[currentLabel] = deformatted == syncedContent;
                        remaining.Remove(currentLabel);
                        currentLabel = string.Empty;
                        scriptContent.Clear();
                        continue;
                    }

                    scriptContent.Append(line);
                    scriptContent.Append('\n');
                }
            }

            return results;
        }

        /// <summary>
        /// Removes the first and last lines from the specified string and returns the processed content.
        /// </summary>
        /// <param name="content">The string from which to remove the first and last lines.</param>
        /// <returns>A string with the first and last lines removed; returns an empty string if not applicable.</returns>
        private string StripLabelTags(string content)
        {
            content = content.Replace("\r\n", "\n").Replace('\r', '\n');
            int firstNewLine = content.IndexOf('\n');
            int lastNewLine = content.LastIndexOf('\n');

            if (firstNewLine != -1 && lastNewLine > firstNewLine)
            {
                string result = content.Substring(firstNewLine + 1, lastNewLine - (firstNewLine + 1));
                return DeformatFromScript(result);
            }
            return string.Empty;
        }

        /// <summary>
        /// Formats the specified content with indentation based on the current indentation settings for use in a script
        /// block.
        /// </summary>
        /// <param name="content">The string content to format.</param>
        /// <returns>A formatted string with applied indentation.</returns>
        public string FormatToScript(string content)
        {
            if (content.Length > 0)
            {
                var indent = IndentationType == "Tab" ? new string('\t', 1) : new string(' ', IndentationAmount);
                return indent + content.TrimEnd().Replace("\n", "\n" + indent);
            }
            return content;
        }

        /// <summary>
        /// Formats the specified content as a script block with a label and indentation based on the current settings.
        /// </summary>
        /// <param name="label">The label to associate with the script content.</param>
        /// <param name="content">The script content to format.</param>
        /// <returns>A formatted string representing the script block, or the original content if label or content is empty.</returns>
        public string FormatToScript(string label, string content)
        {
            if (label.Length > 0 && content.Length > 0)
            {
                var indent = IndentationType == "Tab" ? new string('\t', 1) : new string(' ', IndentationAmount);
                var newContent = $"\"{label}\": [ {ContentGuides["ScriptLabel"][0]}\n{indent}";
                content = content.TrimEnd().Replace("\n", "\n" + indent);
                newContent += $"{content}\n";
                newContent += $"], {ContentGuides["ScriptLabel"][1]}";
                return indent + newContent.TrimEnd().Replace("\n", "\n" + indent);
            }

            return content;
        }

        /// <summary>
        /// Removes leading indentation from each line in the specified script content based on the current indentation
        /// settings.
        /// </summary>
        /// <param name="content">The script content to deformat.</param>
        /// <returns>A string with leading indentation removed from each line.</returns>
        private string DeformatFromScript(string content)
        {
            if (content.Length > 0)
            {
                bool isTab = IndentationType == "Tab";
                int amnt = IndentationAmount;
                string indent = isTab ? new string('\t', 1) : new string(' ', amnt);
                string replace = "\n" + indent;

                return content.StartsWith(indent) ?
                    content.Substring(isTab ? 1 : amnt).Replace(replace, "\n") :
                    content.Replace(replace, "\n");
            }
            return content;
        }
        #endregion

        #region Preloading Methods
        /// <summary>
        /// Initializes and returns the collection of unsynced script labels, optionally forcing a reload.
        /// </summary>
        /// <param name="forceReload">true to force reloading the script labels; otherwise, false.</param>
        /// <returns>A TypedOrderedDictionary containing the unsynced script labels.</returns>
        public TypedOrderedDictionary InitializeScripts(bool forceReload = false)
        {
            if (forceReload == true || UnsyncedLabels == null)
            {
                UnsyncedLabels = PreloadAllScripts();
                _scriptsInitialized = true;
            }
            return UnsyncedLabels;
        }

        /// <summary>
        /// Removes the specified baseline indentation from the beginning of a line, or trims leading whitespace if the
        /// baseline is not present.
        /// </summary>
        /// <param name="line">The input string to process.</param>
        /// <param name="baseline">The baseline indentation to remove from the start of the line.</param>
        /// <returns>A string with the baseline indentation removed, or with leading whitespace trimmed if the baseline is not
        /// found.</returns>
        private static string StripBaselineIndent(string line, string baseline)
        {
            if (line.StartsWith(baseline))
                return line[baseline.Length..];
            return line.TrimStart();
        }

        /// <summary>
        /// Detects the leading whitespace from the first non-empty line in the specified array, starting at the given
        /// index.
        /// </summary>
        /// <param name="lines">The array of lines to search.</param>
        /// <param name="startIndex">The zero-based index at which to begin searching.</param>
        /// <returns>A string containing the leading whitespace of the first non-empty line found; otherwise, an empty string.</returns>
        private static string DetectBaseline(string[] lines, int startIndex = 0)
        {
            for (int i = startIndex; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;
                int j = 0;
                while (j < line.Length && (line[j] == ' ' || line[j] == '\t'))
                    j++;
                return line[..j];
            }
            return string.Empty;
        }

        /// <summary>
        /// Loads and parses all script labels of the specified type from the associated file.
        /// </summary>
        /// <param name="type">The script type to load labels for.</param>
        /// <returns>A collection of loaded script labels and their associated content.</returns>
        /// <exception cref="Exception">Thrown when the script file for the specified type is missing or invalid.</exception>
        public TypedOrderedDictionary PreloadAllScripts()
        {
            var labels = new TypedOrderedDictionary();
            if (!Files.TryGetValue(Type, out string? filePath) || filePath == null)
            {
                DialogBox.Show(
                    $"Something has gone wrong while building script label data!\n\n{filePath}",
                    "Failed to Compile Label Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Bad file data...\n{filePath}");
            }

            string[] content = File.ReadAllLines(filePath);
            int start = Array.FindIndex(content, line => line.Trim() == ContentGuides[Type][0]);
            int end = Array.FindIndex(content, start + 1, line => line.Trim() == ContentGuides[Type][1]);

            if (start > -1 && end > -1)
            {
                string[] innerContent = content[(start + 1)..end];
                string baseline = DetectBaseline(innerContent);

                string label = string.Empty;
                var labelContent = new LoadedLabel(label, new StringBuilder(), 0, false, true);
                var endLabelTag = ContentGuides[TypeLabel][1];
                int index = 0;

                foreach (string rawLine in innerContent)
                {
                    string line = StripBaselineIndent(rawLine, baseline);
                    var labelRes = LabelRegex().Match(line);
                    bool isEndLabel = line.StartsWith("] " + endLabelTag) || line.StartsWith("], " + endLabelTag);

                    if (labelRes.Success)
                    {
                        label = labelRes.Groups["label"].Value;
                        labelContent = new LoadedLabel(label, new StringBuilder(), 0, false, true);
                        baseline = DetectBaseline(innerContent, index + 1);

                        if (label == string.Empty)
                        {
                            DialogBox.Show(
                                "A label is missing string content and will not run properly during gameplay.\nThe problem will not affect this program, but will affect your game. Consider resolving the issue.",
                                "Missing Label Content",
                                DialogButtonDefaults.OK,
                                DialogIcon.Warning);
                        }
                        index++;
                        continue;
                    }

                    if (label != string.Empty)
                    {
                        if (isEndLabel)
                        {
                            labelContent.WordCount = CountWords(labelContent.Content?.ToString() ?? "");
                            if (LabelExists(label))
                            {
                                SyncedLabels.TryGetValue(label, out var existingLabel);
                                var stringifiedSyncedLabel = existingLabel?.Content?.ToString() ?? string.Empty;
                                var stringifiedUnsyncedLabel = labelContent.Content?.ToString() ?? string.Empty;
                                if (stringifiedSyncedLabel == stringifiedUnsyncedLabel)
                                {
                                    label = string.Empty;
                                    baseline = DetectBaseline(innerContent, index + 1);
                                    index++;
                                    continue;
                                }
                            }

                            labels[label] = labelContent;
                            label = string.Empty;
                            baseline = DetectBaseline(innerContent, index + 1);
                            index++;
                            continue;
                        }

                        labelContent.Content?.Append(line);
                        labelContent.Content?.Append('\n');
                    }

                    index++;
                }
            }
            else
            {
                DialogBox.Show(
                    $"Script file does not contain a \"{ContentGuides[Type][0]}\" or \"{ContentGuides[Type][1]}\".",
                    "Invalid Guide Content",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }

            return labels;
        }

        public void CheckSynchronicity(bool showMessage = true)
        {
            List<string> labels = SyncedLabels.Keys.ToList();
            var existingLabels = ScriptLabelExists(labels);
            var existingScripts = ScriptContentMatches(labels);

            bool labelsHaveChanged = false;
            foreach (string label in labels)
            {
                if (SyncedLabels.TryGetValue(label, out LoadedLabel? content) && content != null)
                {
                    existingLabels.TryGetValue(label, out bool inScript);
                    existingScripts.TryGetValue(label, out bool synced);
                    var originalValue = content.InScript;

                    content.InScript = inScript && synced;

                    if (originalValue != content.InScript)
                    {
                        labelsHaveChanged = true;
                    }
                }
            }

            if (labelsHaveChanged && showMessage)
            {
                DialogBox.Show(
                    $"It looks like something changed from the last time the program was opened.\nLabels that have been modified will appear as such when opening the label loader.\n\nIf you would like to disable this message, and the synchronicity checks, you can do so from the settings menu.",
                    "Changes Have Been Made",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
            }
        }
        #endregion

        #region General Methods
        /// <summary>
        /// Appends a new script label and its content to the script file, updating program state and handling file
        /// operations as needed.
        /// </summary>
        /// <param name="label">The label to associate with the script content.</param>
        /// <param name="content">The script content to add.</param>
        /// <exception cref="Exception">Thrown when file data is invalid or an error occurs during the append operation.</exception>
        public void AddToScript(string label, string content)
        {
            if (label.Length > 0 && content.Length > 0)
            {
                if (!Files.TryGetValue(Type, out string? filePath) || filePath == null)
                {
                    DialogBox.Show(
                        $"Something has gone wrong while attempting to add new script label data!\n\nPath: {filePath}",
                        "Failed to Append Label Data",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                    throw new Exception($"Bad file data...\n{filePath}");
                }

                string tempPath = Path.GetTempFileName();

                try
                {
                    var lines = File.ReadAllLines(filePath).ToList();
                    int lastIndex = lines.FindLastIndex(line => line.Contains(ContentGuides[TypeLabel][1]));

                    if (lastIndex != -1)
                    {
                        if (lines[lastIndex].Trim().StartsWith("] "))
                            lines[lastIndex] = lines[lastIndex].Replace("] ", "], ");

                        string makeReadable = "\n" + content.TrimEnd();
                        lines.Insert(lastIndex + 1, makeReadable);

                        File.WriteAllLines(tempPath, lines);
                        File.Replace(tempPath, filePath, filePath + ".bak");

                        SyncedLabels.TryGetValue(label, out var labelExists);
                        if (labelExists != null)
                        {
                            labelExists.InScript = true;
                            SaveProgram();
                        }
                        else
                        {
                            var deformattedContent = StripLabelTags(DeformatFromScript(content));
                            Dangerous_ForceSaveToProgram(label, deformattedContent);
                        }
                    }
                    else
                    {
                        lastIndex = lines.FindLastIndex(line => line.Contains(ContentGuides[Type][0]));

                        if (lastIndex != -1)
                        {
                            string makeReadable = "\n" + content.TrimEnd();
                            lines.Insert(lastIndex + 1, makeReadable);

                            File.WriteAllLines(tempPath, lines);
                            File.Replace(tempPath, filePath, filePath + ".bak");

                            SyncedLabels.TryGetValue(label, out var labelExists2);
                            if (labelExists2 != null)
                            {
                                labelExists2.InScript = true;
                                SaveProgram();
                            }
                            else
                            {
                                var deformattedContent = StripLabelTags(DeformatFromScript(content));
                                Dangerous_ForceSaveToProgram(label, deformattedContent);
                            }
                        }
                        else
                        {
                            DialogBox.Show(
                                $"Missing label formatting!\nIn order to use \"Add to Script\" or \"Marge Scripts\" functionality, your script labels must be formatted with with opening and ending tags.\n{ContentGuides[Type][0]}\nExample:\n    \"YourLabel\": [ {ContentGuides[TypeLabel][0]}\n        // Your content\n    ] {ContentGuides[TypeLabel][1]}\n{ContentGuides[Type][1]}",
                                "Missing Labels");
                        }
                    }
                }
                catch (Exception error)
                {
                    DialogBox.Show(
                        $"Something went wrong while adding new script data for \"{label}\"!\nNo need to panic, the process was cut off before anything saved.\n\n{error}",
                        "Failed to Append Label Data",
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
            else
            {
                DialogBox.Show(
                    "Missing \"Label\" or \"Script\" content...",
                    "Bad Data Input",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
        }

        /// <summary>
        /// Merges the specified script label and content into the corresponding script file, replacing the existing
        /// label data if found.
        /// </summary>
        /// <param name="label">The label identifying the script section to merge.</param>
        /// <param name="content">The new content to merge for the specified label.</param>
        /// <exception cref="Exception">Thrown when the script file cannot be found or an error occurs during the merge process.</exception>
        public void MergeWithScript(string label, string content)
        {
            if (label.Length > 0 && content.Length > 0)
            {
                if (!Files.TryGetValue(Type, out string? filePath) || filePath == null)
                {
                    DialogBox.Show(
                        $"Something has gone wrong while attempting to merge script label data for \"{label}\"!\n\nPath: {filePath}",
                        "Failed to Merge Label Data",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                    throw new Exception($"Bad file data...\n{filePath}");
                }

                string tempPath = Path.GetTempFileName();

                try
                {
                    using (var reader = new StreamReader(filePath))
                    using (var writer = new StreamWriter(tempPath))
                    {
                        string? line;
                        bool startMerging = false;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.EndsWith(ContentGuides[TypeLabel][0], StringComparison.Ordinal) && line.Contains(label, StringComparison.Ordinal))
                            {
                                string trimmed = line.Trim();
                                var result = LabelRegex().Match(trimmed);
                                if (result.Success && result.Groups["label"].Value == label)
                                {
                                    writer.WriteLine(content);
                                    startMerging = true;
                                }
                            }

                            if (startMerging && line.EndsWith(ContentGuides[TypeLabel][1]))
                            {
                                startMerging = false;
                                continue;
                            }

                            if (!startMerging)
                                writer.WriteLine(line);
                        }
                    }

                    File.Replace(tempPath, filePath, filePath + ".bak");
                }
                catch (Exception error)
                {
                    DialogBox.Show(
                        $"Something went wrong while merging script data for \"{label}\"!\nNo need to panic, the process was cut off before anything saved.\n\n{error}",
                        "Failed to Merge Label Data",
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
            else
            {
                DialogBox.Show(
                    "Missing \"Label\" or \"Script\" content...",
                    "Bad Data Input",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
        }

        /// <summary>
        /// Removes a script label and its associated data from the script file.
        /// </summary>
        /// <param name="label">The label identifying the script data to remove.</param>
        /// <param name="save">true to save changes after removal; otherwise, false.</param>
        /// <exception cref="Exception">Thrown when the label is invalid, file data is incorrect, or an error occurs during removal.</exception>
        public void RemoveFromScript(string label, bool save)
        {
            SyncedLabels.TryGetValue(label, out var labelExists);
            if (labelExists != null)
            {
                if (!Files.TryGetValue(Type, out string? filePath) || filePath == null)
                {
                    DialogBox.Show(
                        $"Something has gone wrong while attempting to remove script label data!\n\nPath: {filePath ?? "null"}",
                        "Failed to Remove Label Data",
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
                            bool isStart = line.EndsWith(ContentGuides[TypeLabel][0]) && line.Contains(label);
                            bool isEnd = isRemoving && line.EndsWith(ContentGuides[TypeLabel][1]);

                            if (isStart)
                            {
                                string trimmed = line.Trim();
                                var result = LabelRegex().Match(trimmed);
                                if (result.Success && result.Groups["label"].Value == label)
                                {
                                    isRemoving = true;
                                    if (string.IsNullOrWhiteSpace(lastLine))
                                        lastLine = null;
                                }
                            }

                            if (lastLine != null && !isRemoving)
                                writer.WriteLine(lastLine);

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

                        if (save)
                            SaveProgram();
                    }

                    File.Replace(tempPath, filePath, filePath + ".bak");
                }
                catch (Exception error)
                {
                    DialogBox.Show(
                        $"Something went wrong while removing script data for \"{label}\"!\nNo need to panic, the process was cut off before anything saved.\n\n{error}",
                        "Failed to Remove Label Data",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                    throw new Exception($"Bad removal data...\n{error}");
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
                    "Missing or Invalid \"Label\"...",
                    "Bad Data Input",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Invalid input data...");
            }
        }

        /// <summary>
        /// Saves a script label and its content to the program, prompting the user to confirm saving or merging if the
        /// label already exists.
        /// </summary>
        /// <param name="label">The label identifying the script to save.</param>
        /// <param name="content">The script content associated with the label.</param>
        /// <exception cref="Exception">Thrown when an error occurs while saving the label.</exception>
        public void SaveToProgram(string label, string content)
        {
            if (label.Length > 0 && content.Length > 0)
            {
                try
                {
                    SyncedLabels.TryGetValue(label, out var labelExists);
                    if (labelExists != null)
                    {
                        var warning = DialogBox.Show(
                            $"A script with that label is already saved in the program.\nWould you like to merge them?\n\nLabel: \"{label}\"",
                            "Duplicate Label",
                            DialogButtonDefaults.YesNo,
                            DialogIcon.Warning);

                        if (warning == DialogResult.Yes)
                        {
                            SyncedLabels[label] = new LoadedLabel(label, new StringBuilder(content), CountWords(content), true, false);
                            SaveProgram();
                        }
                        return;
                    }

                    var question = DialogBox.Show(
                        "Would you like to save this label to the program?\nIt will not be automatically synced to the script yet.",
                        "Save to Program",
                        DialogButtonDefaults.YesNo,
                        DialogIcon.Question);

                    if (question == DialogResult.Yes)
                    {
                        SyncedLabels[label] = new LoadedLabel(label, new StringBuilder(content), CountWords(content), true, false);
                        SaveProgram();
                    }
                }
                catch (Exception error)
                {
                    DialogBox.Show(
                        $"Attempted to safely save a label and failed!\n\n{error}",
                        "Failed to Save",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                    throw new Exception($"Bad label data...\n\n{error}");
                }
            }
            else
            {
                DialogBox.Show(
                    "Missing \"Label\" or \"Script\" content...",
                    "Bad Data Input",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
        }

        /// <summary>
        /// Forcibly saves the specified label and content to the program, overwriting existing data if necessary.
        /// </summary>
        /// <remarks>Use with caution as this operation may overwrite existing data and bypass normal
        /// validation.</remarks>
        /// <param name="label">The label identifier to save.</param>
        /// <param name="content">The content associated with the label.</param>
        /// <exception cref="Exception">Thrown when saving the label fails.</exception>
        public void Dangerous_ForceSaveToProgram(string label, string content)
        {
            try
            {
                SyncedLabels[label] = new LoadedLabel(label, new StringBuilder(content), CountWords(content), true, true);
                SaveProgram();
            }
            catch (Exception error)
            {
                DialogBox.Show(
                    $"Attempted to dangerously save a label and failed!\n\n{error}",
                    "Failed to Save",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                throw new Exception($"Bad label data...\n\n{error}");
            }
        }

        /// <summary>
        /// Removes the specified label from the program and optionally saves the changes.
        /// </summary>
        /// <param name="label">The label to remove from the program.</param>
        /// <param name="save">true to save the program after removing the label; otherwise, false.</param>
        public void RemoveFromProgram(string label, bool save = false)
        {
            if (SyncedLabels.ContainsKey(label))
            {
                SyncedLabels.Remove(label);
                if (save) SaveProgram();
            }
        }
        #endregion

        #region Saving Methods
        /// <summary>
        /// Loads label data from the scripts XML file into the SyncedLabels collection, creating necessary directories
        /// if they do not exist.
        /// </summary>
        /// <remarks>Displays an error dialog if loading fails due to an exception.</remarks>
        public void LoadProgram()
        {
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
                Directory.CreateDirectory("data/content");
            }

            if (!Directory.Exists("data/content"))
                Directory.CreateDirectory("data/content");

            if (!File.Exists("data/content/scripts.xml")) return;

            try
            {
                var doc = XDocument.Load("data/content/scripts.xml");
                SyncedLabels = new TypedOrderedDictionary();
                foreach (var el in doc.Descendants("Label"))
                {
                    var name = el.Attribute("Name")?.Value ?? string.Empty;
                    SyncedLabels[name] = new LoadedLabel(
                        name,
                        new StringBuilder(el.Value),
                        int.TryParse(el.Attribute("WordCount")?.Value, out int wc) ? wc : 0,
                        bool.TryParse(el.Attribute("Synced")?.Value, out bool synced) && synced,
                        bool.TryParse(el.Attribute("InScript")?.Value, out bool inScript) && inScript
                    );
                }
            }
            catch (Exception error)
            {
                DialogBox.Show(
                    $"Failed to load saved scripts!\n\n{error}",
                    "Load Error",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
        }

        /// <summary>
        /// Saves the current program state by serializing synchronized labels to an XML file.
        /// </summary>
        /// <remarks>The XML file is saved to 'data/content/scripts.xml' and includes label metadata and
        /// content.</remarks>
        public void SaveProgram()
        {
            XElement[] xElements = new XElement[SyncedLabels.Count];
            int index = 0;
            foreach (LoadedLabel l in SyncedLabels.Values)
            {
                xElements[index++] = new XElement("Label",
                    new XAttribute("Name", l.Name),
                    new XAttribute("WordCount", l.WordCount),
                    new XAttribute("Synced", l.Synced),
                    new XAttribute("InScript", l.InScript),
                    new XCData(l.Content?.ToString() ?? ""));
            }

            SaveData = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Root",
                    new XElement("Scripts", xElements)));

            SaveData.Save("data/content/scripts.xml");
        }
        #endregion
    }

    /// <summary>
    /// Represents a label loaded with associated content, word count, and synchronization state.
    /// </summary>
    public class LoadedLabel
    {
        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }
        /// <summary>Gets or sets the content as a mutable string.</summary>
        public StringBuilder? Content { get; set; }
        /// <summary>Gets or sets the number of words.</summary>
        public int WordCount { get; set; }
        /// <summary>Gets or sets a value indicating whether the data is synchronized.</summary>
        public bool Synced { get; set; }
        /// <summary>Gets or sets a value indicating whether the context is within a script block.</summary>
        public bool InScript { get; set; }

        /// <summary>
        /// Initializes a new instance of the LoadedLabel class with the specified name, content, word count,
        /// synchronization state, and script inclusion.
        /// </summary>
        /// <param name="name">The label name.</param>
        /// <param name="content">The label content.</param>
        /// <param name="words">The number of words in the label.</param>
        /// <param name="synced">true if the label is synchronized; otherwise, false.</param>
        /// <param name="inScript">true if the label is included in the script; otherwise, false.</param>
        public LoadedLabel(string name, StringBuilder content, int words, bool synced, bool inScript)
        {
            Name = name;
            Content = content;
            WordCount = words;
            Synced = synced;
            InScript = inScript;
        }
    }

    /// <summary>Represents a row in a label grid, containing label information and related metadata.</summary>
    public class LabelGridRow
    {
        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }
        /// <summary>Gets or sets the number of words.</summary>
        [DisplayName("Word Count")]
        public int WordCount { get; set; }
        /// <summary>Gets or sets a value indicating whether the data is synchronized.</summary>
        public bool Synced { get; set; }
        /// <summary>Indicates whether the item is included in the script.</summary>
        [DisplayName("In Script")]
        public bool InScript { get; set; }

        /// <summary>
        /// Initializes a new instance of the LabelGridRow class with the specified name, word count, synchronization
        /// state, and script inclusion.
        /// </summary>
        /// <param name="name">The label name.</param>
        /// <param name="count">The word count associated with the label.</param>
        /// <param name="synced">true if the label is synchronized; otherwise, false.</param>
        /// <param name="inScript">true if the label is included in the script; otherwise, false.</param>
        public LabelGridRow(string name, int count, bool synced, bool inScript)
        {
            Name = name;
            WordCount = count;
            Synced = synced;
            InScript = inScript;
        }
    }
}
