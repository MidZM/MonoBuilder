using MonoBuilder.Views.ViewUtils;
using MonoBuilder.Models.generics.structs;
using MonoBuilder.Models.generics.enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace MonoBuilder.Models.helpers
{
    public class ActionHelper
    {
        public ObservableCollection<IAction> AllActions { get; } = new();
        private XDocument SystemData { get; set; } = new();

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
                SystemData = XDocument.Load("data/actions.xml");
                AllActions.Clear();
                foreach (var element in SystemData.Descendants("Action"))
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
}
