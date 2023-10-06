namespace FramePFX.Interactivity {
    /// <summary>
    /// Additional registration data for an entry in a <see cref="DragDropRegistry"/>
    /// </summary>
    public class DropMetadata {
        /// <summary>
        /// Gets or sets if the droppable object(s) could be in the form of a collection,
        /// and if so, try to access the list for the objects
        /// </summary>
        public bool IsCollectionBased { get; }

        /// <summary>
        /// Used when <see cref="IsCollectionBased"/> is true: only allow a drop when a single item is present
        /// </summary>
        public bool OnlyUseSingleItem { get; }

        public static DropMetadata SingleDrop() => new DropMetadata(true, true);
        public static DropMetadata MultiDrop() => new DropMetadata(true, false);

        public DropMetadata(bool isCollectionBased, bool onlyUseSingleItem) {
            this.IsCollectionBased = isCollectionBased;
            this.OnlyUseSingleItem = onlyUseSingleItem;
        }
    }
}