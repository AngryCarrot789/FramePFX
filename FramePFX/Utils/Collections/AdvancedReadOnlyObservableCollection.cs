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
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FramePFX.Utils.Collections
{
    public class AdvancedReadOnlyObservableCollection<T> : ReadOnlyObservableCollection<T>
    {
        private readonly List<Action> multiOperationHandlerList;
        private bool isProcessingMultiOperation;
        private bool isProcessingCollectionChanged;

        public AdvancedReadOnlyObservableCollection(ObservableCollection<T> list) : base(list)
        {
            this.multiOperationHandlerList = new List<Action>();
        }

        /// <summary>
        /// Sets up the state of this collection to allow processing of multiple added, removed, moved, etc. objects once those operations are completed
        /// </summary>
        public void BeginMultiOperation()
        {
            if (this.isProcessingMultiOperation || this.multiOperationHandlerList.Count > 0)
            {
                throw new Exception(nameof(this.FinishMultiOperation) + " was not previously called");
            }

            this.multiOperationHandlerList.Clear();
            this.isProcessingMultiOperation = true;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (this.isProcessingMultiOperation)
            {
                this.isProcessingCollectionChanged = true;
                base.OnCollectionChanged(args);
                this.isProcessingCollectionChanged = false;
            }
            else
            {
                base.OnCollectionChanged(args);
                this.ProcessOperationList();
            }
        }

        public void FinishMultiOperation()
        {
            this.CheckProcessingMultiOperation();
            this.isProcessingMultiOperation = false;
            this.ProcessOperationList();
        }

        public void AddMultiOperationCompletionHandler(Action action)
        {
            if (!this.isProcessingCollectionChanged)
                throw new InvalidOperationException("Not currently processing a collection change event");
            if (!this.multiOperationHandlerList.Contains(action))
                this.multiOperationHandlerList.Add(action);
        }

        private void ProcessOperationList()
        {
            using (ErrorList list = new ErrorList("One or more multi-operation completion handlers threw an exception"))
            {
                foreach (Action handler in this.multiOperationHandlerList)
                {
                    try
                    {
                        handler();
                    }
                    catch (Exception e)
                    {
                        list.Add(e);
                    }
                }

                this.multiOperationHandlerList.Clear();
            }
        }

        private void CheckProcessingMultiOperation()
        {
            if (!this.isProcessingMultiOperation)
                throw new InvalidOperationException("Not already processing multi operation");
        }
    }
}