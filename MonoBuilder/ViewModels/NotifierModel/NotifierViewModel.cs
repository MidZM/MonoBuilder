using ICSharpCode.AvalonEdit.Document;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.helpers;
using MonoBuilder.Models.image_management;
using MonoBuilder.Models.notification_management;
using MonoBuilder.ViewModels._generic_models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace MonoBuilder.ViewModels.NotifierModel
{
    public partial class NotifierViewModel : TabbedSpecialCommandModel<Notifications, Notification>
	{
		#region System Management Properties
		public override required Notifications DataController { get; init; }
		#endregion

		#region State Management Properties

		#region Window Resizing
		private int _currentWindowWidth = 1000;
		private double _totalWindowWidth = 388;

		private double _cachedMarginSize; // Prevent recalculating multiplication.
		private double _cachedBorderSize; // Prevent recalculating multiplication.

		public int EditingContentColumnWidth { get; set; } = 500;
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
				_totalWindowWidth = Math.Clamp(WindowSizeChanged(
					CurrentWindowWidth,
					_cachedMarginSize,
					_cachedBorderSize,
					EditingContentColumnWidth + 1), 0, 738);

				OnPropertyChanged(nameof(TotalWindowWidth));
			}
		}
		#endregion

		#region Preview Data
		private TextDocument _previewMessage = new();
		public TextDocument PreviewMessage
		{
			get => _previewMessage;
			set => SetProperty(ref _previewMessage, value);
		}
		#endregion

		#region Editing Data

		#region Name Box
		private string _boxNameText = string.Empty;

		public string BoxNameLabel { get; } = "Message ID (Name)";
		public string BoxNameTag { get; } = "Enter Message ID (Name) Here...";
		public string BoxNameText
		{
			get => _boxNameText;
			set
			{
				if (SetProperty(ref _boxNameText, value))
				{
					SaveCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion

		#region Title Box
		private string _boxTitleText = string.Empty;
		public string BoxTitleText
		{
			get => _boxTitleText;
			set => SetProperty(ref _boxTitleText, value);
		}
		#endregion

		#region Subtitle Box
		private string _boxSubtitleLabel = string.Empty;

		public string BoxSubtitleLabel { get; } = "Subtitle";
		public string BoxSubtitleTag { get; } = "Enter Subtitle Here...";
		public string BoxSubtitleText
		{
			get => _boxSubtitleLabel;
			set => SetProperty(ref _boxSubtitleLabel, value);
		}
		#endregion

		#region Body Box
		private TextDocument _boxBodyText = new();
		public TextDocument BoxBodyText
		{
			get => _boxBodyText;
			set => SetProperty(ref _boxBodyText, value);
		}
		#endregion

		#region Close Button Box
		private string _boxCloseText = string.Empty;

		public string BoxCloseGridWidth { get; } = "2*";
		public Visibility BoxCloseVisibility { get; } = Visibility.Visible;
		public string BoxCloseText
		{
			get => _boxCloseText;
			set => SetProperty(ref _boxCloseText, value);
		}
		#endregion

		#region File Select
		private string _boxFileSelectedFile = string.Empty;
		public string BoxFileSelectedFile
		{
			get => _boxFileSelectedFile;
			set => SetProperty(ref _boxFileSelectedFile, value);
		}
		#endregion

		#endregion

		private string Mode { get; set; } = "Messages";
		#endregion

		public NotifierViewModel() : base()
		{
			_cachedMarginSize = DataGridMargin.Left * 2;
			_cachedBorderSize = DataGridBorderThickness.Left * 2;
			_totalWindowWidth = WindowSizeChanged(
				CurrentWindowWidth,
				_cachedMarginSize,
				_cachedBorderSize,
				EditingContentColumnWidth + 1);

			SaveCommand = new(ExecuteSaveCommand, SaveCanExecute);
			CancelCommand = new(ExecuteCancelCommand);

			PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(SelectedEntity) && SelectedEntity != null)
				{
					ShowPreviewMessage(SelectedEntity);
				}
				else if (!IsEditingData.Value && SelectedEntity == null)
				{
					RemovePreviewMessage();
				}
			};
		}

		#region State Mangement Methods
		private bool SaveCanExecute(object? _) => !string.IsNullOrWhiteSpace(BoxNameText);
		protected override bool ExitCanExecute(object? _) => IsEditingData.Inverse;
		#endregion

		#region Initialization Methods
		public void RunInitializations(string mode)
		{
			Mode = mode;
			DataController.SetDataMode(mode.ToLower());
			BoxFileSelectedFile = AvailableFiles.FirstOrDefault() ?? string.Empty;

			InitializeAvailableFiles(mode);
			InitializeDataTabs(mode, DataController.DataMode.Collection);
		}
		#endregion

		#region Utility Methods
		private string FormatPreviewMessage(Notification notification)
		{
			string format = string.Empty;

			if (!string.IsNullOrEmpty(notification.Title))
				format += $"#### Title\n{notification.Title}\n\n";

			if (!string.IsNullOrEmpty(notification.Icon))
				format += $"##### Icon Path\n{notification.Icon}\n\n";

			if (!string.IsNullOrEmpty(notification.Subtitle))
				format += $"###### Subtitle\n{notification.Subtitle}\n\n";

			if (!string.IsNullOrEmpty(notification.Body))
				format += $"##### Body\n{notification.Body}\n\n";

			if (!string.IsNullOrEmpty(notification.CloseAction))
				format += $"###### Close Button Text\n{notification.CloseAction}\n\n";

			return format;
		}

		private void ShowPreviewMessage(Notification notification)
		{
			PreviewMessage.Text = FormatPreviewMessage(notification);
		}

		private void RemovePreviewMessage()
		{
			ClearDataEditors();
			PreviewMessage.Text = string.Empty;
		}
		#endregion
	}
}
