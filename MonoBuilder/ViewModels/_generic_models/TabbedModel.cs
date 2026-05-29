using MonoBuilder.Commands;
using MonoBuilder.Models;
using MonoBuilder.Models.generics.interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace MonoBuilder.ViewModels._generic_models
{
    public class TabbedModel<T> : BaseViewModel, IBuilderTabbedModel<T> where T : IMultiFile
    {
		public required Window Owner { get; set; }
		public required AppSettings ApplicationSettings { get; set; }

		private TabEntries _dataTabs = new();
		private ObservableCollection<string> _availableFiles = new();
		private TabEntry? _selectedTab;
		private T? _selectedEntity;
		private ObservableCollection<T> _selectedEntities = new();
		private CollectionViewSource? _dataViewSource;

		public TabEntries DataTabs
		{
			get => _dataTabs;
			set => SetProperty(ref _dataTabs, value);
		}

		public ObservableCollection<string> AvailableFiles
		{
			get => _availableFiles;
			set => SetProperty(ref _availableFiles, value);
		}

		public TabEntry? SelectedTab
		{
			get => _selectedTab;
			set
			{
				SetProperty(ref _selectedTab, value);

				if (_selectedTab?.Type != null)
				{
					ApplyDataTabs(_selectedTab.Type);
				}
			}
		}

		private TimeSpan DebounceDuration { get; } = TimeSpan.FromMilliseconds(400);
		private DateTime LastClickedTime { get; set; } = DateTime.MinValue;
		public T? SelectedEntity
		{
			get => _selectedEntity;
			set
			{
				// Currently, this isn't working... figure it out later.
				//if (_selectedEntity == value)
				//{
				//	DateTime currentTime = DateTime.UtcNow;

				//	if (currentTime - LastClickedTime > DebounceDuration)
				//	{
				//		LastClickedTime = currentTime;
				//		ClearSelectedEntities();
				//	}
				//}

				SetProperty(ref _selectedEntity, value);
			}
		}
		public ObservableCollection<T> SelectedEntities
		{
			get => _selectedEntities;
			set => SetProperty(ref _selectedEntities, value);
		}
		public CollectionViewSource? DataViewSource
		{
			get => _dataViewSource;
			set => SetProperty(ref _dataViewSource, value);
		}

		#region Initialization Methods
		internal void InitializeAvailableFiles(string type)
		{
			AvailableFiles.Clear();

			foreach (var file in ApplicationSettings.GetAllFiles(type))
			{
				var value = $"({file.Key}) {file.Value}";
				AvailableFiles.Add(value);
			}
		}

		internal void InitializeDataTabs(string type, ObservableCollection<T> dataSource)
		{
			var tabs = new List<TabEntry>()
			{
				new($"All {type}") { Tag = "All", Type = type }
			};

			var files = ApplicationSettings.GetAllFilePaths(type)
				.OrderBy(entry => entry.Key)
				.ToList();

			foreach (var (fileKey, filePath) in files)
			{
				var header = Path.GetFileNameWithoutExtension(filePath);
				if (string.IsNullOrWhiteSpace(header))
					header = fileKey;

				tabs.Add(new(header) { Tag = fileKey, Type = type });
			}

			DataTabs = new(tabs);

			DataViewSource = new()
			{
				Source = dataSource
			};

			SelectedTab = DataTabs.Tabs.First();
			ApplyDataTabs(type);
		}
		#endregion

		#region Utility Methods
		internal void ClearSelectedEntities()
		{
			SelectedEntities.Clear();
			SelectedEntity = default(T);
		}

		internal void ApplyDataTabs(string type)
		{
			if (DataViewSource?.View == null)
				return;

			var selectedKey = SelectedTab?.Tag;
			var defaultFileKey = GetDefaultDataFileKey(type);

			if (string.IsNullOrWhiteSpace(selectedKey) || selectedKey == "All")
			{
				DataViewSource.View.Filter = null;
				DataViewSource.View.Refresh();
				return;
			}

			DataViewSource.View.Filter = item =>
			{
				if (item is not IMultiFile entity)
					return false;

				if (entity.FileKey == selectedKey)
					return true;

				return string.IsNullOrEmpty(entity.FileKey) && selectedKey == defaultFileKey;
			};

			DataViewSource.View.Refresh();
			ClearSelectedEntities();
		}

		public string GetDefaultDataFileKey(string type)
		{
			return ApplicationSettings.GetAllFilePaths(type)
				.OrderBy(e => e.Key)
				.Select(e => e.Key)
				.FirstOrDefault() ?? string.Empty;
		}

		internal double WindowSizeChanged(int currentWidth, double margin, double border, int padding = 0)
		{
			var resizeBorder = (SystemParameters.WindowResizeBorderThickness.Left +
								SystemParameters.WindowNonClientFrameThickness.Left) * 2;

			var occupied =	SystemParameters.VerticalScrollBarWidth
							+ margin + border
							+ resizeBorder + padding;

			return currentWidth - occupied;
		}
		#endregion
	}
}
