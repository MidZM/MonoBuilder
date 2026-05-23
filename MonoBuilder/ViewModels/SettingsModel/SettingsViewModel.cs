using Microsoft.Win32;
using MonoBuilder.Commands;
using MonoBuilder.Models;
using MonoBuilder.Models.character_management;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.ViewModels._generic_models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace MonoBuilder.ViewModels.SettingsModel
{
    public partial class SettingsViewModel : TabbedModel<Character>
    {
		#region System Management Properties
		public required Characters CharacterData { get; set; }
		public required ScriptConversion Converter { get; set; }
		#endregion

		#region State Management Properties

		#region Window Resizing
		private int _currentWindowWidth = 825;
		private double _totalWindowWidth = 825;

		private double _cachedMarginSize; // Prevent recalculating multiplication.
		private double _cachedBorderSize; // Prevent recalculating multiplication.

		public Thickness DataGridMargin { get; set; } = new(10, 0, 10, 0);
		public Thickness DataGridBorderThickness { get; set; } = new(1);
		public int CurrentWindowWidth
		{
			get => _currentWindowWidth;
			set
			{
				if (SetProperty(ref _currentWindowWidth, value))
				{
					TotalWindowWidth = _totalWindowWidth;
				}
			}
		}
		public double TotalWindowWidth
		{
			get => _totalWindowWidth;
			set
			{
				_totalWindowWidth = WindowSizeChanged(
					CurrentWindowWidth,
					_cachedMarginSize,
					_cachedBorderSize);

				OnPropertyChanged(nameof(TotalWindowWidth));
			}
		}
		#endregion

		public readonly List<string> SystemTypes = new()
		{
			"Script",
			"Characters",
			"Images",
			"Scenes",
			"Gallery",
			"Messages",
			"Notifications"
		};
		private readonly List<string> FolderTypes = new()
		{
			"Base",
			"Assets",
			"Images",
			"Scenes",
			"Gallery"
		};

		public TabEntries MixedEntries { get; set; } = new();

		public bool HasScriptFile => GetScriptTab()?.Files.Any(HasValidPath) == true;

		private bool _isValidRegex = true;
		private bool _changesMade = false;

		private bool IsValidRegex {
			get => _isValidRegex;
			set
			{
				if (SetProperty(ref _isValidRegex, value))
				{
					IsValidRegex = value;
					SaveDataCommand.RaiseCanExecuteChanged();
				}
			}
		}

		private bool ChangesMade {
			get => _changesMade;
			set
			{
				if (SetProperty(ref _changesMade, value))
				{
					ChangesMade = value;
					SaveDataCommand.RaiseCanExecuteChanged();
				}
			}
		}

		public StringWrapper SelectedBasePath { get; set; } = new();

		private ObservableCollection<ConversionRule> _conversionRules = new();

		public ObservableCollection<ConversionRule> ConversionRules
		{
			get => _conversionRules;
			set
			{
				SetProperty(ref _conversionRules, value);
			}
		}
		#endregion

		public SettingsViewModel()
		{
			_cachedMarginSize = DataGridMargin.Left * 2;
			_cachedBorderSize = DataGridBorderThickness.Left * 2;
			_totalWindowWidth = WindowSizeChanged(
				CurrentWindowWidth,
				_cachedMarginSize,
				_cachedBorderSize);

			MoveCharacterCommand = new(ExecuteMoveCharacterCommand);

			AddCharactersCommand = new(ExecuteAddCharactersCommand, AddCanExecute);
			ModifyCharactersCommand = new(ExecuteModifyCharactersCommand, ModifyOrRemoveCanExecute);
			RemoveCharactersCommand = new(ExecuteRemoveCharactersCommand, ModifyOrRemoveCanExecute);
			SaveToScriptCommand = new(ExecuteSaveToScriptCommand, AddCanExecute);
			ImportCharactersCommand = new(ExecuteImportCharactersCommand, AddCanExecute);

			SaveDataCommand = new(ExecuteSaveDataCommand, SaveCanExecute);
			ExitCommand = new(ExecuteExitCommand);

			CheckBoxCommand = new(ExecuteCheckBoxCommand);
			TextBoxCommand = new(ExecuteTextBoxComand);
			NumberBoxCommand = new(ExecuteNumberBoxCommand);

			SelectedBasePath.AddCommandRange([AddCharactersCommand, SaveToScriptCommand, ImportCharactersCommand]);

			SelectedEntities.CollectionChanged += (s, e) =>
			{
				ModifyCharactersCommand.RaiseCanExecuteChanged();
				RemoveCharactersCommand.RaiseCanExecuteChanged();
			};
		}

		#region State Management Methods
		private bool SaveCanExecute(object? parameter = null) => ChangesMade && IsValidRegex;
		#endregion

		#region Initialization Methods
		public void RunInitializations()
		{
			SelectedBasePath.Value = ApplicationSettings.GetFolderPath("Base") ?? string.Empty;

			AutoSyncChecked = ApplicationSettings.GetAutoSyncLabels();
			ColorEnabledChecked = ApplicationSettings.GetColorFormatting();
			SelectedIndentation = ApplicationSettings.GetIndentationType();
			SpacesAmount = ApplicationSettings.GetIndentationAmount().ToString();

			foreach (var rule in Converter.ConversionRules)
			{
				ConversionRules.Add(new ConversionRule()
				{
					Name = rule.Name,
					Pattern = rule.Pattern,
					IsEnabled = rule.IsEnabled,
					Priority = rule.Priority
				});
			}

			Converter.UnsetSortedRules();

			string type = "Characters";
			InitializeAvailableFiles(type);
			InitializeDataTabs(type, CharacterData.DataMode.Collection);
			InitializeMultiFileEntries();
			SetupAutomaticNotifications();
		}

		private void SetupAutomaticNotifications()
		{
			MixedEntries.Tabs.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasScriptFile));

			var scriptTab = GetScriptTab();
			if (scriptTab != null)
			{
				scriptTab.Files.CollectionChanged += (s, e) =>
					OnPropertyChanged(nameof(HasScriptFile));

				foreach (var file in scriptTab.Files)
				{
					file.PropertyChanged += File_PropertyChanged;
				}

				scriptTab.Files.CollectionChanged += (s, e) =>
				{
					if (e.NewItems != null)
					{
						foreach (FileEntry newFile in e.NewItems.OfType<FileEntry>())
						{
							newFile.PropertyChanged += File_PropertyChanged;
						}
					}
				};
			}

			var characterTab = GetCharacterTab();
			if (AvailableFiles.Count == 0 && characterTab != null)
			{
				characterTab.Files.CollectionChanged += (s, e) =>
				{
					Views.ViewUtils.DialogBox.Show(AvailableFiles.Count.ToString());
					if (e.NewItems != null)
					{
						foreach (FileEntry newFile in e.NewItems.OfType<FileEntry>())
						{
							newFile.PropertyChanged += (ss, ee) =>
							{
								if (ee.PropertyName == nameof(FileEntry.Path))
								{
									DispatcherTimer? timer = new DispatcherTimer();
									timer.Tick += (s, e) =>
									{
										var type = "Characters";
										InitializeAvailableFiles(type);
										InitializeDataTabs(type, CharacterData.DataMode.Collection);

										AddCharactersCommand.RaiseCanExecuteChanged();
										ImportCharactersCommand.RaiseCanExecuteChanged();
										SaveToScriptCommand.RaiseCanExecuteChanged();

										timer.Stop();
										timer = null;
									};

									timer.Interval = TimeSpan.FromMilliseconds(50);
									timer.Start();
								}
							};
						}
					}
				};
			}

			// Any time the SelectedBasePath value is changed, it needs to percolate that change
			// across all FileEntry instances.
			SelectedBasePath.PropertyChanged += (s, e) =>
			{
				foreach (var entries in MixedEntries.Tabs)
				{
					if (entries is FileTabEntry fileEntry)
					{
						foreach (var entry in fileEntry.Files)
						{
							entry.UpdateBasePath(SelectedBasePath.Value);
						}
					}

					if (entries is FolderTabEntry folderEntry)
					{
						foreach (var entry in folderEntry.Folders)
						{
							entry.UpdateBasePath(SelectedBasePath.Value);
						}
					}
				}
			};

			foreach (var rule in ConversionRules)
			{
				rule.PropertyChanged += (s, e) =>
				{
					ChangesMade = true;	
				};
			}
		}

		private void InitializeMultiFileEntries()
		{
			var mixedEntries = new List<TabEntry>()
			{
				new FolderTabEntry(ApplicationSettings, "General", SelectedBasePath, GetFolderInformation())
			};
			mixedEntries.AddRange(SystemTypes
				.Select(system => new FileTabEntry(ApplicationSettings, system, SelectedBasePath.Value, GetFileInformation(system))));

			MixedEntries = new(mixedEntries);
		}

		private void File_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(FileEntry.Path))
			{
				OnPropertyChanged(nameof(HasScriptFile));
				AddCharactersCommand.RaiseCanExecuteChanged();
				ImportCharactersCommand.RaiseCanExecuteChanged();
				SaveToScriptCommand.RaiseCanExecuteChanged();
			}
		}
		#endregion

		#region Utility Methods
		private FileEntry[]? GetFileInformation(string fileType)
		{
			var filePaths = ApplicationSettings.GetAllFilePaths(fileType);
			if (filePaths.Count > 0)
			{
				return filePaths
					.Select(file => new FileEntry(
						string.Empty,
						file.Key,
						file.Value,
						ApplicationSettings,
						SelectedBasePath.Value))
					.ToArray();
			}

			return null;
		}

		private FileEntry[] GetFolderInformation()
		{
			var folderPaths = ApplicationSettings.GetAllFolderPaths();
			var folders = FolderTypes
				.Select(folder => new FileEntry(
					$"{folder} Folder",
					folder,
					string.Empty,
					ApplicationSettings,
					SelectedBasePath.Value))
				.ToArray();

			foreach (var folder in folders)
			{
				if (folderPaths.TryGetValue(folder.Key, out string? path))
				{
					folder.Path = path;
				}
			}

			return folders;
		}

		private static bool HasValidPath(FileEntry file)
		{
			return !string.IsNullOrEmpty(file.Path);
		}

		private FileTabEntry? GetScriptTab()
		{
			return MixedEntries.Tabs.OfType<FileTabEntry>()
				.FirstOrDefault(t => t.Name == "Script");
		}

		private FileTabEntry? GetCharacterTab()
		{
			return MixedEntries.Tabs.OfType<FileTabEntry>()
				.FirstOrDefault(t => t.Name == "Characters");
		}
		#endregion
	}

	public class StringWrapper : BaseViewModel
	{
		private string _value = string.Empty;
		private ObservableCollection<RelayCommand> Commands { get; set; } = new();

		public string Value
		{
			get => _value;
			set
			{
				if (SetProperty(ref _value, value))
				{
					OnPropertyChanged(nameof(Value));
					CommandsRaiseCanExecuteChanged();
				}
			}
		}

		public void AddCommand(RelayCommand command)
		{
			Commands.Add(command);
		}

		public void AddCommandRange(IEnumerable<RelayCommand> commands)
		{
			foreach (var command in commands)
			{
				Commands.Add(command);
			}
		}

		public void CommandsRaiseCanExecuteChanged()
		{
			if (Commands.Count == 0) return;

			foreach (var command in Commands)
			{
				command.RaiseCanExecuteChanged();
			}
		}
	}
}
