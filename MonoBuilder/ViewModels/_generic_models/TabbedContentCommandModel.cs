using MonoBuilder.Commands;
using MonoBuilder.Models;
using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.helpers;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MonoBuilder.ViewModels._generic_models
{
	public abstract class TabbedContentCommandModel<TController, TEntity> : TabbedModel<TEntity>, ITabbedCommandModel<TController, TEntity>, IBuilderTabbedContentModel<TController, TEntity>
		where TEntity : IMultiFile, INamedEntity
		where TController : MonoSystem<TEntity>
	{
		public abstract TController DataController { get; init; }

		public RelayCommand MoveDataCommand { get; }

		public RelayCommand AddDataCommand { get; }
		public RelayCommand ModifyDataCommand { get; }
		public RelayCommand RemoveDataCommand { get; }
		public RelayCommand SaveToScriptCommand { get; }
		public RelayCommand ImportDataCommand { get; }

		public RelayCommand ExitCommand { get; }

		public TabbedContentCommandModel()
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
		}

		#region Special Utility Methods
		void ITabbedCommandModel<TController, TEntity>.RunDataImportSetup(string entity, Dictionary<string, TEntity> entityData)
			=> RunDataImportSetup(entity, entityData);
		string ITabbedCommandModel<TController, TEntity>.GetContentUniqueField()
			=> GetContentUniqueField();
		List<string> ITabbedCommandModel<TController, TEntity>.GetContentNames()
			=> GetContentNames();
		List<ContentBoxType> ITabbedCommandModel<TController, TEntity>.GetContentBoxTypes()
			=> GetContentBoxTypes();
		(string, string)[] ITabbedCommandModel<TController, TEntity>.GetPlaceholderValues()
			=> GetPlaceholderValues();
		ObservableCollection<IEnumerable<string?>> ITabbedCommandModel<TController, TEntity>.GenerateContentFieldData(string folderPath)
			=> GenerateContentFieldData(folderPath);

		ContentManipulator IBuilderTabbedContentModel<TController, TEntity>.GetManipulator(AppSettings settings, string type, ContentBuilderTemplate templatedElements, ObservableCollection<IEnumerable<string?>>? data)
		{
			List<Dictionary<string, string?>>? dataParams = data != null
				? templatedElements.GetCombination(data)
				: null;

			return new ContentManipulator(settings, type, new ContentTemplate(
				columnHeaders: templatedElements.Names,
				contentBoxes: templatedElements.BoxTypes,
				placeholders: templatedElements.Placeholders.ToDictionary()),
				dataParams);
		}

		void IBuilderTabbedContentModel<TController, TEntity>.SaveData(ContentManipulator window, MonoSystem system, string uniqueField, ObservableCollection<TEntity>? content)
		{
			var tags = new List<string>();
			window.DynamicRows.Values.ToList().ForEach(row =>
			{
				var tagElement = row.FirstOrDefault(e => e is TextBox tb && tb.Name.EndsWith(uniqueField));
				if (tagElement is TextBox tagBox)
				{
					tags.Add(tagBox.Text);
				}
			});

			// Check for duplicate tags
			var duplicateTags = tags
				.GroupBy(t => t)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();
			if (duplicateTags.Count > 0)
			{
				var duplicateList = string.Join("\n", duplicateTags.Select(t => $"- {t}"));
				DialogBox.Show(
					$"Duplicate tags found:\n{duplicateList}\n\nPlease ensure all images have unique tags.",
					"Duplicate Tags Detected",
					DialogButtonDefaults.OK,
					DialogIcon.Warning);
				return;
			}

			var rowArray = window.DynamicRows.Values.ToArray();
			for (int i = 0; i < rowArray.Length; i++)
			{
				ApplyContentChanges(window, rowArray[i], i, content?.ToArray());
			}

			system.SaveData(DataController._SaveString);
		}

		Dictionary<string, string> IBuilderTabbedContentModel<TController, TEntity>.ExtractElements(List<FrameworkElement> control)
		{
			Dictionary<string, string> elements = [];
			foreach (var element in control)
			{
				if (element is TextBox textBox)
				{
					var split = textBox.Name.Split('_');
					var lastElement = split[split.Length - 1];
					elements.Add(lastElement, textBox.Text);
				}
				else if (element is ComboBox comboBox)
				{
					var split = comboBox.Name.Split('_');
					var lastElement = split[split.Length - 1];

					var files = (ObservableCollection<string>)comboBox.Tag;
					var selectedTag = files[comboBox.SelectedIndex];

					elements.Add(lastElement, selectedTag);
				}
				else if (element is Button button)
				{
					var split = button.Name.Split('_');
					var lastElement = split[split.Length - 1];

					var textContent = (string)button.Tag;
					elements.Add(lastElement, textContent);
				}
			}

			return elements;
		}

		string IBuilderTabbedContentModel<TController, TEntity>.GetRelativePath(string folderPath, string contentIsActive)
		{
			var userProfile = contentIsActive;
			string? result = Path.GetRelativePath(userProfile, folderPath).Replace('\\', '/');

			return result;
		}


		protected abstract void RunDataImportSetup(string entity, Dictionary<string, TEntity> entityData);
		protected abstract string GetContentUniqueField();
		protected abstract List<string> GetContentNames();
		protected abstract List<ContentBoxType> GetContentBoxTypes();
		protected abstract (string, string)[] GetPlaceholderValues();
		protected abstract ObservableCollection<IEnumerable<string?>> GenerateContentFieldData(string folderPath);
		protected abstract void ApplyContentChanges(ContentManipulator window, List<FrameworkElement> control, int index, TEntity[]? entities = null);
		#endregion

		#region Execution Commands
		protected void ExecuteMoveDataCommand(object? element) => this.HelperExecuteMoveDataCommand(element);

		protected void ExecuteAddDataCommand(object? parameters = null) => this.HelperExecuteAddDataCommand(parameters);

		protected void ExecuteModifyDataCommand(object? parameters = null) => this.HelperExecuteModifyDataCommand(parameters);

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
