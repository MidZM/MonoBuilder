using MonoBuilder.Utils;
using MonoBuilder.Utils.character_management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MonoBuilder.Screens.ScreenUtils
{
    /// <summary>
    /// Interaction logic for AddModifyCharacter.xaml
    /// </summary>
    public partial class AddModifyCharacter : Window
    {
        private AppSettings ApplicationSettings { get; set; }
        private Characters CharacterData { get; set; }

        private List<Character> ModifiyingCharacter { get; set; } = new();

        private int CurrentRow { get; set; } = 1;
        private Dictionary<string, List<UIElement>> DynamicLists { get; set; } = new Dictionary<string, List<UIElement>>();
        private Dictionary<string, List<UIElement>> DynamicRows { get; set; } = new Dictionary<string, List<UIElement>>();
        private readonly static Regex GetDynamicRowList = new Regex(@"^DynamicRow_(\d+)", RegexOptions.Compiled);

        public ObservableCollection<string> AvailableFiles { get; set; } = new();
        public ObservableCollection<string> AvailableFileTags { get; set; } = new();

        public bool SaveBtnShouldEnable = false;

        public AddModifyCharacter(AppSettings settings, Characters characters, Character[]? characterList = null)
        {
            ApplicationSettings = settings;
            CharacterData = characters;

            InitializeComponent();
            InitializeAvailableFiles();

            DynamicLists.Add("DynamicRow_0", new List<UIElement>());
            DynamicRows.Add("DynamicRow_0", new List<UIElement>());

            DynamicLists["DynamicRow_0"].Add(DynamicRow_0_Name);
            DynamicLists["DynamicRow_0"].Add(DynamicRow_0_Tag);

            DynamicRows["DynamicRow_0"].Add(DynamicRow_0_Name);
            DynamicRows["DynamicRow_0"].Add(DynamicRow_0_Tag);
            DynamicRows["DynamicRow_0"].Add(DynamicRow_0_Color);
            DynamicRows["DynamicRow_0"].Add(DynamicRow_0_Path);
            DynamicRows["DynamicRow_0"].Add(DynamicRow_0_FileKey);

            if (characterList != null && characterList.Length > 0)
            {
                var firstCharacter = characterList.First();

                ModifiyingCharacter.Add(firstCharacter);
                Title = $"Modify Character | {string.Join(", ", characterList.Select(c => c.Name))}";
                EditingType.Content = "Modify Character";
                BtnAdd.IsEnabled = false;
                BtnAdd.Visibility = Visibility.Collapsed;

                var initialFileIndex = AvailableFileTags.IndexOf(firstCharacter.FileKey);

                DynamicRow_0_Name.Text = firstCharacter.Name;
                DynamicRow_0_Tag.Text = firstCharacter.Tag;
                DynamicRow_0_Color.Content = firstCharacter.Color ?? string.Empty;
                DynamicRow_0_Path.Text = firstCharacter.Directory ?? string.Empty;
                DynamicRow_0_FileKey.SelectedIndex = initialFileIndex >= 0 ? initialFileIndex : 0;

                for (int i = 1; i < characterList.Length; i++)
                {
                    var character = characterList[i];
                    var rowContents = AddRow();
                    var fileIndex = AvailableFileTags.IndexOf(character.FileKey);

                    ((TextBox)rowContents["Name"]).Text = character.Name;
                    ((TextBox)rowContents["Tag"]).Text = character.Tag;
                    ((Button)rowContents["Color"]).Content = character.Color ?? string.Empty;
                    ((TextBox)rowContents["Path"]).Text = character.Directory ?? string.Empty;
                    ((ComboBox)rowContents["FileKey"]).SelectedIndex = fileIndex >= 0 ? fileIndex : 0;

                    ModifiyingCharacter.Add(character);
                }
            }
        }

        private void InitializeAvailableFiles()
        {
            AvailableFiles.Clear();
            AvailableFileTags.Clear();

            foreach (var file in ApplicationSettings.GetAllFiles("Characters"))
            {
                AvailableFiles.Add(file.Value);
                AvailableFileTags.Add(file.Key);
            }
        }

        private UIElement CreateInputTemplate(string name, string description, int column, int row)
        {
            TextBox element = new TextBox
            {
                Name = name,
                Tag = description,
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                Style = (Style)Application.Current.Resources["DefaultTextBox"],
            };

            Grid.SetColumn(element, column);
            Grid.SetRow(element, row);
            element.TextChanged += ShouldEnableSave;

            if (name.Contains("Name") || name.Contains("Tag"))
            {
                DynamicLists[$"DynamicRow_{row}"].Add(element);
            }

            DynamicRows[$"DynamicRow_{row}"].Add(element);

            return element;
        }

        private UIElement CreateSelectTemplate(string name, int column, int row)
        {
            Binding fileBinding = new Binding("AvailableFiles")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Window), 1)
            };
            Binding tagBinding = new Binding("AvailableFileTags")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Window), 1)
            };

            ComboBox element = new()
            {
                Name = name,
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                SelectedIndex = 0,
                Style = (Style)Application.Current.Resources["DefaultComboBox"],
            };

            Grid.SetColumn(element, column);
            Grid.SetRow(element, row);

            element.SetBinding(ComboBox.ItemsSourceProperty, fileBinding);
            element.SetBinding(TagProperty, tagBinding);

            DynamicRows[$"DynamicRow_{row}"].Add(element);

            return element;
        }

        private UIElement CreateColorButton(string name, int row, TextBox textbox)
        {
            Binding textbinding = new Binding("Text")
            {
                Source = textbox
            };

            Button element = new Button
            {
                Name = name,
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                Content = "#FF0000",
                FontWeight = FontWeights.Bold,
                Style = (Style)FindResource("ColorButton")
            };

            element.Click += ColorButton_Click;
            Grid.SetColumn(element, 2);
            Grid.SetRow(element, row);
            element.SetBinding(Button.TagProperty, textbinding);

            DynamicRows[$"DynamicRow_{row}"].Add(element);

            return element;
        }
        
        private UIElement CreateRemoveRowButton(int row)
        {
            Button element = new Button
            {
                Name = $"RemoveRow_{row}",
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                Content = "—",
                FontWeight = FontWeights.UltraBold,
                Style = (Style)Application.Current.Resources["DefaultButton"]
            };

            element.Click += (s, e) =>
            {
                if (s is Button btn)
                {
                    int currentRow = Grid.GetRow(btn);
                    RemoveRowAt(currentRow);
                }
            };
            Grid.SetColumn(element, 5);
            Grid.SetRow(element, row);

            return element;
        }

        private void CreateNewRow()
        {
            CharacterContent.RowDefinitions.Add(new RowDefinition { Height = new GridLength(45) });
        }

        private void RemoveRowAt(int row)
        {
            if (row < 1 || row >= CharacterContent.RowDefinitions.Count) return;

            List<UIElement> elementsToRemove = new List<UIElement>();
            foreach (UIElement element in CharacterContent.Children)
            {
                var elementRow = Grid.GetRow(element);

                if (elementRow == row)
                {
                    elementsToRemove.Add(element);
                }
                else if (elementRow > row)
                {
                    Grid.SetRow(element, Grid.GetRow(element) - 1);
                }
            }

            foreach (UIElement element in elementsToRemove)
            {
                CharacterContent.Children.Remove(element);
            }

            DynamicLists.Remove($"DynamicRow_{row}");
            DynamicRows.Remove($"DynamicRow_{row}");

            CharacterContent.RowDefinitions.RemoveAt(row);

            CurrentRow--;
            Height -= 45;

            ShouldEnableSave();
        }

        private Dictionary<string, FrameworkElement> AddRow()
        {
            CreateNewRow();
            CurrentRow++;
            ShouldEnableSave(false);

            DynamicLists.Add($"DynamicRow_{CurrentRow}", new List<UIElement>());
            DynamicRows.Add($"DynamicRow_{CurrentRow}", new List<UIElement>());
            TextBox nameInput = (TextBox)CreateInputTemplate($"DynamicRow_{CurrentRow}_Name", "Enter Character Name", 0, CurrentRow);
            TextBox tagInput = (TextBox)CreateInputTemplate($"DynamicRow_{CurrentRow}_Tag", "Enter Character Tag", 1, CurrentRow);
            Button colorButton = (Button)CreateColorButton($"DynamicRow_{CurrentRow}_Color", CurrentRow, nameInput);
            TextBox directoryInput = (TextBox)CreateInputTemplate($"DynamicRow_{CurrentRow}_Path", "Path to Character Assets", 3, CurrentRow);
            ComboBox fileComboBox = (ComboBox)CreateSelectTemplate($"DynamicRow_{CurrentRow}_File", 4, CurrentRow);
            Button removeButton = (Button)CreateRemoveRowButton(CurrentRow);

            CharacterContent.Children.Add(nameInput);
            CharacterContent.Children.Add(tagInput);
            CharacterContent.Children.Add(colorButton);
            CharacterContent.Children.Add(directoryInput);
            CharacterContent.Children.Add(fileComboBox);
            CharacterContent.Children.Add(removeButton);

            Height += 45;

            return new Dictionary<string, FrameworkElement>()
            {
                { "Name", nameInput },
                { "Tag", tagInput },
                { "Color", colorButton },
                { "Path", directoryInput },
                { "FileKey", fileComboBox }
            };
        }

        private void SetSaveEnableState(bool set)
        {
            SaveBtnShouldEnable = set;
        }

        private void ShouldEnableSave(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            var lists = GetDynamicRowList.Match(textbox.Name);
            bool canSave = false;

            if (lists.Success)
            {
                var rowList = DynamicLists[$"DynamicRow_{lists.Groups[1].Value}"];
                foreach (var list in rowList)
                {
                    if (list is TextBox textBox)
                    {
                        if (string.IsNullOrWhiteSpace(textBox.Text))
                        {
                            canSave = false;
                            break;
                        }
                        else
                        {
                            canSave = true;
                        }
                    }
                }
            }

            SetSaveEnableState(canSave);
        }

        private void ShouldEnableSave(bool canSave)
        {
            SetSaveEnableState(canSave);
        }

        private void ShouldEnableSave()
        {
            foreach (var list in DynamicLists)
            {
                foreach (var element in list.Value)
                {
                    if (element is TextBox textBox)
                    {
                        if (string.IsNullOrWhiteSpace(textBox.Text))
                        {
                            SetSaveEnableState(false);
                            return;
                        }
                    }
                }
            }

            SetSaveEnableState(true);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddRow();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var rowArray = DynamicRows.Values.ToArray();
            for (int i = 0; i < rowArray.Length; i++)
            {
                var control = rowArray[i];
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

                elements.TryGetValue("Name", out string? name);
                elements.TryGetValue("Tag", out string? tag);
                elements.TryGetValue("Color", out string? color);
                elements.TryGetValue("Path", out string? path);
                elements.TryGetValue("FileKey", out string? fileKey);

                if (name != null && tag != null)
                {
                    var character = new Normal(name, tag, color, path);
                    if (ModifiyingCharacter.Count > 0)
                    {
                        var modifiedCharacter = ModifiyingCharacter[i];
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

            CharacterData.SaveCharacters();
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                OpenColorPickerDialog(btn);
            }
        }

        private void OpenColorPickerDialog(Button element)
        {
            var ColorPickerDialog = new ColorSelector(element.Tag as string);

            ColorPickerDialog.Owner = this;
            ColorPickerDialog.SetSelectedColor(element.Content as string ?? "#FF0000");
            ColorPickerDialog.ShowDialog();

            if (ColorPickerDialog.SelectedColor)
            {
                element.Content = ColorPickerDialog.GetSelectedColor();
            }
        }
    }
}