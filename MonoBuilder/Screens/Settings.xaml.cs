using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using MonoBuilder.Screens.ScreenUtils;
using MonoBuilder.Utils;
using MonoBuilder.Utils.character_management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MonoBuilder.Screens
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private OpenFileDialog FilePath { get; set; } = new();
        private OpenFolderDialog FolderPath { get; set; } = new();

        public Characters CharacterData { get; set; }
        private AppSettings ApplicationSettings { get; set; }
        private ScriptConversion ScriptConverter { get; set; }
        private bool IsRegexInvalid { get; set; }
        private string SelectedBasePath { get; set; }
        private CollectionViewSource? CharacterCollectionView { get; set; }

        public ObservableCollection<string> AvailableFiles { get; set; } = new();
        private static readonly Regex GetFileKey = new(@"^\((.+?)\)", RegexOptions.Compiled);

        public ObservableBoolean SaveBtnShouldEnable { get; set; } = new();

        public Settings(Characters characters, AppSettings settings, ScriptConversion converter)
        {
            CharacterData = characters;
            ApplicationSettings = settings;
            ScriptConverter = converter;
            SelectedBasePath = settings.GetFolderPath("Base") ?? string.Empty;

            InitializeComponent();
            RunSetup();
            InitializeAvailableFiles();
            DataContext = this;

            SetSaveEnableState(false);
        }

        private void SetSaveEnableState(bool set)
        {
            SaveBtnShouldEnable.Value = set;
        }

        private void InitializeAvailableFiles()
        {
            AvailableFiles.Clear();

            foreach (var file in ApplicationSettings.GetAllFiles("Characters"))
            {
                var value = $"({file.Key}) {file.Value}";
                AvailableFiles.Add(value);
            }
        }

        private void RunSetup()
        {
            var hasCharacterFiles = ApplicationSettings.GetAllFilePaths("Characters").Count > 0;

            if (CharactersDataGrid.SelectedCells.Count > 0)
            {
                BtnModifyCharacter.IsEnabled = true;
                BtnRemoveCharacter.IsEnabled = true;
            }

            if (hasCharacterFiles)
            {
                BtnImportCharacters.IsEnabled = true;
                BtnSaveCharacters.IsEnabled = true;
            }

            InitializeCharacterTabs();

            BaseFolderButton.Tag = new Dictionary<string, string>
            {
                { "Type", "Base" },
                { "Link", "BaseFolderInput" }
            };
            AssetsFolderButton.Tag = new Dictionary<string, string>
            {
                { "Type", "Assets" },
                { "Link", "AssetsFolderInput" }
            };
            ImagesFolderButton.Tag = new Dictionary<string, string>
            {
                { "Type", "Images" },
                { "Link", "ImagesFolderInput" }
            };
            ScenesFolderButton.Tag = new Dictionary<string, string>
            {
                { "Type", "Scenes" },
                { "Link", "ScenesFolderInput" }
            };
            GalleryFolderButton.Tag = new Dictionary<string, string>
            {
                { "Type", "Gallery" },
                { "Link", "GalleryFolderInput" }
            };

            InitializeDefaultFileButtonTags();

            var basePath = ApplicationSettings.GetFolderPath("Base");
            var assetsPath = ApplicationSettings.GetFolderPath("Assets");
            var imagesPath = ApplicationSettings.GetFolderPath("Images");
            var scenesPath = ApplicationSettings.GetFolderPath("Scenes");
            var galleryPath = ApplicationSettings.GetFolderPath("Gallery");

            var allFilePaths = ApplicationSettings.GetAllFilePaths().ToList();
            for (int i = 0; i < allFilePaths.Count; i++)
            {
                (string file, string path) = allFilePaths[i];

                var split = file.Split(':');
                string fileType = split[0];
                int.TryParse(split[1], out int fileId);
                var tab = Helpers.FindLogicalChild<TabItem>(AssetTabs, $"{fileType}Tab");

                if (tab != null)
                {
                    var grid = Helpers.FindLogicalChild<Grid>(tab, $"{fileType}FileContainer");
                    if (grid != null)
                    {
                        var textBox = fileId == 0 ?
                            Helpers.FindVisualChild<TextBox>(grid, $"{fileType}FileInput_0") :
                            null;
                        var button = fileId == 0 ?
                            Helpers.FindVisualChild<Button>(grid, $"{fileType}FileButton_0") :
                            null;

                        if (textBox != null && button != null)
                        {
                            textBox.Text = GetRelativePath(path, false, basePath);
                            button.Tag = new Dictionary<string, string>
                            {
                                { "Type", $"{fileType}:{fileId}" },
                                { "Link", $"{fileType}FileInput_{fileId}" }
                            };
                        }
                        else
                        {
                            // If the corresponding TextBox or Button doesn't exist, create them
                            var newRowIndex = grid.RowDefinitions.Count;
                            CreateNewRow(grid);

                            var newTextBox = BuildPathTextBox(fileType, "File", fileId);
                            var newButton = BuildPathSelectButton(fileType, "File", fileId);
                            var removeButton = BuildPathRemoveButton(grid, fileType, "File", fileId);

                            grid.Children.Add(newTextBox);
                            grid.Children.Add(newButton);
                            grid.Children.Add(removeButton);

                            Grid.SetRow(newTextBox, newRowIndex);
                            Grid.SetRow(newButton, newRowIndex);
                            Grid.SetRow(removeButton, newRowIndex);

                            newTextBox.Text = GetRelativePath(path, false, basePath);
                        }
                    }
                }
            }

            var autoSync = ApplicationSettings.GetAutoSyncLabels();
            var shouldColor = ApplicationSettings.GetColorFormatting();
            var indentType = ApplicationSettings.GetIndentationType();
            var indentAmount = ApplicationSettings.GetIndentationAmount();

            BaseFolderInput.Text = string.IsNullOrEmpty(basePath) ? "" : GetRelativePath(basePath, true, basePath);
            AssetsFolderInput.Text = string.IsNullOrEmpty(assetsPath) ? "" : GetRelativePath(assetsPath, false, basePath);
            ImagesFolderInput.Text = string.IsNullOrEmpty(imagesPath) ? "" : GetRelativePath(imagesPath, false, basePath);
            ScenesFolderInput.Text = string.IsNullOrEmpty(scenesPath) ? "" : GetRelativePath(scenesPath, false, basePath);
            GalleryFolderInput.Text = string.IsNullOrEmpty(galleryPath) ? "" : GetRelativePath(galleryPath, false, basePath);

            ShouldAutoSync.IsChecked = autoSync;
            ShouldColor.IsChecked = shouldColor;
            IndentationType.SelectedIndex = indentType == "Spaces" ? 0 : 1;
            IndentationAmount.Text = indentAmount < 2 ? "2" : indentAmount > 8 ? "8" : indentAmount.ToString();

            foreach (var rule in ScriptConverter.ConversionRules)
            {
                var checkbox = Helpers.FindVisualChild<CheckBox>(ScriptBuilderSettingsContainer, $"ChkEnable{rule.Name}");
                var input = Helpers.FindVisualChild<TextBox>(ScriptBuilderSettingsContainer, $"BoxRegex{rule.Name}");

                checkbox?.IsChecked = rule.IsEnabled;
                input?.Text = rule.Pattern.ToString();
            }
        }

        private void InitializeDefaultFileButtonTags()
        {
            CharactersFileButton_0.Tag = new Dictionary<string, string>
            {
                { "Type", "Characters:0" },
                { "Link", "CharactersFileInput_0" }
            };

            ScriptFileButton_0.Tag = new Dictionary<string, string>
            {
                { "Type", "Script:0" },
                { "Link", "ScriptFileInput_0" }
            };

            ImagesFileButton_0.Tag = new Dictionary<string, string>
            {
                { "Type", "Images:0" },
                { "Link", "ImagesFileInput_0" }
            };

            ScenesFileButton_0.Tag = new Dictionary<string, string>
            {
                { "Type", "Scenes:0" },
                { "Link", "ScenesFileInput_0" }
            };

            GalleryFileButton_0.Tag = new Dictionary<string, string>
            {
                { "Type", "Gallery:0" },
                { "Link", "GalleryFileInput_0" }
            };
        }

        private string GetDefaultCharacterFileKey()
        {
            return ApplicationSettings.GetAllFilePaths("Characters")
                .OrderBy(e => e.Key)
                .Select(e => e.Key)
                .FirstOrDefault() ?? string.Empty;
        }

        private void InitializeCharacterTabs()
        {
            CharacterFilterTabs.Items.Clear();

            CharacterFilterTabs.Items.Add(new TabItem
            {
                Header = "All Characters",
                Tag = "All"
            });

            var files = ApplicationSettings.GetAllFilePaths("Characters")
                .OrderBy(entry => entry.Key)
                .ToList();

            foreach (var (fileKey, filePath) in files)
            {
                var header = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrWhiteSpace(header))
                    header = fileKey;

                CharacterFilterTabs.Items.Add(new TabItem
                {
                    Header = header,
                    Tag = fileKey
                });
            }

            CharacterCollectionView = new CollectionViewSource
            {
                Source = CharacterData.AllCharacters
            };

            CharactersDataGrid.ItemsSource = CharacterCollectionView.View;

            CharacterFilterTabs.SelectedIndex = 0;
            ApplyCharacterTabFilter();
        }

        private void ApplyCharacterTabFilter()
        {
            if (CharacterCollectionView?.View == null)
                return;

            var selectedTab = CharacterFilterTabs.SelectedItem as TabItem;
            var selectedKey = selectedTab?.Tag as string;
            var defaultFileKey = GetDefaultCharacterFileKey();

            if (string.IsNullOrWhiteSpace(selectedKey) || selectedKey == "All")
            {
                CharacterCollectionView.View.Filter = null;
                CharacterCollectionView.View.Refresh();
                return;
            }

            CharacterCollectionView.View.Filter = item =>
            {
                if (item is not Character character)
                    return false;

                if (character.FileKey == selectedKey)
                    return true;

                return string.IsNullOrEmpty(character.FileKey) && selectedKey == defaultFileKey;
            };

            CharacterCollectionView.View.Refresh();
            CharactersDataGrid.UnselectAll();
        }

        private void CharacterFilterTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            ApplyCharacterTabFilter();
        }

        private void ShouldEnableSave(bool value)
        {
            if (IsRegexInvalid) SetSaveEnableState(false);
            else SetSaveEnableState(value);
        }

        private void SaveContent()
        {
            bool RegexError = false;
            foreach (var rule in ScriptConverter.ConversionRules)
            {
                var checkbox = Helpers.FindVisualChild<CheckBox>(ScriptBuilderSettingsContainer, $"ChkEnable{rule.Name}");
                var input = Helpers.FindVisualChild<TextBox>(ScriptBuilderSettingsContainer, $"BoxRegex{rule.Name}");

                if (checkbox != null && input != null)
                {
                    rule.IsEnabled = checkbox.IsChecked == true;
                    rule.Pattern = new System.Text.RegularExpressions.Regex(input.Text);
                }
                else
                {
                    RegexError = true;
                }
            }

            if (RegexError)
            {
                DialogBox.Show(
                    "Something went wrong while saving Regexes...\nIt's likely there is a typo, error, or missing input box. All other Regexes that weren't broken should have saved correctly.\n\nIf the issue persists, please contact the developer.",
                    "Error",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
            }

            ScriptConverter.SaveSettings();
            ApplicationSettings.SaveDirectories();
        }

        private void ModifyCharacter(Character[] characters)
        {
            var characterparams = characters
                .Select(c => new Dictionary<string, string?>()
                    {
                        { "Name", c.Name },
                        { "Tag", c.Tag },
                        { "Color", c.Color },
                        { "Directory", c.Directory },
                        { "File", c.FileKey }
                    })
                .ToList();

            var ModifyCharacterWindow = new ContentManipulator(ApplicationSettings, "Characters", new ContentTemplate(
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
                    { "Name", "Enter Character Name" },
                    { "Tag", "Enter Character Tag" },
                    { "Directory", "Path to Character Assets" }
                }),
                characterparams);
            ModifyCharacterWindow.Owner = this;
            ModifyCharacterWindow.ShowDialog();

            if (ModifyCharacterWindow.DidCloseWithSave == false) return;

            SaveCharacters(ModifyCharacterWindow, characters);
        }

        private void SaveCharacters(ContentManipulator window, Character[]? characters = null)
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

            CharacterData.SaveCharacters();
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

        private void ApplyCharacterChanges(ContentManipulator window, List<FrameworkElement> control, int index, Character[]? characters = null)
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
                    Debug.WriteLine(tagValue);
                    var modifiedCharacter = characters?.FirstOrDefault(c => c.Tag == tagValue);
                    Debug.WriteLine(modifiedCharacter);
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

        private string GetRelativePath(string folderPath, bool isBase, string? baseIsActive)
        {
            var userProfile = !isBase ?
                baseIsActive ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) :
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string? result = "...\\" +
                Path.GetRelativePath(userProfile, folderPath);

            return result;
        }

        private void CreateNewRow(Grid grid)
        {
            RowDefinition newRow = new RowDefinition { Height = new GridLength(0, GridUnitType.Star) };
            grid.RowDefinitions.Add(newRow);
        }

        private void RemoveRowAt(Grid grid, int row, string type)
        {
            if (row < 1 || row >= grid.RowDefinitions.Count) return;

            List<UIElement> elementsToRemove = new();
            foreach (UIElement element in grid.Children)
            {
                var elementRow = Grid.GetRow(element);

                if (elementRow == row)
                {
                    if (element is FrameworkElement elm)
                    {
                        string name = elm.Name;
                        elm.Name = $"ToDeBeleted_{name}";
                    }

                    elementsToRemove.Add(element);
                }
                else if (elementRow > row)
                {
                    int newRowNumber = Grid.GetRow(element) - 1;
                    Grid.SetRow(element, newRowNumber);
                }
            }

            foreach (UIElement element in elementsToRemove)
            {
                grid.Children.Remove(element);

                if (element is FrameworkElement frameworkElement && frameworkElement.Tag is Dictionary<string, string> tag)
                {
                    var parts = tag["Type"].Split(':');
                    if (int.TryParse(parts[1], out int trueRow))
                    {
                        ApplicationSettings.RemoveFilePath(type, trueRow);
                    }
                }
            }

            grid.RowDefinitions.RemoveAt(row);

            ShouldEnableSave(true);
            ApplicationSettings.SaveDirectories();
        }

        private TextBox BuildPathTextBox(string assetType, string type, int id)
        {
            TextBox box = new TextBox
            {
                Name = $"{assetType}{type}Input_{id}",
                FontSize = 12,
                IsReadOnly = true,
                Margin = new Thickness(0, 10, 0, 10),
                Style = (Style)Application.Current.Resources["DefaultTextBox"],
                Tag = $"Select a Script File From the \"Select {type}\" Button..."
            };

            Grid.SetColumn(box, 1);

            return box;
        }

        private Button BuildPathSelectButton(string assetType, string type, int id)
        {
            Button button = new Button
            {
                Name = $"{assetType}{type}Button_{id}",
                Width = 150,
                Margin = new Thickness(10, 10, 0, 10),
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                Content = $"Select {type}",
                Style = (Style)Application.Current.Resources["DefaultButton"],
                Tag = new Dictionary<string, string>
                {
                    { "Type", $"{assetType}:{id}" },
                    { "Link", $"{assetType}{type}Input_{id}" }
                }
            };

            button.Click += SelectFile;
            Grid.SetColumn(button, 2);

            return button;
        }

        private Button BuildPathRemoveButton(Grid grid, string assetType, string type, int id)
        {
            Button button = new Button
            {
                Name = $"Remove{assetType}{type}Button_{id}",
                Margin = new Thickness(0, 10, 10, 10),
                Padding = new Thickness(5),
                Content = "Remove File",
                Style = (Style)Application.Current.Resources["ErrorButton"]
            };

            button.Click += (s, e) =>
            {
                if (s is Button btn)
                {
                    int currentRow = Grid.GetRow(btn);
                    RemoveRowAt(grid, currentRow, assetType);
                }
            };

            Grid.SetColumn(button, 0);

            return button;
        }

        private void AddPathGridRow(Grid grid, string fileType, int fileId)
        {
            var newRowIndex = grid.RowDefinitions.Count;
            CreateNewRow(grid);

            var newTextBox = BuildPathTextBox(fileType, "File", fileId);
            var newButton = BuildPathSelectButton(fileType, "File", fileId);
            var removeButton = BuildPathRemoveButton(grid, fileType, "File", fileId);

            grid.Children.Add(newTextBox);
            grid.Children.Add(newButton);
            grid.Children.Add(removeButton);

            Grid.SetRow(newTextBox, newRowIndex);
            Grid.SetRow(newButton, newRowIndex);
            Grid.SetRow(removeButton, newRowIndex);

            ShouldEnableSave(true);
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private double TotalWindowWidth = 0;
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TotalWindowWidth == 0)
            {
                var margin = CharactersDataGrid.Margin.Left * 2;
                var borderThickness = CharactersDataGrid.BorderThickness.Left * 2;
                var windowResizeBorderThickness = (SystemParameters.WindowResizeBorderThickness.Left + SystemParameters.WindowNonClientFrameThickness.Left) * 2;
                TotalWindowWidth = SystemParameters.VerticalScrollBarWidth + margin + borderThickness + windowResizeBorderThickness;
            }
            CharactersDataGrid.Width = e.NewSize.Width - TotalWindowWidth;
        }

        private void CharactersDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                var scrollViewer = Helpers.GetScrollViewer(CharactersDataGrid);
                var canScroll = Helpers.ElementCanScroll(scrollViewer, e.Delta, true);

                if (!canScroll)
                {
                    e.Handled = true;
                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = sender
                    };

                    // Find the parent and raise the event there
                    var parent = ((Control)sender).Parent as UIElement;
                    parent?.RaiseEvent(eventArg);
                }
            }
        }

        private void BoxRegex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!e.Handled)
            {
                var textbox = sender as TextBox;
                if (textbox != null)
                {
                    if (string.IsNullOrEmpty(textbox.Text))
                    {
                        IsRegexInvalid = true;
                        textbox.Background = Brushes.MistyRose;
                        ShouldEnableSave(false);
                    }
                    else
                    {
                        try
                        {
                            _ = new System.Text.RegularExpressions.Regex(textbox.Text);
                            IsRegexInvalid = false;
                            textbox.Background = Brushes.White;
                            ShouldEnableSave(true);
                        }
                        catch (Exception)
                        {
                            IsRegexInvalid = true;
                            textbox.Background = Brushes.Orchid;
                            ShouldEnableSave(false);
                        }
                    }
                }
            }
        }

        private void CheckEnable_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox != null)
            {
                var ruleName = checkbox.Name.Replace("ChkEnable", "");
                var rule = ScriptConverter.ConversionRules.Find(r => r.Name == ruleName);
                rule?.IsEnabled = checkbox.IsChecked == true;
                checkbox.Content = rule?.IsEnabled == true ? "Enabled" : "Disabled";

                ShouldEnableSave(true);
            }
        }

        private void ShouldAutoSync_Checked(object sender, RoutedEventArgs e)
        {
            var ShouldAutoSyncValue = ShouldAutoSync.IsChecked == true;
            ApplicationSettings.SetAutoSyncLabels(ShouldAutoSyncValue);
            ScriptConverter.SetAutoSyncLabels(ShouldAutoSyncValue);

            ShouldEnableSave(true);
        }

        private void ShouldColor_Checked(object sender, RoutedEventArgs e)
        {
            var ShouldColorValue = ShouldColor.IsChecked == true;
            ApplicationSettings.SetColorFormatting(ShouldColorValue);
            ScriptConverter.SetIsFormattingColor(ShouldColorValue);

            ShouldEnableSave(true);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SetSaveEnableState(false);
            SaveContent();
        }

        private void BtnSaveClose_Click(object sender, RoutedEventArgs e)
        {
            SetSaveEnableState(false);
            SaveContent();
            Close();
        }

        private void BtnAddCharacter_Click(object sender, RoutedEventArgs e)
        {
            var AddCharacterWindow = new ContentManipulator(ApplicationSettings, "Characters", new ContentTemplate(
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
                    { "Name", "Enter Character Name" },
                    { "Tag", "Enter Character Tag" },
                    { "Directory", "Path to Character Assets" }
                }));
            AddCharacterWindow.Owner = this;
            AddCharacterWindow.ShowDialog();

            if (AddCharacterWindow.DidCloseWithSave == false) return;

            SaveCharacters(AddCharacterWindow);
        }

        private void BtnModifyCharacter_Click(object sender, RoutedEventArgs e)
        {
            ModifyCharacter(CharactersDataGrid.SelectedItems.Cast<Character>().ToArray());
        }

        private void BtnRemoveCharacter_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = CharactersDataGrid.SelectedItems;
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

                    CharacterData.SaveCharacters();
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

        private void CharactersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is UIElement element)
            {
                var parent = VisualTreeHelper.GetParent(element);

                while ((parent != null) && !(parent is DataGridRow))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is DataGridRow row)
                {
                    ModifyCharacter(CharactersDataGrid.SelectedItems.Cast<Character>().ToArray());
                }
            }
        }

        private void IndentationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = IndentationType.SelectedItem as ComboBoxItem;
            ApplicationSettings.SetIndentationType(selectedType?.Content.ToString() ?? "Spaces");
            ScriptConverter.ChangeIndentationType(ApplicationSettings.GetIndentationType());

            ShouldEnableSave(true);
        }

        private void IndentationAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(IndentationAmount.Text, out int amount))
            {
                if (amount < 2) amount = 2;
                if (amount > 8) amount = 8;

                IndentationAmount.Text = amount.ToString();
                IndentationAmount.SelectionStart = IndentationAmount.Text.Length;
                ApplicationSettings.SetIndentationAmount(amount);
                ScriptConverter.ChangeIndentationAmount(amount);

                ShouldEnableSave(true);
            }
        }

        private void IndentationAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\b") return;
            if (e.Text == IndentationAmount.Text)
            {
                e.Handled = true;
                return;
            }

            if (int.TryParse(e.Text, out int result))
            {
                IndentationAmount.Text = "";
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void CharactersDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedCells.Count > 0)
            {
                BtnModifyCharacter.IsEnabled = true;
                BtnRemoveCharacter.IsEnabled = true;
            }
            else
            {
                BtnModifyCharacter.IsEnabled = false;
                BtnRemoveCharacter.IsEnabled = false;
            }
        }

        private TimeSpan DebounceDuration { get; } = TimeSpan.FromMilliseconds(400);
        private DateTime LastClickedTime { get; set; } = DateTime.MinValue;
        private void CharactersDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DateTime currentTime = DateTime.UtcNow;

            if (currentTime - LastClickedTime > DebounceDuration)
            {
                LastClickedTime = currentTime;
                DataGrid? grid = sender as DataGrid;
                DependencyObject dep = (DependencyObject)e.OriginalSource;

                if (dep is ScrollViewer)
                {
                    grid?.UnselectAll();
                    e.Handled = true;
                    return;
                }

                while((dep != null) && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                if (dep is DataGridRow row && row.IsSelected)
                {
                    if (grid?.SelectedCells.Count > grid?.Columns.Count)
                    {
                        grid?.UnselectAll();
                        row.IsSelected = true;
                    }
                    else
                    {
                        row.IsSelected = false;
                        e.Handled = true;
                    }
                }
            }
        }

        private void SelectDirectory(object sender, RoutedEventArgs e)
        {
            var baseIsActive = ApplicationSettings.GetFolderPath("Base");
            var assetsIsActive = ApplicationSettings.GetFolderPath("Assets");
            FolderPath.InitialDirectory = assetsIsActive ?? baseIsActive ?? string.Empty;

            var btn = sender as Button;
            if (btn != null)
            {
                var tag = btn.Tag as Dictionary<string, string>;
                var tagType = tag != null && tag.TryGetValue("Type", out string? type) ? type : null;
                var tagLink = tag != null && tag.TryGetValue("Link", out string? link) ? link : null;

                if (tagType != null)
                {
                    FolderPath.Title = $"Select {tagType} Folder";
                }

                if (FolderPath.ShowDialog() == true)
                {
                    bool btnIsBase = tagType != null && tagType == "Base";
                    var input = Helpers.FindVisualChild<TextBox>(GeneralSettingsContainer, tagLink ?? string.Empty);

                    if (input != null)
                    {
                        string folderPath = FolderPath.FolderNames.FirstOrDefault() ?? string.Empty;
                        string result = GetRelativePath(folderPath, btnIsBase, baseIsActive);

                        input.Text = result;
                        ApplicationSettings.AddReplaceFolderPath(tagType ?? string.Empty, folderPath);
                        ApplicationSettings.SaveDirectories();
                    }
                }
            }
        }

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            var baseIsActive = ApplicationSettings.GetFolderPath("Base");
            FilePath.Filter = "JavaScript files (*.js)|*.js|All files (*.*)|*.*";
            FilePath.InitialDirectory = baseIsActive ?? string.Empty;
            FilePath.FilterIndex = 1;

            var btn = sender as Button;
            if (btn != null)
            {
                var tag = btn.Tag as Dictionary<string, string>;
                var tagType = tag != null && tag.TryGetValue("Type", out string? type) ? type : null;
                var tagLink = tag != null && tag.TryGetValue("Link", out string? link) ? link : null;

                if (tagType != null)
                {
                    Title = $"Select {tagType} File";
                }

                if (FilePath.ShowDialog() == true)
                {
                    var input = Helpers.FindVisualChild<TextBox>(GeneralSettingsContainer, tagLink ?? string.Empty);

                    if (input != null)
                    {
                        string filePath = FilePath.FileName;
                        string result = GetRelativePath(filePath, false, baseIsActive);

                        input.Text = result;
                        string fileType = tagType ?? string.Empty;

                        ApplicationSettings.AddReplaceFilePath(fileType, filePath);
                        ApplicationSettings.SaveDirectories();

                        if (fileType.StartsWith("Characters", StringComparison.Ordinal))
                        {
                            InitializeCharacterTabs();
                        }
                    }
                }
            }
        }

        private void RemoveFileLine(object sender, RoutedEventArgs e)
        {

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

        private void ImportCharacters(object sender, RoutedEventArgs e)
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
        }

        private void SaveCharactersToScript(object sender, RoutedEventArgs e)
        {
            var selectedItems = CharactersDataGrid?.SelectedItems;
            if (selectedItems != null && selectedItems.Count > 0)
            {
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

                        foreach (Character row in selectedItems)
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

                            if (index == selectedItems.Count - 1)
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
        }

        private void BtnAddFileLine_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBasePath != string.Empty && sender is Button btn)
            {
                var currentTab = (TabItem)AssetTabs.SelectedItem;
                var currentTabContent = (DockPanel)AssetTabs.SelectedContent;
                var grid = Helpers.FirstOfVisualType<Grid>(currentTabContent);
                var type = currentTab.Header.ToString() ?? string.Empty;

                if (grid != null)
                {
                    AddPathGridRow(grid, type, grid.RowDefinitions.Count);
                }
            }
        }

        private void ContextMoveCharacter_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var selectedCharacters = CharactersDataGrid.SelectedItems.Cast<Character>().ToList();
            var defaultFileKey = GetDefaultCharacterFileKey();
            string? commonFileKey =
                (selectedCharacters.Select(c => string.IsNullOrEmpty(c.FileKey)
                ? defaultFileKey
                : c.FileKey).Distinct().Count() == 1)
                    ? selectedCharacters.First().FileKey
                    : null;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in ContextMoveCharacter.Items)
                {
                    if (ContextMoveCharacter.ItemContainerGenerator.ContainerFromItem(item) is MenuItem container)
                    {
                        var match = GetFileKey.Match(item?.ToString() ?? "");
                        container.Icon = match.Success && commonFileKey != null && match.Groups[1].Value == commonFileKey
                            ? new TextBlock {
                                Text = "•",
                                FontSize = 25,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                            : null;
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void ContextMoveCharacter_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem clickedItem)
            {
                string? selectedItem = clickedItem.Header.ToString() ?? null;
                var selectedCharacters = CharactersDataGrid.SelectedItems.Cast<Character>().ToList();

                if (selectedCharacters.Count > 0 && selectedItem != null)
                {
                    var match = GetFileKey.Match(selectedItem);

                    if (match.Success)
                    {
                        var targetFileKey = match.Groups[1].Value;
                        if (targetFileKey != null)
                        {
                            foreach (Character character in selectedCharacters)
                            {
                                character.FileKey = targetFileKey;
                                CharacterData.UpdateCharacter(character.EntityID, character);
                            }
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
        }
    }
}
