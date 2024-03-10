// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;

namespace FramePFX.Editors.Collections
{
    /// <summary>
    /// A list class that stores a selection of object, with support for lazy collection of selected items
    /// </summary>
    public class SelectionList<T> where T : class
    {
        private readonly List<T> items;
        private readonly Func<IEnumerable<T>> sourceFunc;
        private bool isListInvalid;

        public IEnumerable<T> Items
        {
            get
            {
                this.EnsureUpdated();
                return this.items;
            }
        }

        public int Count
        {
            get
            {
                this.EnsureUpdated();
                return this.items.Count;
            }
        }

        public SelectionList(Func<IEnumerable<T>> sourceFunc)
        {
            this.sourceFunc = sourceFunc ?? throw new ArgumentNullException(nameof(sourceFunc));
            this.items = new List<T>();
        }

        /// <summary>
        /// Clears the internal collection, leaving the invalid state unmodified
        /// </summary>
        public void Clear()
        {
            this.items.Clear();
        }

        public void Add(T item)
        {
            this.items.Add(item);
        }

        /// <summary>
        /// Gets the index of a specific item within this list
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The index, or -1 if no such item exists</returns>
        public int IndexOf(T item)
        {
            return this.items.IndexOf(item);
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index == -1)
                return false;
            this.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes the item at the specified index, leaving the invalid state unmodified
        /// </summary>
        /// <param name="index">The index of the item to remove</param>
        public void RemoveAt(int index)
        {
            this.items.RemoveAt(index);
        }

        // /// <summary>
        // /// Updates the internal list with the new collection of items
        // /// </summary>
        // /// <param name="source">The source of valid items</param>
        // public void Update(IEnumerable<T> source) {
        //     this.items.Clear();
        //     this.items.AddRange(source);
        //     this.isListInvalid = false;
        // }

        /// <summary>
        /// Sets the invalid state to true, meaning the internal collection cannot be relied
        /// upon to be accurate, meaning the items will be required when requested
        /// </summary>
        public void Invalidate()
        {
            this.isListInvalid = true;
        }

        /// <summary>
        /// Ensures that the internal collection is up to date/not invalid
        /// </summary>
        /// <returns>
        /// The 'is now updated' state. False means it the list was already valid,
        /// true means it was invalid and is now valid
        /// </returns>
        public bool EnsureUpdated()
        {
            if (!this.isListInvalid)
            {
                return false;
            }

            this.UpdateList();
            return true;
        }

        private void UpdateList()
        {
            this.items.Clear();
            this.items.AddRange(this.sourceFunc());
            this.isListInvalid = false;
        }
    }
}