using System;
using System.Collections.Generic;

namespace FramePFX.Editors.ResourceManaging.Autoloading {
    public delegate void ResourceLoaderEventHandler(ResourceLoader loader);
    public delegate void ResourceLoaderEntryEventHandler(ResourceLoader loader, InvalidResourceEntry entry, int index);
    /// <summary>
    /// A class used to assist in auto-loading of resource objects, specifically <see cref="ResourceItem"/> objects
    /// </summary>
    public sealed class ResourceLoader {
        private readonly List<InvalidResourceEntry> entries;

        public IReadOnlyList<InvalidResourceEntry> Entries => this.entries;

        public event ResourceLoaderEntryEventHandler EntryAdded;
        public event ResourceLoaderEntryEventHandler EntryRemoved;

        public ResourceLoader() {
            this.entries = new List<InvalidResourceEntry>();
        }

        public void AddEntry(InvalidResourceEntry entry) => this.InsertEntry(entry, this.entries.Count);

        public void InsertEntry(InvalidResourceEntry entry, int index) {
            if (entry.ResourceLoader != null)
                throw new Exception("Resource already added to a resource loader");
            this.entries.Insert(index, entry);
            InvalidResourceEntry.InternalSetLoader(entry, this);
            this.EntryAdded?.Invoke(this, entry, index);
        }

        public bool RemoveEntry(InvalidResourceEntry entry) {
            int index = this.entries.IndexOf(entry);
            if (index == -1)
                return false;
            this.RemoveEntryAt(index);
            return true;
        }

        public void RemoveEntryAt(int index) {
            InvalidResourceEntry entry = this.entries[index];
            InvalidResourceEntry.InternalSetLoader(entry, null);
            this.EntryRemoved?.Invoke(this, entry, index);
        }

        public bool TryLoadEntry(ResourceItem item) {
            int index = this.entries.FindIndex(x => x.Resource == item);
            if (index == -1) {
                throw new InvalidOperationException("Resource is not in this loader");
            }

            return this.TryLoadEntry(index);
        }

        public bool TryLoadEntry(int index) {
            ResourceItem item = this.entries[index].Resource;
            if (item.IsOnline) {
                return true;
            }

            if (item.TryEnableForLoaderEntry(this.entries[index])) {
                this.RemoveEntryAt(index);
                return true;
            }

            return false;
        }
    }
}