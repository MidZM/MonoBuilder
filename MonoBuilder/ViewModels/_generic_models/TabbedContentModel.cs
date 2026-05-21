using MonoBuilder.Models;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace MonoBuilder.ViewModels._generic_models
{
    public abstract class TabbedContentModel<T> : ContentBuilderModel<T>
		where T : IMultiFile, INamedEntity
    {
		private readonly TabbedModel<T> _tabbedModel;

		#region Tabbed Model Properties
		public required Window Owner
		{ get => _tabbedModel.Owner; set { _tabbedModel.Owner = value; } }

		public AppSettings ApplicationSettings
		{ get => _tabbedModel.ApplicationSettings; set { _tabbedModel.ApplicationSettings = value; } }

		public static Regex GetFileKey => TabbedModel<T>.GetFileKey;

		public TabEntries DataTabs
		{ get => _tabbedModel.DataTabs; set { _tabbedModel.DataTabs = value; } }

		public CollectionViewSource? DataViewSource
		{ get => _tabbedModel.DataViewSource; set { _tabbedModel.DataViewSource = value; } }

		public ObservableCollection<string> AvailableFiles
		{ get => _tabbedModel.AvailableFiles; set { _tabbedModel.AvailableFiles = value; } }

		public TabEntry? SelectedTab
		{ get => _tabbedModel.SelectedTab; set { _tabbedModel.SelectedTab = value; } }

		public T? SelectedEntity
		{ get => _tabbedModel.SelectedEntity; set { _tabbedModel.SelectedEntity = value; } }

		public ObservableCollection<T> SelectedEntities
		{ get => _tabbedModel.SelectedEntities; set { _tabbedModel.SelectedEntities = value; } }
		#endregion

		public TabbedContentModel(TabbedModel<T> tabbedModel)
		{
			_tabbedModel = tabbedModel ?? throw new ArgumentNullException(nameof(tabbedModel));
			_tabbedModel.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
		}

		#region Tabbed Model Methods
		protected void InitializeAvailableFiles(string type)
			=> _tabbedModel.InitializeAvailableFiles(type);
		protected void InitializeDataTabs(string type, ObservableCollection<T> dataSource)
			=> _tabbedModel.InitializeDataTabs(type, dataSource);
		protected void ApplyDataTabs(string type)
			=> _tabbedModel.ApplyDataTabs(type);
		protected double WindowSizeChanged(int currentWidth, double margin, double borderThickness, int padding = 0)
			=> _tabbedModel.WindowSizeChanged(currentWidth, margin, borderThickness, padding);
		public string GetDefaultDataFileKey(string type)
			=> _tabbedModel.GetDefaultDataFileKey(type);


		// =========================================================================================
		// As stated on "TabbedModel<T>," this doesn't really go here, but it can be assumed to be
		// needed due to the very high likelyhood that this type of tabbed model will contain Add,
		// Modify, and Remove methods.
		// =========================================================================================
		protected bool AddCanExecute(object? parameters = null) => _tabbedModel.AddCanExecute(parameters);
		protected bool ModifyOrRemoveCanExecute(object? parameters = null) => _tabbedModel.ModifyOrRemoveCanExecute(parameters);
		#endregion
	}
}
