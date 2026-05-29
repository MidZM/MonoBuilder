using MonoBuilder.Commands;
using MonoBuilder.Models;
using MonoBuilder.Models.character_management;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.image_management;
using MonoBuilder.Views.ViewUtils;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MonoBuilder.ViewModels.SettingsModel
{
    public partial class SettingsViewModel
    {
		public RelayCommand SaveDataCommand { get; }
		
		public RelayCommand CheckBoxCommand { get; }
		public RelayCommand TextBoxCommand { get; }
		public RelayCommand NumberBoxCommand { get; }

		private bool _autoSyncChecked = true;
		public bool AutoSyncChecked
		{
			get => _autoSyncChecked;
			set => SetProperty(ref _autoSyncChecked, value);
		}

		private bool _colorEnabledChecked = true;
		public bool ColorEnabledChecked
		{
			get => _colorEnabledChecked;
			set => SetProperty(ref _colorEnabledChecked, value);
		}

		public string[] IndentationTypes { get; } =
		{
			"Spaces",
			"Tabs"
		};

		private string _selectedIndentation = "Spaces";
		public string SelectedIndentation {
			get => _selectedIndentation;
			set
			{
				if (SetProperty(ref _selectedIndentation, value))
				{
					ApplicationSettings.SetIndentationType(value);
					Converter.ChangeIndentationType(value);

					ChangesMade = true;
				}
			}
		}

		private string _spacesAmount = "4";
		public string SpacesAmount
		{
			get => _spacesAmount;
			set
			{
				_spacesAmount = string.Empty;
				if (value == string.Empty) return;

				int.TryParse(value.Length > 1
					? value[1..].ToString()
					: value.ToString(), out int intValue);

				if (intValue < 2) intValue = 2;
				if (intValue > 8) intValue = 8;

				_spacesAmount = $"{intValue}";
			}
		}

		#region Utility Methods
		protected override List<string> GetContentNames()
			=> ["Name", "Tag", "Color", "Directory", "File"];
		protected override List<ContentBoxType> GetContentBoxTypes()
			=> [ContentBoxType.RequiredTextBox,
				ContentBoxType.RequiredTextBox,
				ContentBoxType.ColorButton,
				ContentBoxType.TextBox,
				ContentBoxType.FileBox];
		protected override (string, string)[] GetPlaceholderValues()
			=> [("Name", "Enter Character Name"),
				("Tag", "Enter Character Tag"),
				("Directory", "Path to Character Assets")];
		protected override string GetContentUniqueField()
			=> "Tag";
		protected override ObservableCollection<IEnumerable<string?>> GenerateContentFieldData(string folderPath)
			=> new(
				SelectedEntities.Select(i => new[]
				{
					i.Name,
					i.Tag,
					i.Color,
					i.Directory,
					i.FileKey
				}));

		protected override void ApplyContentChanges(ContentManipulator window, List<FrameworkElement> control, int index, Character[]? entities = null)
		{
			var elements = ExtractElements(control);

			elements.TryGetValue("Name", out string? name);
			elements.TryGetValue("Tag", out string? tag);
			elements.TryGetValue("Color", out string? color);
			elements.TryGetValue("Directory", out string? directory);
			elements.TryGetValue("File", out string? fileKey);

			if (name != null && tag != null)
			{
				var character = new Character(name, tag, color, directory);
				if (window.ModifyingContent?.Count > 0)
				{
					window.ModifyingContent[index].TryGetValue("Tag", out string? tagValue);
					var modifierCharacter = entities?.FirstOrDefault(c => c.Tag == tagValue);

					if (modifierCharacter != null)
					{
						if (fileKey != null)
						{
							character.FileKey = fileKey;
						}

						DataController.UpdateData(modifierCharacter.EntityID, character);
					}
				}
				else
				{
					if (fileKey != null)
					{
						character.FileKey = fileKey;
					}

					DataController.AddData(character);
				}
			}
		}

		protected override void RunDataImportSetup(string character, Dictionary<string, Character> characterData)
		{
			int characterId = DataController.DataMode.Collection.First(c => c.Tag == character).EntityID;
			string name = characterData[character].Name;
			string tag = characterData[character].Tag;
			string fileKey = characterData[character].FileKey;
			string? color = characterData[character].Color;
			string? directory = characterData[character].Directory;

			Character newCharacter = new(name, tag, color, directory)
			{
				EntityID = characterId,
				FileKey = fileKey,
				IsSynced = true
			};

			DataController.UpdateData(characterId, newCharacter);

			characterData.Remove(character);
		}
		#endregion

		#region Execution Commands
		private void ExecuteSaveDataCommand(object? withClose)
		{
			bool regexError = false;
			var converterRules = Converter.ConversionRules.ToList();
			for (int i = 0; i < converterRules.Count; i++)
			{
				var saveRule = ConversionRules[i];

				if (!saveRule.PatternMismatch)
				{
					Converter.ConversionRules[i] = saveRule;
				}
				else
				{
					DialogBox.Show(
						"One or more Regex Patterns are invalid...!\nDouble check your Regex Patterns and try again.",
						"Regex Pattern Mismatch",
						DialogButtonDefaults.OK,
						DialogIcon.Error);
					return;
				}
			}

			if (regexError)
			{
				DialogBox.Show(
					"Something went wrong while saving Regexes...\nIt's likely there is a typo, error, or missing input box. All other Regexes that weren't broken should have saved correctly.\n\nIf the issue persists, please contact the developer.",
					"Error",
					DialogButtonDefaults.OK,
					DialogIcon.Warning);
			}

			Converter.SaveSettings();
			ApplicationSettings.SaveDirectories();

			if (withClose is string && string.Equals((string)withClose, "true", StringComparison.OrdinalIgnoreCase))
			{
				Owner.Close();
			}
		}

		private void ExecuteCheckBoxCommand(object? element)
		{
			if (element is CheckBox checkBox)
			{
				var shouldSetValue = checkBox.IsChecked == true;
				if (string.Equals(checkBox.Name, "ShouldAutoSync", StringComparison.Ordinal))
				{
					ApplicationSettings.SetAutoSyncLabels(shouldSetValue);
					Converter.SetAutoSyncLabels(shouldSetValue);
					AutoSyncChecked = shouldSetValue;
				}
				else if (string.Equals(checkBox.Name, "ShouldColor", StringComparison.Ordinal))
				{
					ApplicationSettings.SetColorFormatting(shouldSetValue);
					Converter.SetIsFormattingColor(shouldSetValue);
					ColorEnabledChecked = shouldSetValue;
				}

				ChangesMade = true;
			}
		}

		private void ExecuteTextBoxComand(object? parameters = null)
		{

		}

		private void ExecuteNumberBoxCommand(object? parameters = null)
		{

		}
		#endregion
	}
}
