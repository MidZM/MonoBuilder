using MonoBuilder.Screens.ScreenUtils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MonoBuilder.Utils
{

    public class ConversionRule
    {
        public string? Name { get; set; }
        public Regex Pattern { get; set; } = new Regex(@"");
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; }
    }

    public class LineFormatInfo
    {
        public string Text { get; set; } = "";
        public Color? TextColor { get; set; }
    }

    public partial class ScriptConversion
    {
        private Characters CharacterDatabase { get; set; }

        public List<ConversionRule> ConversionRules { get; set; } = new()
        {
            new() { Name = "Comment",               Pattern = new Regex(@"^\s*//.*$"),          Priority = 1 },
            new() { Name = "Empty",                 Pattern = new Regex(@"^\s*$"),              Priority = 2 },
            new() { Name = "StringAction",          Pattern = new Regex(@"^\[(?<text>.+)\]$"),  Priority = 3 },
            new() { Name = "ObjectEnclosedAction",  Pattern = new Regex(@"^\{(?<text>.+)\}$"),  Priority = 4 },
            new() { Name = "ObjectActionOpen",      Pattern = new Regex(@"^\{$"),               Priority = 5 },
            new() { Name = "ObjectActionClose",     Pattern = new Regex(@"^\}$"),               Priority = 6 },
            new() { Name = "CharacterLine",         Pattern = new Regex(@"^(?<character>.+?)\s*-\s*(?<text>.+)$"),  Priority = 7 },
            new() { Name = "Narration",             Pattern = new Regex(@"^(?<text>.+)$"),      Priority = 8 },
        };

        private IOrderedEnumerable<ConversionRule>? SortedRules { get; set; } = null;

        private bool PreventIndent { get; set; } = false;
        private int IndentationAmount { get; set; } = 4;
        private string IndentationType { get; set; } = "Spaces";
        private bool IsColorFormatting { get; set; } = true;
        private bool IsAutoSyncLabels { get; set; } = true;

        private XDocument data { get; set; } = new XDocument();

        [GeneratedRegex(@"^(function)|(.*=>)")]
        private static partial Regex FunctionCheck();

        public ScriptConversion(Characters characters)
        {
            CharacterDatabase = characters;
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                if (!Directory.Exists("data"))
                {
                    Directory.CreateDirectory("data");
                }

                if (!File.Exists("data/scripts.xml"))
                {
                    SaveSettings();
                    return;
                }

                data = XDocument.Load("data/scripts.xml");

                List<XElement> ruleList = data.Descendants("Rule").ToList();
                foreach (XElement rule in ruleList)
                {
                    string? name = (string?)rule.Attribute("Name");
                    Regex? pattern = new Regex((string?)rule.Attribute("Pattern") ?? "");
                    bool? colorIsEnabled = (bool?)rule.Attribute("IsEnabled") ?? true;
                    int? priority = int.Parse((string?)rule.Attribute("Priority") ?? "-1");

                    var Rule = ConversionRules.FirstOrDefault<ConversionRule>(r => r.Name == name);
                    if (Rule != null)
                    {
                        Rule.Pattern = pattern;
                        Rule.IsEnabled = (bool)colorIsEnabled;
                        Rule.Priority = (int)priority;
                    }
                }
            }
            catch (FileNotFoundException error)
            {
                Console.WriteLine(error);
            }
            catch (XmlException error)
            {
                Console.WriteLine(error);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }
        }

        public void SaveSettings()
        {
            data = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Root",
                    new XElement("Conversions",
                        ConversionRules.Select(r => new XElement("Rule",
                            new XAttribute("Name", r.Name ?? "N/A"),
                            new XAttribute("Pattern", r.Pattern),
                            new XAttribute("IsEnabled", r.IsEnabled),
                            new XAttribute("Priority", r.Priority)
                        ))
                    )
                )
            );

            data.Save("data/scripts.xml");
        }

        public string Convert(string scriptInput)
        {
            SortedRules = ConversionRules.Where(r => r.IsEnabled).OrderBy(r => r.Priority);
            var outputLines = new List<string>();

            PreventIndent = true;

            foreach (var line in CollectEntries(scriptInput))
            {
                var processedLine = ProcessEntry(line);
                if (processedLine != null)
                {
                    outputLines.Add(processedLine);
                }
            }

            PreventIndent = false;

            return BuildOutput(outputLines);
        }

        public string Convert(string label, string scriptInput)
        {
            SortedRules = ConversionRules.Where(r => r.IsEnabled).OrderBy(r => r.Priority);
            PreventIndent = false; // Fallback in-case of an issue.

            var outputLines = new List<string>();

            foreach (var line in CollectEntries(scriptInput))
            {
                var processedLine = ProcessEntry(line);
                if (processedLine != null)
                {
                    outputLines.Add(processedLine);
                }
            }

            return BuildOutput(label, outputLines);
        }

        public List<LineFormatInfo> ConvertWithFormtting(string label, string scriptInput)
        {
            SortedRules = ConversionRules.Where(r => r.IsEnabled).OrderBy(r => r.Priority);
            PreventIndent = false; // Fallback in-case of an issue.

            List<LineFormatInfo> outputLines = new();

            outputLines.Add(new LineFormatInfo
            {
                Text = $"\"{label}\": [ // START_LABEL",
                TextColor = null
            });

            List<(string text, Color? color)> processedLines = new();

            foreach (string line in CollectEntries(scriptInput))
            {
                var (processedLine, color) = ProcessEntryWithColor(line);
                if (processedLine != null)
                {
                    processedLines.Add((processedLine, color));
                }
            }

            for (int i = 0; i < processedLines.Count; i++)
            {
                var (line, color) = processedLines[i];

                // Empty lines get a blank space.
                if (string.IsNullOrWhiteSpace(line))
                {
                    outputLines.Add(new LineFormatInfo { Text = "", TextColor = null });
                    continue;
                }

                // Ending commas on the last line of dialogue need to be removed.
                if (i == processedLines.Count - 1 || IsLastDialogueLineInList(processedLines, i))
                {
                    line = line.TrimEnd(',');
                }

                outputLines.Add(new LineFormatInfo { Text = line, TextColor = color });
            }

            outputLines.Add(new LineFormatInfo
            {
                Text = "], // END_LABEL",
                TextColor = null
            });

            return outputLines;
        }

        public string Deconvert(string scriptInput)
        {
            var lines = scriptInput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var outputLines = new List<string>();

            var blockBuffer = new List<string>();
            int braceDepth = 0;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                // Only count structural braces when we are already inside an open
                // multi-line block, OR when this line explicitly begins with '{',
                // indicating it is intentionally an object action line.
                // Lines that do not start with '{' (dialogue, narration, etc.) may
                // legitimately contain '{' or '}' in their text, which must not
                // affect brace depth tracking.
                bool isObjectActionLine = trimmed.StartsWith('{') ||
                                          trimmed.StartsWith("function") ||
                                          trimmed.StartsWith("(");

                if (isObjectActionLine || blockBuffer.Count > 0)
                {
                    braceDepth += trimmed.Count(c => c == '{');
                    braceDepth -= trimmed.Count(c => c == '}');
                }

                if (braceDepth > 0 || blockBuffer.Count > 0)
                {
                    blockBuffer.Add(line);

                    if (braceDepth <= 0)
                    {
                        outputLines.Add(string.Join(Environment.NewLine, blockBuffer));
                        blockBuffer.Clear();
                        braceDepth = 0;
                    }

                    continue;
                }

                string? deprocessed = DeprocessLine(trimmed);
                if (deprocessed != null)
                {
                    outputLines.Add(deprocessed);
                }
            }

            return string.Join(Environment.NewLine, outputLines);
        }

        #region Variable Helper Functions
        private bool ValidateDataType(string type, object value)
        {
            string TypeOf = type.ToLower();
            if (TypeOf == "indentation_amount")
            {
                if (value is int integer)
                {
                    return (integer > 1 && integer < 9);
                }

                DialogBox.Show(
                    $"Invalid type and value check!\n{type} must pair with an integer above 1 and below 9.\nValue ({value}) is \"{value.GetType()}\".",
                    "Bad Typing");
                return false;
            }

            if (TypeOf == "indentation_type")
            {
                if (value is string str)
                {
                    return (str == "Spaces" || str == "Tab");
                }

                DialogBox.Show(
                    $"Invalid type and value check!\n{type} must pair with a string of \"Spaces\" or \"Tab\".\nValue ({value}) is \"{value.GetType()}\".",
                    "Bad Typing");
                return false;
            }

            if (TypeOf == "color_formatting" ||
                TypeOf == "auto_sync_labels")
            {
                if (value is bool boolean)
                {
                    return true;
                }

                DialogBox.Show(
                    $"Invalid type and value check!\n{type} must pair with a string of \"false\" or \"true\".\nValue ({value}) is \"{value.GetType()}\".",
                    "Bad Typing");
                return false;
            }

            DialogBox.Show($"Invalid type checked!\n{type} must be a valid data type check...");
            return false;
        }

        public string GetIndentationType()
        {
            return IndentationType;
        }

        public void ChangeIndentationType(string type)
        {
            if (ValidateDataType("INDENTATION_TYPE", type))
            {
                if (type.ToLower() == "tab")
                {
                    IndentationType = "Tab";
                }
                else
                {
                    IndentationType = "Spaces";
                }
            }
        }

        public int GetIndentationAmount()
        {
            return IndentationAmount;
        }

        public void ChangeIndentationAmount(int value)
        {
            if (ValidateDataType("INDENTATION_AMOUNT", value))
            {
                IndentationAmount = value;
            }
        }

        private string AddIndentation()
        {
            if (!PreventIndent)
            {
                if (IndentationType == "Tab")
                {
                    return $"\t";
                }
                else
                {
                    return new string(' ', IndentationAmount);
                }
            }

            return string.Empty;
        }

        public bool CheckIsFormattingColor()
        {
            return IsColorFormatting;
        }

        public void SetIsFormattingColor(bool set)
        {
            if (ValidateDataType("COLOR_FORMATTING", set))
            {
                IsColorFormatting = set;
            }
        }

        public bool CheckIsAutoSyncLabels()
        {
            return IsAutoSyncLabels;
        }

        public void SetAutoSyncLabels(bool set)
        {
            if (ValidateDataType("AUTO_SYNC_LABELS", set))
            {
                IsAutoSyncLabels = set;
            }
        }
        #endregion

        #region Conversion Helper Functions
        private IEnumerable<string> CollectEntries(string scriptInput)
        {
            var lines = scriptInput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var blockBuffer = new List<string>();
            int braceDepth = 0;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                // Only count structural braces when we are already inside an open
                // multi-line block, OR when this line explicitly begins with '{',
                // indicating it is intentionally an object action line.
                // Lines that do not start with '{' (dialogue, narration, etc.) may
                // legitimately contain '{' or '}' in their text, which must not
                // affect brace depth tracking.
                bool isObjectActionLine = trimmed.StartsWith('{') ||
                                          trimmed.StartsWith("function") ||
                                          trimmed.StartsWith("(");

                if (isObjectActionLine || blockBuffer.Count > 0)
                {
                    braceDepth += trimmed.Count(c => c == '{');
                    braceDepth -= trimmed.Count(c => c == '}');
                }

                if (braceDepth > 0 || blockBuffer.Count > 0)
                {
                    blockBuffer.Add(line);

                    if (braceDepth <= 0)
                    {
                        yield return string.Join(Environment.NewLine, blockBuffer);
                        blockBuffer.Clear();
                        braceDepth = 0;
                    }
                }
                else
                {
                    yield return line;
                }
            }

            if (blockBuffer.Count > 0)
            {
                yield return string.Join(Environment.NewLine, blockBuffer);
            }
        }

        private string? ProcessEntry(string entry)
        {
            if (entry.Contains('\n'))
            {
                return FormatObjectActions(entry, "multiline");
            }

            return ProcessLine(entry);
        }

        private (string? line, Color? color) ProcessEntryWithColor(string entry)
        {
            if (entry.Contains('\n'))
            {
                return (FormatObjectActions(entry, "multiline"), Color.Cyan);
            }

            return ProcessLineWithColor(entry);
        }

        private (string? line, Color? color) ProcessLineWithColor(string line)
        {
            if (SortedRules != null)
            {
                foreach (ConversionRule rule in SortedRules)
                {
                    var match = rule.Pattern.Match(line);

                    if (match.Success)
                    {
                        return rule.Name switch
                        {
                            "Comment"               => (line, Color.Olive),
                            "Empty"                 => ("", null),
                            "StringAction"          => (FormatStringAction(match.Groups["text"].Value), Color.LimeGreen),
                            "ObjectEnclosedAction"  => (FormatObjectActions(match.Groups["text"].Value, "enclosed"), Color.Cyan),
                            "ObjectActionOpen"      => (FormatObjectActions(match.Groups["text"].Value, "open"), Color.Cyan),
                            "ObjectActionClose"    => (FormatObjectActions(match.Groups["text"].Value, "closed"), Color.Cyan),
                            "CharacterLine"         => (FormatCharacterLine(
                                                            match.Groups["character"].Value,
                                                            match.Groups["text"].Value), null),
                            "Narration"             => (FormatNarration(match.Groups["text"].Value), Color.DarkOrange),
                            _ => (null, null)
                        };
                    }
                }
            }

            return (null, null);
        }

        private bool IsLastDialogueLineInList(List<(string text, Color? color)> lines, int currentIndex)
        {
            for (int i = currentIndex + 1; i < lines.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i].text) && !lines[i].text.Trim().StartsWith("//"))
                {
                    return false;
                }
            }
            return true;
        }

        private string? ProcessLine(string line)
        {
            if (SortedRules != null)
            {
                foreach (ConversionRule rule in SortedRules)
                {
                    Regex regex = rule.Pattern;
                    var match = regex.Match(line);

                    if (match.Success)
                    {
                        return rule.Name switch
                        {
                            "Comment"               => line,
                            "Empty"                 => "",
                            "StringAction"          => FormatStringAction(match.Groups["text"].Value),
                            "ObjectEnclosedAction"  => FormatObjectActions(match.Groups["text"].Value, "enclosed"),
                            "ObjectActionOpen"      => FormatObjectActions(match.Groups["text"].Value, "open"),
                            "ObjectActionClose"    => FormatObjectActions(match.Groups["text"].Value, "closed"),
                            "CharacterLine"         => FormatCharacterLine(
                                                        match.Groups["character"].Value,
                                                        match.Groups["text"].Value
                            ),
                            "Narration"             => FormatNarration(match.Groups["text"].Value),
                            _ => null
                        };
                    }
                }
            }

            return null;
        }

        private string FormatCharacterLine(string characterName, string text)
        {
            var character = CharacterDatabase.AllCharacters
                .FirstOrDefault(c => c.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));

            string tag = character?.Tag ?? characterName.ToLower().Substring(0, Math.Min(3, characterName.Length));

            return $"{AddIndentation()}\"{tag} {text.Replace("\"", "\\\"")}\",";
        }

        private string FormatNarration(string text)
        {
            return $"{AddIndentation()}\"{text.Replace("\"", "\\\"")}\",";
        }

        private string FormatStringAction(string text)
        {
            return $"{AddIndentation()}\"{text.Replace("\"", "\\\"")}\",";
        }

        private string FormatObjectActions(string text, string type)
        {

            string indent = AddIndentation();

            return type switch
            {
                "enclosed"  => $"{indent}{{{text}}},",
                "open"      => $"{indent}{{",
                "closed"    => $"{indent}}},",
                "multiline" => FormatMultilineBlock(text, indent),
                _           => $"{indent}{{{text}}},"
            };
        }

        private string FormatMultilineBlock(string rawBlock, string baseIndent)
        {
            var lines = rawBlock.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                string reindented = $"{baseIndent}{lines[i]}";

                if (i == lines.Length - 1)
                {
                    reindented = reindented.TrimEnd(',') + ",";
                }

                sb.AppendLine(reindented);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private string BuildOutput(List<string> lines)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                // Certain lines should be colored and have a special coloring format.
                if (line.StartsWith("$="))
                {
                    line = Regex.Replace(line, @"^(\$=[A-Za-z]+)", "");
                }

                // Empty lines just get a blank space.
                if (string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine();
                    continue;
                }

                // Ending commas on the last line of dialogue need to be removed.
                if (i == lines.Count - 1 || IsLastDialogueLine(lines, i))
                {
                    line = line.TrimEnd(',');
                }

                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        private string BuildOutput(string label, List<string> lines)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\"{label}\": [ // START_LABEL");

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                // Certain lines should be colored and have a special coloring format.
                if (line.StartsWith("$="))
                {
                    line = Regex.Replace(line, @"^(\$=[A-Za-z]+)", "");
                }

                // Empty lines just get a blank space.
                if (string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine();
                    continue;
                }

                // Ending commas on the last line of dialogue need to be removed.
                if (i == lines.Count - 1 || IsLastDialogueLine(lines, i))
                {
                    line = line.TrimEnd(',');
                }

                sb.AppendLine(line);
            }

            sb.AppendLine("], // END_LABEL");
            return sb.ToString();
        }

        private bool IsLastDialogueLine(List<string> lines, int currentIndex)
        {
            for (int i = currentIndex + 1; i < lines.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]) && !lines[i].Trim().StartsWith("//"))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Deconversion Helper Functions
        private string? DeprocessLine(string line)
        {
            string type = CheckType(line);
            return type switch
            {
                "string"    => DeformatStringAction(line),
                "object"    => DeformatObjectAction(line),
                "function"  => DeformatFunctionAction(line),
                _ => null
            };
        }

        private string CheckType(string line)
        {
            if (IsStringLine(line)) return "string";
            if (IsObjectAction(line)) return "object";
            if (IsFunctionLine(line)) return "function";

            return "null";
        }

        private string? DeformatStringAction(string line)
        {
            string newLine = StripEndComma(line);
            newLine = StripStringFormat(newLine);

            if (string.IsNullOrEmpty(newLine))
                return newLine;

            string firstWord = newLine.Split(' ')[0];
            (bool isCharacterDialog, Character? character) = IsCharacterDialog(firstWord);

            if (isCharacterDialog && character != null)
            {
                string dialogText = newLine.Substring(firstWord.Length).TrimStart();
                return $"{character.Name} - {dialogText}"; // Temporary until Regex check/reformat
            }
            else if (IsActionDialog(newLine[0]))
            {
                return $"[{newLine}]"; // Temporary until Regex check/reformat
            }
            else
            {
                return newLine;
            }
        }

        private string? DeformatObjectAction(string line)
        {
            return IsOpenEndedObjectAction(line) ? null : line;
        }

        private string? DeformatFunctionAction(string line)
        {
            bool isArrow = !line.TrimStart().StartsWith("function");
            return IsOpenEndedFunction(line, isArrow) ? null : line;
        }

        private string StripEndComma(string line)
        {
            if (line.EndsWith(","))
            {
                line = line.Substring(0, line.Length - 1);
            }

            return line;
        }

        private string StripStringFormat(string line)
        {
            if (line.Length >= 2 && (
                (line.StartsWith("'") && line.EndsWith("'")) ||
                (line.StartsWith("\"") && line.EndsWith("\"")) ||
                (line.StartsWith("`") && line.EndsWith("`"))))
            {
                return line.Substring(1, line.Length - 2);
            }

            return line;
        }

        private bool IsStringLine(string line)
        {
            return line.StartsWith("\"") || line.StartsWith("'") || line.StartsWith("`");
        }

        private (bool, Character?) IsCharacterDialog(string? firstWord)
        {
            var character = CharacterDatabase.AllCharacters.FirstOrDefault(c => c.Tag.Equals(firstWord));
            return (character != null, character);
        }

        private bool IsActionDialog(char firstLetter)
        {
            return Char.IsLower(firstLetter);
        }

        private bool IsObjectAction(string line)
        {
            return line.StartsWith("{");
        }

        private bool IsOpenEndedObjectAction(string line)
        {
            return !line.EndsWith("}");
        }

        private bool IsFunctionLine(string line)
        {
            return FunctionCheck().Match(line).Success;
        }

        private bool IsOpenEndedFunction(string line, bool isArrow)
        {
            if (!isArrow)
            {
                return !line.EndsWith("}");
            }

            return !((line.Contains("{") && !line.EndsWith("}")) || FunctionCheck().Match(line).Success);
        }
        #endregion
    }
}
