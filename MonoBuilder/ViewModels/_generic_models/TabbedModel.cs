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
		public Window Owner { get; set; }
		AppSettings IBuilderBaseModel.ApplicationSettings { get => ApplicationSettings; set => ApplicationSettings = value; }
		public AppSettings ApplicationSettings { get; set; }

		public static readonly Regex GetFileKey = new(@"^\((.+?)\)", RegexOptions.Compiled);

		public TabEntries DataTabs { get; set; } = new();
		public CollectionViewSource? DataViewSource { get; set; }

		private ObservableCollection<string> _availableFiles = new();
		private TabEntry? _selectedTab;
		private T? _selectedEntity;
		private ObservableCollection<T> _selectedEntities = new();

		public ObservableCollection<string> AvailableFiles
		{
			get => _availableFiles;
			set
			{
				SetProperty(ref _availableFiles, value);
			}
		}

		public TabEntry? SelectedTab
		{
			get => _selectedTab;
			set
			{
				SetProperty(ref _selectedTab, value);
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
			set
			{
				SetProperty(ref _selectedEntities, value);
			}
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

		// =========================================================================================
		// This doesn't really go here, but since the program really only uses these kinds of tabs
		// alongside Add, Modify, and Remove commands, it tends to be be an okay assumption.
		// =========================================================================================
		// In a future version, this might be moved to an "ExecutionModel<T>", along with the
		// execution commands, to cut down on redundent code.
		// =========================================================================================
		#region Common Execution Methods
		internal bool AddCanExecute(object? _) => AvailableFiles.Count > 0;
		internal bool ModifyOrRemoveCanExecute(object? _) => AvailableFiles.Count > 0 && SelectedEntities.Any();
		#endregion
	}
}
