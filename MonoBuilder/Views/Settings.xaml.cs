using Microsoft.Win32;
using MonoBuilder.Models;
using MonoBuilder.Models.character_management;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.helpers;
using MonoBuilder.ViewModels.SettingsModel;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MonoBuilder.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
	{
		public Settings(Characters characters, AppSettings settings, ScriptConversion converter)
        {
			SettingsViewModel settingsViewModel = new()
			{
				DataController = characters,
				ApplicationSettings = settings,
				Converter = converter,
				Owner = this
			};
            DataContext = settingsViewModel;

			// Run initializations watcher after the fact so that the Owner has a chance to initialize properly.
			settingsViewModel.RunInitializations();

            InitializeComponent();
        }

		private void CharactersDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                var scrollViewer = Helpers.GetScrollViewer(CharactersDataGrid);
                var canScroll = Helpers.ElementCanScroll(scrollViewer, e.Delta, true);

                if (!canScroll)
                {
                    e.Handled = true;
                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = sender
                    };

                    // Find the parent and raise the event there
                    var parent = ((Control)sender).Parent as UIElement;
                    parent?.RaiseEvent(eventArg);
                }
            }
        }

		private void ContextMoveCharacter_SubmenuOpened(object sender, RoutedEventArgs e)
        {
			var context = DataContext as SettingsViewModel;
            var selectedCharacters = CharactersDataGrid.SelectedItems.Cast<Character>().ToList();
            var defaultFileKey = context?.GetDefaultDataFileKey("Characters");
            string? commonFileKey =
                (selectedCharacters.Select(c => string.IsNullOrEmpty(c.FileKey)
                ? defaultFileKey
                : c.FileKey).Distinct().Count() == 1)
                    ? selectedCharacters.First().FileKey
                    : null;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in ContextMoveCharacter.Items)
                {
                    if (ContextMoveCharacter.ItemContainerGenerator.ContainerFromItem(item) is MenuItem container)
                    {
                        var match = IBuilderTabbedModel<Character>.GetFileKey.Match(item?.ToString() ?? "");
                        container.Icon = match.Success && commonFileKey != null && match.Groups[1].Value == commonFileKey
                            ? new TextBlock {
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

		private void CharactersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DataContext is SettingsViewModel vm)
			{
				vm.SelectedEntities.Clear();

				foreach (var item in ((DataGrid)sender).SelectedItems.OfType<Character>())
				{
					vm.SelectedEntities.Add(item);
				}
			}
		}
	}
}
