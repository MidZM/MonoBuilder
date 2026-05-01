using MonoBuilder.Utils.image_management;
using MonoBuilder.Utils.interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MonoBuilder.Utils
{
    public static class CollectionExtensions
    {
        public static void RemoveAll<T>(this ObservableCollection<T> collection,
                                        Predicate<T> match)
        {
            if (collection == null || match == null)
                return;

            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (match(collection[i]))
                {
                    collection.RemoveAt(i);
                }
            }
        }

        public static void RemoveByIds<T>(this ObservableCollection<T> collection,
                                          HashSet<int> idsToRemove)
            where T : INamedEntity
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (idsToRemove.Contains(collection[i].EntityID))
                {
                    collection.RemoveAt(i);
                }
            }
        }
    }
}
