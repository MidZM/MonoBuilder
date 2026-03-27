using Krypton.Toolkit;
using MonoBuilder.Screens.ScreenUtils;
using MonoBuilder.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace MonoBuilder.Screens
{
    public partial class ScriptBuilder : KryptonForm
    {
        private ScriptConversion ScriptConverter { get; set; }
        private AppSettings ApplicationSettings { get; set; }
        private FileEditor? Files { get; set; }

        private string? TabType { get; set; }
        private int? TabAmount { get; set; }

        public ScriptBuilder(Characters characters, AppSettings settings, ScriptConversion converter)
        {
            ScriptConverter = converter;
            ApplicationSettings = settings;

            if (settings.GetAllFilePaths().Count > 0 && settings.GetFilePath("Script") != null)
            {
                Files = new FileEditor(
                    settings.GetAllFilePaths(),
                    settings.GetAllFolderPaths(),
                    settings.GetIndentationType(),
                    settings.GetIndentationAmount()
                );
                Files.LoadProgram();
            }

            InitializeComponent();
            LoadSettings(settings);
        }

        private void LoadSettings(AppSettings settings)
        {
            ScriptConverter.SetIsFormattingColor(settings.GetColorFormatting());
            ScriptConverter.SetAutoSyncLabels(settings.GetAutoSyncLabels());
            ScriptConverter.ChangeIndentationType(settings.GetIndentationType());
            ScriptConverter.ChangeIndentationAmount(settings.GetIndentationAmount());

            TabType = settings.GetIndentationType();
            TabAmount = settings.GetIndentationAmount();
        }

        private void ScriptBuilder_Shown(object sender, EventArgs e)
        {
            if (!ApplicationSettings.HasBeenSynchronized() && Files != null && ScriptConverter.CheckIsAutoSyncLabels())
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
                        backColor: Color.FromArgb(20,20,20));

                    var panel = transitioner.Show();

                    this.Controls.Add(panel);
                    panel.BringToFront();

                    this.Enabled = false;
                    Files.CheckSynchronicity();

                    Files.SaveProgram();
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

        private void btnReset_Click(object sender, EventArgs e)
        {
            labelInput.Clear();
            scriptInput.Clear();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            string label = labelInput.Text;
            string script = scriptInput.Text;

            using (ScriptBuilderOutput ScriptOutputScreen = new ScriptBuilderOutput(
                ScriptConverter,
                Files,
                ApplicationSettings,
                label, script
                ))
            {
                var marginWidth = this.Width / 2 - 100;
                var marginHeight = this.Height / 2 - 100;
                var transitioner = new ScreenTransitioner(
                    location: new Point(marginWidth, marginHeight),
                    width: 100,
                    height: 100,
                    font: new System.Drawing.Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold),
                    foreColor: Color.White,
                    backColor: Color.FromArgb(20, 20, 20));

                var panel = transitioner.Show();
                this.Controls.Add(panel);
                panel.BringToFront();

                ScriptOutputScreen.Load += (s, e) =>
                {
                    this.Controls.Remove(panel);
                    transitioner.Hide();
                    FileWatcher.SetCurrentContext(ScriptOutputScreen);
                };

                ScriptOutputScreen.FormClosed += (s, e) =>
                {
                    this.Enabled = true;
                    FileWatcher.SetCurrentContext(this);
                };

                this.Enabled = false;
                ScriptOutputScreen.ShowDialog();
            }
        }

        private void scriptInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                string tabType = TabType ?? "Spaces";

                if (tabType == "Tab")
                {
                    scriptInput.SelectedText = "\t";
                }
                else if (tabType == "Spaces")
                {
                    int tabAmount = TabAmount ?? 4;
                    scriptInput.SelectedText = new string(' ', tabAmount);
                }
            }

            if (e.KeyCode == Keys.Back && scriptInput.SelectionLength == 0)
            {
                int carrotIndex = scriptInput.SelectionStart;
                if (carrotIndex == 0) return;

                int lineIndex = scriptInput.GetLineFromCharIndex(carrotIndex);
                int lineStart = scriptInput.GetFirstCharIndexFromLine(lineIndex);

                string prefix = scriptInput.Text.Substring(lineStart, carrotIndex - lineStart);

                int tabAmount = TabAmount ?? 4;
                if (prefix.Length > 0 && prefix.Trim().Length == 0 && prefix.Length % tabAmount == 0)
                {
                    int charsToRemove = Math.Min(prefix.Length, tabAmount);

                    scriptInput.SelectionStart = carrotIndex - charsToRemove;
                    scriptInput.SelectionLength = charsToRemove;
                    scriptInput.SelectedText = "";

                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }

            if (e.Control && (e.KeyCode == Keys.Divide || e.KeyCode == Keys.OemQuestion))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                int selStart = scriptInput.SelectionStart;
                int selLen = scriptInput.SelectionLength;

                int startLine = scriptInput.GetLineFromCharIndex(selStart);
                int endLine = scriptInput.GetLineFromCharIndex(selStart + selLen);

                int addedChars = 0;

                for (int i = startLine; i <= endLine; i++)
                {
                    int lineStartCharIndex = scriptInput.GetFirstCharIndexFromLine(i);

                    scriptInput.SelectionStart = lineStartCharIndex;
                    scriptInput.SelectionLength = 0;

                    bool lineIsAlreadyCommented =
                        scriptInput.Text.Length > lineStartCharIndex + 1 &&
                        scriptInput.Text.Substring(lineStartCharIndex, 2) == "//";

                    if (lineIsAlreadyCommented)
                    {
                        scriptInput.SelectionLength = 2;
                        scriptInput.SelectedText = "";
                        addedChars -= 2;
                    }
                    else
                    {
                        scriptInput.SelectedText = "//";
                        addedChars += 2;
                    }
                }

                scriptInput.SelectionStart = addedChars < 0 ? selStart - 2 : selStart + 2;
                scriptInput.SelectionLength = addedChars < 0 ? selLen + (addedChars + 2) : selLen + (addedChars - 2);
            }

            if (e.Control && e.KeyCode == Keys.V)
            {
                e.Handled = true;

                string plainText = Clipboard.GetText(TextDataFormat.Text);
                scriptInput.SelectedText = plainText;
            }
        }

        private void btnLoadCopy_Click(object sender, EventArgs e)
        {
            if (ApplicationSettings != null)
            {
                using (LoadScripts LoadScriptScreen = new LoadScripts(
                    ApplicationSettings,
                    Files,
                    ScriptConverter,
                    scriptInput,
                    labelInput
                    ))
                {
                    var marginWidth = this.Width / 2 - 100;
                    var marginHeight = this.Height / 2 - 100;
                    var transitioner = new ScreenTransitioner(
                        location: new Point(marginWidth, marginHeight),
                        width: 100,
                        height: 100,
                        font: new System.Drawing.Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold),
                        foreColor: Color.White,
                        backColor: Color.FromArgb(20, 20, 20));

                    var panel = transitioner.Show();
                    this.Controls.Add(panel);
                    panel.BringToFront();

                    LoadScriptScreen.Load += (s, e) =>
                    {
                        this.Controls.Remove(panel);
                        transitioner.Hide();
                        FileWatcher.SetCurrentContext(LoadScriptScreen);
                    };

                    LoadScriptScreen.FormClosed += (s, e) =>
                    {
                        this.Enabled = true;
                        FileWatcher.SetCurrentContext(this);
                    };

                    this.Enabled = false;
                    LoadScriptScreen.ShowDialog();
                }
            }
        }

        private void btnSaveCopy_Click(object sender, EventArgs e)
        {
            if (Files != null)
            {
                string label = labelInput.Text;
                string script = scriptInput.Text;
                if (label.Length > 0 && script.Length > 0)
                {
                    var convertedScript = ScriptConverter.Convert(script);
                    Files.SaveToProgram(label, convertedScript);
                }
            }
            else
            {
                DialogBox.Show(
                    "Unable to save labels without a \"Script\" file selected.",
                    "No Script Data",
                    DialogButtonDefaults.OK,
                    DialogIcon.Error);
            }
        }

        private void ShouldEnableConversion(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(scriptInput.Text) || !string.IsNullOrEmpty(labelInput.Text))
            {
                btnReset.Enabled = true;
            }
            else
            {
                btnReset.Enabled = false;
            }

            if (!string.IsNullOrEmpty(scriptInput.Text) && !string.IsNullOrEmpty(labelInput.Text))
            {
                btnConvert.Enabled = true;
                btnSaveCopy.Enabled = true;
            }
            else
            {
                btnConvert.Enabled = false;
                btnSaveCopy.Enabled = false;
            }
        }

        private void BtnEnabledChanged(object sender, EventArgs e)
        {
            Button? button = sender as Button;
            if (button != null)
            {
                button.BackColor = button.Enabled ? SystemColors.Window : Color.LightGray;
                button.ForeColor = button.Enabled ? SystemColors.ControlText : Color.Gray;
            }
        }

        public void RunSynchronicityCheck()
        {
            if (Files != null && ScriptConverter.CheckIsAutoSyncLabels())
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
                    Files.CheckSynchronicity(false);

                    Files.SaveProgram();
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
