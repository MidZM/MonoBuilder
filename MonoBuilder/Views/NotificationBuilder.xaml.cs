using ICSharpCode.AvalonEdit.Highlighting;
using MonoBuilder.Models;
using MonoBuilder.Models.character_management;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.helpers;
using MonoBuilder.Models.notification_management;
using MonoBuilder.ViewModels.NotifierModel;
using MonoBuilder.ViewModels.SettingsModel;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Resources.ResXFileRef;

namespace MonoBuilder.Views
{
	/// <summary>
	/// Interaction logic for NotificationBuilder.xaml
	/// </summary>
	public partial class NotificationBuilder : Window
	{
		private string Mode { get; set; } = "Messages";

		public NotificationBuilder(AppSettings settings, Notifications notificationData, string mode)
		{
			Mode = mode;

			NotifierViewModel settingsViewModel = new()
			{
				DataController = notificationData,
				ApplicationSettings = settings,
				Owner = this
			};
			DataContext = settingsViewModel;

			// Run initializations watcher after the fact so that the Owner has a chance to initialize properly.
			settingsViewModel.RunInitializations(mode);

			InitializeComponent();

			switch (mode)
			{
				case "Messages":
					Title = "MonoBuilder | Message Builder";
					break;
				case "Notifications":
					Title = "MonoBuilder | Notification Builder";
					ComboFile.Margin = new Thickness(0);
					break;
			}

			NotifierOutput.TextArea.TextView.LinkTextForegroundBrush = Brushes.Cyan;
			EditBody.TextArea.TextView.LinkTextForegroundBrush = Brushes.Cyan;
		}

		private void ContextMoveNotification_Click(object sender, RoutedEventArgs e)
		{
			if (e.OriginalSource is MenuItem clickedItem)
			{
				var context = DataContext as NotifierViewModel;
				string? selectedItem = clickedItem.Header.ToString() ?? null;
				var selectedNotifiers = NotificationsDataGrid.SelectedItems.Cast<Notification>().ToList();

				if (selectedNotifiers.Count > 0 && selectedItem != null)
				{
					var match = IBuilderTabbedModel<Notification>.GetFileKey.Match(selectedItem);

					if (match.Success)
					{
						var targetFileKey = match.Groups[1].Value;
						if (targetFileKey != null)
						{
							foreach (Notification notif in selectedNotifiers)
							{
								notif.FileKey = targetFileKey;
								context?.DataController.UpdateData(notif.EntityID, notif);
							}
						}
						else
						{
							DialogBox.Show(
								$"The selected file path for {selectedItem} is not set. Please set it before moving notifiers.",
								"File Path Not Set",
								DialogButtonDefaults.OK,
								DialogIcon.Warning);
						}
					}
				}
			}
		}

		private void ContextMoveNotification_SubmenuOpened(object sender, RoutedEventArgs e)
		{
			var context = DataContext as NotifierViewModel;
			var selectedNotifier = NotificationsDataGrid.SelectedItems.Cast<Notification>().ToList();
			var defaultFileKey = context?.GetDefaultDataFileKey(Mode);
			string? commonFileKey =
				(selectedNotifier.Select(c => string.IsNullOrEmpty(c.FileKey)
				? defaultFileKey
				: c.FileKey).Distinct().Count() == 1)
					? selectedNotifier.First().FileKey
					: null;

			Dispatcher.BeginInvoke(new Action(() =>
			{
				foreach (var item in ContextMoveNotification.Items)
				{
					if (ContextMoveNotification.ItemContainerGenerator.ContainerFromItem(item) is MenuItem container)
					{
						var match = IBuilderTabbedModel<Notification>.GetFileKey.Match(item?.ToString() ?? string.Empty);
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

		private void NotificationsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DataContext is NotifierViewModel vm)
			{
				vm.SelectedEntities.Clear();

				foreach (var item in ((DataGrid)sender).SelectedItems.OfType<Notification>())
				{
					vm.SelectedEntities.Add(item);
				}
			}
		}
	}
}
