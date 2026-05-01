using MonoBuilder.Utils;
using MonoBuilder.Utils.character_management;
using MonoBuilder.Utils.image_management;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MonoBuilder.Screens.ScreenUtils
{
    /// <summary>
    /// Interaction logic for ImageBuilder.xaml
    /// </summary>
    public partial class ImageBuilder : Window
    {
        private AppSettings ApplicationSettings { get; set; }
        private MonoImages ImageData { get; set; }

        private CollectionViewSource? ImageCollectionView { get; set; }

        public ObservableCollection<string> AvailableFiles { get; set; } = [];
        private static readonly Regex GetFileKey = new(@"^\((.+?)\)", RegexOptions.Compiled);

        private double TotalWindowWidth = 0;
        private TimeSpan DebounceDuration { get; } = TimeSpan.FromMilliseconds(400);
        private DateTime LastClickedTime { get; set; } = DateTime.MinValue;

        private string Mode { get; set; } = "Images";

        public ImageBuilder(AppSettings settings, MonoImages imageData, string mode)
        {
            ApplicationSettings = settings;
            ImageData = imageData;
            Mode = mode;

            ImageData.SetDataMode(mode.ToLower());

            InitializeComponent();
            RunSetup();
            InitializeAvailableFiles();
            DataContext = this;

            switch (mode)
            {
                case "Images": Title = "MonoBuilder | Image Builder"; break;
                case "Scenes": Title = "MonoBuilder | Scene Builder"; break;
                case "Gallery": Title = "MonoBuilder | Gallery Builder"; break;
            }
        }

        #region Setup Methods
        private void InitializeImageTabs()
        {
            ImageFilterTabs.Items.Clear();

            ImageFilterTabs.Items.Add(new TabItem
            {
                Header = $"All {Mode}",
                Tag = "All"
            });

            var files = ApplicationSettings.GetAllFilePaths(Mode)
                .OrderBy(entry => entry.Key)
                .ToList();

            foreach (var (fileKey, filePath) in files)
            {
                var header = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrWhiteSpace(header))
                    header = fileKey;

                ImageFilterTabs.Items.Add(new TabItem
                {
                    Header = header,
                    Tag = fileKey
                });
            }

            var source =
                string.Equals(Mode, "Scenes", StringComparison.Ordinal) ?
                    ImageData.AllScenes :
                string.Equals(Mode, "Gallery", StringComparison.Ordinal) ?
                    ImageData.AllGalleryImages :
                    ImageData.AllImages;

            ImageCollectionView = new CollectionViewSource
            {
                Source = source
            };

            ImagesDataGrid.ItemsSource = ImageCollectionView.View;

            ImageFilterTabs.SelectedIndex = 0;
            ApplyImageTabFilter();
        }

        private void InitializeAvailableFiles()
        {
            AvailableFiles.Clear();

            foreach (var file in ApplicationSettings.GetAllFiles(Mode))
            {
                var value = $"({file.Key}) {file.Value}";
                AvailableFiles.Add(value);
            }
        }

        private void RunSetup()
        {
            var hasImageFiles = ApplicationSettings.GetAllFilePaths(Mode).Count > 0;

            ShouldEnableImageManipulationButtons(ImagesDataGrid);
            ShouldEnableScriptManipulationButtons(hasImageFiles);

            InitializeImageTabs();
        }
        #endregion

        #region Utility Methods

        private ContentManipulator? BuildContentManipulationWindow(bool isModifying = false, MonoImage[]? images = null)
        {
            var folderPath = ApplicationSettings.GetFolderPath(Mode);
            var filePath = ApplicationSettings.GetAllFilePaths(Mode);

            if (folderPath == null ||
                filePath.Count  == 0)
            {
                DialogBox.Show(
                    $"The \"{Mode}\" folder has not been set up yet. In order to preoprly build images, the folder being used by the engine to serve images must be set in the settings menu.",
                    $"No Valid {Mode} Folder",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
                return null;
            }

            ContentManipulator ManipulationWindow = new(ApplicationSettings, Mode, new ContentTemplate(
                columnHeaders:
                [
                    "Name",
                    "Path",
                    "File"
                ],
                contentBoxes:
                [
                    ContentBoxType.RequiredTextBox,
                    ContentBoxType.RequiredFileButton,
                    ContentBoxType.FileBox
                ],
                placeholders: new()
                {
                    { "Name", "Enter Image Name" },
                    { "Path", "Path to Image Assets" }
                }),
                isModifying ?
                images?
                    .Select(i => new Dictionary<string, string?>()
                    {
                        { "Name", i.Name },
                        { "Path", Path.Combine(folderPath, i.Path)},
                        { "File", i.FileKey }
                    })
                    .ToList() :
                null);
            ManipulationWindow.Owner = this;

            return ManipulationWindow;
        }

        private string GetDefaultImageFileKey()
        {
            return ApplicationSettings.GetAllFilePaths(Mode)
                .OrderBy(e => e.Key)
                .Select(e => e.Key)
                .FirstOrDefault() ?? string.Empty;
        }

        private void ShouldEnableImageManipulationButtons(DataGrid grid)
        {
            if (grid.SelectedCells.Count > 0)
            {
                BtnModifyImage.IsEnabled = true;
                BtnRemoveImage.IsEnabled = true;
            }
            else
            {
                BtnModifyImage.IsEnabled = false;
                BtnRemoveImage.IsEnabled = false;
            }
        }

        private void ShouldEnableScriptManipulationButtons(bool hasFiles)
        {
            if (hasFiles)
            {
                BtnImportImage.IsEnabled = true;
                BtnSyncImage.IsEnabled = true;
            }
            else
            {
                BtnImportImage.IsEnabled = false;
                BtnSyncImage.IsEnabled = false;
            }
        }

        private void OpenMoveImageSubmenu()
        {
            var selectedImage = ImagesDataGrid.SelectedItems.Cast<MonoImage>().ToList();
            var defaultFileKey = GetDefaultImageFileKey();
            string? commonFileKey =
                (selectedImage.Select(c => string.IsNullOrEmpty(c.FileKey)
                ? defaultFileKey
                : c.FileKey).Distinct().Count() == 1)
                    ? selectedImage.First().FileKey
                    : null;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in ContextMoveImage.Items)
                {
                    if (ContextMoveImage.ItemContainerGenerator.ContainerFromItem(item) is MenuItem container)
                    {
                        var match = GetFileKey.Match(item?.ToString() ?? string.Empty);
                        container.Icon = match.Success && commonFileKey != null && match.Groups[1].Value == commonFileKey
                            ? new TextBlock
                            {
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

        private void MoveImageToSelectedFile(MenuItem selectedTab)
        {
            string? selectedItem = selectedTab.Header.ToString() ?? null;
            var selectedImages = ImagesDataGrid.SelectedItems.Cast<MonoImage>().ToList();

            if (selectedImages.Count > 0 && selectedItem != null)
            {
                var match = GetFileKey.Match(selectedItem);

                if (match.Success)
                {
                    var targetFileKey = match.Groups[1].Value;
                    if (targetFileKey != null)
                    {
                        foreach (MonoImage image in selectedImages)
                        {
                            image.FileKey = targetFileKey;
                            ImageData.UpdateImage(image.EntityID, image);
                        }
                    }
                    else
                    {
                        DialogBox.Show(
                            $"The selected file path for {selectedItem} is not set. Please set it before moving images.",
                            "File Path Not Set",
                            DialogButtonDefaults.OK,
                            DialogIcon.Warning);
                    }
                }
            }
        }

        private void ApplyImageTabFilter()
        {
            if (ImageCollectionView?.View == null)
                return;

            var selectedTab = ImageFilterTabs.SelectedItem as TabItem;
            var selectedKey = selectedTab?.Tag as string;
            var defaultFileKey = GetDefaultImageFileKey();

            if (string.IsNullOrWhiteSpace(selectedKey) || selectedKey == "All")
            {
                ImageCollectionView.View.Filter = null;
                ImageCollectionView.View.Refresh();
                return;
            }

            ImageCollectionView.View.Filter = item =>
            {
                if (item is not MonoImage image)
                    return false;

                if (image.FileKey == selectedKey)
                    return true;

                return string.IsNullOrEmpty(image.FileKey) && selectedKey == defaultFileKey;
            };

            ImageCollectionView.View.Refresh();
            ImagesDataGrid.UnselectAll();
        }

        private void AddImages()
        {
            var AddImageWindow = BuildContentManipulationWindow();

            if (AddImageWindow != null)
            {
                AddImageWindow.ShowDialog();

                if (AddImageWindow.DidCloseWithSave == false) return;

                SaveImages(AddImageWindow);
            }
        }

        private void ModifyImages(MonoImage[] images)
        {
            var ModifyImageWindow = BuildContentManipulationWindow(true, images);

            if (ModifyImageWindow != null)
            {
                ModifyImageWindow.ShowDialog();

                if (ModifyImageWindow.DidCloseWithSave == false) return;

                SaveImages(ModifyImageWindow, images);

                UpdateImagePreview((MonoImage)ImagesDataGrid.SelectedItem);
            }
        }

        private void RemoveImages()
        {
            var selectedItems = ImagesDataGrid.SelectedItems.Cast<MonoImage>().ToList();

            if (selectedItems.Count == 0) return;

            int maxAmount = 10;
            string phrasing = selectedItems.Count > 1 ? "these images" : "this image";
            var imageNames = string.Join('\n', selectedItems
                .Cast<MonoImage>()
                .Take(maxAmount)
                .Select(image => $"- {image.Name}"));

            if (selectedItems.Count > maxAmount)
            {
                imageNames += $"- And {selectedItems.Count - maxAmount} more...";
            }

            var question = DialogBox.Show($"Are you sure you wish to remove {phrasing}?\r\n{imageNames}",
                "Confirm Removal",
                600,
                DialogIcon.Question,
                new DialogButton("From Program", DialogBoxResult.Continue),
                new DialogButton("From Script", DialogBoxResult.Retry),
                new DialogButton("From Both", DialogBoxResult.Yes, "ErrorButton"),
                new DialogButton("Cancel", DialogBoxResult.No, "ErrorButton"));

            if (question == DialogBoxResult.No) return;

            try
            {
                bool removeFromProgram = question == DialogBoxResult.Continue ||
                                         question == DialogBoxResult.Yes;

                bool removeFromScript  = question == DialogBoxResult.Retry ||
                                         question == DialogBoxResult.Yes;

                int[] imageIds = selectedItems.Select(i => i.EntityID).ToArray();

                if (removeFromScript)
                {
                    ImageData.RemoveImagesFromScript(imageIds, false);
                }

                if (removeFromProgram)
                {
                    ImageData.RemoveImages(imageIds, false);
                }

                ImageData.SaveImages();

                DialogBox.Show(
                    "Successfully Removed!",
                    "Success",
                    DialogButtonDefaults.OK,
                    DialogIcon.Information);
            }
            catch (Exception error)
            {
                DialogBox.Show(
                    $"Something went wrong when removing images!\n\n{error}",
                    "Failed to Remove Images",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
        }

        private void ImportImages()
        {
            var imageData = ImageData.SyncImages();

            var duplicates = imageData.Keys
                .Where(name => ImageData.ContainsName(name))
                .ToList();

            if (duplicates.Count > 0)
            {
                int selectedCount = duplicates.Count;
                int maxAmount = 10;
                string phrasing = duplicates.Count > 1 ? "multiple images" : "an image";
                string names = string.Join('\n', duplicates.Take(maxAmount).Select(i => $"- {i}"));

                if (selectedCount > maxAmount)
                {
                    names += $"\n- And {selectedCount - maxAmount} more...";
                }

                var result = DialogBox.Show(
                    $"Found {phrasing} with a similar name that already exist...\nDo you want to merge them?\r\n{names}",
                    "Confirm Merge Status",
                    DialogButtonDefaults.YesNo,
                    DialogIcon.Question);

                if (result == DialogBoxResult.Yes)
                {
                    foreach (string name in duplicates)
                    {
                        var existing = ImageData.CheckImage(name)!;
                        var newInfo = imageData[name];

                        ImageData.UpdateImage(existing.EntityID, new MonoImage(name, newInfo.Path)
                        {
                            FileKey = newInfo.FileKey,
                            IsSynced = true
                        });

                        imageData.Remove(name);
                    }
                }
                else
                {
                    foreach (string name in duplicates)
                        imageData.Remove(name);
                }
            }

            foreach (MonoImage image in imageData.Values)
            {
                image.IsSynced = true;
                ImageData.AddImage(image);
            }
        }

        private void SyncImages()
        {
            var selectedItems = ImagesDataGrid.SelectedItems.Cast<MonoImage>().ToList();

            var selectedButton = new DialogButton("Selected", DialogBoxResult.Continue);
            var allButton = new DialogButton("All", DialogBoxResult.Yes);
            var cancelButton = new DialogButton("Cancel", DialogBoxResult.No, "ErrorButton");

            if (selectedItems.Count == 0)
                selectedButton.Btn.IsEnabled = false;

            var result = DialogBox.Show(
                $"Would you like to update the selected images, or all images?",
                "Save Images To Script",
                DialogIcon.Question,
                selectedButton, allButton, cancelButton);

            if (result == DialogBoxResult.No) return;

            bool addSelected = result == DialogBoxResult.Continue;
            bool addAll      = result == DialogBoxResult.Yes;

            if (addAll) 
                selectedItems = ImagesDataGrid.Items.Cast<MonoImage>().ToList();

            try
            {
                var imagesHash = selectedItems.Select(image => image.Name).ToHashSet();
                Dictionary<string, bool> namesExist = ImageData.ImagesExistsInScript(imagesHash);

                int maxImageMerge = 10;
                int currentImageMerge = 0;
                string mergeDialog = "";
                bool skipExistingNames = false;

                foreach ((string name, bool inScript) in namesExist)
                {
                    if (inScript)
                    {
                        if (currentImageMerge < maxImageMerge)
                        {
                            mergeDialog += $"- {name}\n";
                        }
                        currentImageMerge++;
                    }
                }

                if (currentImageMerge > maxImageMerge)
                {
                    mergeDialog += $"- And {currentImageMerge - maxImageMerge} more...";
                }

                if (currentImageMerge > 0)
                {
                    if (DialogBox.Show(
                        $"Detected images that already exist within the script.\nWould you like to merge existing images?\n(Regardless of the answer, any new images will be added)\n\n{mergeDialog}",
                        "Existing Images Detected",
                        DialogButtonDefaults.YesNo,
                        DialogIcon.Warning) == DialogBoxResult.No)
                    {
                        skipExistingNames = true;
                    }
                }

                foreach (var (name, inScript) in namesExist)
                {
                    var content = skipExistingNames
                        ? null
                        : ImageData.ConvertToScriptContent(ImageData.CheckImage(name)!);

                    if (!skipExistingNames && inScript)
                    {
                        ImageData.UpdateImageInScript(name, content!);
                    }
                    else
                    {
                        ImageData.AddImageToScript(name, content!);
                    }
                }

                DialogBox.Show(
                    "Images successfully saved to script!",
                    "Success",
                    DialogButtonDefaults.OK,
                    DialogIcon.Information);
            }
            catch (Exception error)
            {
                DialogBox.Show(
                    $"Something went wrong when adding images!\n\n{error}",
                    "Failed to Add Images",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
        }

        private string GetRelativePath(string folderPath, string imagesIsActive)
        {
            var userProfile = imagesIsActive;
            string? result = Path.GetRelativePath(userProfile, folderPath).Replace('\\', '/');

            return result;
        }

        private void SaveImages(ContentManipulator window, MonoImage[]? images = null)
        {
            var names = new List<string>();
            window.DynamicRows.Values.ToList().ForEach(row =>
            {
                var nameElement = row.FirstOrDefault(e => e is TextBox tb && tb.Name.EndsWith("Name"));
                if (nameElement is TextBox nameBox)
                {
                    names.Add(nameBox.Text);
                }
            });

            var duplicateNames = names
                .GroupBy(t => t)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateNames.Count > 0)
            {
                var duplicateList = string.Join("\n", duplicateNames.Select(n => $"- {n}"));
                DialogBox.Show(
                    $"Duplicate names found:\n{duplicateList}\n\nPlease ensure all images have unique names.",
                    "Duplicate Names Detected",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning);
                return;
            }

            var rowArray = window.DynamicRows.Values.ToArray();
            for (int i = 0; i < rowArray.Length; i++)
            {
                ApplyImageChanges(window, rowArray[i], i, images);
            }

            ImageData.SaveImages();
        }

        private void ApplyImageChanges(ContentManipulator window, List<FrameworkElement> control, int index, MonoImage[]? images = null)
        {
            var elements = ExtractElements(control);

            elements.TryGetValue("Name", out string? name);
            elements.TryGetValue("Path", out string? path);
            elements.TryGetValue("File", out string? fileKey);

            if (name != null && path != null)
            {
                path = GetRelativePath(path, ApplicationSettings.GetFolderPath(Mode)!);
                var image = new MonoImage(name, path);
                if (window.ModifyingContent?.Count > 0)
                {
                    window.ModifyingContent[index].TryGetValue("Name", out string? nameValue);
                    var modifiedImage = images?.FirstOrDefault(c => c.Name == nameValue);

                    if (modifiedImage != null)
                    {
                        if (fileKey != null)
                        {
                            image.FileKey = fileKey;
                        }

                        ImageData.UpdateImage(modifiedImage.EntityID, image);
                    }
                }
                else
                {
                    if (fileKey != null)
                    {
                        image.FileKey = fileKey;
                    }

                    ImageData.AddImage(image);
                }
            }
        }

        private Dictionary<string, string> ExtractElements(List<FrameworkElement> control)
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

                    var textContent = (string)button.Tag;
                    elements.Add(lastElement, textContent);
                }
            }

            return elements;
        }

        private void UpdateImagePreview(MonoImage? image)
        {
            string? imagesPath = ApplicationSettings.GetFolderPath(Mode);

            if (image == null || string.IsNullOrWhiteSpace(imagesPath))
            {
                ImgPreview.Source = null;
                return;
            }

            try
            {
                string path = Path.Combine(imagesPath, image.Path);
                Uri uri = new(path, UriKind.Absolute);
                ImageSource imgSource = new BitmapImage(uri);
                ImgPreview.Source = imgSource;
            }
            catch
            {
                ImgPreview.Source = null;
            }
        }
        #endregion

        #region Event Handlers
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TotalWindowWidth == 0)
            {
                var margin = ImagesDataGrid.Margin.Left * 2;
                var borderThickness = ImagesDataGrid.BorderThickness.Left * 2;
                var windowResizeBorderThickness = (SystemParameters.WindowResizeBorderThickness.Left + SystemParameters.WindowNonClientFrameThickness.Left) * 2;
                TotalWindowWidth = SystemParameters.VerticalScrollBarWidth + margin + borderThickness + windowResizeBorderThickness + EditingContent.ColumnDefinitions[1].Width.Value + 1;
            }
            ImagesDataGrid.Width = e.NewSize.Width - TotalWindowWidth;
        }

        private void ImagesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                    ModifyImages(ImagesDataGrid.SelectedItems.Cast<MonoImage>().ToArray());
                }
            }
        }

        private void ImagesDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
                    ImgPreview.Source = null;
                    return;
                }

                while ((dep != null) && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                if (dep is DataGridRow row)
                {
                    if (row.IsSelected)
                    {
                        if (grid?.SelectedCells.Count > grid?.Columns.Count)
                        {
                            grid?.UnselectAll();
                            row.IsSelected = true;
                            UpdateImagePreview(row.Item as MonoImage);
                        }
                        else
                        {
                            row.IsSelected = false;
                            e.Handled = true;
                            ImgPreview.Source = null;
                        }
                    }
                }
            }
            else
            {
                DataGrid? grid = sender as DataGrid;
                UpdateImagePreview(grid?.SelectedItem as MonoImage);
            }
        }

        private void ImagesDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                var scrollViewer = Helpers.GetScrollViewer(ImagesDataGrid);
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

        private void ImagesDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (sender is DataGrid grid)
            {
                ShouldEnableImageManipulationButtons(grid);
                UpdateImagePreview(grid.SelectedItem as MonoImage);
            }
        }

        private void ContextMoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem clickedItem)
            {
                MoveImageToSelectedFile(clickedItem);
            }
        }

        private void ContextMoveImage_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            OpenMoveImageSubmenu();
        }

        private void ImageFilterTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyImageTabFilter();
        }

        private void BtnAddImage_Click(object sender, RoutedEventArgs e)
        {
            AddImages();
        }

        private void BtnModifyImage_Click(object sender, RoutedEventArgs e)
        {
            ModifyImages(ImagesDataGrid.SelectedItems.Cast<MonoImage>().ToArray());
        }

        private void BtnRemoveImage_Click(object sender, RoutedEventArgs e)
        {
            RemoveImages();
        }

        private void BtnSyncImage_Click(object sender, RoutedEventArgs e)
        {
            SyncImages();
        }

        private void BtnImportImage_Click(object sender, RoutedEventArgs e)
        {
            ImportImages();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }
}
