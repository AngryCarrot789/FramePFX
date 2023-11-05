using System.Collections.Generic;
using System.Linq;
using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.AdvancedContextService;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Actions;
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
                    list.Add(new ActionContextEntry("actions.general.RenameItem", "Rename"));
                    list.Add(SeparatorEntry.Instance);
                }

                list.Add(new ActionContextEntry("actions.resources.GroupSelectionIntoFolder", "Group into folder"));
                list.Add(new ActionContextEntry("actions.resources.DeleteItems", "Delete"));
                list.Add(SeparatorEntry.Instance);

                if (resItem is ResourceCompositionViewModel) {
                    list.Add(new ActionContextEntry("actions.timeline.OpenCompositionObjectsTimeline", "Open timeline"));
                }
                else if (resItem is ResourceColourViewModel) {
                    list.Add(new ActionContextEntry("action.resources.ChangeResourceColour", "Change Colour..."));
                }

                if (resItem is ResourceItemViewModel item) {
                    if (selected.Count == 1) {
                        if (item.IsOnline) {
                            list.Add(new ActionContextEntry("actions.resources.ToggleOnlineState", "Set Offline"));
                        }
                        else {
                            list.Add(new ActionContextEntry("actions.resources.ToggleOnlineState", "Set Online"));
                        }
                    }
                    else {
                        list.Add(new ActionContextEntry("actions.resources.ToggleOnlineState", "Set All Online"));
                        list.Add(new ActionContextEntry("actions.resources.ToggleOnlineState", "Set All Offline"));
                    }
                }
            }

            if (context.TryGetContext(out ResourceManagerViewModel manager) || (resItem != null && (manager = resItem.Manager) != null)) {
                List<IContextEntry> newList = new List<IContextEntry> {
                    new ActionContextEntry("action.create.new.resource.ResourceFolder", "New Folder", "Create a new folder"),
                    SeparatorEntry.Instance,
                    new ActionContextEntry("action.create.new.resource.ResourceTextStyle", "New Text Style"),
                    new ActionContextEntry("action.create.new.resource.ResourceTextFile", "New Text File", "Create a new text file"),
                    new ActionContextEntry("action.create.new.clip.TextClip", "New Text", "Create a new text style resource and a text clip"),
                    new ActionContextEntry("action.create.new.resource.ResourceColour", "New ARGB Colour"),
                    new ActionContextEntry("action.create.new.resource.ResourceImage", "New Image"),
                    new ActionContextEntry("action.create.new.resource.ResourceComposition", "New Composition Sequence")
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