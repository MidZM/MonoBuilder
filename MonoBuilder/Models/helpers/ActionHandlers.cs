using MonoBuilder.Models.generics.structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoBuilder.Models.helpers
{

	public class PairedAction : IAction
	{
		public string Name { get; }
		public int UsageCount { get; set; }

		public PairedAction(string name)
		{
			Name = name;
		}

		public string DisplayName => $"{{{Name}}}";

		public bool MatchesPrefix(string prefix) =>
			Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

		public InsertionResult GetInsertionResult()
		{
			string open = $"{{{Name}}}";
			string close = $"{{/{Name}}}";
			return new InsertionResult(open + close, open.Length);
		}
	}

	public class ValueAction : IAction
	{
		public string Name { get; }
		private readonly string _placeholder;
		public int UsageCount { get; set; }

		public ValueAction(string name, string placeholder)
		{
			Name = name;
			_placeholder = placeholder;
		}

		public string DisplayName => $"{{{Name}:{_placeholder}}}";

		public bool MatchesPrefix(string prefix) =>
			Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

		public InsertionResult GetInsertionResult()
		{
			string text = $"{{{Name}:{_placeholder}}}";
			int cursorOffset = $"{{{Name}:".Length;
			return new InsertionResult(text, cursorOffset, _placeholder.Length, _placeholder);
		}
	}

	public class SelfClosingAction : IAction
	{
		public string Name { get; }
		public int UsageCount { get; set; }

		public SelfClosingAction(string name)
		{
			Name = name;
		}

		public string DisplayName => $"{{{Name}/}}";

		public bool MatchesPrefix(string prefix) =>
			Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

		public InsertionResult GetInsertionResult()
		{
			string text = $"{{{Name}}}";
			return new InsertionResult(text, text.Length);
		}
	}
}
