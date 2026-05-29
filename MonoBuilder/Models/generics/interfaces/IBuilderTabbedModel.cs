using MonoBuilder.ViewModels;
using MonoBuilder.ViewModels._generic_models;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace MonoBuilder.Models.generics.interfaces
{
    public interface IBuilderTabbedModel<T> : IBuilderBaseModel where T : IMultiFile
	{
		ObservableCollection<string> AvailableFiles { get; set; }

		TabEntries DataTabs { get; set; }
		abstract TabEntry? SelectedTab { get; set; }
		abstract T? SelectedEntity { get; set; }
		abstract ObservableCollection<T> SelectedEntities { get; set; }
		CollectionViewSource? DataViewSource { get; set; }

		static readonly Regex GetFileKey = new(@"^\((.+?)\)", RegexOptions.Compiled);

		abstract string GetDefaultDataFileKey(string type);
	}

	public interface IBuilderTabbedContentModel<TController, TEntity> : ITabbedCommandModel<TController, TEntity>
		where TEntity : IMultiFile, INamedEntity
		where TController : MonoSystem<TEntity>
	{
		ContentManipulator GetManipulator(AppSettings settings, string type, ContentBuilderTemplate templatedElements, ObservableCollection<IEnumerable<string?>>? data = null);
		void SaveData(ContentManipulator window, MonoSystem system, string uniqueField, ObservableCollection<TEntity>? content = null);
		Dictionary<string, string> ExtractElements(List<FrameworkElement> control);
		string GetRelativePath(string folderPath, string contentIsActive);
	}
}
