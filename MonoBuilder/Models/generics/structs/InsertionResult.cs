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
		public string? StartValueText { get; }
		public string? EndValueText { get; }

		public InsertionResult(string text, int cursorOffset, int selectionLength = 0, string placeholderText = "", string? startValue = null, string? endValue = null)
		{
			TextToInsert = text;
			CursorOffset = cursorOffset;
			SelectionLength = selectionLength;

			PlaceholderText = placeholderText;
			StartValueText = startValue;
			EndValueText = endValue;
		}
	}
}
