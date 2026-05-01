using ICSharpCode.AvalonEdit.Document;
using MonoBuilder.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
    /// Interaction logic for ScriptBuilderOutput.xaml
    /// </summary>
    public partial class ScriptBuilderOutput : Window
    {
        private AppSettings ApplicationSettings { get; set; }
        private ScriptConversion ScriptConverter { get; set; }
        private FileEditor? Editor { get; set; }

        private string Label { get; set; }
        private TextDocument Script { get; set; }

        public ScriptBuilderOutput(ScriptConversion converter, FileEditor? editor, AppSettings settings, string label, TextDocument script)
        {
            ScriptConverter = converter;
            ApplicationSettings = settings;
            Editor = editor;
            Label = label;
            Script = script;

            InitializeComponent();
            DataContext = this;

            if (string.IsNullOrEmpty(label) || script == null || script.Text.Length == 0)
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
                string convertedLines = converter.Convert(label, script);
                DisplayUnformattedOutput(convertedLines);
            }

            if (editor != null)
            {
                BtnSaveScript.IsEnabled = true;
            }
        }

        private void BtnClipboard_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(ScriptOutput.Document.ContentStart, ScriptOutput.Document.ContentEnd);
            Clipboard.SetText(textRange.Text.Trim());
        }

        private void BtnSaveScript_Click(object sender, RoutedEventArgs e)
        {
            if (Editor?.ScriptLabelExists(Label) == true)
            {
                if (DialogBox.Show(
                        $"A label with this name already exists.\nAre you sure you would like to overwrite it?\n\nLabel: \"{Label}\"",
                        "Label Exists",
                        DialogButtonDefaults.YesNo,
                        DialogIcon.Warning) == DialogBoxResult.Yes)
                {
                    TextRange textRange = new TextRange(ScriptOutput.Document.ContentStart, ScriptOutput.Document.ContentEnd);
                    var formatted = Editor?.FormatToScript(textRange.Text);
                    if (formatted != null)
                    {
                        Editor?.MergeWithScript(Label, formatted);
                    }
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
                TextRange textRange = new TextRange(ScriptOutput.Document.ContentStart, ScriptOutput.Document.ContentEnd);
                var formatted = Editor?.FormatToScript(textRange.Text);
                if (formatted != null)
                {
                    Editor?.AddToScript(Label, formatted);
                }
            }

            DialogBox.Show($"Success!", "",
                DialogButtonDefaults.OK,
                DialogIcon.Information);

            Close();
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DisplayFormattedOutput(List<LineFormatInfo> lines)
        {
            ScriptOutput.Document.Blocks.Clear();
            foreach (var line in lines)
            {
                Paragraph paragraph = new Paragraph();
                Run run = new Run(line.Text);

                if (line.TextColor.HasValue)
                {
                    run.Foreground = new SolidColorBrush(line.TextColor.Value);
                }
                paragraph.Inlines.Add(run);
                ScriptOutput.Document.Blocks.Add(paragraph);
            }
        }

        private void DisplayUnformattedOutput(string lines)
        {
            ScriptOutput.Document.Blocks.Clear();
            foreach (var line in lines.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                Paragraph paragraph = new Paragraph();
                Run run = new Run(line);
                paragraph.Inlines.Add(run);
                ScriptOutput.Document.Blocks.Add(paragraph);
            }
        }
    }
}
