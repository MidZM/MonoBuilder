using System;
using System.Collections.Generic;
using System.Text;

namespace MonoBuilder.Models.generics.structs
{
	public struct InsertionResult
	{
		public string TextToInsert { get; }
		public int CursorOffset { get; }
		public int SelectionLength { get; }
		public string PlaceholderText { get; }

		public InsertionResult(string text, int cursorOffset, int selectionLength = 0, string placeholderText = "")
		{
			TextToInsert = text;
			CursorOffset = cursorOffset;
			SelectionLength = selectionLength;
			PlaceholderText = placeholderText;
		}
	}
}
