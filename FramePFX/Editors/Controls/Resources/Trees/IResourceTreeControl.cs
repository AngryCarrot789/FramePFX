using FramePFX.Editors.ResourceManaging;

namespace FramePFX.Editors.Controls.Resources.Trees {
    /// <summary>
    /// An interface for shared properties between a <see cref="ResourceTreeView"/> and <see cref="ResourceTreeViewItem"/>
    /// </summary>
    public interface IResourceTreeControl {
        ResourceTreeView ResourceTree { get; }

        ResourceTreeViewItem ParentNode { get; }

        MovedResource MovedResource { get; set; }

        BaseResource Resource { get; }

        ResourceTreeViewItem GetNodeAt(int index);

        void InsertNode(BaseResource item, int index);

        void InsertNode(ResourceTreeViewItem control, BaseResource resource, int index);

        void RemoveNode(int index, bool canCache = true);
    }
}