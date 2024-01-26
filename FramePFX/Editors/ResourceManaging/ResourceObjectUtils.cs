namespace FramePFX.Editors.ResourceManaging {
    public static class ResourceObjectUtils {
        /// <summary>
        /// Helper function for calling <see cref="AddItem"/> and returning the parameter value
        /// </summary>
        /// <param name="item">The item to add and return</param>
        /// <typeparam name="T">The type of item to add and also return</typeparam>
        /// <returns>The <see cref="item"/> parameter</returns>
        public static T AddItemAndRet<T>(this ResourceFolder folder, T item) where T : BaseResource {
            folder.AddItem(item);
            return item;
        }
    }
}