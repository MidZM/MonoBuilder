using ICSharpCode.AvalonEdit;
using MonoBuilder.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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

namespace MonoBuilder.Screens.ScreenUtils
{
    /// <summary>
    /// Interaction logic for LoadScripts.xaml
    /// </summary>
    public partial class LoadScripts : Window, ISynchronizable
    {
        private static readonly ObservableCollection<LoadedLabel> EmptyUnsyncedLabels = [];
        private static readonly ObservableCollection<LoadedLabel> EmptySyncedLabels = [];

        private AppSettings ApplicationSettings { get; set; }
        private FileEditor? Editor { get; }
        private ScriptConversion ScriptConverter { get; set; }

        private TextEditor InputBox { get; set; }
        private TextBox LabelBox { get; set; }

        public ObservableCollection<LoadedLabel> UnsyncedLabels => Editor?.GetUnsyncedLabelItems() ?? EmptyUnsyncedLabels;
        public ObservableCollection<LoadedLabel> SyncedLabels => Editor?.GetSyncedLabelItems() ?? EmptySyncedLabels;
        public ObservableBoolean UnsyncedLabelsExist { get; } = new();
        public ObservableBoolean SyncedLabelSelected { get; } = new();

        public LoadScripts(AppSettings settings, FileEditor? editor, ScriptConversion converter, TextEditor inputBox, TextBox labelBox)
        {
            ApplicationSettings = settings;
            Editor = editor;
            ScriptConverter = converter;
            InputBox = inputBox;
            LabelBox = labelBox;

            InitializeComponent();
            DataContext = this;

            if (settings.GetAllFilePaths("Script").Count == 0)
            {
                DialogBox.Show(
                    "A script file has not been specified. Labels cannot be synced or saved.\nTo specify a script file, open the Settings, scroll down, select \"ScriptFile\", locate and load your script file.",
                    "No script file",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning
                );
            }
            
            if (editor != null && !editor.ScriptInitialized())
            {
                editor.InitializeScripts();
            }

            CheckLabelValues();
        }

        private void CheckLabelValues()
        {
            UnsyncedLabelsExist.Value = UnsyncedLabels.Count > 0;
            SyncedLabelSelected.Value = DGSynced.SelectedItems.Count > 0 || DGUnsynced.SelectedItems.Count > 0;
        }

        public void RunSynchronicityCheck()
        {
            if (Editor != null && ScriptConverter.CheckIsAutoSyncLabels())
            {
                Editor.CheckSynchronicity(false);

                Editor.SaveProgram();
                ApplicationSettings.SetSynchronicityCheck(true);
            }
        }

        #region Event Handlers
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadLabel();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            BtnImport.IsEnabled = false;
            ChangesMadeLabel.Visibility = Visibility.Collapsed;
            try
            {
                var allScripts = ApplicationSettings.GetAllFilePaths("Script");
                foreach (string key in allScripts.Keys)
                {
                    Editor?.InitializeScripts(key, true);

                    BtnImport.Content = "Synced";

                    if (!UnsyncedLabelsExist.Value)
                    {
                        UnsyncedLabelsExist.Value = true;
                    }
                }
            }
            catch (Exception)
            {
                BtnImport.IsEnabled = true;
            }
        }

        private void BtnSync_Click(object sender, RoutedEventArgs e)
        {
            if (Editor != null)
            {
                var syncedLabels = SyncedLabels;
                var unsyncedLabels = UnsyncedLabels;
                foreach (var label in unsyncedLabels)
                {
                    label.Synced = true;
                    syncedLabels.Add(label);
                }

                unsyncedLabels.Clear();

                Editor.SaveProgram();
            }
        }

        private void BtnMergeWithScript_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            var labels = DGSynced.SelectedItems.Cast<LoadedLabel>().ToList();

            if (labels.Count > 0 && Editor != null)
            {
                var result = DialogBox.Show(
                    "Would you like to merge labels with the script?",
                    "Add Saved Labels",
                    DialogIcon.Question,
                    new DialogButton("Selected", DialogBoxResult.Continue),
                    new DialogButton("All", DialogBoxResult.Yes),
                    new DialogButton("Cancel", DialogBoxResult.Cancel, "ErrorButton"));

                if (result == DialogBoxResult.Continue || result == DialogBoxResult.Yes)
                {
                    try
                    {
                        var fileKeys = ApplicationSettings.GetAllFilePaths("Script").Keys.ToList();
                        List<string> labelNames = labels.Select(l => l.Name).ToList();
                        int maxLabelMerge = 10;
                        int currentLabelMerge = 0;
                        var mergeDialog = new StringBuilder();

                        var existingInScript = new HashSet<string>();
                        foreach (string key in fileKeys)
                        {
                            var existenceMap = Editor.ScriptLabelExists(labelNames, key);
                            foreach (var kvp in existenceMap)
                            {
                                if (kvp.Value) existingInScript.Add(kvp.Key);
                            }
                        }

                        foreach (LoadedLabel label in labels)
                        {
                            if (existingInScript.Contains(label.Name))
                            {
                                if (currentLabelMerge < maxLabelMerge)
                                    mergeDialog.Append($"- {label.Name}\n");
                                currentLabelMerge++;
                            }
                        }

                        if (currentLabelMerge > maxLabelMerge)
                            mergeDialog.Append($"- And {currentLabelMerge - maxLabelMerge} more...");

                        if (currentLabelMerge > 0)
                        {
                            var mergeResult = DialogBox.Show(
                                $"Detected labels that already exist within the script.\nWould you like to merge existing labels?\n(Regardless of the answer, any new labels will be added)\n\n{mergeDialog}",
                                "Exiting Labels Detected",
                                DialogButtonDefaults.YesNo,
                                DialogIcon.Warning);
                            if (mergeResult == DialogBoxResult.No)
                            {
                                labels.RemoveAll(label => label.InScript);
                            }
                        }

                        foreach (string key in fileKeys)
                        {
                            foreach (LoadedLabel label in labels)
                            {
                                if (label.InScript)
                                {
                                    Editor.MergeWithScript(label.Name, label.Content?.ToString() ?? string.Empty, key);
                                }
                                else
                                {
                                    Editor.AddToScript(label.Name, label.Content?.ToString() ?? string.Empty, key);
                                }
                            }
                        }

                        DialogBox.Show(
                            "Labels successfully saved to script!",
                            "Success",
                            DialogButtonDefaults.OK,
                            DialogIcon.Information);
                    }
                    catch (Exception error)
                    {
                        IsEnabled = true;
                        DialogBox.Show(
                            $"Something went wrong when adding labels!\n\n{error}",
                            "Failed to Add Labels",
                            DialogButtonDefaults.OK,
                            DialogIcon.Error);
                    }
                }
            }

            IsEnabled = true;
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            var labels = DGSynced.SelectedItems.Cast<LoadedLabel>().ToList();

            if (labels.Count > 0 && Editor != null)
            {
                var result = DialogBox.Show(
                    "Would you like to remove all currently selected labels?",
                    "Remove Saved Labels",
                    600,
                    DialogIcon.Error,
                    new DialogButton("From Program", DialogBoxResult.Continue),
                    new DialogButton("From Script", DialogBoxResult.Retry),
                    new DialogButton("From Both", DialogBoxResult.Yes, "ErrorButton"),
                    new DialogButton("Cancel", DialogBoxResult.Cancel, "ErrorButton"));

                if (result == DialogBoxResult.Continue || result == DialogBoxResult.Retry || result == DialogBoxResult.Yes)
                {
                    try
                    {
                        var fileKeys = ApplicationSettings.GetAllFilePaths("Script").Keys.ToList();
                        List<string> labelNames = labels.Select(l => l.Name).ToList();

                        foreach (string key in fileKeys)
                        {
                            foreach (var label in labelNames)
                            {
                                if (result == DialogBoxResult.Retry || result == DialogBoxResult.Yes)
                                {
                                    Editor.RemoveFromScript(label, false, key);
                                }

                                if (result == DialogBoxResult.Continue || result == DialogBoxResult.Yes)
                                {
                                    Editor.RemoveFromProgram(label, false, key);
                                }
                            }
                        }

                        Editor.SaveProgram();

                        DialogBox.Show(
                            "Labels removed successfully!",
                            "Success",
                            DialogButtonDefaults.OK,
                            DialogIcon.Information);
                    }
                    catch (Exception error)
                    {
                        IsEnabled = true;
                        DialogBox.Show(
                            $"Something went wrong when removing labels!\n\n{error}",
                            "Failed to Remove Labels",
                            DialogButtonDefaults.OK,
                            DialogIcon.Error);
                    }
                }
            }

            IsEnabled = true;
        }

        private void DG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadLabel();
        }

        private void DG_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var tab = (TabItem)MainTabControl.SelectedItem;
            var grid = Helpers.FirstOfLogicalType<DataGrid>(tab, true);
            var label = (LoadedLabel?)grid?.SelectedItem;

            PreviewBox.Document.Blocks.Clear();
            if (label != null)
            {
                var content = (label.Content?.ToString() ?? string.Empty).Split('\n');
                foreach (var line in content)
                {
                    PreviewBox.Document.Blocks.Add(new Paragraph(new Run(line)));
                }
            }

            CheckLabelValues();
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (e.OriginalSource is TabControl)
            {
                var tab = (TabItem)MainTabControl.SelectedItem;
                var grid = Helpers.FirstOfLogicalType<DataGrid>(tab, true);
                var labels = grid?.Items.Cast<LoadedLabel>().ToList();

                PreviewBox.Document.Blocks.Clear();
                DGSynced.SelectedIndex = -1;
                DGUnsynced.SelectedIndex = -1;

                grid?.SelectedIndex = -1;

                if (labels != null && labels.Count > 0)
                {
                    grid?.SelectedIndex = 0;
                }
            }

            CheckLabelValues();
        }
        #endregion

        #region Utility Methods
        private void LoadLabel()
        {
            IsEnabled = false;
            var tab = (TabItem)MainTabControl.SelectedItem;
            var grid = Helpers.FirstOfLogicalType<DataGrid>(tab, true);
            var label = (LoadedLabel?)grid?.SelectedItem;

            if (label != null)
            {
                var content = ScriptConverter.Deconvert(label.Content?.ToString() ?? string.Empty).Split('\n');
                LabelBox.Text = label.Name;
                InputBox.Document.Text = "";

                foreach (string lineData in content)
                {
                    InputBox.AppendText(lineData);
                }

                Close();
            }
        }
        #endregion
    }
}
