namespace FramePFX.FileBrowser.Explorer.ViewModes
{
    public class ListBasedViewMode : BaseViewModel, IExplorerViewMode
    {
        public static ListBasedViewMode Instance => ExplorerViewModes.ListBased;

        public string Id => "List";

        public ListBasedViewMode()
        {
        }
    }
}