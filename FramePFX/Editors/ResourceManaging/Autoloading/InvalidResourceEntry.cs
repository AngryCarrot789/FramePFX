using System;
using FramePFX.Utils;

namespace FramePFX.Editors.ResourceManaging.Autoloading {
    public delegate void InvalidResourceEntryEventHandler(InvalidResourceEntry entry);

    public abstract class InvalidResourceEntry {
        private string displayName;

        public ResourceItem Resource { get; }

        public ResourceLoader ResourceLoader { get; private set; }

        public string DisplayName {
            get => this.displayName;
            set {
                if (this.displayName == value)
                    return;
                this.displayName = value;
                this.DisplayNameChanged?.Invoke(this);
            }
        }

        public event InvalidResourceEntryEventHandler DisplayNameChanged;

        protected InvalidResourceEntry(ResourceItem resource) {
            this.Resource = resource;
        }

        public bool TryLoad() {
            if (this.ResourceLoader == null) {
                throw new InvalidOperationException("No loader");
            }

            return this.ResourceLoader.TryLoadEntry(this.ResourceLoader.Entries.IndexOf(this));
        }

        internal static void InternalSetLoader(InvalidResourceEntry resource, ResourceLoader loader) {
            resource.ResourceLoader = loader;
        }
    }
}