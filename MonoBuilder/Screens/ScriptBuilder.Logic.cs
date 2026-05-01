using ICSharpCode.AvalonEdit.Document;
using MonoBuilder.Screens.ScreenUtils;
using MonoBuilder.Utils;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace MonoBuilder.Screens
{
	public partial class ScriptBuilder
	{
		private readonly string BtnSelectedHex = "#FF0000";
		private readonly string BtnDeselectedHex = "#DDDDDD";

		public void RunSynchronicityCheck()
		{
			if (Editor != null && ScriptConverter.CheckIsAutoSyncLabels())
			{
				Editor.CheckSynchronicity(false);

				Editor.SaveProgram();
				ApplicationSettings.SetSynchronicityCheck(true);
			}
		}

		private void BoxesHaveContent()
		{
			bool scriptContent = RawScriptInput.Text.Length > 0;
			bool labelContent = ScriptLabel.Text.Length > 0;
			ShouldEnableConvert.Value = scriptContent && labelContent;
			ShouldEnableReset.Value = scriptContent || labelContent;
		}

		private void SetButtonBrush(Button element, string hex, bool isSelected)
		{
			var brush = Helpers.HexToBrush(hex);
			element.Foreground = brush;
			element.BorderBrush = brush;
			element.Tag = isSelected;
		}

		private void ToggleButtonStyle(Button button)
		{
			if (button.Tag is bool isSelected && isSelected)
			{
				SetButtonBrush(button, BtnDeselectedHex, false);
			}
			else
			{
				SetButtonBrush(button, BtnSelectedHex, true);
			}
		}

		private void ToggleComment(TextDocument document, DocumentLine line)
		{
			var lineText = document.GetText(line.Offset, line.Length);
			string trimmedText = lineText.TrimStart();

			int textLength = lineText.Length;
			int trimmedLength = trimmedText.Length;
			int conetentLength = textLength - trimmedLength;
			int caretOffect = line.Offset + conetentLength;

			if (trimmedText.StartsWith("// "))
			{
				document.Remove(caretOffect, 3);
			}
			else
			{
				document.Insert(caretOffect, "// ");
			}
		}

		private void UnsetIndentation(TextDocument document, DocumentLine line, string indentation, string fakeIndentation)
		{
			string lineText = document.GetText(line);

			if (lineText.StartsWith('\t') || lineText.StartsWith(indentation))
			{
				if (TabType == "Spaces" && lineText.StartsWith('\t'))
				{
					document.Remove(line.Offset, 1);
				}
				else if (TabType == "Tab" && lineText.StartsWith(fakeIndentation))
				{
					document.Remove(line.Offset, fakeIndentation.Length);
				}
				else
				{
					document.Remove(line.Offset, indentation.Length);
				}
			}
		}

		private void PositionAndShowPopup()
		{
			Rect caretRect = RawScriptInput.TextArea.Caret.CalculateCaretRectangle();
			int offset = 10;

			ActionPopup.PlacementTarget = RawScriptInput;
			ActionPopup.Placement = PlacementMode.RelativePoint;
			ActionPopup.HorizontalOffset = caretRect.X;
			ActionPopup.VerticalOffset = caretRect.Y + caretRect.Height + offset;
			ActionPopup.IsOpen = true;
		}

		private void CompleteAction(IAction action)
		{
			CloseActionPopup();

			var document = RawScriptInput.Document;
			var result = action.GetInsertionResult();

			int caret = RawScriptInput.CaretOffset;
			int insertPosition = Math.Max(0, Math.Min(_triggerPosition, caret));
			int replaceLength = Math.Max(0, caret - insertPosition);

			if (insertPosition + replaceLength > document.TextLength)
			{
				replaceLength = document.TextLength - insertPosition;
			}

			using (document.RunUpdate())
			{
				document.Remove(insertPosition, replaceLength);
				document.Insert(insertPosition, result.TextToInsert);
			}

			// Move caret + optional selection for placeholder
			var newCaret = Math.Max(0, Math.Min(insertPosition + result.CursorOffset, document.TextLength));
			RawScriptInput.CaretOffset = newCaret;

			if (result.SelectionLength > 0)
			{
				RawScriptInput.SelectionStart = newCaret;
				RawScriptInput.SelectionLength = result.SelectionLength;
			}

			_currentPrefix = string.Empty;

			ActionUtility.IncrementUsage(action);
		}

		#region Text Manipulation Commands
		private void ExecuteBoldCommand()
		{
			var document = RawScriptInput.Document;
			int selectionStart = RawScriptInput.SelectionStart;
			int selectionLength = RawScriptInput.SelectionLength;

			if (selectionLength == 0) return;

			StylingTag tag = StylingTags["bold"];
			int startLength = tag.StartLength;
			int endLength = tag.EndLength;

			bool hasTagBefore = selectionStart >= startLength &&
								document.GetText(selectionStart - startLength, startLength) == tag.Start;
			bool hasTagAfter = (selectionStart + selectionLength <= document.TextLength - endLength) &&
								document.GetText(selectionStart + selectionLength, endLength) == tag.End;

			using (document.RunUpdate())
			{
				if (hasTagBefore && hasTagAfter)
				{
					document.Remove(selectionStart + selectionLength, endLength);
					document.Remove(selectionStart - startLength, startLength);

					RawScriptInput.SelectionStart = selectionStart - startLength;
				}
				else
				{
					document.Insert(selectionStart + selectionLength, tag.End);
					document.Insert(selectionStart, tag.Start);

					RawScriptInput.SelectionStart = selectionStart + startLength;
					RawScriptInput.SelectionLength = selectionLength;
				}
			}
		}

		private void ExecuteItalicCommand()
		{
			var document = RawScriptInput.Document;
			var selectionStart = RawScriptInput.SelectionStart;
			var selectionLength = RawScriptInput.SelectionLength;

			if (selectionLength == 0) return;

			StylingTag tag = StylingTags["italic"];
			int startLength = tag.StartLength;
			int endLength = tag.EndLength;

			bool hasTagBefore = selectionStart >= startLength &&
								document.GetText(selectionStart - startLength, startLength) == tag.Start;
			bool hasTagAfter = (selectionStart + selectionLength <= document.TextLength - endLength) &&
								document.GetText(selectionStart + selectionLength, endLength) == tag.End;

			using (document.RunUpdate())
			{
				if (hasTagBefore && hasTagAfter)
				{
					document.Remove(selectionStart + selectionLength, endLength);
					document.Remove(selectionStart - startLength, startLength);

					RawScriptInput.SelectionStart = selectionStart - startLength;
				}
				else
				{
					document.Insert(selectionStart + selectionLength, tag.End);
					document.Insert(selectionStart, tag.Start);

					RawScriptInput.SelectionStart = selectionStart + startLength;
					RawScriptInput.SelectionLength = selectionLength;
				}
			}
		}

		private void ExecuteIndentForward()
		{
			if (TabType != null && TabAmount != null)
			{
				var document = RawScriptInput.Document;
				using (document.RunUpdate())
				{
					string indentation = TabType == "Spaces" ? new string(' ', TabAmount.Value) : "\t";

					var selectionSegments = RawScriptInput.TextArea.Selection.Segments;

					// For when the user has made a selection of 1 or more line.
					if (selectionSegments.Count() > 0)
					{
						foreach (var segment in selectionSegments)
						{
							int startLine = document.GetLineByOffset(segment.StartOffset).LineNumber;
							int endLine = document.GetLineByOffset(segment.EndOffset).LineNumber;

							for (int i = startLine; i <= endLine; i++)
							{
								var line = document.GetLineByNumber(i);
								document.Insert(line.Offset, indentation);
							}
						}
					}
					// For when the user has made no selection, but still wants to indent the full line.
					else
					{
						var line = document.GetLineByOffset(RawScriptInput.CaretOffset);
						document.Insert(line.Offset, indentation);
					}
				}
			}
		}

		private void ExecuteIndentBackward()
		{
			if (TabType != null && TabAmount != null)
			{
				var document = RawScriptInput.Document;
				using (document.RunUpdate())
				{
					string fakeIndent = new string(' ', TabAmount.Value);
					string indentation = TabType == "Spaces" ? fakeIndent : "\t";

					var selectionSegments = RawScriptInput.TextArea.Selection.Segments;

					// For when the user has made a selection of 1 or more line.
					if (selectionSegments.Count() > 0)
					{
						foreach (var segment in selectionSegments)
						{
							int startLine = document.GetLineByOffset(segment.StartOffset).LineNumber;
							int endLine = document.GetLineByOffset(segment.EndOffset).LineNumber;

							for (int i = startLine; i <= endLine; i++)
							{
								var line = document.GetLineByNumber(i);
								UnsetIndentation(document, line, indentation, fakeIndent);
							}
						}
					}
					// For when the user has made no selection, but still wants to indent the full line.
					else
					{
						var line = document.GetLineByOffset(RawScriptInput.CaretOffset);
						UnsetIndentation(document, line, indentation, fakeIndent);
					}
				}
			}
		}

		private void ExecuteBigCommand()
		{
			var document = RawScriptInput.Document;
			var selectionStart = RawScriptInput.SelectionStart;
			var selectionLength = RawScriptInput.SelectionLength;

			if (selectionLength == 0) return;

			StylingTag tag = StylingTags["big"];
			int startLength = tag.StartLength;
			int endLength = tag.EndLength;

			bool hasTagBefore = selectionStart >= startLength &&
								document.GetText(selectionStart - startLength, startLength) == tag.Start;
			bool hasTagAfter = (selectionStart + selectionLength <= document.TextLength - endLength) &&
								document.GetText(selectionStart + selectionLength, endLength) == tag.End;

			using (document.RunUpdate())
			{
				if (hasTagBefore && hasTagAfter)
				{
					document.Remove(selectionStart + selectionLength, endLength);
					document.Remove(selectionStart - startLength, startLength);

					RawScriptInput.SelectionStart = selectionStart - startLength;
				}
				else
				{
					document.Insert(selectionStart + selectionLength, tag.End);
					document.Insert(selectionStart, tag.Start);

					RawScriptInput.SelectionStart = selectionStart + startLength;
					RawScriptInput.SelectionLength = selectionLength;
				}
			}
		}

		private void ExecuteSmallCommand()
		{
			var document = RawScriptInput.Document;
			var selectionStart = RawScriptInput.SelectionStart;
			var selectionLength = RawScriptInput.SelectionLength;

			if (selectionLength == 0) return;

			StylingTag tag = StylingTags["small"];
			int startLength = tag.StartLength;
			int endLength = tag.EndLength;

			bool hasTagBefore = selectionStart >= startLength &&
								document.GetText(selectionStart - startLength, startLength) == tag.Start;
			bool hasTagAfter = (selectionStart + selectionLength <= document.TextLength - endLength) &&
								document.GetText(selectionStart + selectionLength, endLength) == tag.End;

			using (document.RunUpdate())
			{
				if (hasTagBefore && hasTagAfter)
				{
					document.Remove(selectionStart + selectionLength, endLength);
					document.Remove(selectionStart - startLength, startLength);

					RawScriptInput.SelectionStart = selectionStart - startLength;
				}
				else
				{
					document.Insert(selectionStart + selectionLength, tag.End);
					document.Insert(selectionStart, tag.Start);

					RawScriptInput.SelectionStart = selectionStart + startLength;
					RawScriptInput.SelectionLength = selectionLength;
				}
			}
		}

		private void ExecuteConvertScriptCommand()
		{
			if (ScriptLabel.Text.Length <= 0)
			{
				DialogBox.Show(
					"Converted engine labels must contain a label name...",
					"Missing Label", DialogButtonDefaults.OK, DialogIcon.Warning);
				return;
			}
			if (RawScriptInput.Document.Text.Length <= 0)
			{
				DialogBox.Show(
					"Converted engine labels must contain script content...",
					"Missing Contents", DialogButtonDefaults.OK, DialogIcon.Warning);
				return;
			}

			string label = ScriptLabel.Text;

			ScriptBuilderOutput outputScreen = new ScriptBuilderOutput(
				ScriptConverter,
				Editor,
				ApplicationSettings,
				label, RawScriptInput.Document);

			outputScreen.Loaded += (s, e) =>
			{
				FileWatcher.SetCurrentContext(outputScreen);
			};

			outputScreen.Closing += (s, e) =>
			{
				IsEnabled = true;
				FileWatcher.SetCurrentContext(this);
			};

			outputScreen.Owner = this;
			IsEnabled = false;
			outputScreen.ShowDialog();
		}

		private void ExecuteSaveCommand()
		{
			if (Editor != null)
			{
				if (ScriptLabel.Text.Length <= 0)
				{
					DialogBox.Show(
						"Saved engine labels must contain a label name...",
						"Missing Label", DialogButtonDefaults.OK, DialogIcon.Warning);
					return;
				}
				if (RawScriptInput.Document.Text.Length == 0)
				{
					DialogBox.Show(
						"Saved engine labels must contain script content...",
						"Missing Contents", DialogButtonDefaults.OK, DialogIcon.Warning);
					return;
				}

				string label = ScriptLabel.Text;
				TextDocument document = RawScriptInput.Document;
				var convertedScript = ScriptConverter.Convert(document);
				// Make a choice box to ask which file the users wants to apply the saved label to.
				var scriptFile = SelectionBox.Show(
					"Select a script file to associate this label with.",
					"Select a Script File",
					DialogIcon.Information,
					ApplicationSettings.GetAllFilePaths("Script").Keys.ToList());

				if (scriptFile != null)
				{
					Editor.SaveToProgram(label, convertedScript, scriptFile.SelectionText);
				}
			}
		}

		private void ExecuteLoadCommand()
		{
			var LoadScriptsScreen = new LoadScripts(ApplicationSettings, Editor, ScriptConverter, RawScriptInput, ScriptLabel);
			LoadScriptsScreen.Owner = this;
			LoadScriptsScreen.Loaded += (s, e) =>
			{
				FileWatcher.SetCurrentContext(LoadScriptsScreen);
			};

			LoadScriptsScreen.Closing += (s, e) =>
			{
				this.IsEnabled = true;
				FileWatcher.SetCurrentContext(this);
			};

			this.IsEnabled = false;
			LoadScriptsScreen.Show();
		}

		private void ExecuteResetCommand()
		{
			if (DialogBox.Show(
				"Are you sure you want to clear all content from the input and label box?",
				"Reset Script Data",
				DialogButtonDefaults.YesNo,
				DialogIcon.Warning) == DialogBoxResult.Yes)
			{
				RawScriptInput.Document.Text = string.Empty;
				ScriptLabel.Clear();
			}
		}

		private void ExecuteToggleCommentsCommand()
		{
			var document = RawScriptInput.Document;
			using (document.RunUpdate())
			{
				var segments = RawScriptInput.TextArea.Selection.Segments;

				if (segments.Count() > 0)
				{
					foreach (var segment in segments)
					{
						int startLine = document.GetLineByOffset(segment.StartOffset).LineNumber;
						int endLine = document.GetLineByOffset(segment.EndOffset).LineNumber;

						for (int i = startLine; i <= endLine; i++)
						{
							var line = document.GetLineByNumber(i);
							ToggleComment(document, line);
						}
					}
				}
				else
				{
					var line = document.GetLineByOffset(RawScriptInput.CaretOffset);
					ToggleComment(document, line);
				}
			}
		}
		#endregion

		#region Script Input Key Commands
		private void DuplicateLine(bool moveCaret)
		{
			var document = RawScriptInput.Document;
			var line = document.GetLineByOffset(RawScriptInput.CaretOffset);
			var lineText = document.GetText(line);
			var lineNumber = line.LineNumber;

			int column = moveCaret ? RawScriptInput.CaretOffset - line.Offset : RawScriptInput.CaretOffset;

			using (document.RunUpdate())
			{
				document.Insert(line.EndOffset, Environment.NewLine);

				var nextLine = document.GetLineByNumber(lineNumber + 1);
				document.Insert(nextLine.Offset, lineText);

				if (moveCaret)
				{
					RawScriptInput.CaretOffset = nextLine.Offset + column;
				}
				else
				{
					RawScriptInput.CaretOffset = column;
				}
			}
		}

		private void DuplicateSelection(bool moveSelection)
		{
			var textArea = RawScriptInput.TextArea;
			var document = textArea.Document;
			var segments = textArea.Selection.Segments;

			if (!segments.Any()) return;

			var firstLine = document.GetLineByOffset(segments.First().StartOffset);
			var lastLine = document.GetLineByOffset(segments.Last().EndOffset);

			int firstLineNumber = firstLine.LineNumber;
			int lastLineNumber = lastLine.LineNumber;
			int selectionStart = RawScriptInput.SelectionStart;
			int selectionLength = RawScriptInput.SelectionLength;

			int selectionStartColumn = selectionStart - firstLine.Offset;

			var selectedLinesText = new List<string>();
			for (int i = firstLineNumber; i <= lastLineNumber; i++)
				selectedLinesText.Add(document.GetText(document.GetLineByNumber(i)));

			string delimiter = lastLine.DelimiterLength > 0
				? document.GetText(lastLine.EndOffset, lastLine.DelimiterLength)
				: Environment.NewLine;

			string textToInsert = delimiter + string.Join(delimiter, selectedLinesText);
			int insertionOffset = lastLine.EndOffset;

			using (document.RunUpdate())
			{
				document.Insert(insertionOffset, textToInsert);

				if (moveSelection)
				{
					int newSelectionStart = insertionOffset + delimiter.Length + selectionStartColumn;

					textArea.Selection = ICSharpCode.AvalonEdit.Editing.Selection.Create(
						textArea,
						newSelectionStart,
						newSelectionStart + selectionLength
					);

					textArea.Caret.Offset = newSelectionStart + selectionLength;
				}
			}
		}

		private void MoveLine(DocumentLine line, LogicalDirection direction)
		{
			var document = RawScriptInput.Document;
			var lineNumber = line.LineNumber;
			bool iSMovingDown = direction == LogicalDirection.Forward;

			int column = RawScriptInput.CaretOffset - line.Offset;

			var secondLine = iSMovingDown ? line.NextLine : line.PreviousLine;
			var lineText = RawScriptInput.Document.GetText(line);
			var secondLineText = RawScriptInput.Document.GetText(secondLine);

			using (document.RunUpdate())
			{
				document.Remove(line);
				document.Remove(secondLine);

				document.Insert(
					iSMovingDown ? line.Offset : secondLine.Offset,
					iSMovingDown ? secondLineText : lineText);

				document.Insert(
					iSMovingDown ? secondLine.Offset : line.Offset,
					iSMovingDown ? lineText : secondLineText);
			}

			var movedLine = document.GetLineByNumber(lineNumber + (iSMovingDown ? 1 : -1));

			RawScriptInput.CaretOffset = movedLine.Offset + column;
		}

		private void MoveSelection(LogicalDirection direction)
		{
			var document = RawScriptInput.Document;
			bool isMovingDown = direction == LogicalDirection.Forward;

			var segments = RawScriptInput.TextArea.Selection.Segments;
			if (!segments.Any()) return;

			int firstLineNumber = document.GetLineByOffset(segments.First().StartOffset).LineNumber;
			int lastLineNumber = document.GetLineByOffset(segments.Last().EndOffset).LineNumber;

			if (isMovingDown && lastLineNumber >= document.LineCount) return;
			if (!isMovingDown && firstLineNumber <= 1) return;

			var firstLine = document.GetLineByNumber(firstLineNumber);
			var lastLine = document.GetLineByNumber(lastLineNumber);

			// Capture selection state for restoration after the move.
			int selStartColumn = RawScriptInput.SelectionStart - firstLine.Offset;
			int caretLineNumber = document.GetLineByOffset(RawScriptInput.CaretOffset).LineNumber;
			int caretColumn = RawScriptInput.CaretOffset - document.GetLineByOffset(RawScriptInput.CaretOffset).Offset;
			int selLength = RawScriptInput.SelectionLength;
			bool caretIsAtSelStart = RawScriptInput.CaretOffset == RawScriptInput.SelectionStart;

			// Collect the text of each selected line (without delimiters).
			var selectedTexts = new List<string>();
			for (int i = firstLineNumber; i <= lastLineNumber; i++)
				selectedTexts.Add(document.GetText(document.GetLineByNumber(i)));

			using (document.RunUpdate())
			{
				if (isMovingDown)
				{
					var obstacle = lastLine.NextLine;
					string obstacleText = document.GetText(obstacle.Offset, obstacle.Length);
					string delimiter = document.GetText(lastLine.EndOffset, lastLine.DelimiterLength);

					// Replace [firstLine .. obstacle end-of-content] with obstacle on top.
					int blockStart = firstLine.Offset;
					int blockLength = obstacle.EndOffset - firstLine.Offset;
					string newBlock = obstacleText + delimiter + string.Join(delimiter, selectedTexts);
					document.Replace(blockStart, blockLength, newBlock);
				}
				else
				{
					var obstacle = firstLine.PreviousLine;
					string obstacleText = document.GetText(obstacle.Offset, obstacle.Length);
					string delimiter = document.GetText(obstacle.EndOffset, obstacle.DelimiterLength);

					// Replace [obstacle start .. lastLine end-of-content] with obstacle on the bottom.
					int blockStart = obstacle.Offset;
					int blockLength = lastLine.EndOffset - obstacle.Offset;
					string newBlock = string.Join(delimiter, selectedTexts) + delimiter + obstacleText;
					document.Replace(blockStart, blockLength, newBlock);
				}
			}

			// Restore the selection, shifted by one line in the direction of movement.
			int lineShift = isMovingDown ? 1 : -1;
			var newFirstLine = document.GetLineByNumber(firstLineNumber + lineShift);
			var newCaretLine = document.GetLineByNumber(caretLineNumber + lineShift);

			int newSelStart = newFirstLine.Offset + selStartColumn;
			int newCaretOffset = newCaretLine.Offset + caretColumn;

			RawScriptInput.SelectionStart = newSelStart;
			RawScriptInput.SelectionLength = selLength;
			RawScriptInput.CaretOffset = caretIsAtSelStart ? newSelStart : newSelStart + selLength;
		}

		private void MoveSelectionCaret(LogicalDirection direction)
		{
			var isMovingRight = direction == LogicalDirection.Backward;

			if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
			{
				if (RawScriptInput.SelectionLength > 0)
				{
					if (isMovingRight)
						RawScriptInput.CaretOffset = RawScriptInput.SelectionStart + 1;
					else
						RawScriptInput.CaretOffset = RawScriptInput.SelectionStart + RawScriptInput.SelectionLength - 1;
				}
			}
		}

		private void CloseActionPopup()
		{
			ActionPopup.IsOpen = false;
			_popupTriggered = false;
		}

		private void InsertBraces()
		{
			var document = RawScriptInput.Document;
			var selectionStart = RawScriptInput.SelectionStart;
			var selectionLength = RawScriptInput.SelectionLength;

			if (selectionLength > 0)
			{
				if (RawScriptInput.TextArea.Selection.IsMultiline)
				{
					document.Insert(selectionStart, "[");
					return;
				}

				using (document.RunUpdate())
				{
					document.Insert(selectionStart + selectionLength, "]");
					document.Insert(selectionStart, "[");
				}

				RawScriptInput.SelectionLength = selectionLength;
				RawScriptInput.SelectionStart = selectionStart + 1;
			}
			else
			{
				int offset = RawScriptInput.CaretOffset;
				int lineEnd = document.GetLineByOffset(offset).EndOffset;

				if (offset == lineEnd || document.GetCharAt(offset) == ' ')
				{
					document.Insert(RawScriptInput.CaretOffset, "[]");
					RawScriptInput.CaretOffset -= 1;
				}
				else
				{
					document.Insert(RawScriptInput.CaretOffset, "[");
				}
			}
		}

		private void RemoveBraces()
		{
			var document = RawScriptInput.Document;
			char previousChar = document.GetCharAt(RawScriptInput.CaretOffset - 1);
			char nextChar = document.GetCharAt(RawScriptInput.CaretOffset);

			if (previousChar == '[' && nextChar == ']')
			{
				document.Remove(RawScriptInput.CaretOffset, 1);
			}
		}

		private void RemoveIndentationOnBackspace(KeyEventArgs e)
		{
			var document = RawScriptInput.Document;
			int offset = RawScriptInput.CaretOffset;
			int tabAmount = (int)TabAmount!;

			var line = document.GetLineByOffset(offset);
			var lineOffset = line.Offset;
			string prefix = document.GetText(lineOffset, offset - lineOffset);

			if (prefix.Length > 0 && string.IsNullOrWhiteSpace(prefix) && prefix.Length % tabAmount == 0)
			{
				document.Remove(offset - tabAmount, tabAmount);
				e.Handled = true;
			}
		}

		private bool HandleTextBoundary(bool isShifting, LogicalDirection direction, KeyEventArgs e)
		{
			var document = RawScriptInput.Document;
			var isMovingDown = direction == LogicalDirection.Forward;

			if (isShifting)
			{
				if (isMovingDown)
				{
					int selectionStart = RawScriptInput.SelectionStart;
					RawScriptInput.SelectionLength = document.TextLength - selectionStart;

					e.Handled = true;
				}
				else
				{
					int selectionStart = RawScriptInput.SelectionStart;
					int selectionLength = RawScriptInput.SelectionLength;

					RawScriptInput.SelectionStart = 0;
					RawScriptInput.SelectionLength = selectionLength + selectionStart;
					RawScriptInput.CaretOffset = 0;

					e.Handled = true;
				}

				return false;
			}

			RawScriptInput.CaretOffset = isMovingDown ? document.TextLength : 0;
			return true;
		}

		private void ChangeActionListSelection(LogicalDirection direction)
		{
			var isMovingDown = direction == LogicalDirection.Forward;

			if (isMovingDown)
				ActionListBox.SelectedIndex++;
			else
				ActionListBox.SelectedIndex--;

			ActionListBox.ScrollIntoView(ActionListBox.SelectedItem);
		}

		private void OpenActionList(Key key, KeyEventArgs e)
		{
			if (key == Key.Escape)
			{
				CloseActionPopup();
				e.Handled = true;
			}
			else if (key == Key.Tab)
			{
				if (ActionListBox.SelectedIndex > 0 && ActionListBox.SelectedItem is IAction selected)
				{
					CompleteAction(selected);
				}

				if (ActionListBox.SelectedIndex <= 0)
				{
					CompleteAction((IAction)ActionListBox.Items[0]);
				}

				e.Handled = true;
			}
		}
		#endregion
	}
}
