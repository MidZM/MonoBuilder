using MonoBuilder.Models.generics.enums;
using MonoBuilder.Models.generics.interfaces;
using MonoBuilder.Models.notification_management;
using MonoBuilder.ViewModels._generic_models;
using MonoBuilder.Views.ViewUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace MonoBuilder.Models.helpers
{
	internal static class TabbedExecutionCommands
	{
		private static ContentManipulator GetDefinedManipulator<TController, TEntity>
			(this IBuilderTabbedContentModel<TController, TEntity> instance, ObservableCollection<IEnumerable<string?>>? data = null)
			where TEntity : IMultiFile, INamedEntity
			where TController : MonoSystem<TEntity>
		{
			return instance.GetManipulator(
				instance.ApplicationSettings,
				instance.DataController.DataModeTypeName,
				new ContentBuilderTemplate()
				{
					Names = instance.GetContentNames(),
					BoxTypes = instance.GetContentBoxTypes(),
					Placeholders = instance.GetPlaceholderValues()
				},
				data);
		}

		internal static void HelperExecuteAddDataCommand<TController, TEntity>
			(this IBuilderTabbedContentModel<TController, TEntity> instance, object? _)
			where TEntity : IMultiFile, INamedEntity
			where TController : MonoSystem<TEntity>
		{
			var manipulator = GetDefinedManipulator(instance);
			manipulator.Owner = instance.Owner;
			manipulator.ShowDialog();

			if (manipulator.DidCloseWithSave == false) return;

			instance.SaveData(manipulator, instance.DataController, instance.GetContentUniqueField());
		}

		internal static void HelperExecuteModifyDataCommand<TController, TEntity>
			(this IBuilderTabbedContentModel<TController, TEntity> instance, object? _)
			where TEntity : IMultiFile, INamedEntity
			where TController : MonoSystem<TEntity>
		{
			string typeName = instance.DataController.DataModeTypeName;
			var folderPath = instance.ApplicationSettings.GetFolderPath(typeName != "Characters" ? typeName: "Assets");
			var filePath = instance.ApplicationSettings.GetAllFilePaths(typeName);

			if (folderPath == null ||
				filePath.Count == 0 ||
				instance.SelectedEntities.Count == 0) return;

			var manipulator = GetDefinedManipulator(instance, instance.GenerateContentFieldData(folderPath));
			manipulator.Owner = instance.Owner;
			manipulator.ShowDialog();

			if (manipulator.DidCloseWithSave == false) return;

			instance.SaveData(manipulator, instance.DataController, instance.GetContentUniqueField(), instance.SelectedEntities);
		}

		internal static void HelperExecuteMoveDataCommand<TController, TEntity>
			(this ITabbedCommandModel<TController, TEntity> instance, object? element)
			where TEntity : IMultiFile, INamedEntity
			where TController : MonoSystem<TEntity>
		{
			if (element is not MenuItem item) return;

			string? selectedItem = item.Header.ToString() ?? null;

			if (instance.SelectedEntities.Count > 0 && selectedItem != null)
			{
				var match = IBuilderTabbedModel<TEntity>.GetFileKey.Match(selectedItem);
				if (match.Success)
				{
					var targetFileKey = match.Groups[1].Value;
					if (targetFileKey != null)
					{
						foreach (TEntity entity in instance.SelectedEntities.ToArray())
						{
							entity.FileKey = targetFileKey;
							instance.DataController.UpdateData(entity.EntityID, entity);
						}
					}
					else
					{
						DialogBox.Show(
							$"{instance.DataController.DataModeTypeName}: The selected file path for {selectedItem} is not set. Please set it before moving entities.",
							"File Path Not Set",
							DialogButtonDefaults.OK,
							DialogIcon.Warning);
					}
				}
			}
		}

		internal static bool HelperExecuteRemoveDataCommand<TController, TEntity>
			(this ITabbedCommandModel<TController, TEntity> instance, object? _)
			where TEntity : IMultiFile, INamedEntity
			where TController : MonoSystem<TEntity>
		{
			if (instance.SelectedEntities.Count == 0) return false;

			var selectedItems = instance.SelectedEntities;
			int maxAmount = 10;
			string phrasing = selectedItems.Count > 1 ? "these entities" : "this entity";
			var entityNames = string.Join("\n", selectedItems
				.Cast<TEntity>()
				.Take(maxAmount)
				.Select(entity => $"- {entity.Name}"));

			if (selectedItems.Count > maxAmount)
			{
				entityNames += $"\n- And {selectedItems.Count - maxAmount} more...";
			}

			var result = DialogBox.Show(
				$"{instance.DataController.DataModeTypeName}: Are you sure you wish to remove {phrasing}?\r\n{entityNames}",
				"Confirm Removal",
				600,
				DialogIcon.Question,
				new DialogButton("From Program", DialogBoxResult.Continue),
				new DialogButton("From Script", DialogBoxResult.Retry),
				new DialogButton("From Both", DialogBoxResult.Yes, "ErrorButton"),
				new DialogButton("Cancel", DialogBoxResult.No, "ErrorButton"));

			if (result == DialogBoxResult.Continue ||
				result == DialogBoxResult.Retry ||
				result == DialogBoxResult.Yes)
			{
				try
				{
					List<TEntity> entitiesToRemove = selectedItems.ToList();
					var shouldRemoveFromScript = result == DialogBoxResult.Retry ||
												 result == DialogBoxResult.Yes;

					var shouldRemoveFromProgram = result == DialogBoxResult.Continue ||
												  result == DialogBoxResult.Yes;

					foreach (TEntity entity in entitiesToRemove)
					{
						if (shouldRemoveFromScript)
						{
							if (instance.DataController.EntityExistsInScript(entity.EntityID))
							{
								instance.DataController.RemoveEntityFromScript(entity.EntityID, false);
							}
						}

						if (shouldRemoveFromProgram)
						{
							instance.DataController.RemoveData(entity.EntityID);
						}
					}
				}
				catch (Exception error)
				{
					DialogBox.Show(
						$"{instance.DataController.DataModeTypeName}: Something went wrong when removing entities!\n\n{error}",
						"Failed to Remove Entities",
						DialogButtonDefaults.OK,
						DialogIcon.Error);
					return false;
				}

				return true;
			}
			return false;
		}

		internal static void HelperExecuteSaveToScriptCommand<TController, TEntity>
			(this ITabbedCommandModel<TController, TEntity> instance, object? _)
			where TEntity : IMultiFile, INamedEntity
			where TController : MonoSystem<TEntity>
		{
			if (instance.SelectedEntities.Count == 0) return;

			var result = DialogBox.Show(
				$"{instance.DataController.DataModeTypeName}: Would you like to update the selected entities, or all entities?",
				"Save Entities To Script",
				DialogIcon.Question,
				new DialogButton("Selected", DialogBoxResult.Continue),
				new DialogButton("All", DialogBoxResult.Yes),
				new DialogButton("Cancel", DialogBoxResult.No, "ErrorButton"));
			if (result == DialogBoxResult.Continue || result == DialogBoxResult.Yes)
			{
				try
				{
					List<(TEntity, bool)> tags = new();
					int maxEntityMerge = 10;
					int currentEntityMerge = 0;
					string mergeDialog = "";
					int index = 0;
					var entities = result == DialogBoxResult.Continue
						? instance.SelectedEntities
						: instance.DataController.DataMode.Collection;

					foreach (TEntity entity in entities)
					{
						var inScript = instance.DataController.EntityExistsInScript(entity.Name, entity.FileKey);

						if (inScript)
						{
							if (currentEntityMerge < maxEntityMerge)
							{
								mergeDialog += $"- {entity.Name}\n";
							}
							currentEntityMerge++;
						}

						tags.Add((entity, inScript));

						if (index == entities.Count - 1)
						{
							if (currentEntityMerge > maxEntityMerge)
							{
								mergeDialog += $"- And {currentEntityMerge - maxEntityMerge} more...";
							}
						}

						index++;
					}

					if (currentEntityMerge > 0)
					{
						if (DialogBox.Show(
							$"{instance.DataController.DataModeTypeName}: Detected entities that already exist within the script.\nWould you like to merge existing entities?\n(Regardless of the answer, any new entities will be added)\n\n{mergeDialog}",
							"Existing Entities Detected",
							DialogButtonDefaults.YesNo,
							DialogIcon.Warning) == DialogBoxResult.No)
						{
							Predicate<(TEntity, bool)> value = tuple => tuple.Item2;
							tags.RemoveAll(value);
						}
					}

					foreach (var (entity, inScript) in tags)
					{
						var content = instance.DataController.ConvertToScriptContent(entity);
						if (inScript)
						{
							instance.DataController.UpdateEntityInScript(entity.Name, content);
						}
						else
						{
							instance.DataController.AddEntityToScript(entity.Name, content);
						}
					}

					DialogBox.Show(
						"Entities successfully saved to script!",
						"Success",
						DialogButtonDefaults.OK,
						DialogIcon.Information);
				}
				catch (Exception error)
				{
					DialogBox.Show(
						$"{instance.DataController.DataModeTypeName}: Something went wrong when adding entities!\n\n{error}",
						"Failed to Add Entities",
						DialogButtonDefaults.OK,
						DialogIcon.Error);
				}
			}
		}

		internal static void HelperExecuteImportDataCommand<TController, TEntity>
			(this ITabbedCommandModel<TController, TEntity> instance, object? _)
			where TEntity : IMultiFile, INamedEntity
			where TController : MonoSystem<TEntity>
		{
			try
			{
				var entityData = instance.DataController.SyncData();
				var duplicates = entityData.Keys
					.Where(name => instance.DataController.ContainsName(name))
					.ToList();

				if (duplicates.Count > 0)
				{
					int selectedCount = duplicates.Count;
					int maxAmount = 10;
					string phrasing = duplicates.Count > 1 ? "multiple notifiers" : "an notifier";
					string names = string.Join("\n", duplicates.Take(maxAmount).Select(c => $"- {c}"));

					if (selectedCount > maxAmount)
					{
						names += $"\n- And {selectedCount - maxAmount} more...";
					}

					var result = DialogBox.Show(
						$"Found {phrasing} with a similar name that already exist...\nDo you want to merge them?\r\n{names}",
						"Confirm Merge Status",
						DialogButtonDefaults.YesNo,
						DialogIcon.Question);

					if (result == DialogBoxResult.Yes)
					{
						foreach (string notifier in duplicates)
						{
							instance.RunDataImportSetup(notifier, entityData);
						}
					}
					else
					{
						foreach (string entity in duplicates)
						{
							entityData.Remove(entity);
						}
					}

					foreach (TEntity entity in entityData.Values)
					{
						entity.IsSynced = true;
						instance.DataController.AddData(entity);
					}
				}
			}
			catch
			{
				return;
			}
		}

		internal static void HelperExecuteExitCommand
			(this IBuilderBaseModel instance, object? _)
		{
			instance.Owner.Close();
		}
	}
}
