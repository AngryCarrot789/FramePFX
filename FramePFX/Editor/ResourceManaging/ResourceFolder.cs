using System;
using System.Collections.Generic;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.RBC;

namespace FramePFX.Editor.ResourceManaging {
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

        public override void OnAttachedToManager() {
            base.OnAttachedToManager();
            foreach (BaseResource resource in this.items) {
                resource.OnAttachedToManager();
            }
        }

        public override void OnDetatchedFromManager() {
            base.OnDetatchedFromManager();
            foreach (BaseResource resource in this.items) {
                resource.OnDetatchedFromManager();
            }
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

        protected override void LoadCloneDataFromObject(BaseResource obj) {
            base.LoadCloneDataFromObject(obj);
            foreach (BaseResource child in ((ResourceFolder) obj).items) {
                this.AddItem(Clone(child));
            }
        }

        public void AddItem(BaseResource item) {
            this.InsertItem(this.items.Count, item);
        }

        public void InsertItem(int index, BaseResource item) {
            if (item.Parent != null)
                throw new InvalidOperationException("Item already exists in another folder");

            if (index < 0 || index > this.items.Count)
                throw new IndexOutOfRangeException($"Index must not be negative or exceed our items count ({index} < 0 || {index} > {this.items.Count})");

            bool isManagerDifferent = !ReferenceEquals(this.Manager, item.Manager);
            bool wasRegistered = false;
            if (isManagerDifferent && item.Manager != null) {
                if (item is ResourceItem resItem && resItem.IsRegistered()) {
                    resItem.Manager.RemoveEntryByItem(resItem);
                    wasRegistered = true;
                }

                item.OnDetatchedFromManager();
                item.manager = null;
            }

            this.items.Insert(index, item);
            InternalSetParent(item, this);
            this.ResourceAdded?.Invoke(this, item, index);
            if (isManagerDifferent && this.Manager != null) {
                item.manager = this.Manager;
                item.OnAttachedToManager();
                if (wasRegistered) {
                    item.Manager.RegisterEntry((ResourceItem) item);
                }
            }
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
            InternalSetParent(item, null);
            this.ResourceRemoved?.Invoke(this, item, index);
        }

        public bool UnregisterAndRemoveItem(BaseResource item) {
            int index = this.items.IndexOf(item);
            if (index == -1)
                return false;
            this.UnregisterAndRemoveItemAt(index);
            return true;
        }

        /// <summary>
        /// Unregisters the item from the <see cref="ResourceManager"/> (if the item is registered), removes the item, and then disposes it
        /// </summary>
        /// <param name="index"></param>
        public void UnregisterAndRemoveItemAt(int index) {
            BaseResource item = this.items[index];
            UnregisterAndDetatch(item as ResourceItem);
            this.RemoveItemAt(index);
        }

        public bool UnregisterRemoveAndDisposeItem(BaseResource item) {
            int index = this.items.IndexOf(item);
            if (index == -1)
                return false;
            this.UnregisterDisposeAndRemoveItemAt(index);
            return true;
        }

        /// <summary>
        /// Unregisters the item from the <see cref="ResourceManager"/> (if the item is registered), disposes it, and then removes it
        /// </summary>
        /// <param name="index">The index of the item being deleted</param>
        public void UnregisterDisposeAndRemoveItemAt(int index) {
            BaseResource item = this.items[index];
            UnregisterAndDetatch(item as ResourceItem);
            item.Dispose();
            this.RemoveItemAt(index);
        }

        private static void UnregisterAndDetatch(ResourceItem item) {
            if (item != null && item.Manager != null) {
                if (item.IsRegistered()) {
                    item.Manager.RemoveEntryByItem(item);
                }

                item.OnDetatchedFromManager();
                item.manager = null;
            }
        }

        public void MoveItemTo(ResourceFolder target, BaseResource item) {
            int index = this.items.IndexOf(item);
            if (index == -1)
                throw new InvalidOperationException("Item is not stored in this folder");
            this.MoveItemTo(target, index, target.items.Count);
        }

        public void MoveItemTo(ResourceFolder target, int srcIndex) {
            this.MoveItemTo(target, srcIndex, target.items.Count);
        }

        public void MoveItemTo(ResourceFolder target, int srcIndex, int dstIndex) {
            BaseResource item = this.items[srcIndex];
            bool isManagerDifferent = !ReferenceEquals(item.Manager, target.Manager);
            if (isManagerDifferent && item.Manager != null) {
                item.OnDetatchedFromManager();
                item.manager = null;
            }

            this.items.RemoveAt(srcIndex);
            target.items.Insert(dstIndex, item);
            InternalSetParent(item, target);
            ResourceMovedEventArgs args = new ResourceMovedEventArgs(this, target, item, srcIndex, dstIndex);
            this.ResourceMoved?.Invoke(args);
            target.ResourceMoved?.Invoke(args);
            if (isManagerDifferent && target.Manager != null) {
                item.manager = target.Manager;
                item.OnAttachedToManager();
            }
        }

        public override void Dispose() {
            base.Dispose();
            for (int i = this.items.Count - 1; i >= 0; i--) {
                this.UnregisterDisposeAndRemoveItemAt(i);
            }
        }

        /// <summary>
        /// Registers all items in the hierarchy of the given resource if it's a group. If
        /// it is just a resource item, then the item is registered. This is a recursive function
        /// </summary>
        /// <param name="manager">The (non-null) manager to register items with</param>
        /// <param name="resource">Target item</param>
        public static void RegisterHierarchy(ResourceManager manager, BaseResource resource) {
            if (resource is ResourceItem) {
                manager.RegisterEntry((ResourceItem) resource);
            }
            else if (resource is ResourceFolder) {
                foreach (BaseResource obj in ((ResourceFolder) resource).items) {
                    RegisterHierarchy(manager, obj);
                }
            }
        }
    }
}