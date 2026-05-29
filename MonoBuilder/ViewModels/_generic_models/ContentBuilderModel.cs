using MonoBuilder.Models;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MonoBuilder.ViewModels._generic_models
{
	public interface IContentBuilder<T> where T : INamedEntity
	{
		ContentManipulator GetManipulator(AppSettings settings, string type, ContentBuilderTemplate templatedElements, ObservableCollection<IEnumerable<string?>>? data = null);
		void SaveData(ContentManipulator window, MonoSystem system, string uniqueField, ObservableCollection<T>? content = null);
		Dictionary<string, string> ExtractElements(List<FrameworkElement> control);
		string GetRelativePath(string folderPath, string contentIsActive);
	}

    public class ContentBuilderModel<T> : BaseViewModel where T : INamedEntity, IMultiFile
	{
		internal ContentManipulator GetManipulator(AppSettings settings, string type, ContentBuilderTemplate templatedElements, ObservableCollection<IEnumerable<string?>>? data = null)
		{
			List<Dictionary<string, string?>>? dataParams = data != null
				? templatedElements.GetCombination(data)
				: null;

			return new ContentManipulator(settings, type, new ContentTemplate(
				columnHeaders: templatedElements.Names,
				contentBoxes: templatedElements.BoxTypes,
				placeholders: templatedElements.Placeholders.ToDictionary()),
				dataParams);
		}

		internal void SaveData(ContentManipulator window, MonoSystem<T> system, string uniqueField, ObservableCollection<T>? content = null)
		{
			var tags = new List<string>();
			window.DynamicRows.Values.ToList().ForEach(row =>
			{
				var tagElement = row.FirstOrDefault(e => e is TextBox tb && tb.Name.EndsWith(uniqueField));
				if (tagElement is TextBox tagBox)
				{
					tags.Add(tagBox.Text);
				}
			});

			// Check for duplicate tags
			var duplicateTags = tags
				.GroupBy(t => t)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();
			if (duplicateTags.Count > 0)
			{
				var duplicateList = string.Join("\n", duplicateTags.Select(t => $"- {t}"));
				DialogBox.Show(
					$"Duplicate tags found:\n{duplicateList}\n\nPlease ensure all images have unique tags.",
					"Duplicate Tags Detected",
					DialogButtonDefaults.OK,
					DialogIcon.Warning);
				return;
			}

			var rowArray = window.DynamicRows.Values.ToArray();
			for (int i = 0; i < rowArray.Length; i++)
			{
				ApplyContentChanges(window, rowArray[i], i, content?.ToArray());
			}

			system.SaveData(system._SaveString);
		}

		internal Dictionary<string, string> ExtractElements(List<FrameworkElement> control)
		{
			Dictionary<string, string> elements = [];
			foreach (var element in control)
			{
				if (element is TextBox textBox)
				{
					var split = textBox.Name.Split('_');
					var lastElement = split[split.Length - 1];
					elements.Add(lastElement, textBox.Text);
				}
				else if (element is ComboBox comboBox)
				{
					var split = comboBox.Name.Split('_');
					var lastElement = split[split.Length - 1];

					var files = (ObservableCollection<string>)comboBox.Tag;
					var selectedTag = files[comboBox.SelectedIndex];

					elements.Add(lastElement, selectedTag);
				}
				else if (element is Button button)
				{
					var split = button.Name.Split('_');
					var lastElement = split[split.Length - 1];

					var textContent = (bool)(button.Content.ToString()?.StartsWith('#') ?? false) ?
						(string)button.Content :
						(string)button.Tag;

					elements.Add(lastElement, textContent);
				}
			}

			return elements;
		}

		internal string GetRelativePath(string folderPath, string contentIsActive)
		{
			var userProfile = contentIsActive;
			string? result = Path.GetRelativePath(userProfile, folderPath).Replace('\\', '/');

			return result;
		}

		internal virtual void ApplyContentChanges(ContentManipulator window, List<FrameworkElement> control, int index, T[]? images = null)
		{}
	}

	public class ContentBuilderTemplate
	{
		public List<string> Names { get; set; } = new();
		public string?[]? Data { get; set; }
		public List<ContentBoxType> BoxTypes { get; set; } = new();
		public required (string, string)[] Placeholders { get; set; }

		public ContentBuilderTemplate()
		{}

		#region Name Methods
		public void AddNames(IEnumerable<string> names) => Names = names.ToList();
		public void AddNames(params string[] names) => Names.AddRange(names);
		#endregion

		#region Data Methods
		public void AddData(IEnumerable<string?> data) => Data = data.ToArray();
		public void AddData(params string?[] data) => Data = data;

		public List<Dictionary<string, string?>> GetCombination(ObservableCollection<IEnumerable<string?>> content)
		{
			List<Dictionary<string, string?>> pairs = [];
			foreach (IEnumerable<string?> enumerator in content)
			{
				Dictionary<string, string?> dict = new();
				for (int i = 0; i < Names.Count; i++)
				{
					string name = Names[i];
					string? data = enumerator.ElementAt(i);
					dict[name] = data;
				}

				pairs.Add(dict);
			}

			return pairs;
		}
		#endregion

		#region Box Methods
		public void AddBoxTypes(IEnumerable<ContentBoxType> boxTypes) => BoxTypes = boxTypes.ToList();
		public void AddBoxTypes(params ContentBoxType[] boxTypes) => BoxTypes.AddRange(boxTypes);
		#endregion

		#region Placeholder Methods
		public void AddPlacheolders(IEnumerable<(string, string)> placeholders) => Placeholders = placeholders.ToArray();
		public void AddPlaceholders(params (string, string)[] placeholders) => Placeholders = placeholders;
		#endregion
	}
}
