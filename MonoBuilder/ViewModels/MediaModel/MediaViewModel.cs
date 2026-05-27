using MonoBuilder.Models.media_management;
using MonoBuilder.ViewModels._generic_models;
using MonoBuilder.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace MonoBuilder.ViewModels.MediaModel
{
	public partial class MediaViewModel : TabbedContentModel<MediaHandler, Media>
	{
		#region System Management Properties
		public override required MediaHandler DataController { get; init; }
		#endregion

		#region State Management Properties

		#region Video Properties
		private Uri? _mediaSource = null;
		private double _currentPositionSeconds = 0;
		private double _totalMediaDurationSeconds = 0;
		private string _currentMediaDurationString = "00:00:00";
		private bool _mediaIsVideo = false;
		private bool _mediaIsPlaying = false;
		private bool _mediaWasPlaying = false;
		private bool _isUserDragging = false;
		private bool _isPressingTrack = false;

		public Uri? MediaSource
		{
			get => _mediaSource;
			set => SetProperty(ref _mediaSource, value);
		}

		public double CurrentPositionSeconds
		{
			get => _currentPositionSeconds;
			set => SetProperty(ref _currentPositionSeconds, value);
		}

		public double TotalMediaDurationSeconds
		{
			get => _totalMediaDurationSeconds;
			set => SetProperty(ref _totalMediaDurationSeconds, value);
		}

		public string CurrentDurationString
		{
			get => _currentMediaDurationString;
			set => SetProperty(ref _currentMediaDurationString, value);
		}

		public bool MediaIsVideo
		{
			get => _mediaIsVideo;
			set => SetProperty(ref _mediaIsVideo, value);
		}

		public bool MediaIsPlaying
		{
			get => _mediaIsPlaying;
			set => SetProperty(ref _mediaIsPlaying, value);
		}

		public bool MediaWasPlaying
		{
			get => _mediaWasPlaying;
			set => SetProperty(ref _mediaWasPlaying, value);
		}

		public bool IsUserDragging
		{
			get => _isUserDragging;
			set => SetProperty(ref _isUserDragging, value);
		}

		public bool IsPressingTrack
		{
			get => _isPressingTrack;
			set => SetProperty(ref _isPressingTrack, value);
		}
		#endregion

		#region Mode Select Properties
		private string _selectedMode = "Music";

		public string SelectedMode
		{
			get => _selectedMode;
			set => SetProperty(ref _selectedMode, value);
		}

		public ObservableCollection<string> MediaModes { get; }
			= [ "Music", "Sounds", "Voices", "Videos" ];
		#endregion

		#region Window Properties
		private int _currentWindowWidth = 825;
		private double _totalWindowWidth = 825;

		private double _cachedMarginSize; // Prevent recalculating multiplication.
		private double _cachedBorderSize; // Prevent recalculating multiplication.

		public int EditingContentColumnWidth { get; set; } = 250;
		public Thickness DataGridMargin { get; set; } = new(5, 0, 5, 5);
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
					_cachedBorderSize,
					EditingContentColumnWidth + 1);

				OnPropertyChanged(nameof(TotalWindowWidth));
			}
		}
		#endregion

		private string Mode { get; set; } = "Music";
		#endregion

		public MediaViewModel() : base(new())
		{
			_cachedMarginSize = DataGridMargin.Left * 2;
			_cachedBorderSize = DataGridBorderThickness.Left * 2;
			_totalWindowWidth = WindowSizeChanged(
				CurrentWindowWidth,
				_cachedMarginSize,
				_cachedBorderSize,
				EditingContentColumnWidth + 1);

			PlayCommand = new(ExecutePlayCommand, PlayPauseStopCanExecute);
			PauseCommand = new(ExecutePauseCommand, PlayPauseStopCanExecute);
			StopCommand = new(ExecuteStopCommand, PlayPauseStopCanExecute);

			ScrubberDragStart = new(ExecuteScrubberDragStarted, PlayPauseStopCanExecute);
			ScrubberDragComplete = new(ExecuteScrubberDragCompleted, PlayPauseStopCanExecute);

			PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(SelectedMode))
				{
					// This must be the first thing that is set...
					// WPF/MVVM bugs are weird...
					MediaIsVideo = SelectedMode == "Videos";

					RunInitializations(SelectedMode);

					AddDataCommand.RaiseCanExecuteChanged();
					ModifyDataCommand.RaiseCanExecuteChanged();
					RemoveDataCommand.RaiseCanExecuteChanged();
					SaveToScriptCommand.RaiseCanExecuteChanged();
					ImportDataCommand.RaiseCanExecuteChanged();

					if (Owner is MediaBuilder mediaBuilder)
					{
						mediaBuilder.InitializeSetup(SelectedMode);
						
						if (SelectedEntities.Any())
						{
							MediaActionRequested?.Invoke("SetupPlayer");
						}
					}
				}

				if (e.PropertyName == nameof(SelectedEntity))
				{
					string? mediaPath = ApplicationSettings?.GetFolderPath(Mode);

					if (SelectedEntity == null)
					{
						MediaSource = null;
						RaiseExecutionStatus();
						return;
					}

					try
					{
						if (mediaPath != null)
						{
							string path = Path.Combine(mediaPath, SelectedEntity.Path);

							if (File.Exists(path))
							{
								MediaSource = new(path, UriKind.Absolute);
							}
							else
							{
								MediaSource = null;
								TotalMediaDurationSeconds = 0;
							}
						}
					}
					catch { MediaSource = null; }
					finally { RaiseExecutionStatus(); }
				}
			};
		}

		private void RaiseExecutionStatus()
		{
			PlayCommand.RaiseCanExecuteChanged();
			PauseCommand.RaiseCanExecuteChanged();
			StopCommand.RaiseCanExecuteChanged();
			ScrubberDragStart.RaiseCanExecuteChanged();
			ScrubberDragComplete.RaiseCanExecuteChanged();
		}

		#region Initialization Methods
		public void RunInitializations(string mode)
		{
			Mode = mode;
			DataController.SetDataMode(mode.ToLower());

			InitializeAvailableFiles(mode);
			InitializeDataTabs(mode, DataController.DataMode.Collection);
		}
		#endregion

		#region Can Execute Methods
		private bool PlayPauseStopCanExecute(object? _) => MediaActionRequested != null && SelectedEntity != null;
		#endregion
	}
}
