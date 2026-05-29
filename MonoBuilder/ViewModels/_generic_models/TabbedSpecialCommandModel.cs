using MonoBuilder.Commands;
using MonoBuilder.Models;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.helpers;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Windows.Controls;

namespace MonoBuilder.ViewModels._generic_models
{
	public abstract class TabbedSpecialCommandModel<TController, TEntity> : TabbedModel<TEntity>, ISpecialTabbedCommandModel<TController, TEntity>
		where TEntity : IMultiFile, INamedEntity
		where TController : MonoSystem<TEntity>
	{
		public abstract required TController DataController { get; init; }

		public ObservableBoolean IsEditingData { get; set; } = new();
		public TEntity? ModifyingData { get; set; }

		public RelayCommand MoveDataCommand { get; }

		public RelayCommand AddDataCommand { get; }
		public RelayCommand ModifyDataCommand { get; }
		public RelayCommand RemoveDataCommand { get; }
		public RelayCommand SaveToScriptCommand { get; }
		public RelayCommand ImportDataCommand { get; }

		public RelayCommand ExitCommand { get; }

		public TabbedSpecialCommandModel()
		{
			MoveDataCommand = new(ExecuteMoveDataCommand);
			ExitCommand = new(ExecuteExitCommand, ExitCanExecute);

			AddDataCommand = new(ExecuteAddDataCommand, AddCanExecute);
			ModifyDataCommand = new(ExecuteModifyDataCommand, ModifyOrRemoveCanExecute);
			RemoveDataCommand = new(ExecuteRemoveDataCommand, ModifyOrRemoveCanExecute);
			SaveToScriptCommand = new(ExecuteSaveToScriptCommand, AddCanExecute);
			ImportDataCommand = new(ExecuteImportDataCommand, AddCanExecute);

			SelectedEntities.CollectionChanged += (s, e) =>
			{
				ModifyDataCommand.RaiseCanExecuteChanged();
				RemoveDataCommand.RaiseCanExecuteChanged();
			};

			IsEditingData.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(IsEditingData.Value))
				{
					ExitCommand.RaiseCanExecuteChanged();
				}
			};
		}

		#region Special Utility Methods
		protected abstract void FillDataEditors();
		protected abstract void ClearDataEditors();

		void ITabbedCommandModel<TController, TEntity>.RunDataImportSetup(string entity, Dictionary<string, TEntity> entityData)
			=> RunDataImportSetup(entity, entityData);

		protected virtual void RunDataImportSetup(string entity, Dictionary<string, TEntity> entityData)
		{}
		#endregion

		#region Execution Commands
		protected void ExecuteMoveDataCommand(object? element) => this.HelperExecuteMoveDataCommand(element);

		protected void ExecuteAddDataCommand(object? parameters = null)
		{
			ClearDataEditors();
			IsEditingData.Value = true;
		}

		protected void ExecuteModifyDataCommand(object? parameters = null)
		{
			if (SelectedEntity == null) return;

			ClearDataEditors();
			FillDataEditors();

			IsEditingData.Value = true;
			ModifyingData = SelectedEntity;
		}

		protected void ExecuteRemoveDataCommand(object? parameters = null)
		{
			if (this.HelperExecuteRemoveDataCommand(parameters))
			{
				DataController.SaveData(DataController._SaveString);
			}
		}

		protected void ExecuteSaveToScriptCommand(object? parameters = null) => this.HelperExecuteSaveToScriptCommand(parameters);

		protected void ExecuteImportDataCommand(object? parameters = null) => this.HelperExecuteImportDataCommand(parameters);

		protected void ExecuteExitCommand(object? parameters = null) => this.HelperExecuteExitCommand(parameters);

		protected virtual bool ExitCanExecute(object? parameters = null) => true;
		internal bool AddCanExecute(object? _) => AvailableFiles.Count > 0;
		internal bool ModifyOrRemoveCanExecute(object? _) => AvailableFiles.Count > 0 && SelectedEntities.Any();
		#endregion
	}
}
