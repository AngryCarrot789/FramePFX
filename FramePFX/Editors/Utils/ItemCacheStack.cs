using System;
using System.Collections.Generic;

namespace FramePFX.Editors.Utils {
    /// <summary>
    /// A class used to cache and reuse objects
    /// </summary>
    /// <typeparam name="T">The type of object to cache</typeparam>
    public sealed class ItemCacheStack<T>{
        private readonly Stack<T> cache;

        public int Count => this.cache.Count;

        public int Limit { get; }

        public ItemCacheStack(int limit = 32) {
            if (limit < 0)
                throw new ArgumentOutOfRangeException(nameof(limit));
            this.Limit = limit;
            this.cache = new Stack<T>();
        }

        public bool Push(T item) {
            if (this.cache.Count < this.Limit) {
                this.cache.Push(item);
                return true;
            }

            return false;
        }

        public bool TryPop(out T control) {
            if (this.cache.Count > 0) {
                control = this.cache.Pop();
                return true;
            }

            control = default;
            return false;
        }

        public T Pop() => this.cache.Pop();

        public T Pop(T def) => this.Count > 0 ? this.cache.Pop() : def;

        public void Clear() => this.cache.Clear();
    }
}