using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MonoBuilder.Screens.ScreenUtils;
using MonoBuilder.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging.Effects;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Xml;
using static System.Windows.Forms.LinkLabel;

namespace MonoBuilder.Screens
{
	/// <summary>
	/// Interaction logic for ScriptBuilder.xaml
	/// </summary>
	public partial class ScriptBuilder : Window, ISynchronizable
	{
		private readonly ScriptConversion ScriptConverter;
		private readonly AppSettings ApplicationSettings;
		private readonly ActionHelper ActionUtility;
		private readonly FileEditor? Editor;

		private readonly Dictionary<string, ConversionRule> ConversionRules = new();
		private readonly Dictionary<string, StylingTag> StylingTags = new();

		private string? TabType { get; set; }
		private int? TabAmount { get; set; }
		public ObservableBoolean ShouldEnableConvert { get; } = new();
		public ObservableBoolean ShouldEnableReset { get; } = new();

		private int _triggerPosition;
		private string _currentPrefix = "";
		private bool _popupTriggered = false;

		public ScriptBuilder(
			AppSettings settings,
			ScriptConversion converter,
			ActionHelper actionHelper)
		{
			ScriptConverter = converter;
			ApplicationSettings = settings;
			ActionUtility = actionHelper;


			if (settings.GetAllFilePaths("Script").Count > 0)
			{
				Editor = new FileEditor(
					settings.GetAllFilePaths(),
					settings.GetAllFolderPaths(),
					settings.GetIndentationType(),
					settings.GetIndentationAmount());
				Editor.LoadProgram();
			}

			InitializeComponent();
			LoadSettings(settings);
			DataContext = this;

			foreach (ConversionRule rule in converter.ConversionRules)
			{
				ConversionRules.Add(rule.Name, rule);
			}

			StylingTags.Add("bold", new StylingTag("bold"));
			StylingTags.Add("italic", new StylingTag("italic"));
			StylingTags.Add("big", new StylingTag("big"));
			StylingTags.Add("small", new StylingTag("small"));

			ScriptInputContainer.SizeChanged += ScriptInputContainer_SizeChanged;

			using (XmlReader reader = XmlReader.Create("data/Monobuilder.Markdown.xshd"))
			{
				RawScriptInput.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
			}

			BoxesHaveContent();
		}

		private void ScriptInputContainer_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			var clip = ScriptInputContainer.Clip as System.Windows.Media.RectangleGeometry;
			if (clip != null)
			{
				clip.Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
			}
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

		#region Text Manipulation Commands
		private void BoldCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteBoldCommand();
		}

		private void ItalicCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteItalicCommand();
		}

		private void BigCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteBigCommand();
		}

		private void SmallCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteSmallCommand();
		}

		private void ConvertScriptCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteConvertScriptCommand();
		}

		private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteSaveCommand();
		}

		private void LoadCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteLoadCommand();
		}

		private void IndentForwardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteIndentForward();
		}

		private void IndentBackwardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteIndentBackward();
		}

		private void ResetInputLabel_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteResetCommand();
		}
		#endregion

		#region Script Building Commands
		private void CompleteAction(object sender, RoutedEventArgs e)
		{
			var menuItem = (MenuItem)sender;
			if (menuItem == null) return;

			var document = RawScriptInput.Document;
			var action = ActionUtility.GetAction(menuItem.Header.ToString() ?? string.Empty);
			var result = action?.GetInsertionResult();
			var insertionPosition = RawScriptInput.CaretOffset;

			if (action != null && result != null)
			{
				document.Insert(insertionPosition, result.Value.TextToInsert);

				var newCaret = insertionPosition + result.Value.CursorOffset;
				RawScriptInput.CaretOffset = newCaret;

				if (result.Value.SelectionLength > 0)
				{
					RawScriptInput.SelectionStart = newCaret;
					RawScriptInput.SelectionLength = result.Value.SelectionLength;
				}

				ActionUtility.IncrementUsage(action);
			}

		}

		private void ScriptLabel_TextChanged(object sender, TextChangedEventArgs e)
		{
			BoxesHaveContent();
		}

		private void RawScriptInput_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			var key = e.Key == Key.System ? e.SystemKey : e.Key;

			if (!_popupTriggered && key == Key.Tab)
			{
				if (TabType != null && TabAmount != null)
				{
					e.Handled = true;

					ExecuteIndentForward();
				}
			}

			if (key == Key.Back)
			{
				if (RawScriptInput.CaretOffset > 0 &&
					RawScriptInput.CaretOffset < RawScriptInput.Document.TextLength)
				{
					RemoveBraces();
				}

				// If the action helper is open, and the user removes the opening
				// curly brace, just close it.
				if (_popupTriggered && _currentPrefix == string.Empty)
					CloseActionPopup();

				// Working to achieve similar functionality to VS Code.
				// When the user backspaces on empty spaced tabs, it should unindent the line.
				// \t style tabs get ignored because they are already single unit tabs.
				// Currently does not support cross unit untabbing (Your TabType must be set to "Spaces") due to potential issues.
				if (RawScriptInput.SelectionLength > 0 ||
					TabType == "Tab" ||
					TabAmount == null) return;

				RemoveIndentationOnBackspace(e);
			}

			// Ctrl+I is under the control of another command binding, so just stop it and do my own thing instead.
			if (key == Key.I && Keyboard.Modifiers == ModifierKeys.Control)
			{
				ExecuteItalicCommand();
				e.Handled = true;
			}

			if (key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (RawScriptInput.SelectionLength > 0)
					DuplicateSelection(true);
				else
					DuplicateLine(true);

				e.Handled = true;
			}

			if (key == Key.Down)
			{
				// If the popup is open, tell the down key to move down the popup list.
				if (_popupTriggered)
				{
					if (ActionListBox.SelectedIndex < ActionListBox.Items.Count - 1)
						ChangeActionListSelection(LogicalDirection.Forward);

					e.Handled = true;
				}
				else
				{
					var document = RawScriptInput.Document;
					var line = document.GetLineByOffset(RawScriptInput.CaretOffset);
					var lineNumber = line.LineNumber;
					var isShifting = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
					var isAlting = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

					// If the user is holding alt, move the selection or line down.
					if (isAlting && lineNumber < document.LineCount)
					{
						if (RawScriptInput.SelectionLength > 0)
							MoveSelection(LogicalDirection.Forward);
						else
							MoveLine(line, LogicalDirection.Forward);

						e.Handled = true;
						return;
					}

					// If we're at the end of the document, move the caret to the end.
					if (lineNumber == document.LineCount)
					{
						if (!HandleTextBoundary(isShifting, LogicalDirection.Forward, e))
							return;
					}

					// If there is selected text, move the caret into the position that best suits it before moving down.
					if (RawScriptInput.SelectionLength > 0)
						RawScriptInput.CaretOffset = RawScriptInput.SelectionStart + RawScriptInput.SelectionLength;
				}
			}

			if (key == Key.Up)
			{
				// If the popup is open, tell the up key to move up the popup list.
				if (_popupTriggered)
				{
					if (ActionListBox.SelectedIndex >= 0)
						ChangeActionListSelection(LogicalDirection.Backward);

					e.Handled = true;
				}
				else
				{
					var document = RawScriptInput.Document;
					var line = document.GetLineByOffset(RawScriptInput.CaretOffset);
					var lineNumber = line.LineNumber;
					var isShifting = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
					var isAlting = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

					if (isAlting && isShifting)
					{
						if (RawScriptInput.SelectionLength > 0)
							DuplicateSelection(false);
						else
							DuplicateLine(false);

						e.Handled = true;
						return;
					}

					// If the user is holding alt, move the selection or line up.
					if (isAlting && lineNumber > 1)
					{
						if (RawScriptInput.SelectionLength > 0)
							MoveSelection(LogicalDirection.Backward);
						else
							MoveLine(line, LogicalDirection.Backward);

						e.Handled = true;
						return;
					}

					// If we're at the start of the document, move the caret to the first offset point.
					if (lineNumber == 1)
					{
						if (!HandleTextBoundary(isShifting, LogicalDirection.Backward, e))
							return;
					}

					// If these is selected text, move the caret into the position that best suit it before moving up.
					if (RawScriptInput.SelectionLength > 0 && !isShifting)
						RawScriptInput.CaretOffset = RawScriptInput.SelectionStart;
				}
			}

			if (key == Key.OemOpenBrackets && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
			{
				InsertBraces();

				e.Handled = true;
				return;
			}

			// If the user made a selection and pressed the left key without holding shift,
			// move the caret to the beginning of the selection... Take a lesson from this LibreOffice...!!!
			if (key == Key.Left) MoveSelectionCaret(LogicalDirection.Backward);

			// If the user made a selection and pressed the right key without holding shift,
			// move the caret to the end of the selection... AGAIN, TAKE A LESSON FROM THIS LIBREOFFICE...!!!
			if (key == Key.Right) MoveSelectionCaret(LogicalDirection.Forward);

			// If the user moves the left or right arrow while the popup window is open,
			// remove the popup window regardless of context. (Maybe I'll make it context aware in the future)
			if (_popupTriggered && (key == Key.Left || key == Key.Right))
				CloseActionPopup();

			// Toggle comments on highlighted lines or an unhighlighted line.
			if (Keyboard.Modifiers == ModifierKeys.Control && (key == Key.Divide || key == Key.OemQuestion))
			{
				ExecuteToggleCommentsCommand();
				e.Handled = true;
			}

			if (key == Key.OemOpenBrackets && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
			{
				_triggerPosition = RawScriptInput.CaretOffset;
				_popupTriggered = true;
				_currentPrefix = "";

				return;
			}

			if (ActionPopup.IsOpen)
				OpenActionList(key, e);
		}

		private void RawScriptInput_TextChanged(object sender, EventArgs e)
		{
			BoxesHaveContent();

			if (!_popupTriggered) return;

			var document = RawScriptInput.Document;
			int caret = RawScriptInput.CaretOffset;

			if (_triggerPosition < 0 || _triggerPosition > caret || _triggerPosition > document.TextLength)
			{
				CloseActionPopup();
				_currentPrefix = string.Empty;
				return;
			}

			int prefixLength = caret - _triggerPosition;
			_currentPrefix = prefixLength > 0
				? document.GetText(_triggerPosition, prefixLength).TrimStart('{')
				: string.Empty;

			if (_currentPrefix.Contains('\n') || _currentPrefix.Contains('\r') || _currentPrefix.Contains(' '))
			{
				CloseActionPopup();
				_currentPrefix = string.Empty;
				return;
			}

			var filtered = ActionUtility.GetFilteredAndSorted(_currentPrefix);
			ActionListBox.ItemsSource = filtered.ToList();

			if (ActionListBox.Items.IsEmpty)
			{
				ActionPopup.IsOpen = false;
				return;
			}

			if (!ActionPopup.IsOpen)
				PositionAndShowPopup();

			if (ActionListBox.SelectedIndex < 0 && ActionListBox.Items.Count > 0)
				ActionListBox.SelectedIndex = 0;
		}

		private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (!ActionPopup.IsOpen) return;

			if (ActionPopup.Child != null)
			{
				Point mousePos = Mouse.GetPosition(ActionPopup.Child);
				if (new Rect(ActionPopup.Child.RenderSize).Contains(mousePos))
					return;
			}

			CloseActionPopup();
		}

		private void ActionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void ActionListBox_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (ActionListBox.SelectedValue is IAction selected)
			{
				CompleteAction(selected);
			}
		}

		private void ActionListBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Down)
			{
				if (ActionListBox.SelectedIndex < ActionListBox.Items.Count - 1)
					ChangeActionListSelection(LogicalDirection.Forward);

				e.Handled = true;

			}
			else if (e.Key == Key.Up)
			{
				if (ActionListBox.SelectedIndex > 0)
					ChangeActionListSelection(LogicalDirection.Backward);


				e.Handled = true;
			}
		}
		#endregion
	}

	public class StylingTag
	{
		public string Start { get; set; }
		public string End { get; set; }
		public int StartLength { get; set; }
		public int EndLength { get; set; }
		public int WholeLength { get; set; }

		public StylingTag(string tag)
		{
			Start = $"{{{tag}}}";
			End = $"{{/{tag}}}";
			StartLength = Start.Length;
			EndLength = End.Length;
			WholeLength = StartLength + EndLength;
		}
	}
}
