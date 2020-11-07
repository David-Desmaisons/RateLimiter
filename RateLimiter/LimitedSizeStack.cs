using System.Collections.Generic;

namespace RateLimiter
{
    /// <summary>
    /// LinkedList with a limited size
    /// If the size exceeds the limit older entry are removed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedSizeStack<T>: LinkedList<T>
    {
        private readonly int _MaxSize;

        /// <summary>
        /// Construct the LimitedSizeStack with the given limit
        /// </summary>
        /// <param name="maxSize"></param>
        public LimitedSizeStack(int maxSize)
        {
            _MaxSize = maxSize;
        }

        /// <summary>
        /// Push new entry. If he size exceeds the limit, the oldest entry is removed
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            AddFirst(item);

            if (Count > _MaxSize)
                RemoveLast();
        }

        /// <summary>
        /// Removes the last occurrence of the specified value from the System.Collections.Generic.LinkedList`1.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="newValue">The new value.</param>
        public bool ReplaceLast(T currentValue, T newValue)
        {
            LinkedListNode<T> node = FindLast(currentValue);
            if (node == null)
                return false;
            node.Value = newValue;
            return true;
        }
    }
}
