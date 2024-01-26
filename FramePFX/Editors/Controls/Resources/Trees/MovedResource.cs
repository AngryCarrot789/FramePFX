using FramePFX.Editors.ResourceManaging;

namespace FramePFX.Editors.Controls.Resources.Trees {
    /// <summary>
    /// A class used to assist in efficient moving of a resource control
    /// </summary>
    public class MovedResource {
        public readonly ResourceTreeViewItem Control;
        public readonly BaseResource Resource;

        public MovedResource(ResourceTreeViewItem control, BaseResource resource) {
            this.Control = control;
            this.Resource = resource;
        }
    }
}