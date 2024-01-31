using System;
using FramePFX.Editors.ResourceManaging.Autoloading;

namespace FramePFX.Editors.ResourceManaging.Resources {
    public class InvalidMediaPathEntry : InvalidResourceEntry {
        public new ResourceAVMedia Resource => (ResourceAVMedia) base.Resource;

        private string filePath;

        public string FilePath {
            get => this.filePath;
            set {
                if (this.filePath == value)
                    return;
                this.filePath = value;
                this.FilePathChanged?.Invoke(this);
            }
        }

        public event InvalidResourceEntryEventHandler FilePathChanged;

        public InvalidMediaPathEntry(ResourceAVMedia resource, Exception exception) : base(resource) {
        }
    }
}