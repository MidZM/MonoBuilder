using MonoBuilder.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MonoBuilder.Screens.ScreenUtils
{
    /// <summary>
    /// Interaction logic for ContentManipulator.xaml
    /// </summary>
    public partial class ContentManipulator : Window
    {
        private Microsoft.Win32.OpenFileDialog FilePath { get; set; } = new();

        private AppSettings ApplicationSettings { get; set; }
        private ContentTemplate DataTemplate { get; set; }
        private string Context { get; set; }

        public List<Dictionary<string, string?>>? ModifyingContent { get; set; } = new();

        private Dictionary<string, List<FrameworkElement>> DynamicLists { get; set; } = new();
        public Dictionary<string, List<FrameworkElement>> DynamicRows { get; set; } = new();
        public FreeList<string> RowState { get; set; } = new();

        private readonly static Regex GetDynamicRowList = new Regex(@"^DynamicRow_(\d+)", RegexOptions.Compiled);

        public ObservableCollection<string> AvailableFiles { get; set; } = new();
        public ObservableCollection<string> AvailableFileTags { get; set; } = new();

        public ObservableBoolean SaveBtnShouldEnable { get; set; } = new();
        public bool DidCloseWithSave { get; set; } = false;

        public ContentManipulator(AppSettings settings, string context, ContentTemplate template, List<Dictionary<string, string?>>? content = null)
        {
            ApplicationSettings = settings;
            Context = context;
            DataTemplate = template;
            ModifyingContent = content;

            InitializeComponent();
            DataContext = this;
            InitializeAvailableFiles(context);

            AddHeaders();
            var firstRow = AddRow(true);

            Title = $"Add {context}";
            LblEditingType.Content = $"Add {context}";

            if (content != null && content.Count > 0)
            {
                var titleValues = content
                    .Select(d => d.TryGetValue(template.ColumnHeaders[0], out var value) ? value?.ToString() : null)
                    .Where(v => !string.IsNullOrWhiteSpace(v));

                Title = $"Modify {context} | {string.Join(", ", titleValues)}";
                LblEditingType.Content = $"Modify {context}";
                BtnAdd.IsEnabled = false;
                BtnAdd.Visibility = Visibility.Collapsed;

                for (int i = 0; i < content.Count; i++)
                {
                    var contentData = content[i];
                    var rowElements = i == 0 ? firstRow : AddRow();
                    PopulateRow(rowElements, contentData);
                }

                ShouldEnableSave();
            }
            else
            {
                if (DataTemplate.ColumnHeaders.Contains("File"))
                {
                    if (firstRow["File"] is ComboBox comboBox)
                    {
                        comboBox.SelectedIndex = 0;
                    }
                }
            }
        }

        private void InitializeAvailableFiles(string context)
        {
            AvailableFiles.Clear();
            AvailableFileTags.Clear();

            foreach (var file in ApplicationSettings.GetAllFiles(context))
            {
                AvailableFiles.Add(file.Value);
                AvailableFileTags.Add(file.Key);
            }
        }

        #region Content Population
        private int GetFileIndex(Dictionary<string, string?> contentData)
        {
            contentData.TryGetValue("File", out var value);
            return AvailableFileTags.IndexOf(value ?? string.Empty);
        }

        private void PopulateRow(Dictionary<string, FrameworkElement> rowElements, Dictionary<string, string?> contentData)
        {
            var fileIndex = GetFileIndex(contentData);

            for (int j = 0; j < DataTemplate.ColumnHeaders.Count; j++)
            {
                var contentName = DataTemplate.ColumnHeaders[j];
                if (!rowElements.TryGetValue(contentName, out var rowElement))
                {
                    continue;
                }

                contentData.TryGetValue(contentName, out var value);
                if (rowElement is TextBox box)
                {
                    box.Text = value?.ToString() ?? string.Empty;
                }
                else if (rowElement is Button button)
                {
                    if (DataTemplate.ContentBoxes[j] == ContentBoxType.FileButton ||
                        DataTemplate.ContentBoxes[j] == ContentBoxType.RequiredFileButton)
                    {
                        var relativeValue = GetRelativePath(value?.ToString() ?? string.Empty, ApplicationSettings.GetFolderPath(Context));
                        button.Content = relativeValue;
                        button.Tag = value?.ToString() ?? string.Empty;
                    }
                    else
                    {
                        button.Content = value?.ToString() ?? string.Empty;
                    }
                }
                else if (rowElement is ComboBox select)
                {
                    if (DataTemplate.ContentBoxes[j] == ContentBoxType.FileBox)
                    {
                        select.SelectedIndex = fileIndex >= 0 ? fileIndex : 0;
                    }
                    else
                    {
                        select.Text = value?.ToString() ?? string.Empty;
                    }
                }
            }
        }
        #endregion

        #region Dynamic Content Management
        private void IncrementLists(FrameworkElement element, string rowName, bool isListElement = false)
        {
            if (isListElement)
                DynamicLists[rowName].Add(element);

            DynamicRows[rowName].Add(element);
        }

        private FrameworkElement CreateInputTemplate(string name, string rowName, string? placeholder, int column, int gridRow, ContentBoxType type, bool isListElement)
        {
            TextBox box = new ContentBox(name, type, column, gridRow, placeholder ?? string.Empty).Initialize() as TextBox ?? new TextBox();
            if (type == ContentBoxType.RequiredTextBox)
            {
                box.TextChanged += ShouldEnableSave;
            }

            IncrementLists(box, rowName, isListElement);

            return box;
        }

        private FrameworkElement CreateSelectTemplate(string name, string rowName, int column, int gridRow, ContentBoxType type, bool isListElement)
        {
            ComboBox select = new ContentBox(name, type, column, gridRow).Initialize() as ComboBox ?? new ComboBox();
            select.SelectedIndex = 0;

            if (type == ContentBoxType.RequiredComboBox ||
                type == ContentBoxType.RequiredEditableComboBox ||
                type == ContentBoxType.FileBox)
            {
                select.SelectionChanged += (_, _) => ShouldEnableSave();
                if (type == ContentBoxType.RequiredEditableComboBox)
                {
                    select.LostKeyboardFocus += (_, _) => ShouldEnableSave();
                }
            }

            IncrementLists(select, rowName, isListElement);

            return select;
        }

        private FrameworkElement CreateColorButton(string name, string rowName, int column, int gridRow, TextBox source, bool isListElement)
        {
            Button button = new ContentBox(name, ContentBoxType.ColorButton, column, gridRow, source: source).Initialize(this) as Button ?? new Button();
            button.Click += ColorButton_Click;

            IncrementLists(button, rowName, isListElement);

            return button;
        }

        private FrameworkElement CreateFileButton(string name, string rowName, int column, int gridRow, ContentBoxType type, bool isListElement)
        {
            Button button = new ContentBox(name, type, column, gridRow).Initialize(this) as Button ?? new Button();
            button.Click += FileButton_Click;

            IncrementLists(button, rowName, isListElement);

            return button;
        }

        private Button CreateRemoveRowButton(int column, int row)
        {
            Button button = new()
            {
                Name = $"RemoveRow_{row}",
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                Content = "—",
                FontWeight = FontWeights.UltraBold,
                Style = (Style)Application.Current.Resources["DefaultButton"]
            };

            button.Click += (s, e) =>
            {
                if (s is Button btn)
                {
                    int currentRow = Grid.GetRow(btn);
                    RemoveRowAt(currentRow);
                }
            };

            Grid.SetColumn(button, column);
            Grid.SetRow(button, row);

            return button;
        }
        #endregion

        #region Save State Management
        private void SetSaveEnableState(bool set)
        {
            SaveBtnShouldEnable.Value = set;
        }

        private void ShouldEnableSave(object sender, TextChangedEventArgs e)
        {
            ShouldEnableSave();
        }

        private void ShouldEnableSave()
        {
            if (DynamicLists.Count == 0)
            {
                SetSaveEnableState(true);
                return;
            }

            foreach (var row in DynamicLists.Values)
            {
                foreach (var element in row)
                {
                    if (element is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        SetSaveEnableState(false);
                        return;
                    }

                    if (element is Button button && button.Tag is string tag && string.IsNullOrWhiteSpace(tag))
                    {
                        SetSaveEnableState(false);
                        return;
                    }

                    if (element is ComboBox comboBox)
                    {
                        bool isValid = comboBox.IsEditable
                            ? !string.IsNullOrWhiteSpace(comboBox.Text)
                            : comboBox.SelectedIndex >= 0;

                        if (!isValid)
                        {
                            SetSaveEnableState(false);
                            return;
                        }
                    }
                }
            }

            SetSaveEnableState(true);
        }
        #endregion

        #region Dynamic Row Management
        private void CreateNewRow(int length)
        {
            ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(length) });
        }

        private void CreateNewColumn(int length)
        {
            ContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = length == 0 ? new GridLength(1, GridUnitType.Star) : new GridLength(length) });
        }

        private void RemoveRowAt(int row)
        {
            if (row < 1 || row >= ContentGrid.RowDefinitions.Count) return;

            List<FrameworkElement> elementsToRemove = new List<FrameworkElement>();
            foreach (FrameworkElement element in ContentGrid.Children)
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

            foreach (FrameworkElement element in elementsToRemove)
            {
                ContentGrid.Children.Remove(element);
            }

            var rowElement = elementsToRemove.FirstOrDefault(e => GetDynamicRowList.IsMatch(e.Name));
            if (rowElement == null)
            {
                ContentGrid.RowDefinitions.RemoveAt(row);
                Height -= 45;
                ShouldEnableSave();
                return;
            }

            var match = GetDynamicRowList.Match(rowElement.Name);
            int rowIndex = int.Parse(match.Groups[1].Value);
            string rowName = $"DynamicRow_{rowIndex}";

            DynamicLists.Remove(rowName);
            DynamicRows.Remove(rowName);
            RowState.RemoveAt(rowIndex);

            ContentGrid.RowDefinitions.RemoveAt(row);

            Height -= 45;

            ShouldEnableSave();
        }

        private void AddHeaders()
        {
            RowState.Add("LabelRow");

            CreateNewRow(30);
            for (int i = 0; i < DataTemplate.ColumnHeaders.Count; i++)
            {
                CreateNewColumn(0);

                string header = DataTemplate.ColumnHeaders[i];
                Label label = new()
                {
                    Name = $"LblContent{header}",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    Content = header
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, i);
                ContentGrid.Children.Add(label);
            }

            CreateNewColumn(50);
        }

        private Dictionary<string, FrameworkElement> AddRow(bool isFirstRow = false)
        {
            int rowKey = RowState.PeekIndex();
            string rowName = $"DynamicRow_{rowKey}";
            RowState.Add(rowName);
            System.Diagnostics.Debug.WriteLine("--------- Testing Rows ---------");
            System.Diagnostics.Debug.WriteLine(rowKey);
            System.Diagnostics.Debug.WriteLine(rowName);

            int gridRow = ContentGrid.RowDefinitions.Count;
            System.Diagnostics.Debug.WriteLine(gridRow);

            CreateNewRow(45);
            SetSaveEnableState(false);

            DynamicLists.Add(rowName, new List<FrameworkElement>());
            DynamicRows.Add(rowName, new List<FrameworkElement>());
            Dictionary<string, FrameworkElement> rowElements = [];

            for (int i = 0; i < DataTemplate.ColumnHeaders.Count; i++)
            {
                string rowElementName = $"{rowName}_{DataTemplate.ColumnHeaders[i]}";
                DataTemplate.Placeholders.TryGetValue(DataTemplate.ColumnHeaders[i], out string? placeholder);

                FrameworkElement element = DataTemplate.ContentBoxes[i] switch
                {
                    ContentBoxType.TextBox =>               CreateInputTemplate(rowElementName, rowName, placeholder, i, gridRow, ContentBoxType.TextBox, false),
                    ContentBoxType.RequiredTextBox =>       CreateInputTemplate(rowElementName, rowName, placeholder, i, gridRow, ContentBoxType.RequiredTextBox, true),
                    ContentBoxType.ColorButton =>           CreateColorButton(
                        rowElementName, rowName, i, gridRow,
                        DynamicRows[rowName].OfType<TextBox>().FirstOrDefault() ?? new TextBox(),
                        false),
                    ContentBoxType.FileButton =>            CreateFileButton(rowElementName, rowName, i, gridRow, ContentBoxType.FileButton, false),
                    ContentBoxType.RequiredFileButton =>    CreateFileButton(rowElementName, rowName, i, gridRow, ContentBoxType.RequiredFileButton, true),
                    ContentBoxType.ComboBox =>              CreateSelectTemplate(rowElementName, rowName, i, gridRow, ContentBoxType.ComboBox, false),
                    ContentBoxType.RequiredComboBox =>      CreateSelectTemplate(rowElementName, rowName, i, gridRow, ContentBoxType.RequiredComboBox, true),
                    ContentBoxType.EditableComboBox =>      CreateSelectTemplate(rowElementName, rowName, i, gridRow, ContentBoxType.EditableComboBox, false),
                    ContentBoxType.RequiredEditableComboBox => CreateSelectTemplate(rowElementName, rowName, i, gridRow, ContentBoxType.RequiredEditableComboBox, true),
                    ContentBoxType.FileBox =>               CreateSelectTemplate(rowElementName, rowName, i, gridRow, ContentBoxType.FileBox, true),
                    _ =>                                    CreateInputTemplate(rowElementName, rowName, placeholder, i, gridRow, ContentBoxType.TextBox, false)
                };

                ContentGrid.Children.Add(element);

                rowElements.Add(DataTemplate.ColumnHeaders[i], element);
            }

            if (!isFirstRow)
            {
                var removeButton = CreateRemoveRowButton(DataTemplate.Columns - 1, gridRow);
                ContentGrid.Children.Add(removeButton);
            }

            Height += 45;

            return rowElements;
        }
        #endregion

        #region Event Handlers
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

        private string GetRelativePath(string folderPath, string? baseIsActive)
        {
            var userProfile = baseIsActive ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string? result = "...\\" +
                Path.GetRelativePath(userProfile, folderPath);

            return result;
        }

        private void OpenFileSelectWindow(Button element)
        {
            var baseIsActive = ApplicationSettings.GetFolderPath("Base");
            var assetsIsActive = ApplicationSettings.GetFolderPath(Context);
            var directoryType = assetsIsActive ?? baseIsActive ?? string.Empty;

            FilePath.Filter = "All files (*.*)|*.*";
            //FilePath.InitialDirectory = assetsIsActive ?? baseIsActive ?? string.Empty;
            FilePath.FilterIndex = 1;
            FilePath.Multiselect = ModifyingContent?.Count > 0 ? false : true;

            if (FilePath.ShowDialog() == true)
            {
                string[] filePath = FilePath.FileNames;
                if (filePath.Length == 1)
                {
                    var result = GetRelativePath(filePath[0], directoryType);
                    element.Content = result;
                    element.Tag = filePath[0];
                    ShouldEnableSave();
                }
                else
                {
                    SetSaveEnableState(false);

                    var split = element.Name.Split("_");
                    string btnHeader = split[split.Length - 1];
                    int rowIndex = DynamicRows.Values.First().FindIndex(e => e.Name.EndsWith(btnHeader));

                    var firstResult = GetRelativePath(filePath[0], directoryType);
                    element.Content = firstResult;
                    element.Tag = filePath[0];

                    for (int i = 1; i < filePath.Length; i++)
                    {
                        var newElement = AddRow();
                        var newElementValuesList = newElement.Values.ToList();

                        Dictionary<string, string?> data = [];
                        for (int j = 0; j < newElement.Values.Count; j++)
                        {
                            var rowContent = newElementValuesList[j];
                            if (rowContent.Name.EndsWith(btnHeader))
                            {
                                data[btnHeader] = filePath[i];
                                rowContent.Tag = filePath[i];
                            }
                            else if (rowContent is ComboBox comboBox && DataTemplate.ContentBoxes[j] == ContentBoxType.FileBox)
                            {
                                data["File"] = AvailableFileTags[0];
                            }
                        }

                        PopulateRow(newElement, data);
                    }
                }

            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddRow();
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                OpenColorPickerDialog(btn);
            }
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                OpenFileSelectWindow(btn);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            DidCloseWithSave = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }

    public class ContentTemplate
    {
        public int Columns { get; set; }
        public List<string> ColumnHeaders { get; set; }
        public List<ContentBoxType> ContentBoxes { get; set; }
        public Dictionary<string, string> Placeholders { get; set; }

        public ContentTemplate(List<string> columnHeaders, List<ContentBoxType> contentBoxes, Dictionary<string, string>? placeholders = null)
        {
            if (columnHeaders.Count != contentBoxes.Count)
            {
                throw new ArgumentException("Column headers and content boxes must have the same number of elements.");
            }

            ColumnHeaders = columnHeaders;
            ContentBoxes = contentBoxes;
            Placeholders = placeholders ?? new Dictionary<string, string>();
            Columns = columnHeaders.Count + 1; // Account for the remove button column.
        }
    }

    public enum ContentBoxType
    {
        TextBox,
        RequiredTextBox,
        FileButton,
        RequiredFileButton,
        ColorButton,
        ComboBox,
        RequiredComboBox,
        EditableComboBox,
        RequiredEditableComboBox,
        FileBox
    }

    public class ContentBox
    {
        public string Name { get; set; } = string.Empty;
        public ContentBoxType Type { get; set; } = ContentBoxType.TextBox;
        public int Column { get; set; } = 0;
        public int Row { get; set; } = 0;
        public string Placeholder { get; set; } = string.Empty;

        // Temporary until I figure out the structure of other classes.
        private string[]? SourceSelection { get; set; } = null;
        private FrameworkElement? SourceElement { get; set; } = null;
        public FrameworkElement? Element { get; set; } = null;

        public FrameworkElement? Owner { get; set; } = null;

        public ContentBox(string name, ContentBoxType type, int column, int row, string placeholder = "", string[]? selection = null, FrameworkElement? source = null)
        {
            Name = name;
            Type = type;
            Column = column;
            Row = row;
            SourceSelection = selection;
            SourceElement = source;
            Placeholder = placeholder;
        }

        public FrameworkElement? Initialize(FrameworkElement? owner = null)
        {
            Owner = owner;
            FrameworkElement element =
                Type == ContentBoxType.ColorButton ?
                    InitColorButton() :
                (Type == ContentBoxType.FileButton ||
                Type == ContentBoxType.RequiredFileButton) ?
                    InitFileButton() :
                (Type == ContentBoxType.ComboBox ||
                Type == ContentBoxType.EditableComboBox ||
                Type == ContentBoxType.RequiredComboBox ||
                Type == ContentBoxType.RequiredEditableComboBox) ?
                    InitComboBox() :
                Type == ContentBoxType.FileBox ?
                    InitFileBox() :
                InitTextBox();

            Grid.SetColumn(element, Column);
            Grid.SetRow(element, Row);
            Element = element;

            return element;
        }

        private TextBox InitTextBox()
        {
            var box = new TextBox()
            {
                Name = Name,
                Tag = Placeholder,
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                Style = (Style)Application.Current.Resources["DefaultTextBox"]
            };

            return box;
        }

        private Button InitButton()
        {
            var button = new Button()
            {
                Name = Name,
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                Content = Placeholder,
                Style = (Style)Application.Current.Resources["DefaultButton"]
            };
            return button;
        }

        private Button InitFileButton()
        {
            var button = new Button()
            {
                Name = Name,
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                Content = "Select a File",
                Tag = string.Empty,
                Style = (Style)Application.Current.Resources["DefaultButton"]
            };
            return button;
        }

        private Button InitColorButton()
        {
            Binding textBinding = new Binding("Text")
            {
                Source = SourceElement
            };

            var button = new Button()
            {
                Name = Name,
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                Content = "#FF0000",
                FontWeight = FontWeights.Bold
            };

            if (Owner != null)
            {
                button.Style = (Style)Owner.FindResource("ColorButton");
            }

            button.SetBinding(Button.TagProperty, textBinding);

            return button;
        }

        private ComboBox InitComboBox()
        {
            var select = new ComboBox()
            {
                Name = Name,
                Tag = Placeholder,
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                Style = (Style)Application.Current.Resources["DefaultComboBox"],
                IsEditable = Type == ContentBoxType.EditableComboBox
            };

            if (SourceSelection != null)
            {
                select.ItemsSource = SourceSelection;
            }

            return select;
        }

        private ComboBox InitFileBox()
        {
            Binding fileBinding = new Binding("AvailableFiles")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Window), 1)
            };
            Binding tagBinding = new Binding("AvailableFileTags")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Window), 1)
            };

            var select = new ComboBox()
            {
                Name = Name,
                Margin = new Thickness(5),
                Padding = new Thickness(3),
                Style = (Style)Application.Current.Resources["DefaultComboBox"]
            };

            select.SetBinding(ComboBox.ItemsSourceProperty, fileBinding);
            select.SetBinding(ComboBox.TagProperty, tagBinding);

            return select;
        }
    }
}
