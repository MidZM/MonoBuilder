using MonoBuilder.Views.ViewUtils;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.generics.structs;
using MonoBuilder.Models.generics.enums;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;

namespace MonoBuilder.Models.helpers
{
	public class EventHelper
	{
		public ObservableCollection<IEvent> AllEvents { get; } = new();
		private Dictionary<string, MonoSystem> AllSystems { get; } = new();
		private Dictionary<string, IEnumerable<INamedEntity>> AllSystemCollections { get; } = new();
		private Dictionary<string, EventPosition> AllEventPositions { get; } = new()
		{
			["Left"] = EventPosition.Left,
			["Center"] = EventPosition.Center,
			["Right"] = EventPosition.Right
		};

		private XDocument SystemData { get; set; } = new();

		public EventHelper(params (string, MonoSystem)[] systems)
		{
			foreach (var system in systems)
			{
				AllSystems.Add(system.Item1, system.Item2);
			}
		}

		public void RegisterCollection(string name, IEnumerable<INamedEntity> collection)
		{
			AllSystemCollections.TryAdd(name, collection);
		}

		public void AddEvent(IEvent ev)
		{
			AllEvents.Add(ev);
		}

		public IEnumerable<IEvent> GetFilteredAndSorted(string prefix)
		{
			if (string.IsNullOrEmpty(prefix))
			{
				return AllEvents.OrderByDescending(e => e.Name);
			}

			return AllEvents
				.Where(e => e.MatchesPrefix(prefix))
				.OrderByDescending(a => a.Name);
		}

		public IEvent? GetEvent(string name)
		{
			return AllEvents.FirstOrDefault(e => e.Name == name);
		}

		public void LoadEvents()
		{
			if (!Directory.Exists("data"))
			{
				Directory.CreateDirectory("data");
				Directory.CreateDirectory("data/content");
			}

			if (!Directory.Exists("data/content"))
			{
				Directory.CreateDirectory("data/content");
			}

			if (!File.Exists("data/events.xml")) return;

			try
			{
				SystemData = XDocument.Load("data/events.xml");
				AllEvents.Clear();
				foreach (var element in SystemData.Descendants("Event"))
				{
					//string name = element.Attribute("Name")?.Value ?? string.Empty;
					string triggerText = element.Attribute("TriggerText")?.Value ?? string.Empty;
					XElement? system = element.Element("System");

					// Name, TriggerText, and Type are required attributes.
					if (triggerText == string.Empty ||
						system == null) continue;

					//string? systemMode = system.Attribute("Mode")?.Value;
					string systemName = system.Attribute("Name")?.Value ?? string.Empty;
					string systemCollection = system.Attribute("Collection")?.Value ?? string.Empty;

					// System Name and System Collection are required attributes.
					if (systemName == string.Empty ||
						systemCollection == string.Empty) continue;

					// Any system provided must be a legitimate system.
					if (!AllSystems.TryGetValue(systemName, out var monoSystem) ||
						!AllSystemCollections.TryGetValue(systemCollection, out var monoSystemCollection)) continue;

					ScriptEvent scriptEvent = new(systemName, triggerText)
					{
						System = monoSystem,
						SystemCollection = monoSystemCollection,
						SystemModeName = monoSystem.DataModeTypeName != string.Empty ? monoSystem.DataModeTypeName : null
					};

					AllEvents.Add(scriptEvent);

					XElement? optionsContainer = element.Element("Options");
					IEnumerable<XElement>? positions = element.Elements("Position");
					string? optionsTemplate = optionsContainer?.Attribute("ItemTemplate")?.Value;

					// Set up the options
					if (optionsContainer != null && (optionsContainer.HasElements || optionsTemplate != null))
					{
						scriptEvent.Options = new List<EventOption>();
						List<XElement>? options = optionsContainer.Elements("Option").ToList();

						if (optionsTemplate != null)
						{
							IEnumerable<XElement>? templateOptions = SystemData.Descendants("OptionTemplate")
																	.Where(d => d.Attribute(optionsTemplate) != null)
																	.Descendants();
							options = templateOptions.Concat(options).ToList();
						}

						foreach (var option in options)
						{
							string optionName = option.Attribute("Name")?.Value ?? string.Empty;
							bool.TryParse(option.Attribute("HasTrailingOptions")?.Value, out bool optionHasTrailingOptions);

							if (optionName == string.Empty) continue;

							EventOption newOption = new(optionName, optionHasTrailingOptions)
							{
								TrailingOptions = optionHasTrailingOptions
									? new List<EventOption>()
									: null
							};

							scriptEvent.Options.Add(newOption);

							if (newOption.TrailingOptions != null)
							{
								IEnumerable<XElement>? trailingOptions = option.Elements("TrailingOption");
								foreach (var trailigOption in trailingOptions)
								{
									string? trailingOptionPlaceholder = trailigOption.Attribute("Placeholder")?.Value;
									string? trailingOptionStartValue = trailigOption.Attribute("StartValue")?.Value;
									string? trailingOptionEndValue = trailigOption.Attribute("EndValue")?.Value;
									newOption.TrailingOptions.Add(new EventOption(optionName, false)
									{
										Placeholder = trailingOptionPlaceholder,
										StartValue = trailingOptionStartValue,
										EndValue = trailingOptionEndValue
									});
								}
							}
						}
					}

					// Set up the positions
					if (positions != null && positions.Count() > 0)
					{
						scriptEvent.Positions = new List<EventPosition>();

						foreach (var position in positions)
						{
							string positionName = position.Attribute("Name")?.Value ?? string.Empty;

							if (AllEventPositions.TryGetValue(positionName, out var eventPosition))
							{
								scriptEvent.Positions.Add(eventPosition);
							}
						}
					}
				}
			}
			catch (Exception error)
			{
				DialogBox.Show(
					$"Something went wrong while loading events:\n\n{error.Message}",
					"Error",
					DialogButtonDefaults.OK,
					DialogIcon.Error);
			}
		}

		public void SaveEvents()
		{

		}
	}

	public interface IEvent
	{
		string Name { get; }
		bool MatchesPrefix(string prefix);
		InsertionResult GetInsertionResult();
	}

	public interface IEventOption : IEvent
	{
		bool HasTrailingOptions { get; }
		List<EventOption>? TrailingOptions { get; set; }
		string? Placeholder { get; set; }
	}
}