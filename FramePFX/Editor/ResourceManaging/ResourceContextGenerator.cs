using System.Collections.Generic;
using System.Collections.ObjectModel;
using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.AdvancedContextService;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// A context generator for a resource manager and its items
    /// </summary>
    public class ResourceContextGenerator : IContextGenerator {
        public static ResourceContextGenerator Instance { get; } = new ResourceContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out BaseResourceViewModel resItem)) {
                ObservableCollection<BaseResourceViewModel> selected = resItem.Manager.SelectedItems;
                if (selected.Count > 0 && selected.Contains(resItem)) {
                    if (selected.Count == 1) {
                        list.Add(new ActionContextEntry(resItem.Manager, "actions.general.RenameItem", "Rename"));
                        list.Add(new ActionContextEntry(resItem.Manager, "actions.resources.GroupSelection", "Add to group"));
                        list.Add(SeparatorEntry.Instance);
                    }
                    else {
                        list.Add(new ActionContextEntry(resItem.Manager, "actions.resources.GroupSelection", "Add to group"));
                    }

                    list.Add(new ActionContextEntry(resItem.Manager, "actions.resources.DeleteItems", "Delete"));
                    list.Add(SeparatorEntry.Instance);

                    if (resItem is ResourceItemViewModel item) {
                        if (selected.Count == 1) {
                            if (item.IsOnline) {
                                list.Add(new ActionContextEntry(item.Manager, "actions.resources.ToggleOnlineState", "Set Offline").Set(ToggleAction.IsToggledKey, BoolBox.False));
                            }
                            else {
                                list.Add(new ActionContextEntry(item.Manager, "actions.resources.ToggleOnlineState", "Set Online").Set(ToggleAction.IsToggledKey, BoolBox.True));
                            }
                        }
                        else {
                            list.Add(new ActionContextEntry(item.Manager, "actions.resources.ToggleOnlineState", "Set All Online").Set(ToggleAction.IsToggledKey, BoolBox.True));
                            list.Add(new ActionContextEntry(item.Manager, "actions.resources.ToggleOnlineState", "Set All Offline").Set(ToggleAction.IsToggledKey, BoolBox.False));
                        }
                    }
                }
                else {
                    list.Add(new ActionContextEntry(resItem.Manager, "actions.general.RenameItem", "Rename"));
                    list.Add(new ActionContextEntry(resItem.Manager, "actions.resources.DeleteItems", "Delete"));
                    list.Add(SeparatorEntry.Instance);
                    if (resItem is ResourceItemViewModel item) {
                        if (item.IsOnline) {
                            list.Add(new ActionContextEntry(item.Manager, "actions.resources.ToggleOnlineState", "Set Offline").Set(ToggleAction.IsToggledKey, BoolBox.False));
                        }
                        else {
                            list.Add(new ActionContextEntry(item.Manager, "actions.resources.ToggleOnlineState", "Set Online").Set(ToggleAction.IsToggledKey, BoolBox.True));
                        }
                    }
                }
            }

            if (context.TryGetContext(out ResourceManagerViewModel manager) || (resItem != null && (manager = resItem.Manager) != null)) {
                ResourceFolderViewModel folder = resItem as ResourceFolderViewModel ?? manager.CurrentFolder;
                List<IContextEntry> newList = new List<IContextEntry>();
                newList.Add(new ActionContextEntry(folder, "actions.resources.newitem.NewText", "Text", "Create a new text resource, and clip"));
                newList.Add(new CommandContextEntry("ARGB Colour", manager.CreateResourceCommand, nameof(ResourceColour)));
                newList.Add(new CommandContextEntry("Image", manager.CreateResourceCommand, nameof(ResourceImage)));
                newList.Add(SeparatorEntry.Instance);
                newList.Add(new CommandContextEntry("Group", manager.CreateResourceCommand, nameof(ResourceFolder)));

                if (list.Count > 0) {
                    list.InsertRange(0, new List<IContextEntry> {
                        new GroupContextEntry("New...", newList),
                        SeparatorEntry.Instance
                    });
                }
                else {
                    list.Add(new GroupContextEntry("New Resource...", newList));
                }
            }
        }
    }
}