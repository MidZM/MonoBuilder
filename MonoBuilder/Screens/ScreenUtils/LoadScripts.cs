using Krypton.Toolkit;
using MonoBuilder.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MonoBuilder.Screens.ScreenUtils
{
    public partial class LoadScripts : KryptonForm, ISynchronizable
    {
        private AppSettings ApplicationSettings { get; set; }
        private FileEditor? Editor { get; set; }
        private ScriptConversion Converter { get; set; }

        private RichTextBox InputBox { get; set; }
        private TextBox LabelBox { get; set; }

        private BindingSource UnsavedSource { get; set; } = new BindingSource();
        private BindingSource SavedSource { get; set; } = new BindingSource();

        private ScreenTransitioner? Transitioner { get; set; }
        private KryptonPanel? Panel { get; set; }

        public LoadScripts(AppSettings settings, FileEditor? editor, ScriptConversion converter, RichTextBox inputBox, TextBox labelBox)
        {
            InitializeComponent();
            labelGridUnsaved.DataBindingComplete += DataBindingComplete;
            labelGridUnsaved.DataBindingComplete += EnableSaveButton;

            ApplicationSettings = settings;
            Editor = editor;
            Converter = converter;
            InputBox = inputBox;
            LabelBox = labelBox;

            labelGridUnsaved.DataSource = UnsavedSource;
            labelGridSaved.DataSource = SavedSource;

            if (settings.GetFilePath("Script") == null)
            {
                btnSyncLabels.Enabled = false;
                DialogBox.Show(
                    "A script file has not been specified. Labels cannot be synced or saved.\nTo specify a script file, open the Settings, scroll down, select \"ScriptFile\", locate and load your script file.",
                    "No script file",
                    DialogButtonDefaults.OK,
                    DialogIcon.Warning
                );
            }
            else if (Editor != null && Editor.ScriptInitialized())
            {
                Editor.InitializeScripts();
                PopulateUnsavedGrid();
            }

            PopulateSavedGrid();

            SetTabHeader(tabUnsavedLabels, BackColor: Color.Brown, ForeColor: Color.White);
            SetTabHeader(tabSavedLabels, BackColor: Color.LightBlue, ForeColor: Color.Black);

            int margin = 2;
            ScriptTypes.Region = new Region(new Rectangle(
                margin,
                margin,
                ScriptTypes.Width - (margin * 2),
                ScriptTypes.Height - (margin * 2)));
        }

        public void RunSynchronicityCheck()
		{
			if (Editor != null && Converter.CheckIsAutoSyncLabels())
            {
                try
				{
					var marginWidth = this.Width / 2 - 250;
                    var marginHeight = this.Height / 2 - 100;
                    var transitioner = new ScreenTransitioner(
                        location: new Point(marginWidth, marginHeight),
                        width: 350,
                        height: 100,
                        font: new System.Drawing.Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold),
                        text: "Checking Synced Scripts...",
                        foreColor: Color.White,
                        backColor: Color.FromArgb(20, 20, 20));

                    var panel = transitioner.Show();

                    this.Controls.Add(panel);
                    panel.BringToFront();

                    this.Enabled = false;
                    Editor.CheckSynchronicity(false);

                    Editor.SaveProgram();
                    ApplicationSettings.SetSynchronicityCheck(true);

                    this.Controls.Remove(panel);
                    transitioner.Hide();
                    this.Enabled = true;

                    PopulateSavedGrid();
                    PopulateUnsavedGrid();
                }
                catch (Exception error)
                {
                    DialogBox.Show(
                        $"An error occurred when running an initial synchronicity check!\n\n{error}",
                        "Error",
                        DialogButtonDefaults.OK,
                        DialogIcon.Error);
                }
            }
        }

        private void LoadScripts_Load(object sender, EventArgs e)
        {
            var UnsavedGrid = ScriptTypes.TabPages[0].Controls
                .Find("labelGridUnsaved", true)[0] as DataGridView;
            var SavedGrid = ScriptTypes.TabPages[1].Controls
                .Find("labelGridSaved", true)[0] as DataGridView;

            if (SavedGrid != null && UnsavedGrid != null)
            {
                if (SavedGrid.Rows.Count == 0) return;
                if (UnsavedGrid.Rows.Count == 0)
                {
                    ScriptTypes.SelectedIndex = 1;
                }
            }
        }

        private void btnSyncScripts_Click(object sender, EventArgs e)
        {
            btnSyncLabels.Enabled = false;
            ChangesMadeLabel.Visible = false;
            try
            {
                ShowLoadingBox();

                Editor?.InitializeScripts(true);
                PopulateUnsavedGrid();

                if (ScriptTypes.TabPages[0].Controls[0] is DataGridView grid)
                {
                    if (grid.Rows.Count > 0)
                    {
                        ScriptTypes.SelectedIndex = 0;
                    }
                }

                HideLoadingBox();

                btnSyncLabels.Values.Text = "Synced";
                ChangesMadeLabel.Visible = false;
            }
            catch (Exception)
            {
                btnSyncLabels.Enabled = true;
                btnSyncLabels.Enabled = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void previewBox_TextChanged(object sender, EventArgs e)
        {
            var box = sender as RichTextBox;
            var grid = ScriptTypes.TabPages[ScriptTypes.SelectedIndex].Controls
                .OfType<DataGridView>()
                .FirstOrDefault();

            if (box != null && grid != null && grid.SelectedRows.Count > 0)
            {
                btnLoad.Enabled = true;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadLabel();
        }

        private void GridCellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            LoadLabel();
        }

        private void LoadLabel()
        {
            var grid = ScriptTypes.TabPages[ScriptTypes.SelectedIndex].Controls
                .OfType<DataGridView>()
                .FirstOrDefault();

            if (grid != null && grid.SelectedRows.Count > 0)
            {
                LabelBox.Text = (string)grid.SelectedRows[0].Cells[0].Value;
                InputBox.Text = Converter.Deconvert(previewBox.Text);
                Close();
            }
        }

        private void DisplayFormattedOutput(List<LineFormatInfo> lines)
        {
            RichTextBoxHelper.DisplayFormattedLines(previewBox, lines);
        }

        private void PopulateUnsavedGrid()
        {
            UnsavedSource.DataSource = Editor?.GetUnsyncedBindingSource();
        }

        private void PopulateSavedGrid()
        {
            SavedSource.DataSource = Editor?.GetSyncedBindingSource();
        }

        private void LabelGrid_SelectionChanged(object sender, EventArgs e)
        {
            var grid = sender as DataGridView;
            var selectedTabGrid = ScriptTypes.TabPages[ScriptTypes.SelectedIndex].Controls
                .OfType<DataGridView>()
                .FirstOrDefault();

            if (grid != null)
            {
                if (grid.SelectedRows.Count == 0 || selectedTabGrid?.SelectedRows.Count == 0)
                {
                    previewBox.Text = "";
                    return;
                }

                string? selectedLabel = grid.SelectedRows[0].Cells[grid.Columns[0].Name].Value?.ToString();

                if (selectedLabel != null)
                {
                    if (Editor != null)
                    {
                        var labels = grid.Name == "labelGridUnsaved" ?
                            Editor.GetUnsyncedLabels() :
                            Editor.GetSyncedLabels();
                        labels.TryGetValue(selectedLabel, out var content);
                        if (content != null)
                        {
                            previewBox.Text = content.Content?.ToString();
                        }
                    }
                }
            }
        }

        private void btnSaveLabel_Click(object sender, EventArgs e)
        {
            if (Editor != null)
            {
                var syncedLabels = Editor.GetSyncedLabels();
                var unsynchedLabels = Editor.GetUnsyncedLabels();
                foreach (DataGridViewRow row in labelGridUnsaved.Rows)
                {
                    var label = Editor.GetLabel(row.Cells[0].Value.ToString() ?? string.Empty);
                    if (label != null)
                    {
                        label.Synced = true;
                        syncedLabels[row.Cells[0].Value.ToString() ?? string.Empty] = label;
                    }
                    else
                    {
                        DialogBox.Show(
                            "Attempted to save a label that doesn't exist!",
                            "Missing Label",
                            DialogButtonDefaults.OK,
                            DialogIcon.Warning);
                    }
                }

                unsynchedLabels.Clear();

                Editor.SaveProgram();
                PopulateSavedGrid();
                PopulateUnsavedGrid();
            }
        }

        private void btnRemoveLabels_Click(object sender, EventArgs e)
        {
            var selectedRows = labelGridSaved.SelectedRows;
            if (selectedRows.Count > 0 && Editor != null)
            {
                var result = DialogBox.Show(
                    "Would you like to remove all currently selected labels?",
                    "Remove Saved Labels",
                    600,
                    DialogIcon.Error,
                    new DialogButton("From Program", DialogResult.Continue),
                    new DialogButton("From Script", DialogResult.Retry),
                    new DialogButton("From Both", DialogResult.Yes, ButtonStyle.Custom2),
                    new DialogButton("Cancel", DialogResult.Cancel, ButtonStyle.Custom2));
                if (result == DialogResult.Continue || result == DialogResult.Retry || result == DialogResult.Yes)
                {
                    try
                    {
                        List<string> names = new List<string>();
                        var savedLabels = Editor.GetSyncedLabels();
                        foreach (DataGridViewRow row in selectedRows)
                        {
                            var name = row.Cells[0].Value.ToString() ?? string.Empty;

                            if (savedLabels != null && name != string.Empty)
                            {
                                if (result != DialogResult.Yes &&
                                    result != DialogResult.Retry)
                                    Editor.RemoveFromProgram(name);

                                names.Add(name);
                            }
                        }

                        if (result == DialogResult.Retry)
                        {
                            foreach (string name in names)
                            {
                                Editor.RemoveFromScript(name, false);
                            }
                        }

                        if (result == DialogResult.Yes)
                        {
                            foreach (string name in names)
                            {
                                Editor.RemoveFromScript(name, false);
                                Editor.RemoveFromProgram(name);
                            }
                        }

                        Editor.SaveProgram();
                        PopulateSavedGrid();
                    }
                    catch (Exception error)
                    {
                        DialogBox.Show(
                            $"Something went wrong when removing labels!\n\n{error}",
                            "Failed to Remove Labels",
                            DialogButtonDefaults.OK,
                            DialogIcon.Error);
                    }
                }
            }
        }

        private void btnMergeLabels_Click(object sender, EventArgs e)
        {
            var selectedRows = labelGridSaved.SelectedRows;
            if (selectedRows.Count > 0 && Editor != null)
            {
                var result = DialogBox.Show(
                    "Would you like to merge labels with the script?",
                    "Add Saved Labels",
                    DialogIcon.Question,
                    new DialogButton("Selected", DialogResult.Continue),
                    new DialogButton("All", DialogResult.Yes),
                    new DialogButton("Cancel", DialogResult.Cancel, ButtonStyle.Custom2));
                if (result == DialogResult.Continue || result == DialogResult.Yes)
                {
                    try
                    {
                        List<(string, bool)> labels = new List<(string, bool)>();
                        int maxLabelMerge = 10;
                        int currentLabelMerge = 0;
                        string mergeDialog = "";
                        int index = 0;

                        foreach (DataGridViewRow row in selectedRows)
                        {
                            var inScript = Editor.ScriptLabelExists((string)row.Cells[0].Value);
                            (string label, bool synced) = ((string)row.Cells[0].Value, inScript);

                            if (inScript)
                            {
                                if (currentLabelMerge  < maxLabelMerge)
                                {
                                    mergeDialog += $"- {label}\n";
                                }
                                currentLabelMerge++;
                            }

                            labels.Add((label, synced));

                            if (index == selectedRows.Count - 1)
                            {
                                if (currentLabelMerge > maxLabelMerge)
                                {
                                    mergeDialog += $"- And {currentLabelMerge - maxLabelMerge} more...";
                                }
                            }

                            index++;
                        }

                        if (currentLabelMerge > 0)
                        {
                            if (DialogBox.Show(
                                $"Detected labels that already exist within the script.\nWould you like to merge existing labels?\n(Regardless of the answer, any new labels will be added)\n\n{mergeDialog}",
                                "Exiting Labels Detected",
                                DialogButtonDefaults.YesNo,
                                DialogIcon.Warning) == DialogResult.No)
                            {
                                Predicate<(string, bool)> value = tuple => tuple.Item2;
                                labels.RemoveAll(value);
                            }
                        }

                        foreach (var (label, inScript) in labels)
                        {
                            var typeLabels = Editor.GetSyncedLabels();
                            typeLabels.TryGetValue(label, out LoadedLabel? content);
                            if (content != null && content.Content != null)
                            {
                                var formatted = Editor.FormatToScript(label, content.Content.ToString());
                                if (inScript)
                                {
                                    Editor.MergeWithScript(label, formatted);
                                }
                                else
                                {
                                    Editor.AddToScript(label, formatted);
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
                        DialogBox.Show(
                            $"Something went wrong when adding labels!\n\n{error}",
                            "Failed to Add Labels",
                            DialogButtonDefaults.OK,
                            DialogIcon.Error);
                    }
                }
            }
        }

        private void ScriptTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (TabPage tab in ScriptTypes.TabPages)
            {
                var grid = tab.Controls.OfType<DataGridView>().FirstOrDefault();
                if (grid != null && grid.TabIndex == ScriptTypes.SelectedIndex)
                {
                    LabelGrid_SelectionChanged(grid, e);

                    if (grid.SelectedRows.Count == 0)
                    {
                        btnLoad.Enabled = false;
                    }
                }
            }
        }

        private void DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid != null)
            {
                int nameColumnIndex = 0;
                int syncedColumnIndex = 2;
                int inScriptColumnIndex = 3;

                int syncedColumnWidth = 50;
                int inScriptColumnWidth = 60;

                foreach (DataGridViewColumn col in grid.Columns)
                {
                    if (col.Index == nameColumnIndex)
                    {
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }

                    if (col.Index == syncedColumnIndex)
                    {
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        col.Width = syncedColumnWidth;
                    }

                    if (col.Index == inScriptColumnIndex)
                    {
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        col.Width = inScriptColumnWidth;
                    }
                }
            }
        }

        private void EnableSaveButton(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid != null)
            {
                btnSaveLabel.Enabled = grid.Rows.Count > 0;
            }
        }

        private void ShowLoadingBox()
        {
            ScriptTypes.Enabled = false;
            btnClose.Enabled = false;
            btnLoad.Enabled = false;
            try
            {
                var marginWidth = (ScriptTypes.Width / 2 - 50) - 12;
                var marginHeight = (ScriptTypes.Height / 2) - 12;
                Transitioner = new ScreenTransitioner(new Point(marginWidth, marginHeight));
                Panel = Transitioner.Show();

                ControlPanel.Controls.Add(Panel);
                Panel.BringToFront();
            }
            catch (Exception error)
            {
                DialogBox.Show(
                    $"Encountered an error while syncing labels\n\n{error}",
                    "Syncing Failed",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
        }

        private void HideLoadingBox()
        {
            var grid = ScriptTypes.TabPages[ScriptTypes.SelectedIndex].Controls
                .OfType<DataGridView>()
                .FirstOrDefault();

            Transitioner?.Hide();
            Transitioner = null;

            ScriptTypes.Enabled = true;
            btnClose.Enabled = true;
            btnLoad.Enabled = grid?.SelectedRows.Count > 0 ? true : false;
        }

        private Dictionary<TabPage, Dictionary<string, Color>> TabColors = new Dictionary<TabPage, Dictionary<string, Color>>();

        private void SetTabHeader(TabPage page, Color? BackColor, Color? ForeColor)
        {
            TabColors[page] = new Dictionary<string, Color>()
            {
                { "BackColor", BackColor ?? Color.LightGray },
                { "ForeColor", ForeColor ?? Color.Black }
            };
            ScriptTypes.Invalidate();
        }

        private void ScriptTypes_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = ScriptTypes.TabPages[e.Index];
            TabColors.TryGetValue(page, out Dictionary<string, Color>? tabColors);

            Color backColor = tabColors?["BackColor"] ?? Color.LightGray;
            Color foreColor = tabColors?["ForeColor"] ?? Color.Black;

            // 1. Draw Background
            using (Brush br = new SolidBrush(backColor))
            {
                Rectangle rect = e.Bounds;
                rect.Inflate(0, 2);
                e.Graphics.FillRectangle(br, rect);
            }

            // 2. Draw Text (Using TextRenderer for better quality)
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            if (e.State == DrawItemState.Selected)
            {
                var font = new Font(page.Font.Name, 9, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, page.Text, font, e.Bounds, foreColor, flags);
            }
            else
            {
                TextRenderer.DrawText(e.Graphics, page.Text, page.Font, e.Bounds, foreColor, flags);
            }
        }

        private void LoadScripts_FormClosing(object sender, FormClosingEventArgs e)
        {
            Elements.Clear();
        }

        private void GridCellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            var box = sender as KryptonDataGridView;
            if (box != null)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                    box.Cursor = Cursors.Hand;
            }
        }

        private void GridCellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            var box = sender as KryptonDataGridView;
            if (box != null)
            {
                labelGridUnsaved.Cursor = Cursors.Default;
            }
        }

        #region Reorder Synced Labels Functionality
        private Rectangle DragBoxFromMouseDown;
        private int RowIndexFromMouseDown = -1;
        private int OriginalMouseY = -1;

        private void RowDragMouseDown(object sender, MouseEventArgs e)
        {
            if (Editor != null)
            {
                RowIndexFromMouseDown = labelGridSaved.HitTest(e.X, e.Y).RowIndex;

                if (RowIndexFromMouseDown != -1)
                {
                    Size dragSize = SystemInformation.DragSize;
                    DragBoxFromMouseDown = new Rectangle(
                        new Point(
                            e.X - (dragSize.Width / 2),
                            e.Y - (dragSize.Height / 2)),
                        dragSize);

                    OriginalMouseY = e.Y;
                }
                else
                {
                    DragBoxFromMouseDown = Rectangle.Empty;
                }
            }
        }

        private void RowDragMouseMove(object sender, MouseEventArgs e)
        {
            if (
                RowIndexFromMouseDown >= 0 &&
                e.Button == MouseButtons.Left &&
                (e.Y < OriginalMouseY - 5 || e.Y > OriginalMouseY + 5))
            {
                labelGridSaved.DoDragDrop(labelGridSaved.Rows[RowIndexFromMouseDown], DragDropEffects.Move);
            }
        }

        private void RowDragOver(object sender, DragEventArgs e)
        {
            if (Editor == null || e.Data == null) return;

            if (e.Data.GetDataPresent(typeof(DataGridViewRow)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void RowDragDrop(object sender, DragEventArgs e)
        {
            if (Editor != null && e.Data != null)
            {
                Point clientPoint = labelGridSaved.PointToClient(new Point(e.X, e.Y));
                int rowIndexFromDrop = labelGridSaved.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

                if (e.Data.GetDataPresent(typeof(DataGridViewRow)))
                {
                    DataGridViewRow? rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
                    int? originalIndex = rowToMove?.Index;

                    if (rowIndexFromDrop! >= 0 && rowIndexFromDrop != originalIndex)
                    {
                        var syncedLabels = Editor.GetSyncedLabels();
                        string key = syncedLabels.Keys.ElementAt(RowIndexFromMouseDown);
                        var value = syncedLabels[RowIndexFromMouseDown];
                        syncedLabels.RemoveAt(RowIndexFromMouseDown);
                        syncedLabels.Insert(rowIndexFromDrop, key, value!);

                        PopulateSavedGrid();
                        Editor.SaveProgram();

                        labelGridSaved.ClearSelection();
                        labelGridSaved.Rows[rowIndexFromDrop].Selected = true;
                    }
                }
            }

            RowIndexFromMouseDown = -1;
        }
        #endregion
    }
}