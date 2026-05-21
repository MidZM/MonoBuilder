using MonoBuilder.Models.image_management;
using MonoBuilder.ViewModels._generic_models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MonoBuilder.ViewModels.ImageModel
{
    public partial class ImageViewModel : TabbedContentModel<MonoImage>
    {
		#region SystemManagement Properties
		public required MonoImages ImageData { get; set; }
		#endregion

		#region State Management Properties
		private int _currentWindowWidth = 825;
		private double _totalWindowWidth = 825;
		private int _containerWidth = 1280;
		private int _containerHeight = 720;
		private ImageSource? _imagePreview;

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

		public int ContainerWidth
		{
			get => _containerWidth;
			set
			{
				int.TryParse(value.ToString(), out int parsed);
				SetProperty(ref _containerWidth, parsed);
			}
		}
		public int ContainerHeight
		{
			get => _containerHeight;
			set
			{
				int.TryParse(value.ToString(), out int parsed);
				SetProperty(ref _containerHeight, parsed);
			}
		}

		public ImageSource? ImagePreview
		{
			get => _imagePreview;
			set => SetProperty(ref _imagePreview, value);
		}

		private string Mode { get; set; } = "Images";
		#endregion

		public ImageViewModel() : base(new())
		{
			_cachedMarginSize = DataGridMargin.Left * 2;
			_cachedBorderSize = DataGridBorderThickness.Left * 2;
			_totalWindowWidth = WindowSizeChanged(
				CurrentWindowWidth,
				_cachedMarginSize,
				_cachedBorderSize,
				EditingContentColumnWidth + 1);

			MoveImagesCommand = new(ExecuteMoveImagesCommand);
			ExitCommand = new(ExecuteExitCommand);

			AddImagesCommand = new(ExecuteAddImagesCommand, AddCanExecute);
			ModifyImagesCommand = new(ExecuteModifyImagesCommand, ModifyOrRemoveCanExecute);
			RemoveImagesCommand = new(ExecuteRemoveImagesCommand, ModifyOrRemoveCanExecute);
			SaveToScriptCommand = new(ExecuteSaveToScriptCommand, AddCanExecute);
			ImportImagesCommand = new(ExecuteImportImagesCommand, AddCanExecute);

			SelectedEntities.CollectionChanged += (s, e) =>
			{
				ModifyImagesCommand.RaiseCanExecuteChanged();
				RemoveImagesCommand.RaiseCanExecuteChanged();
			};

			PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(SelectedEntity))
				{
					string? imagesPath = ApplicationSettings.GetFolderPath(Mode);

					if (SelectedEntity == null || string.IsNullOrWhiteSpace(imagesPath))
					{
						ImagePreview = null;
						return;
					}

					try
					{
						string path = Path.Combine(imagesPath, SelectedEntity.Path);
						Uri uri = new(path, UriKind.Absolute);
						ImagePreview = new BitmapImage(uri);
					}
					catch
					{
						ImagePreview = null;
					}
				}
			};
		}

		#region Initialization Methods
		public void RunInitializations(string mode)
		{
			Mode = mode;
			ImageData.SetDataMode(mode.ToLower());

			InitializeAvailableFiles(mode);
			InitializeDataTabs(mode, ImageData.DataMode.Collection);
		}
		#endregion
	}
}
