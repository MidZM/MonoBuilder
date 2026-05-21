using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.generics.structs;
using MonoBuilder.Models.generics.enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MonoBuilder.Models.helpers
{
	public class ScriptEvent : IEvent
	{
		public string Name { get; } = string.Empty;
		public string DisplayName { get; } = string.Empty;
		public string TriggerText { get; } = string.Empty;
		public required MonoSystem System { get; set; }
		public string? SystemModeName { get; set; }
		public required IEnumerable<INamedEntity> SystemCollection { get; set; }
		public List<EventOption>? Options { get; set; }
		public List<EventPosition>? Positions { get; set; }

		public ScriptEvent(string name, string triggerText)
		{
			Name = name;
			TriggerText = triggerText;
		}

		public bool MatchesPrefix(string prefix) =>
			TriggerText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

		public InsertionResult GetInsertionResult()
		{
			string text = $"{TriggerText} ";
			int cursorOffset = text.Length;
			return new InsertionResult(text, cursorOffset);
		}
	}

	public class EventOption : IEventOption
	{
		public string Name { get; } = string.Empty;
		public bool HasTrailingOptions { get; } = false;
		public List<EventOption>? TrailingOptions { get; set; }
		public string? Placeholder { get; set; }

		public bool MatchesPrefix(string prefix) =>
			Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

		public InsertionResult GetInsertionResult()
		{
			string text = $"{Name}";
			int cursorOffset = text.Length;
			if (Placeholder != null)
			{
				text += $" {Placeholder}";
				return new InsertionResult(text, ++cursorOffset, Placeholder.Length, Placeholder);
			}

			return new InsertionResult(text, cursorOffset);
		}

		public EventOption(string name, bool hasTrailingOptions)
		{
			Name = name;
			HasTrailingOptions = hasTrailingOptions;
		}
	}
}
