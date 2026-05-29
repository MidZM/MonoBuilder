using MonoBuilder.Models;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace MonoBuilder.ViewModels._generic_models
{
	public interface ITabbedContentCommandModel<TController, TEntity> : INotifyPropertyChanged
		where TEntity : IMultiFile, INamedEntity
		where TController : MonoSystem<TEntity>
	{
		Window Owner { get; set; }
		AppSettings ApplicationSettings { get; set; }
		TabEntries DataTabs { get; set; }
		CollectionViewSource? DataViewSource { get; set; }
		ObservableCollection<string> AvailableFiles { get; set; }
		TabEntry? SelectedTab { get; set; }
		TEntity? SelectedEntity { get; set; }
		ObservableCollection<TEntity> SelectedEntities { get; set; }

		internal void InitializeAvailableFiles(string type);
		internal void InitializeDataTabs(string type, ObservableCollection<TEntity> dataSource);
		internal void ClearSelectedEntities();
		internal void ApplyDataTabs(string type);
		internal double WindowSizeChanged(int currentWidth, double margin, double borderThickness, int padding = 0);
		string GetDefaultDataFileKey(string type);

		internal bool AddCanExecute(object? parameters = null);
		internal bool ModifyOrRemoveCanExecute(object? parameters = null);
	}

    public abstract class TabbedContentModel<TController, TEntity> : TabbedContentCommandModel<TController, TEntity>
		where TEntity : IMultiFile, INamedEntity
		where TController : MonoSystem<TEntity>
    {
		private readonly ContentBuilderModel<TEntity> _contentModel;

		#region Content Model Properties
		protected ContentManipulator GetManipulator(AppSettings settings, string type, ContentBuilderTemplate templatedElements, ObservableCollection<IEnumerable<string?>>? data = null)
			=> _contentModel.GetManipulator(settings, type, templatedElements, data);
		protected void SaveData(ContentManipulator window, MonoSystem<TEntity> system, string uniqueField, ObservableCollection<TEntity>? content = null)
			=> _contentModel.SaveData(window, system, uniqueField, content);
		protected Dictionary<string, string> ExtractElements(List<FrameworkElement> control)
			=> _contentModel.ExtractElements(control);
		protected string GetRelativePath(string folderPath, string contentIsActive)
			=> _contentModel.GetRelativePath(folderPath, contentIsActive);
		protected override void ApplyContentChanges(ContentManipulator window, List<FrameworkElement> control, int index, TEntity[]? entities = null)
			=> _contentModel.ApplyContentChanges(window, control, index, entities);

		public static Regex GetFileKey => IBuilderTabbedModel<TEntity>.GetFileKey;
		#endregion

		public TabbedContentModel(ContentBuilderModel<TEntity> contentModel)
		{
			_contentModel = contentModel ?? throw new ArgumentNullException(nameof(contentModel));
			_contentModel.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
		}


	}
}
