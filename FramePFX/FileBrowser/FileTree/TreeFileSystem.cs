using System.Threading.Tasks;

namespace FramePFX.FileBrowser.FileTree {
    /// <summary>
    /// Used to load a tree entry's contents
    /// </summary>
    public abstract class TreeFileSystem {
        protected TreeFileSystem() {
        }

        /// <summary>
        /// Loads the given target's contents. This is called automatically by entries whose <see cref="TreeEntry.IsDirectory"/>
        /// property is true, and when its content has not already been loaded (using lazy loading logic)
        /// <para>
        /// Errors should be handled by this function, and exceptions should only be thrown if something really bad
        /// has happened (e.g. trying to load the content of a file that cannot store items, e.g. a text file; this is not allowed)
        /// </para>
        /// </summary>
        /// <param name="target">The entry to load the content of</param>
        /// <returns>
        /// A flag to indicate if content was actually loaded for the given entry.
        /// </returns>
        public abstract Task<bool> LoadContent(TreeEntry target);

        /// <summary>
        /// Refreshes the content of the given entry. This can be as simple as clearing the entry and then
        /// loading its content again, however, certain file systems may
        /// do nothing (e.g. zip file systems) and others may do a more optimised refresh
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public virtual Task RefreshContent(TreeEntry entry) {
            entry.ClearItemsRecursiveCore();
            return this.LoadContent(entry);
        }
    }
}