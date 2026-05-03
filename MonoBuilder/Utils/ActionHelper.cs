using MonoBuilder.Screens.ScreenUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace MonoBuilder.Utils
{
    public class ActionHelper
    {
        public ObservableCollection<IAction> AllActions { get; } = new();
        private XDocument SaveData { get; set; } = new();

        public void AddAction(IAction action)
        {
            AllActions.Add(action);
        }

        public IEnumerable<IAction> GetFilteredAndSorted(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return AllActions.OrderByDescending(a => a.UsageCount).ThenBy(a => a.Name);
            }

            return AllActions
                .Where(a => a.MatchesPrefix(prefix))
                .OrderByDescending(a => a.UsageCount)
                .ThenBy(a => a.Name);
        }

        public IAction? GetAction(string name)
        {
            return AllActions.FirstOrDefault(a => a.Name == name);
        }

        public void IncrementUsage(IAction action)
        {
            action.UsageCount++;
        }

        public void LoadActions()
        {

            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
                Directory.CreateDirectory("data/content");
            }

            if (!Directory.Exists("data/content"))
            {
                Directory.CreateDirectory("data/content");
            }

            if (!File.Exists("data/actions.xml")) return;

            try
            {
                var doc = XDocument.Load("data/actions.xml");
                AllActions.Clear();
                foreach (var element in doc.Descendants("Action"))
                {
                    string name = element.Attribute("Name")?.Value ?? "";
                    string type = element.Attribute("Type")?.Value ?? "";
                    string? placeholder = element.Attribute("Placeholder")?.Value;
                    int.TryParse(element.Attribute("UsageCount")?.Value, out int usageCount);
                    AllActions.Add(type switch
                    {
                        "paired" => new PairedAction(name) { UsageCount = usageCount },
                        "value" => new ValueAction(name, placeholder ?? "") { UsageCount = usageCount },
                        "closing" => new SelfClosingAction(name) { UsageCount = usageCount },
                        _ => throw new Exception($"Unknown action type: {type}")
                    });
                }
            }
            catch (Exception error)
            {
                DialogBox.Show(
                    $"Something went wrong while loading actions:\n\n{error.Message}",
                    "Error",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
        }

        public void SaveActions()
        {

        }
    }

    public interface IAction
    {
        string Name { get; }
        string DisplayName { get; }
        int UsageCount { get; set; }
        bool MatchesPrefix(string prefix);
        InsertionResult GetInsertionResult();
    }

    public struct InsertionResult
    {
        public string TextToInsert { get; }
        public int CursorOffset { get; }
        public int SelectionLength { get; }
        public string PlaceholderText { get; }

        public InsertionResult(string text, int cursorOffset, int selectionLength = 0, string placeholderText = "")
        {
            TextToInsert = text;
            CursorOffset = cursorOffset;
            SelectionLength = selectionLength;
            PlaceholderText = placeholderText;
        }
    }

    public class PairedAction : IAction
    {
        public string Name { get; }
        public int UsageCount { get; set; }

        public PairedAction(string name)
        {
            Name = name;
        }

        public string DisplayName => $"{{{Name}}}";

        public bool MatchesPrefix(string prefix) =>
            Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

        public InsertionResult GetInsertionResult()
        {
            string open = $"{{{Name}}}";
            string close = $"{{/{Name}}}";
            return new InsertionResult(open + close, open.Length);
        }
    }

    public class ValueAction : IAction
    {
        public string Name { get; }
        private readonly string _placeholder;
        public int UsageCount { get; set; }

        public ValueAction(string name, string placeholder)
        {
            Name = name;
            _placeholder = placeholder;
        }

        public string DisplayName => $"{{{Name}:{_placeholder}}}";

        public bool MatchesPrefix(string prefix) =>
            Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

        public InsertionResult GetInsertionResult()
        {
            string text = $"{{{Name}:{_placeholder}}}";
            int cursorOffset = $"{{{Name}:".Length;
            return new InsertionResult(text, cursorOffset, _placeholder.Length, _placeholder);
        }
    }

    public class SelfClosingAction : IAction
    {
        public string Name { get; }
        public int UsageCount { get; set; }

        public SelfClosingAction(string name)
        {
            Name = name;
        }

        public string DisplayName => $"{{{Name}/}}";

        public bool MatchesPrefix(string prefix) =>
            Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

        public InsertionResult GetInsertionResult()
        {
            string text = $"{{{Name}}}";
            return new InsertionResult(text, text.Length);
        }
    }
}
