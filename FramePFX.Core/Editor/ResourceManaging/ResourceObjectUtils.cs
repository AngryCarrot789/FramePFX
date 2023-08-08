namespace FramePFX.Core.Editor.ResourceManaging
{
    public static class ResourceObjectUtils
    {
        /// <summary>
        /// Helper function for calling <see cref="AddItem"/> and returning the parameter value
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddItemAndRet<T>(this ResourceGroup group, T item) where T : BaseResourceObject
        {
            group.AddItem(item);
            return item;
        }
    }
}