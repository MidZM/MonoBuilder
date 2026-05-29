using MonoBuilder.Commands;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.helpers;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MonoBuilder.Models.generics.interfaces
{
	public interface ITabbedCommandModel<TController, TEntity> : IBuilderTabbedModel<TEntity>
		where TEntity : IMultiFile, INamedEntity
		where TController : MonoSystem<TEntity>
	{
		TController DataController { get; init; }

		RelayCommand MoveDataCommand { get; }
		RelayCommand AddDataCommand { get; }
		RelayCommand ModifyDataCommand { get; }
		RelayCommand RemoveDataCommand { get; }
		RelayCommand SaveToScriptCommand { get; }
		RelayCommand ImportDataCommand { get; }

		internal abstract void RunDataImportSetup(string entity, Dictionary<string, TEntity> entityData);
		internal virtual string GetContentUniqueField() => string.Empty;
		internal virtual List<string> GetContentNames() => new();
		internal virtual List<ContentBoxType> GetContentBoxTypes() => new();
		internal virtual (string, string)[] GetPlaceholderValues() => [];
		internal virtual ObservableCollection<IEnumerable<string?>> GenerateContentFieldData(string folderPath) => new();
	}

	public interface ISpecialTabbedCommandModel<TController, TEntity> : ITabbedCommandModel<TController, TEntity>
		where TEntity : IMultiFile, INamedEntity
		where TController : MonoSystem<TEntity>
	{
		ObservableBoolean IsEditingData { get; set; }
		TEntity? ModifyingData { get; set; }
	}
}
