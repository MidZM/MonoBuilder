using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonoBuilder.Utils
{
    public class LabelRepository
    {
        private readonly Dictionary<string, OrderedDictionary<string, LoadedLabel>> _labelsByFile = [];

        public ObservableCollection<LoadedLabel> Items { get; } = [];
        public IEnumerable<string> FileKeys => _labelsByFile.Keys;

        public bool HasFile(string fileKey) => _labelsByFile.ContainsKey(fileKey);

        public OrderedDictionary<string, LoadedLabel> GetFile(string fileKey, bool createIfMissing = true)
        {
            if (_labelsByFile.TryGetValue(fileKey, out var labels))
                return labels;

            if (!createIfMissing)
                return [];

            labels = [];
            _labelsByFile[fileKey] = labels;
            return labels;
        }

        public IEnumerable<string> GetKeys(string fileKey)
        {
            if (!_labelsByFile.TryGetValue(fileKey, out var labels))
                return [];

            return labels.Keys;
        }

        public IEnumerable<LoadedLabel> GetItems(string fileKey)
        {
            if (!_labelsByFile.TryGetValue(fileKey, out var labels))
                return [];

            return labels.Values;
        }

        public bool Contains(string fileKey, string labelName) =>
            _labelsByFile.TryGetValue(fileKey, out var labels) && labels.ContainsKey(labelName);

        public bool TryGetValue(string fileKey, string labelName, out LoadedLabel? label)
        {
            if (_labelsByFile.TryGetValue(fileKey, out var labels) && labels.TryGetValue(labelName, out label))
                return true;

            label = null;
            return false;
        }

        public void ReplaceFile(string fileKey, OrderedDictionary<string, LoadedLabel> labels)
        {
            foreach (var label in labels.Values)
                label.FileKey = fileKey;

            _labelsByFile[fileKey] = labels;
            RebuildItems();
        }

        /// <summary>
        /// Adds a new label or updates an existing label associated with the specified file key.
        /// </summary>
        /// <remarks>If a label with the same name already exists for the specified file key, it is
        /// replaced with the provided label. Otherwise, the label is added as a new entry. The method updates both the
        /// internal label collection and the Items list to reflect the change.</remarks>
        /// <param name="fileKey">The unique identifier of the file to which the label belongs. Cannot be null or empty.</param>
        /// <param name="label">The label to add or update. The label's name is used as the key within the file's label collection. Cannot
        /// be null.</param>
        public void Upsert(string fileKey, LoadedLabel label)
        {
            label.FileKey = fileKey;
            var labels = GetFile(fileKey);

            if (labels.TryGetValue(label.Name, out var existing))
            {
                labels[label.Name] = label;
                int index = Items.IndexOf(existing);
                if (index >= 0)
                    Items[index] = label;
                else
                    RebuildItems();
                return;
            }

            labels[label.Name] = label;
            Items.Add(label);
        }

        public bool Remove(string fileKey, string labelName)
        {
            if (!TryGetValue(fileKey, labelName, out var label) || label == null)
                return false;

            _labelsByFile[fileKey].Remove(labelName);
            Items.Remove(label);
            return true;
        }

        public void Clear()
        {
            _labelsByFile.Clear();
            Items.Clear();
        }

        public IEnumerable<(string FileKey, LoadedLabel Label)> Enumerate()
        {
            foreach (var pair in _labelsByFile)
            {
                foreach (var label in pair.Value.Values)
                    yield return (pair.Key, label);
            }
        }

        private void RebuildItems()
        {
            Items.Clear();
            foreach (var pair in _labelsByFile)
            {
                foreach (var label in pair.Value.Values)
                    Items.Add(label);
            }
        }
    }
}
