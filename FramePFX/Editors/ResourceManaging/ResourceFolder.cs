using System;
using System.Collections.Generic;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.RBC;

namespace FramePFX.Editors.ResourceManaging {
    /// <summary>
    /// A group of resource items
    /// </summary>
    public sealed class ResourceFolder : BaseResource {
        private readonly List<BaseResource> items;

        public IReadOnlyList<BaseResource> Items => this.items;

        public event ResourceAddedEventHandler ResourceAdded;
        public event ResourceRemovedEventHandler ResourceRemoved;
        public event ResourceMovedEventHandler ResourceMoved;

        public ResourceFolder() {
            this.items = new List<BaseResource>();
        }

        public ResourceFolder(string displayName) : this() {
            this.DisplayName = displayName;
        }

        public bool IsNameFree(string name) {
            foreach (BaseResource item in this.items) {
                if (item.DisplayName == name) {
                    return false;
                }
            }

            return true;
        }

        protected internal override void OnAttachedToManager() {
            base.OnAttachedToManager();
            foreach (BaseResource resource in this.items) {
                InternalSetResourceManager(resource, this.Manager);
            }
        }

        protected internal override void OnDetatchedFromManager() {
            base.OnDetatchedFromManager();
            foreach (BaseResource resource in this.items) {
                InternalSetResourceManager(resource, null);
            }
        }

        /// <summary>
        /// Adds the item to this resource folder
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(BaseResource item) {
            this.InsertItem(this.items.Count, item);
        }

        public void InsertItem(int index, BaseResource item) {
            if (item.Parent != null)
                throw new InvalidOperationException("Item already exists in another folder");
            if (index < 0 || index > this.items.Count)
                throw new IndexOutOfRangeException($"Index must not be negative or exceed our items count ({index} < 0 || {index} > {this.items.Count})");
            this.items.Insert(index, item);
            InternalOnItemAdded(item, this);
            this.ResourceAdded?.Invoke(this, item, index);
            if (this.Manager != null && item is ResourceItem && ((ResourceItem) item).UniqueId == ResourceManager.EmptyId)
                throw new Exception("Expected item to be registered");
        }

        public bool RemoveItem(BaseResource item) {
            int index = this.items.IndexOf(item);
            if (index < 0)
                return false;
            this.RemoveItemAt(index);
            return true;
        }

        public void RemoveItemAt(int index) {
            BaseResource item = this.items[index];
            this.items.RemoveAt(index);
            InternalOnItemRemoved(item, this);
            this.ResourceRemoved?.Invoke(this, item, index);
        }

        public void MoveItemTo(ResourceFolder target, BaseResource item) {
            int index = this.items.IndexOf(item);
            if (index == -1)
                throw new InvalidOperationException("Item is not stored in this folder");
            this.MoveItemTo(target, index, target.items.Count);
        }

        public void MoveItemTo(ResourceFolder target, int srcIndex) => this.MoveItemTo(target, srcIndex, target.items.Count);

        public void MoveItemTo(ResourceFolder target, int srcIndex, int dstIndex) {
            BaseResource item = this.items[srcIndex];
            if (target.Manager != null && target.Manager != this.Manager)
                throw new Exception("Target's manager is non-null and different from the current instance");
            this.items.RemoveAt(srcIndex);
            target.items.Insert(dstIndex, item);
            InternalOnItemMoved(item, target);
            ResourceMovedEventArgs args = new ResourceMovedEventArgs(this, target, item, srcIndex, dstIndex);
            this.ResourceMoved?.Invoke(this, args);
            target.ResourceMoved?.Invoke(target, args);
        }

        public bool IsParentInHierarchy(ResourceFolder item, bool startAtThis = true) {
            for (ResourceFolder parent = startAtThis ? this : this.Parent; item != null; item = item.Parent) {
                if (ReferenceEquals(parent, item)) {
                    return true;
                }
            }

            return false;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            RBEList list = data.CreateList("Items");
            foreach (BaseResource item in this.items) {
                list.Add(WriteSerialisedWithType(item));
            }
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            RBEList list = data.GetList("Items");
            foreach (RBEDictionary dictionary in list.Cast<RBEDictionary>()) {
                this.AddItem(ReadSerialisedWithType(dictionary));
            }
        }

        protected override void LoadDataIntoClone(BaseResource clone) {
            base.LoadDataIntoClone(clone);
            ResourceFolder folder = (ResourceFolder) clone;
            foreach (BaseResource child in this.items) {
                folder.AddItem(Clone(child));
            }
        }

        /// <summary>
        /// Clears a folder recursively, while also allowing all resources to be destroyed too
        /// </summary>
        /// <param name="folder">The folder to clear recursively. Null accepted for convenience (e.g. resource as ResourceFolder)</param>
        /// <param name="destroy">True to destroy resources before removing</param>
        public static void ClearHierarchy(ResourceFolder folder, bool destroy = true) {
            if (folder == null)
                return;

            // we iterate back to front as it's far more efficient since it won't result
            // in n-1 copies of all elements when removing from the front of the list.
            // Even if we made 'items' a LinkedList, WPF still uses array based collections
            for (int i = folder.items.Count - 1; i >= 0; i--) {
                BaseResource item = folder.items[i];
                if (destroy)
                    item.Destroy();
                if (item is ResourceFolder)
                    ClearHierarchy((ResourceFolder) item, destroy);
                folder.RemoveItemAt(i);
            }
        }
    }
}