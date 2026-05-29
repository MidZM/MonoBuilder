using MonoBuilder.Models;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.media_management;
using MonoBuilder.ViewModels.MediaModel;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MonoBuilder.Views
{
	/// <summary>
	/// Interaction logic for MediaBuilder.xaml
	/// </summary>
	public partial class MediaBuilder : Window
	{
		private string Mode { get; set; } = "Music";
		private DispatcherTimer _timer = new();

		public MediaBuilder(AppSettings settings, MediaHandler mediaData, string mode)
		{
			MediaViewModel mediaViewModel = new()
			{
				Owner = this,
				DataController = mediaData,
				ApplicationSettings = settings
			};
			DataContext = mediaViewModel;

			mediaViewModel.RunInitializations(mode);

			InitializeComponent();

			if (DataContext is MediaViewModel vm)
			{
				vm.MediaActionRequested += OnMediaActionRequested;
				if (vm.SelectedEntities.Any())
				{
					vm.SelectedEntity = null;
					vm.SelectedEntities.Clear();
					OnMediaActionRequested("SetupPlayer");
				}
			}

			InitializeSetup(mode);
		}

		#region Utility Methods
		internal void InitializeSetup(string mode)
		{
			Mode = mode;
			switch (mode)
			{
				case "Music": Title = "MonoBuilder | Music Builder"; break;
				case "Sounds": Title = "MonoBuilder | Sound Builder"; break;
				case "Voices": Title = "MonoBuilder | Voice Builder"; break;
				case "Videos": Title = "MonoBuilder | Video Builder"; break;
			}

			_timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
			_timer.Tick += Timer_Tick;
			_timer.Start();
		}

		private void OnMediaActionRequested(string action)
		{
			if (DataContext is MediaViewModel vm)
			{
				switch (action)
				{
					case "Play":
						vm.MediaIsPlaying = true;
						VideoPlayer.Play();
						break;
					case "Pause":
						vm.MediaIsPlaying = false;
						VideoPlayer.Pause();
						break;
					case "Stop":
						vm.MediaIsPlaying = false;
						VideoPlayer.Stop();
						break;
					case "DragStart":
						vm.MediaWasPlaying = vm.MediaIsPlaying;
						vm.MediaIsPlaying = false;
							VideoPlayer.Pause();
						break;
					case "DragComplete":
						if (vm.MediaWasPlaying)
						{
							vm.MediaIsPlaying = true;
							VideoPlayer.Play();
						}

						vm.MediaWasPlaying = false;
						VideoPlayer.Position = TimeSpan.FromSeconds(VideoScrubber.Value);
						vm.CurrentPositionSeconds = VideoScrubber.Value;
						vm.CurrentDurationString = VideoPlayer.Position.ToString(@"hh\:mm\:ss");
						break;
					case "SetupPlayer":
						VideoPlayer.Position = TimeSpan.FromSeconds(0);
						vm.CurrentPositionSeconds = 0;
						vm.CurrentDurationString = VideoPlayer.Position.ToString(@"hh\:mm\:ss");

						// Starting, stopping, and reseting fixes a bug where everything is blank
						VideoPlayer.Play();
						VideoPlayer.Stop();
						vm.CurrentPositionSeconds = 0;
						break;

				}
			}
		}

		private void OpenMoveImageSubmenu()
		{
			var context = DataContext as MediaViewModel;
			var selectedImage = MediaDataGrid.SelectedItems.Cast<Media>().ToList();
			var defaultFileKey = context?.GetDefaultDataFileKey(Mode);
			string? commonFileKey =
				(selectedImage.Select(c => string.IsNullOrEmpty(c.FileKey)
				? defaultFileKey
				: c.FileKey).Distinct().Count() == 1)
					? selectedImage.First().FileKey
					: null;

			Dispatcher.BeginInvoke(new Action(() =>
			{
				foreach (var item in ContextMoveMedia.Items)
				{
					if (ContextMoveMedia.ItemContainerGenerator.ContainerFromItem(item) is MenuItem container)
					{
						var match = MediaViewModel.GetFileKey.Match(item?.ToString() ?? string.Empty);
						container.Icon = match.Success && commonFileKey != null && match.Groups[1].Value == commonFileKey
							? new TextBlock
							{
								Text = "•",
								FontSize = 25,
								VerticalAlignment = VerticalAlignment.Center,
								HorizontalAlignment = HorizontalAlignment.Center
							}
							: null;
					}
				}
			}), DispatcherPriority.Render);
		}

		private void Timer_Tick(object? sender, EventArgs e)
		{
			if (DataContext is MediaViewModel vm)
			{
				if (!vm.IsUserDragging && !vm.IsPressingTrack)
				{
					VideoScrubber.Value = VideoPlayer.Position.TotalSeconds;
					vm.CurrentDurationString = VideoPlayer.Position.ToString(@"hh\:mm\:ss");
				}
				else if (Mouse.LeftButton == MouseButtonState.Released && vm.IsPressingTrack)
				{
					vm.IsPressingTrack = false;
				}
			}
		}
		#endregion

		#region Event Handlers
		private void ContextMoveMedia_SubmenuOpened(object sender, RoutedEventArgs e)
		{
			OpenMoveImageSubmenu();
		}

		private void MediaDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DataContext is MediaViewModel vm)
			{
				vm.SelectedEntities.Clear();

				foreach (var item in ((DataGrid)sender).SelectedItems.OfType<Media>())
				{
					vm.SelectedEntities.Add(item);
				}
			}
		}

		private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
		{
			if (VideoPlayer.NaturalDuration.HasTimeSpan)
			{
				if (DataContext is MediaViewModel vm)
				{
					TimeSpan duration = VideoPlayer.NaturalDuration.TimeSpan;
					vm.TotalMediaDurationSeconds = duration.TotalSeconds;
				}
			}

			if (!VideoPlayer.HasVideo)
				MediaImage.Visibility = Visibility.Visible;
			else
				MediaImage.Visibility = Visibility.Collapsed;
		}

		private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
		{
			VideoPlayer.Stop();
		}

		private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
		{
			DialogBox.Show(
				"This media format is not available...\n" +
				"Media formats are influenced by the audio and video codecs installed on your machine.\n\n" +
				"Rule of Thumb: If an audio or video file cannot be played on the Windows Media Player, it is very unlikely to be playable here.\n\n" +
				$"{e.ErrorException.Message}",
				"Failed To Player Media Source",
				DialogButtonDefaults.OK,
				DialogIcon.Error);
		}

		private void VideoScrubber_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
		{
			if (DataContext is MediaViewModel vm)
			{
				vm.IsUserDragging = true;
				OnMediaActionRequested("DragStart");
			}
		}

		private void VideoScrubber_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			if (DataContext is MediaViewModel vm)
			{
				vm.IsUserDragging = false;
				OnMediaActionRequested("DragComplete");
			}
		}

		private void VideoScrubber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (DataContext is MediaViewModel vm)
			{
				if (vm.IsUserDragging)
				{
					VideoPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
					vm.CurrentDurationString = VideoPlayer.Position.ToString(@"hh\:mm\:ss");
					vm.CurrentPositionSeconds = e.NewValue;
					return;
				}

				if (Mouse.LeftButton == MouseButtonState.Pressed)
				{
					VideoPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
					vm.CurrentPositionSeconds = e.NewValue;
					vm.IsPressingTrack = true;
				}
			}
		}
		#endregion
	}
}
