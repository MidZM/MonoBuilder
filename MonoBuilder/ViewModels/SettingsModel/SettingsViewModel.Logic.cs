using MonoBuilder.Commands;
using MonoBuilder.Models;
using MonoBuilder.Models.character_management;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Views.ViewUtils;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace MonoBuilder.ViewModels.SettingsModel
{
	// ========================================================
	// Change this class to a `TabbedContentModel<Character>`
	// ========================================================
    public partial class SettingsViewModel
    {
		public RelayCommand MoveCharacterCommand { get; }

		public RelayCommand AddCharactersCommand { get; }
		public RelayCommand ModifyCharactersCommand { get; }
		public RelayCommand RemoveCharactersCommand { get; }
		public RelayCommand SaveToScriptCommand { get; }
		public RelayCommand ImportCharactersCommand { get; }

		public RelayCommand SaveDataCommand { get; }
		public RelayCommand ExitCommand { get; }
		
		public RelayCommand CheckBoxCommand { get; }
		public RelayCommand TextBoxCommand { get; }
		public RelayCommand NumberBoxCommand { get; }

		private bool _autoSyncChecked = true;
		public bool AutoSyncChecked
		{
			get => _autoSyncChecked;
			set
			{
				if (SetProperty(ref _autoSyncChecked, value))
				{
					OnPropertyChanged(nameof(AutoSyncChecked));
				}
			}
		}

		private bool _colorEnabledChecked = true;
		public bool ColorEnabledChecked
		{
			get => _colorEnabledChecked;
			set
			{
				if (SetProperty(ref _colorEnabledChecked, value))
				{
					OnPropertyChanged(nameof(ColorEnabledChecked));
				}
			}
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
					OnPropertyChanged(nameof(SelectedIndentation));

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
		private ContentManipulator GetManipulator(ObservableCollection<Character>? characters = null)
		{
			List<Dictionary<string, string?>>? characterParams = characters != null
				? characterParams = characters
					.Select(c => new Dictionary<string, string?>()
					{
						["Name"] = c.Name,
						["Tag"] = c.Tag,
						["Color"] = c.Color,
						["Directory"] = c.Directory,
						["File"] = c.FileKey
					})
					.ToList()
				: null;

			return new ContentManipulator(ApplicationSettings, "Characters", new ContentTemplate(
				columnHeaders:
				[
					"Name",
					"Tag",
					"Color",
					"Directory",
					"File"
				],
				contentBoxes:
				[
					ContentBoxType.RequiredTextBox,
					ContentBoxType.RequiredTextBox,
					ContentBoxType.ColorButton,
					ContentBoxType.TextBox,
					ContentBoxType.FileBox
				],
				placeholders: new()
				{
					["Name"] = "Enter Character Name",
					["Tag"] = "Enter Character Tag",
					["Directory"] = "Path to Character Assets"
				}),
				characterParams);
		}

		private void SaveCharacters(ContentManipulator window, ObservableCollection<Character>? characters = null)
		{
			var tags = new List<string>();
			window.DynamicRows.Values.ToList().ForEach(row =>
			{
				var tagElement = row.FirstOrDefault(e => e is TextBox tb && tb.Name.EndsWith("Tag"));
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
					$"Duplicate tags found:\n{duplicateList}\n\nPlease ensure all characters have unique tags.",
					"Duplicate Tags Detected",
					DialogButtonDefaults.OK,
					DialogIcon.Warning);
				return;
			}

			var rowArray = window.DynamicRows.Values.ToArray();
			for (int i = 0; i < rowArray.Length; i++)
			{
				ApplyCharacterChanges(window, rowArray[i], i, characters);
			}

			CharacterData.SaveData();
			Converter.UnsetCharacterList();
		}

		private Dictionary<string, string> ExtractElements(List<FrameworkElement> control)
		{
			var elements = new Dictionary<string, string>();
			foreach (var element in control)
			{
				if (element is TextBox textBox)
				{
					var split = textBox.Name.Split('_');
					var lastElement = split[split.Length - 1];
					elements.Add(lastElement, textBox.Text);
				}
				else if (element is Button button)
				{
					var split = button.Name.Split('_');
					var lastElement = split[split.Length - 1];
					elements.Add(lastElement, (string)button.Content);
				}
				else if (element is ComboBox comboBox)
				{
					var split = comboBox.Name.Split('_');
					var lastElement = split[split.Length - 1];

					var files = (ObservableCollection<string>)comboBox.Tag;
					var selectedTag = files[comboBox.SelectedIndex];

					elements.Add(lastElement, selectedTag);
				}
			}

			return elements;
		}

		private void ApplyCharacterChanges(ContentManipulator window, List<FrameworkElement> control, int index, ObservableCollection<Character>? characters = null)
		{
			var elements = ExtractElements(control);

			elements.TryGetValue("Name", out string? name);
			elements.TryGetValue("Tag", out string? tag);
			elements.TryGetValue("Color", out string? color);
			elements.TryGetValue("Directory", out string? path);
			elements.TryGetValue("File", out string? fileKey);

			if (name != null && tag != null)
			{
				var character = new Normal(name, tag, color, path);
				if (window.ModifyingContent?.Count > 0)
				{
					window.ModifyingContent[index].TryGetValue("Tag", out string? tagValue);
					var modifiedCharacter = characters?.FirstOrDefault(c => c.Tag == tagValue);
					if (modifiedCharacter != null)
					{
						if (fileKey != null)
						{
							modifiedCharacter.FileKey = fileKey;
						}

						CharacterData.UpdateCharacter(modifiedCharacter.EntityID, character);
					}
				}
				else
				{
					if (fileKey != null)
					{
						character.FileKey = fileKey;
					}

					CharacterData.AddCharacter(character);
				}
			}
		}

		private List<Character> NewCharactersFromData(List<CharacterStructure> characters)
		{
			var list = new List<Character>();
			foreach (CharacterStructure character in characters)
			{
				Normal newCharacter = new(character.Name, character.Tag, character.Color, character.Directory);
				newCharacter.FileKey = character.FileKey;
				newCharacter.IsSynced = true;

				list.Add(newCharacter);
			}

			return list;
		}
		#endregion

		#region Execution Commands
		private void ExecuteMoveCharacterCommand(object? element)
		{
			if (element is not MenuItem item) return;

			string? selectedItem = item.Header.ToString() ?? null;

			if (SelectedEntities.Count > 0 && selectedItem != null)
			{
				var match = GetFileKey.Match(selectedItem);
				if (match.Success)
				{
					var targetFileKey = match.Groups[1].Value;
					if (targetFileKey != null)
					{
						foreach (Character character in SelectedEntities.ToArray())
						{
							character.FileKey = targetFileKey;
							CharacterData.UpdateCharacter(character.EntityID, character);
						}

						Converter.UnsetCharacterList();
					}
					else
					{
						DialogBox.Show(
							$"The selected file path for {selectedItem} is not set. Please set it before moving characters.",
							"File Path Not Set",
							DialogButtonDefaults.OK,
							DialogIcon.Warning);
					}
				}
			}
		}

		private void ExecuteAddCharactersCommand(object? parameters = null)
		{
			var manipulator = GetManipulator();
			manipulator.Owner = Owner;
			manipulator.ShowDialog();

			if (manipulator.DidCloseWithSave == false) return;

			SaveCharacters(manipulator);
		}

		private void ExecuteModifyCharactersCommand(object? parameters = null)
		{
			if (SelectedEntities.Count == 0) return;

			var manipulator = GetManipulator(SelectedEntities);
			manipulator.Owner = Owner;
			manipulator.ShowDialog();

			if (manipulator.DidCloseWithSave == false) return;

			SaveCharacters(manipulator, SelectedEntities);
		}

		private void ExecuteRemoveCharactersCommand(object? parameters = null)
		{
			if (SelectedEntities.Count == 0) return;

			var selectedItems = SelectedEntities;
			int maxAmount = 10;
			string phrasing = selectedItems.Count > 1 ? "these characters" : "this character";
			var characterNames = string.Join("\n", selectedItems
				.Cast<Character>()
				.Take(maxAmount)
				.Select(character => $"- {character.Name}"));

			if (selectedItems.Count > maxAmount)
			{
				characterNames += $"\n- And {selectedItems.Count - maxAmount} more...";
			}

			var question = DialogBox.Show($"Are you sure you wish to remove {phrasing}?\r\n{characterNames}",
				"Confirm Removal",
				600,
				DialogIcon.Question,
				new DialogButton("From Program", DialogBoxResult.Continue),
				new DialogButton("From Script", DialogBoxResult.Retry),
				new DialogButton("From Both", DialogBoxResult.Yes, "ErrorButton"),
				new DialogButton("Cancel", DialogBoxResult.No, "ErrorButton"));

			if (question == DialogBoxResult.Continue || question == DialogBoxResult.Retry || question == DialogBoxResult.Yes)
			{
				try
				{
					List<Character> charactersToRemove = new();
					foreach (Character character in selectedItems)
					{
						charactersToRemove.Add(character);
					}

					foreach (Character character in charactersToRemove)
					{
						if (question == DialogBoxResult.Retry ||
							question == DialogBoxResult.Yes)
						{
							if (CharacterData.CharacterExistsInScript(character.EntityID))
							{
								CharacterData.RemoveCharacterFromScript(character.EntityID, false);
							}
						}

						if (question == DialogBoxResult.Continue ||
							question == DialogBoxResult.Yes)
						{
							CharacterData.RemoveCharacter(character.EntityID);
						}
					}

					CharacterData.SaveData();
					Converter.UnsetCharacterList();
				}
				catch (Exception error)
				{
					DialogBox.Show(
						$"Something went wrong when removing characters!\n\n{error}",
						"Failed to Remove Characters",
						DialogButtonDefaults.OK,
						DialogIcon.Error);
				}
			}
		}

		private void ExecuteSaveToScriptCommand(object? parameters = null)
		{
			if (SelectedEntities.Count == 0) return;

			var result = DialogBox.Show(
				$"Would you like to update the selected characters, or all characters?",
				"Save Characters To Script",
				DialogIcon.Question,
				new DialogButton("Selected", DialogBoxResult.Continue),
				new DialogButton("All", DialogBoxResult.Yes),
				new DialogButton("Cancel", DialogBoxResult.No, "ErrorButton"));
			if (result == DialogBoxResult.Continue || result == DialogBoxResult.Yes)
			{
				try
				{
					List<(Character, bool)> tags = new List<(Character, bool)>();
					int maxCharacterMerge = 10;
					int currentCharacterMerge = 0;
					string mergeDialog = "";
					int index = 0;

					foreach (Character row in SelectedEntities)
					{
						var inScript = CharacterData.CharacterExistsInScript(row.Tag, row.FileKey);

						if (inScript)
						{
							if (currentCharacterMerge < maxCharacterMerge)
							{
								mergeDialog += $"- {row.Name} ({row.Tag})\n";
							}
							currentCharacterMerge++;
						}

						tags.Add((row, inScript));

						if (index == SelectedEntities.Count - 1)
						{
							if (currentCharacterMerge > maxCharacterMerge)
							{
								mergeDialog += $"- And {currentCharacterMerge - maxCharacterMerge} more...";
							}
						}

						index++;
					}

					if (currentCharacterMerge > 0)
					{
						if (DialogBox.Show(
							$"Detected characters that already exist within the script.\nWould you like to merge existing characters?\n(Regardless of the answer, any new characters will be added)\n\n{mergeDialog}",
							"Existing Characters Detected",
							DialogButtonDefaults.YesNo,
							DialogIcon.Warning) == DialogBoxResult.No)
						{
							Predicate<(Character, bool)> value = tuple => tuple.Item2;
							tags.RemoveAll(value);
						}
					}

					foreach (var (character, inScript) in tags)
					{
						var content = CharacterData.ConvertToScriptContent(character);
						if (inScript)
						{
							CharacterData.UpdateCharacterInScript(character.Tag, content);
						}
						else
						{
							CharacterData.AddCharacterToScript(character.Tag, content);
						}
					}

					DialogBox.Show(
						"Characters successfully saved to script!",
						"Success",
						DialogButtonDefaults.OK,
						DialogIcon.Information);
				}
				catch (Exception error)
				{
					DialogBox.Show(
						$"Something went wrong when adding characters!\n\n{error}",
						"Failed to Add Characters",
						DialogButtonDefaults.OK,
						DialogIcon.Error);
				}
			}
		}

		private void ExecuteImportCharactersCommand(object? parameters = null)
		{
			var characterData = CharacterData.SyncCharacters();
			var characterStructList = new List<CharacterStructure>();
			var characterList = new List<Character>();
			var duplicates = new List<string>();

			foreach (CharacterStructure character in characterData.Values)
			{
				if (CharacterData.CheckedDuplicates(character))
				{
					duplicates.Add(character.Tag);
				}
			}

			if (duplicates.Count > 0)
			{
				int selectedCount = duplicates.Count;
				int maxAmount = 10;
				string phrasing = duplicates.Count > 1 ? "multiple characters" : "a character";
				string tags = string.Join("\n", duplicates.Take(maxAmount).Select(c => $"- {c}"));

				if (selectedCount > maxAmount)
				{
					tags += $"\n- And {selectedCount - maxAmount} more...";
				}

				var result = DialogBox.Show(
					$"Found {phrasing} with a similar tag that already exist...\nDo you want to merge them?\r\n{tags}",
					"Confirm Merge Status",
					DialogButtonDefaults.YesNo,
					DialogIcon.Question);

				if (result == DialogBoxResult.Yes)
				{
					foreach (string character in duplicates)
					{
						int characterId = CharacterData.AllCharacters.First(c => c.Tag == character).EntityID;
						string name = characterData[character].Name;
						string tag = characterData[character].Tag;
						string fileKey = characterData[character].FileKey;
						string? color = characterData[character].Color;
						string? directory = characterData[character].Directory;

						Normal newNormal = new(name, tag, color, directory);

						newNormal.FileKey = fileKey;
						newNormal.IsSynced = true;

						CharacterData.UpdateCharacter(characterId, newNormal);

						characterData.Remove(character);
					}

					characterStructList.AddRange(characterData.Values.ToList());
					characterList = NewCharactersFromData(characterStructList);
				}
				else
				{
					foreach (string character in duplicates)
					{
						characterData.Remove(character);
					}

					characterStructList.AddRange(characterData.Values.ToList());
					characterList = NewCharactersFromData(characterStructList);
				}
			}
			else
			{
				characterStructList.AddRange(characterData.Values.ToList());
				characterList = NewCharactersFromData(characterStructList);
			}

			foreach (Character character in characterList)
			{
				CharacterData.AddCharacter(character);
			}

			Converter.UnsetCharacterList();
		}

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

		private void ExecuteExitCommand(object? _)
		{
			Owner.Close();
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
