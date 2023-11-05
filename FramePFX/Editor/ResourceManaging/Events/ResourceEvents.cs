using System;

namespace FramePFX.Editor.ResourceManaging.Events {
    public delegate void ResourceEventHandler(BaseResource resource);

    public delegate void ResourceModifiedEventHandler(BaseResource item, string property);
    public delegate void ResourceReferencedEventHandler(ResourceItem item, ResourcePath reference, bool added);

    public delegate void ResourceAddedEventHandler(ResourceFolder parent, BaseResource item, int index);
    public delegate void ResourceRemovedEventHandler(ResourceFolder parent, BaseResource item, int index);
    public delegate void ResourceMovedEventHandler(ResourceMovedEventArgs e);

    /// <summary>
    /// Event args for when a resource is moved from one folder to another
    /// </summary>
    public class ResourceMovedEventArgs : EventArgs {
        /// <summary>
        /// The source/original folder
        /// </summary>
        public ResourceFolder OldFolder { get; }

        /// <summary>
        /// The target/destination folder
        /// </summary>
        public ResourceFolder NewFolder { get; }

        /// <summary>
        /// The item that was moved
        /// </summary>
        public BaseResource Item { get; }

        /// <summary>
        /// The old index that <see cref="Item"/> was located at when it existed in <see cref="OldFolder"/>
        /// </summary>
        public int OldIndex { get; }

        /// <summary>
        /// The index of <see cref="Item"/> in <see cref="NewFolder"/>
        /// </summary>
        public int NewIndex { get; }

        /// <summary>
        /// An additional object that can be used to pass information between handlers
        /// </summary>
        public object Parameter { get; set; }

        public ResourceMovedEventArgs(ResourceFolder oldFolder, ResourceFolder newFolder, BaseResource item, int oldIndex, int newIndex) {
            this.OldFolder = oldFolder;
            this.NewFolder = newFolder;
            this.Item = item;
            this.OldIndex = oldIndex;
            this.NewIndex = newIndex;
        }
    }
}