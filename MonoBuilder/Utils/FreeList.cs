using System;
using System.Collections.Generic;
using System.Text;

namespace MonoBuilder.Utils
{
    public class FreeList<T>
    {
        private Dictionary<int, T> _storage = new();
        private PriorityQueue<int, int> _emptySlots = new();
        private int _nextNewSlot = 0;

        public int Add(T item)
        {
            int index = _emptySlots.Count > 0
                ? _emptySlots.Dequeue()
                : _nextNewSlot++;

            _storage[index] = item;
            return index;
        }

        public bool Insert(int index, T item)
        {
            if (_storage.TryGetValue(index, out T? value) && value != null)
            {
                value = item;
                return true;
            }

            return false;
        }

        public bool Remove(T item)
        {
            var itemIndex = _storage.FirstOrDefault(e => e.Value != null && e.Value.Equals(item)).Key;

            if (_storage.Remove(itemIndex))
            {
                _emptySlots.Enqueue(itemIndex, itemIndex);
                return true;
            }

            return false;
        }

        public bool RemoveAt(int index)
        {
            if (_storage.Remove(index))
            {
                _emptySlots.Enqueue(index, index);
                return true;
            }

            return false;
        }

        public int PeekIndex()
        {
            if (_emptySlots.Count > 0)
            {
                return _emptySlots.Peek();
            }

            return _nextNewSlot;
        }

        public T? Peek(T item)
        {
            return _storage.FirstOrDefault(e => e.Value != null && e.Value.Equals(item)).Value;
        }

        public T? PeekAt(int index)
        {
            _storage.TryGetValue(index, out T? value);
            return value;
        }
    }
}
