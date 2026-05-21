using MonoBuilder.Models;
using MonoBuilder.Models.helpers;
using MonoBuilder.Models.image_management;
using MonoBuilder.Models.generics.enums;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonoBuilder.ViewModels.ImageModel;

namespace MonoBuilder.Views.ViewUtils
{
    /// <summary>
    /// Interaction logic for ImageBuilder.xaml
    /// </summary>
    public partial class ImageBuilder : Window
    {
        private double TotalWindowWidth = 0;
        private string Mode { get; set; } = "Images";

        public ImageBuilder(AppSettings settings, MonoImages imageData, string mode)
		{
			ImageViewModel imageViewModel = new()
			{
				Owner = this,
				ImageData = imageData,
				ApplicationSettings = settings
			};
			DataContext = imageViewModel;

			imageViewModel.RunInitializations(mode);

			InitializeComponent();
			
			Mode = mode;
			switch (mode)
            {
                case "Images": Title = "MonoBuilder | Image Builder"; break;
                case "Scenes": Title = "MonoBuilder | Scene Builder"; break;
                case "Gallery": Title = "MonoBuilder | Gallery Builder"; break;
            }
        }

        #region Utility Methods

        private void OpenMoveImageSubmenu()
        {
			var context = DataContext as ImageViewModel;
            var selectedImage = ImagesDataGrid.SelectedItems.Cast<MonoImage>().ToList();
            var defaultFileKey = context?.GetDefaultDataFileKey(Mode);
            string? commonFileKey =
                (selectedImage.Select(c => string.IsNullOrEmpty(c.FileKey)
                ? defaultFileKey
                : c.FileKey).Distinct().Count() == 1)
                    ? selectedImage.First().FileKey
                    : null;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in ContextMoveImage.Items)
                {
                    if (ContextMoveImage.ItemContainerGenerator.ContainerFromItem(item) is MenuItem container)
                    {
						var match = ImageViewModel.GetFileKey.Match(item?.ToString() ?? string.Empty);
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
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
        #endregion

        #region Event Handlers
        private void ContextMoveImage_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            OpenMoveImageSubmenu();
		}

		private void ImagesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DataContext is ImageViewModel vm)
			{
				vm.SelectedEntities.Clear();

				foreach (var item in ((DataGrid)sender).SelectedItems.OfType<MonoImage>())
				{
					vm.SelectedEntities.Add(item);
				}
			}
		}
		#endregion
	}
}
