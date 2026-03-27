using Krypton.Toolkit;
using MonoBuilder.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonoBuilder.Screens.ScreenUtils
{
    public partial class ScriptBuilderOutput : KryptonForm
    {
        private AppSettings ApplicationSettings { get; set; }
        private ScriptConversion ScriptConverter { get; set; }
        private FileEditor? Editor { get; set; }
        private string Label { get; set; }
        private string Script { get; set; }

        public ScriptBuilderOutput(ScriptConversion converter, FileEditor? editor, AppSettings settings, string label, string script)
        {
            ApplicationSettings = settings;
            ScriptConverter = converter;
            Editor = editor;
            Label = label;
            Script = script;

            InitializeComponent();

            if (Editor != null)
            {
                btnAddScript.Enabled = true;
            }

            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(script))
            {
                DialogBox.Show(
                    $"Missing label and/or script!\r\nLabel:\n{label}\nScript\n{script}",
                    "Missing Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
                Close();
                return;
            }

            if (converter.CheckIsFormattingColor())
            {
                var formattedLines = converter.ConvertWithFormtting(label, script);
                DisplayFormattedOutput(formattedLines);
            }
            else
            {
                string result = converter.Convert(label, script);
                scriptOutput.Text = result;
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCopyClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(scriptOutput.Text);
        }

        private void btnAddScript_Click(object sender, EventArgs e)
        {
            if (Editor != null)
            {
                if (Editor.ScriptLabelExists(Label))
                {
                    if (DialogBox.Show(
                        $"A label with this name already exists.\nAre you sure you would like to overwrite it?\n\nLabel: \"{Label}\"",
                        "Label Exists",
                        DialogButtonDefaults.YesNo,
                        DialogIcon.Warning) == DialogResult.Yes)
                    {
                        var formatted = Editor.FormatToScript(scriptOutput.Text);
                        Editor.MergeWithScript(Label, formatted);
                    }
                    else
                    {
                        DialogBox.Show(
                            "Label merge aborted!", "",
                            DialogButtonDefaults.OK,
                            DialogIcon.Information);
                        return;
                    }
                }
                else
                {
                    var formatted = Editor.FormatToScript(scriptOutput.Text);
                    Editor.AddToScript(Label, formatted);
                }

                DialogBox.Show($"Success!", "",
                    DialogButtonDefaults.OK,
                    DialogIcon.Information);

                this.Close();
            }
        }

        private void DisplayFormattedOutput(List<LineFormatInfo> lines)
        {
            RichTextBoxHelper.DisplayFormattedLines(scriptOutput, lines);
        }

        public void RunSynchronicityCheck()
        {
            if (Editor != null && ScriptConverter.CheckIsAutoSyncLabels())
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
    }
}
