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
using FramePFX.Utils;

namespace FramePFX.PropertyEditing {
    public delegate void PropertyEditorGroupChildEventHandler(BasePropertyEditorGroup group, BasePropertyEditorObject item, int index);
    public delegate void PropertyEditorGroupChildMovedEventHandler(BasePropertyEditorGroup group, BasePropertyEditorObject item, int oldIndex, int newIndex);

    public abstract class BasePropertyEditorGroup : BasePropertyEditorItem {
        private readonly List<BasePropertyEditorObject> propObjs;
        private string displayName;
        private bool isExpanded = true; // expand by default

        /// <summary>
        /// Gets a read-only collection that contains all of our child <see cref="BasePropertyEditorObject"/> objects
        /// </summary>
        public ReadOnlyCollection<BasePropertyEditorObject> PropertyObjects { get; }

        /// <summary>
        /// Gets or sets this group's display name
        /// </summary>
        public string DisplayName {
            get => this.displayName;
            set {
                if (this.displayName == value)
                    return;
                this.displayName = value;
                this.DisplayNameChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets if this group is expanded or not
        /// </summary>
        public bool IsExpanded {
            get => this.isExpanded;
            set {
                if (this.isExpanded == value)
                    return;
                this.isExpanded = value;
                this.IsExpandedChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets the group type. This should not change during the lifetime of this instance
        /// </summary>
        public GroupType GroupType { get; }

        public bool IsRoot => this.Parent == null;

        public event PropertyEditorGroupChildEventHandler ItemAdded;
        public event PropertyEditorGroupChildEventHandler ItemRemoved;
        public event PropertyEditorGroupChildMovedEventHandler ItemMoved;
        public event BasePropertyEditorItemEventHandler DisplayNameChanged;
        public event BasePropertyEditorItemEventHandler IsExpandedChanged;

        public BasePropertyEditorGroup(Type applicableType, GroupType groupType = GroupType.PrimaryExpander) : base(applicableType) {
            this.propObjs = new List<BasePropertyEditorObject>();
            this.PropertyObjects = this.propObjs.AsReadOnly();
            this.GroupType = groupType;
        }

        protected override void OnPropertyEditorChanged(BasePropertyEditor oldEditor, BasePropertyEditor newEditor) {
            base.OnPropertyEditorChanged(oldEditor, newEditor);
            foreach (BasePropertyEditorObject obj in this.propObjs) {
                SetPropertyEditor(obj, newEditor);
            }
        }

        public void ExpandHierarchy() {
            this.IsExpanded = true;
            foreach (BasePropertyEditorObject obj in this.propObjs) {
                if (obj is BasePropertyEditorGroup group) {
                    group.ExpandHierarchy();
                }
            }
        }

        public void CollapseHierarchy() {
            // probably more performant to expand the top first, so that closing child ones won't cause rendering
            this.IsExpanded = false;
            foreach (BasePropertyEditorObject obj in this.propObjs) {
                if (obj is BasePropertyEditorGroup group) {
                    group.CollapseHierarchy();
                }
            }
        }

        public void AddItem(BasePropertyEditorObject propObj) => this.InsertItem(this.propObjs.Count, propObj);

        public void InsertItem(int index, BasePropertyEditorObject propObj) {
            if (propObj == null)
                throw new ArgumentNullException(nameof(propObj));
            if (!this.IsPropertyEditorObjectAcceptable(propObj))
                throw new ArgumentException("The specific property editor object is not allowed: " + propObj);
            this.propObjs.Insert(index, propObj);
            OnAddedToGroup(propObj, this);
            this.ItemAdded?.Invoke(this, propObj, index);
        }

        public bool RemoveItem(BasePropertyEditorObject propObj) {
            int index = this.propObjs.IndexOf(propObj);
            if (index == -1)
                return false;
            this.RemoveItemAt(index);
            return true;
        }

        public void RemoveItemAt(int index) {
            BasePropertyEditorObject propObj = this.propObjs[index];
            this.propObjs.RemoveAt(index);
            OnRemovedFromGroup(propObj, this);
            this.ItemRemoved?.Invoke(this, propObj, index);
        }

        public void MoveItem(int oldIndex, int newIndex) {
            BasePropertyEditorObject propObj = this.propObjs[oldIndex];
            this.propObjs.MoveItem(oldIndex, newIndex);
            this.ItemMoved?.Invoke(this, propObj, oldIndex, newIndex);
        }

        /// <summary>
        /// Used to determine if calling <see cref="InsertItem"/> or <see cref="AddItem"/> with the given object is allowed or not
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract bool IsPropertyEditorObjectAcceptable(BasePropertyEditorObject obj);
    }
}