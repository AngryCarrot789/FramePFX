using System.Collections.Generic;
using System.Linq;
using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.AdvancedContextService;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Utils;

namespace FramePFX.Editor.Contexts {
    /// <summary>
    /// A context generator for a resource manager and its items
    /// </summary>
    public class ResourceContextGenerator : IContextGenerator {
        public static ResourceContextGenerator Instance { get; } = new ResourceContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out BaseResourceViewModel resItem)) {
                List<BaseResourceViewModel> selected = resItem.Manager.SelectedItems.ToList();
                if (!selected.Contains(resItem)) {
                    selected.Add(resItem);
                }

                if (selected.Count == 1) {
                    list.Add(new ActionContextEntry(context, "actions.general.RenameItem", "Rename"));
                    list.Add(SeparatorEntry.Instance);
                }

                list.Add(new ActionContextEntry(context, "actions.resources.GroupSelectionIntoFolder", "Group into folder"));
                list.Add(new ActionContextEntry(context, "actions.resources.DeleteItems", "Delete"));
                list.Add(SeparatorEntry.Instance);

                if (resItem is ResourceCompositionViewModel) {
                    list.Add(new ActionContextEntry(context, "actions.timeline.OpenCompositionObjectsTimeline", "Open timeline"));
                }

                if (resItem is ResourceItemViewModel item) {
                    if (selected.Count == 1) {
                        if (item.IsOnline) {
                            list.Add(new ActionContextEntry(context, "actions.resources.ToggleOnlineState", "Set Offline").Set(ToggleAction.IsToggledKey, BoolBox.False));
                        }
                        else {
                            list.Add(new ActionContextEntry(context, "actions.resources.ToggleOnlineState", "Set Online").Set(ToggleAction.IsToggledKey, BoolBox.True));
                        }
                    }
                    else {
                        list.Add(new ActionContextEntry(context, "actions.resources.ToggleOnlineState", "Set All Online").Set(ToggleAction.IsToggledKey, BoolBox.True));
                        list.Add(new ActionContextEntry(context, "actions.resources.ToggleOnlineState", "Set All Offline").Set(ToggleAction.IsToggledKey, BoolBox.False));
                    }
                }
            }

            if (context.TryGetContext(out ResourceManagerViewModel manager) || (resItem != null && (manager = resItem.Manager) != null)) {
                ResourceFolderViewModel folder = resItem as ResourceFolderViewModel ?? manager.CurrentFolder;
                List<IContextEntry> newList = new List<IContextEntry> {
                    new CommandContextEntry("New Folder", manager.CreateResourceCommand, nameof(ResourceFolder)),
                    SeparatorEntry.Instance,
                    new ActionContextEntry(context, "actions.resources.newitem.NewText", "New Text", "Create a new text resource, and clip"),
                    new CommandContextEntry("New ARGB Colour", manager.CreateResourceCommand, nameof(ResourceColour)),
                    new CommandContextEntry("New Image", manager.CreateResourceCommand, nameof(ResourceImage)),
                    new CommandContextEntry("New Composition Sequence", manager.CreateResourceCommand, nameof(ResourceComposition))
                };

                if (list.Count > 0) {
                    list.Insert(0, new GroupContextEntry("New...", newList));
                    list.Insert(1, SeparatorEntry.Instance);
                }
                else {
                    list.Insert(0, new GroupContextEntry("New...", newList));
                }
            }
        }
    }
}