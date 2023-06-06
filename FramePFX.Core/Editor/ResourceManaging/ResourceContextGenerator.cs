using System.Collections.Generic;
using System.Collections.ObjectModel;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Core.Editor.ResourceManaging {
    /// <summary>
    /// A context generator for a resource manager and its items
    /// </summary>
    public class ResourceContextGenerator : IContextGenerator {
        public static ResourceContextGenerator Instance { get; } = new ResourceContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out ResourceItemViewModel item)) {
                ObservableCollection<ResourceItemViewModel> selected = item.Manager.SelectedItems;
                if (selected.Count > 0) {
                    if (selected.Count == 1) {
                        list.Add(new ActionContextEntry(item.Manager, "actions.resources.RenameItem", "Rename"));
                        list.Add(SeparatorEntry.Instance);
                    }

                    list.Add(new ActionContextEntry(item.Manager, "actions.resources.DeleteItems", "Delete"));
                    return;
                }
            }

            if (context.TryGetContext(out ResourceManagerViewModel manager)) {
                List<IContextEntry> newList = new List<IContextEntry>();
                newList.Add(new CommandContextEntry("Text", manager.CreateResourceCommand, nameof(ResourceText)));
                newList.Add(new CommandContextEntry("ARGB Colour", manager.CreateResourceCommand, nameof(ResourceColour)));
                newList.Add(new CommandContextEntry("Image", manager.CreateResourceCommand, nameof(ResourceImage)));
                list.Add(new GroupContextEntry("New...", newList));
            }
        }
    }
}