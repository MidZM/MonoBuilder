using Krypton.Toolkit;
using MonoBuilder.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonoBuilder.Screens.ScreenUtils
{
    public partial class AddModifyCharacter : KryptonForm
    {
        public bool IsModifying { get; set; } = false;
        public Character? CurrentCharacter { get; set; }
        private Characters characters { get; set; }
        private bool CanSave { get; set; } = false;
        private List<DynamicInputRow> dynamicRows = new List<DynamicInputRow>();

        private class DynamicInputRow
        {
            public FlowLayoutPanel? NamePanel { get; set; }
            public FlowLayoutPanel? TagPanel { get; set; }
            public FlowLayoutPanel? ColorPanel { get; set; }
            public FlowLayoutPanel? PathPanel { get; set; }
            public KryptonTextBox? NameInput { get; set; }
            public KryptonTextBox? TagInput { get; set; }
            public KryptonTextBox? ColorInput { get; set; }
            public KryptonTextBox? PathInput { get; set; }
            public KryptonColorButton? ColorButton { get; set; }
            public KryptonButton? RemoveButton { get; set; }
        }

        public AddModifyCharacter(Characters charactersList)
        {
            characters = charactersList;
            InitializeComponent();
        }

        private void SetupPage()
        {
            if (IsModifying)
            {
                this.Text = "MonoBuilder | Modify Character";
                lblHeader.Text = "Modify Character";
                if (CurrentCharacter != null)
                {
                    inputName.Text = CurrentCharacter.Name;
                    inputTag.Text = CurrentCharacter.Tag;
                    inputColor.Text = CurrentCharacter.Color;
                    inputPath.Text = CurrentCharacter.Directory;
                }

                CanSave = true;
                btnSave.Enabled = true;
                btnAdd.Hide();
            }
            else
            {
                DynamicInputRow row = new DynamicInputRow();

                row.NamePanel = namePanel;
                row.TagPanel = tagPanel;
                row.ColorPanel = colorPanel;
                row.PathPanel = pathPanel;

                row.NameInput = inputName;
                row.TagInput = inputTag;
                row.ColorInput = inputColor;
                row.PathInput = inputPath;
                row.ColorButton = btnColorSelect;

                dynamicRows.Add(row);
            }
        }

        private void AddModifyCharacter_Load(object sender, EventArgs e)
        {
            SetupPage();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (IsModifying)
            {
                Normal character = new Normal(
                    inputName.Text,
                    inputTag.Text,
                    inputColor.Text,
                    inputPath.Text
                );
                characters.UpdateCharacter(CurrentCharacter?.CharacterID ?? 0, character);
            }
            else
            {
                var duplicates = new List<string>();

                foreach (var row in dynamicRows)
                {
                    Normal character = new Normal(
                        row.NameInput?.Text ?? String.Empty,
                        row.TagInput?.Text ?? String.Empty,
                        row.ColorInput?.Text ?? String.Empty,
                        row.PathInput?.Text ?? String.Empty
                    );

                    if (characters.CheckedDuplicates(character))
                    {
                        duplicates.Add(character.Tag);
                        continue;
                    }

                    characters.AddCharacter(character);
                }

                if (duplicates.Count > 0)
                {
                    string tagList = string.Join("\n", duplicates.Select(t => $"- {t}"));
                    string noun = duplicates.Count == 1 ? "tag" : "tags";
                    DialogBox.Show(
                        $"The following {noun} already exist and were skipped:\n{tagList}",
                        "Duplicate Tags Skipped",
                        DialogButtonDefaults.OK,
                        DialogIcon.Warning);
                }
            }

            this.Close();
        }

        private string RGBToHex(Color color)
        {
            string hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            return hex;
        }

        private void SelectColor(KryptonTextBox input)
        {
            if (KryptonColorWheel.ShowDialog() == DialogResult.OK)
            {
                input.Text = RGBToHex(KryptonColorWheel.Color);
            }
            KryptonColorWheel.Dispose();
        }

        private void ColorFromHex(KryptonColorButton button, string? text)
        {
            if (text != null && text.StartsWith("#") && text.Length == 7)
            {
                try
                {
                    Color color = ColorTranslator.FromHtml(text);
                    button.SelectedColor = color;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private Control? GetControlByTag(Control root, object tag)
        {
            foreach (Control control in root.Controls)
            {
                if (control.Tag != null && control.Tag.Equals(tag))
                {
                    return control;
                }

                if (control.HasChildren)
                {
                    Control? foundControl = GetControlByTag(control, tag);
                    if (foundControl != null)
                    {
                        return foundControl;
                    }
                }
            }
            return null;
        }

        private void SetInputColor(KryptonColorButton button, ColorEventArgs ev)
        {
            KryptonTextBox? input = (KryptonTextBox?)TableContainer.Controls.Find((string?)button.Tag ?? "", true)[0];
            if (input != null)
            {
                input.Text = RGBToHex(ev.Color);
                button.SelectedColor = ev.Color;
            }
        }

        private void inputColor_TextChanged(object? sender, EventArgs e)
        {
            ColorFromHex(btnColorSelect, (sender as KryptonTextBox)?.Text);
        }

        private void btnColorSelect_Click(object sender, EventArgs e)
        {
            SelectColor(inputColor);
        }

        private void btnColorSelect_ColorChange(object? sender, ColorEventArgs e)
        {
            var button = sender as KryptonColorButton;
            if (button != null)
            {
                SetInputColor(button, e);
            }
        }

        private bool ShouldSave()
        {
            foreach (Control control in TableContainer.Controls)
            {
                if (control is FlowLayoutPanel flp)
                {
                    if (flp.Controls[0] is KryptonTextBox box &&
                        string.IsNullOrEmpty(box.Text) &&
                        !box.Name.Contains("Path") &&
                        !box.Name.Contains("Color"))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void StringValidation(KryptonTextBox textBox)
        {
            CanSave = false;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.StateCommon.Back.Color1 = Color.MistyRose;
                inputError.SetError(textBox, "Set a valid string value");
            }
            else
            {
                textBox.StateCommon.Back.Color1 = Color.FromArgb(50, 50, 50);
                inputError.SetError(textBox, String.Empty);
                CanSave = ShouldSave();
            }
        }

        private void StringInputValidation(object? sender, EventArgs e)
        {
            KryptonTextBox? input = sender as KryptonTextBox;
            if (input != null)
            {
                StringValidation(input);
                btnSave.Enabled = CanSave;
            }
        }

        private FlowLayoutPanel CreateFlowLayoutPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = false,
                Size = new Size(187, 41),
                Margin = new Padding(3),
                TabStop = false,
                FlowDirection = FlowDirection.LeftToRight  // Add this to ensure horizontal flow
            };
        }

        private KryptonTextBox CopyTextBoxAttributes(KryptonTextBox oldBox, string name)
        {
            KryptonTextBox newBox = new KryptonTextBox
            {
                Font = oldBox.Font,
                Size = oldBox.Size,
                Margin = oldBox.Margin,
                InputControlStyle = InputControlStyle.Custom1,
                Name = name,
                TabIndex = TableContainer.Controls.Count
            };

            if (!name.StartsWith("dynamicColor") &&
                !name.StartsWith("dynamicPath"))
            {
                newBox.TextChanged += StringInputValidation;
            }

            return newBox;
        }

        private void AddColorEvent(KryptonTextBox input, KryptonColorButton button)
        {
            input.TextChanged += (s, e) => ColorFromHex(button, (s as KryptonTextBox)?.Text);
        }

        private KryptonColorButton CreateColorButton(KryptonTextBox colorInput)
        {
            KryptonColorButton btn = new KryptonColorButton
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Size = new Size(64, 27),
                Margin = new Padding(3),
                Height = colorInput.Height,
                ButtonStyle = ButtonStyle.Alternate,
                SelectedRect = new Rectangle(0, -1, 25, 25),
                SelectedColor = Color.Red,
                Tag = colorInput.Name,
                Splitter = false,
                TabStop = false,
                Cursor = Cursors.Hand
            };

            btn.DoubleClick += (s, e) => SelectColor(colorInput);
            btn.SelectedColorChanged += btnColorSelect_ColorChange;

            return btn;
        }

        private KryptonButton CreateRemoveButton(DynamicInputRow row)
        {
            KryptonButton btn = new KryptonButton
            {
                Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Pixel),
                Size = new Size(35, 27),
                Margin = new Padding(3),
                ButtonStyle = ButtonStyle.Alternate,
                Text = "-",
                TabStop = false,
                Cursor = Cursors.Hand
            };

            btn.Click += (s, e) => RemoveDynamicRow(row);

            return btn;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            TableContainer.SuspendLayout();

            btnSave.Enabled = false;
            int newRowIndex = TableContainer.RowCount;

            TableContainer.RowCount++;
            TableContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 47F));

            DynamicInputRow newRow = new DynamicInputRow();

            // FlowLayoutPanels
            newRow.NamePanel = CreateFlowLayoutPanel();
            newRow.TagPanel = CreateFlowLayoutPanel();
            newRow.ColorPanel = CreateFlowLayoutPanel();
            newRow.PathPanel = CreateFlowLayoutPanel();
            newRow.PathPanel.Size = new Size(301, 41);

            // TextBoxes
            newRow.NameInput = CopyTextBoxAttributes(inputName, $"dynamicName_{newRowIndex}");
            newRow.TagInput = CopyTextBoxAttributes(inputTag, $"dynamicTag_{newRowIndex}");
            newRow.ColorInput = CopyTextBoxAttributes(inputColor, $"dynamicColor_{newRowIndex}");
            newRow.ColorInput.Size = new Size(107, 25);
            newRow.PathInput = CopyTextBoxAttributes(inputPath, $"dynamicPath_{newRowIndex}");
            newRow.PathInput.Size = new Size(250, 25);

            // Buttons
            newRow.ColorButton = CreateColorButton(newRow.ColorInput);
            newRow.RemoveButton = CreateRemoveButton(newRow);

            // Setup an Event on the Color Button
            AddColorEvent(newRow.ColorInput, newRow.ColorButton);

            // Controls to FlowLayoutPanels
            newRow.NamePanel.Controls.Add(newRow.NameInput);
            newRow.TagPanel.Controls.Add(newRow.TagInput);
            newRow.ColorPanel.Controls.Add(newRow.ColorInput);
            newRow.ColorPanel.Controls.Add(newRow.ColorButton);
            newRow.PathPanel.Controls.Add(newRow.PathInput);
            newRow.PathPanel.Controls.Add(newRow.RemoveButton);

            // FlowLayoutPanels to TableLayoutPanel
            TableContainer.Controls.Add(newRow.NamePanel, 0, newRowIndex);
            TableContainer.Controls.Add(newRow.TagPanel, 1, newRowIndex);
            TableContainer.Controls.Add(newRow.ColorPanel, 2, newRowIndex);
            TableContainer.Controls.Add(newRow.PathPanel, 3, newRowIndex);

            // Adjust TabIndexes
            btnAdd.TabIndex = TableContainer.ColumnCount * TableContainer.RowCount + 1;
            addPanel.TabIndex = TableContainer.ColumnCount * TableContainer.RowCount + 1;
            btnSave.TabIndex = TableContainer.ColumnCount * TableContainer.RowCount + 2;
            btnCancel.TabIndex = TableContainer.ColumnCount * TableContainer.RowCount + 3;

            dynamicRows.Add(newRow);

            MoveAddButtonToBottom();
            AdjustFormHeight();

            TableContainer.ResumeLayout(false);
            TableContainer.PerformLayout();
        }

        private void RemoveDynamicRow(DynamicInputRow row)
        {
            TableContainer.SuspendLayout();

            if (
                row.NamePanel != null &&
                row.TagPanel != null &&
                row.ColorPanel != null &&
                row.PathPanel != null
            )
            {
                int rowIndex = TableContainer.GetRow(row.NamePanel);

                TableContainer.Controls.Remove(row.NamePanel);
                TableContainer.Controls.Remove(row.TagPanel);
                TableContainer.Controls.Remove(row.ColorPanel);
                TableContainer.Controls.Remove(row.PathPanel);

                row.NamePanel.Dispose();
                row.TagPanel.Dispose();
                row.ColorPanel.Dispose();
                row.PathPanel.Dispose();

                var controls = TableContainer.Controls.Cast<Control>().ToList();
                for (int i = rowIndex + 1; i < TableContainer.RowCount; i++)
                {
                    foreach (Control control in controls)
                    {
                        if (TableContainer.GetRow(control) == i)
                        {
                            TableContainer.SetRow(control, i - 1);
                        }
                    }
                }

                if (TableContainer.RowStyles.Count > rowIndex)
                    TableContainer.RowStyles.RemoveAt(rowIndex);

                TableContainer.RowCount--;

                dynamicRows.Remove(row);
                AdjustFormHeight();

                CanSave = ShouldSave();
                btnSave.Enabled = CanSave;
            }

            TableContainer.ResumeLayout(true);
            TableContainer.PerformLayout();
        }

        private void MoveAddButtonToBottom()
        {
            var addButtonPanel = TableContainer.Controls
                .OfType<FlowLayoutPanel>()
                .FirstOrDefault(flp => flp.Controls.Contains(btnAdd));

            if (addButtonPanel != null)
            {
                TableContainer.SetRow(addButtonPanel, TableContainer.RowCount);
            }
        }

        private void AdjustFormHeight()
        {
            int baseHeight = 307;
            int rowHeight = 47;
            int maxHeight = this.MaximumSize.Height;
            int newHeight = Math.Min(baseHeight + (dynamicRows.Count * rowHeight), maxHeight);
            int tableHeight = (dynamicRows.Count + 1) * rowHeight + 100;
            int panelHeight = newHeight - 133;

            this.SuspendLayout();
            this.Height = newHeight;
            TableContainer.Height = tableHeight;
            settingsPanel.Height = panelHeight;
            this.ResumeLayout(true);
        }
    }
}
