using System;
using FramePFX.AdvancedContextService;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Interactivity.DataContexts;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Utils;

namespace FramePFX.Editors.Contextual {
    public class ResourceContextRegistry : IContextGenerator {
        public static ResourceContextRegistry Instance { get; } = new ResourceContextRegistry();

        /// <summary>
        /// Extracts the contextual resource selection from the most desirable folder (based on the contextual resource)
        /// </summary>
        /// <param name="ctx">Data Context</param>
        /// <param name="folder">The folder that contains items in the selection array</param>
        /// <param name="selection">
        /// Either a single item (not selected or only selected item in folder) or all selected
        /// items in the folder. Always contains at least 1 item when this method returns true
        /// </param>
        /// <returns>True if the context contained a resource</returns>
        public static bool GetSingleFolderSelectionContext(IDataContext ctx, out ResourceFolder folder, out BaseResource[] selection) {
            if (!DataKeys.ResourceObjectKey.TryGetContext(ctx, out BaseResource resource) || resource.Parent == null || resource.Manager == null) {
                folder = null;
                selection = null;
                return false;
            }

            folder = resource.Manager.CurrentFolder;
            if (!folder.Contains(resource)) {
                folder = resource.Parent;
            }

            int selected = folder.Items.Count(x => x.IsSelected);
            if (!resource.IsSelected || selected == 1) {
                selection = new BaseResource[] {resource};
                return true;
            }
            else if (selected > 0) {
                selection = folder.Items.Where(x => x.IsSelected).ToArray();
                if (selection.Length < 1) {
                    Debugger.Break();
                    throw new Exception("Selection corruption: was folder modified during selection evaluation?");
                }

                return true;
            }
            else {
                Debugger.Break();
                throw new Exception("Selection corruption: zero selected items in folder while resource in folder was selected");
            }
        }

        public static bool GetTreeContext(IDataContext ctx, out BaseResource[] selection) {
            if (!DataKeys.ResourceObjectKey.TryGetContext(ctx, out BaseResource resource) || resource.Parent == null || resource.Manager == null) {
                selection = null;
                return false;
            }

            int selected = resource.Manager.SelectedItems.Count;
            if (!resource.IsSelected || selected == 1) {
                selection = new BaseResource[] {resource};
                return true;
            }
            else if (selected > 0) {
                selection = resource.Manager.SelectedItems.ToArray();
                Debug.Assert(selection.Length > 0, "selection.Length > 0");
                return true;
            }
            else {
                Debugger.Break();
                throw new Exception("Selection corruption: zero selected items in folder while resource in folder was selected");
            }
        }

        public static bool GetSingleSelection(IDataContext ctx, out BaseResource resource) {
            if (!DataKeys.ResourceObjectKey.TryGetContext(ctx, out resource) || resource.Parent == null || resource.Manager == null) {
                return false;
            }

            ResourceFolder folder = resource.Manager.CurrentFolder;
            if (!folder.Contains(resource)) {
                folder = resource.Parent;
            }

            return !resource.IsSelected || folder.Items.Count(x => x.IsSelected) == 1;
        }

        /// <summary>
        /// Either gets the <see cref="DataKeys.ResourceObjectKey"/> as a folder or the resource
        /// manager's current folder. Does not process any selection states
        /// </summary>
        /// <param name="ctx">Data Context</param>
        /// <param name="folder">The folder</param>
        /// <returns>True if the data contains contained a folder or a resource manager</returns>
        public static bool GetTargetFolder(IDataContext ctx, out ResourceFolder folder) {
            if (DataKeys.ResourceObjectKey.TryGetContext(ctx, out BaseResource resource) && (folder = resource as ResourceFolder) != null) {
                return true;
            }
            else if (DataKeys.ResourceManagerKey.TryGetContext(ctx, out ResourceManager manager)) {
                folder = manager.CurrentFolder;
                return true;
            }
            else {
                folder = null;
                return false;
            }
        }

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (!GetSingleFolderSelectionContext(context, out ResourceFolder folder, out BaseResource[] selection)) {
                if (context.ContainsKey(DataKeys.ResourceManagerKey)) {
                    GenerateNewResourceEntries(list);
                }

                return;
            }

            if (selection.Length == 1) {
                BaseResource resource = selection[0];
                list.Add(new CommandContextEntry("commands.resources.RenameResourceCommand", "Rename"));
                list.Add(new CommandContextEntry("commands.resources.GroupResourcesCommand", "Add to new folder", "Adds this item to a new folder"));

                if (resource is ResourceItem item) {
                    list.Add(new SeparatorEntry());
                    if (item.IsOnline) {
                        list.Add(new CommandContextEntry("commands.resources.DisableResourcesCommand", "Set Offline"));
                    }
                    else {
                        list.Add(new CommandContextEntry("commands.resources.EnableResourcesCommand", "Set Online"));
                    }

                    switch (resource) {
                        case ResourceComposition _:
                            list.Add(new SeparatorEntry());
                            list.Add(new CommandContextEntry("commands.resources.OpenCompositionResourceTimelineCommand", "Open Timeline"));
                            break;
                        case ResourceImage _:
                            list.Add(new SeparatorEntry());
                            list.Add(new EventContextEntry(ChangeResourceImagePath, "Change Image Path"));
                            break;
                    }
                }

                list.Add(new SeparatorEntry());
                list.Add(new CommandContextEntry("commands.resources.DeleteResourcesCommand", "Delete Resource"));
            }
            else {
                list.Add(new CommandContextEntry("commands.resources.GroupResourcesCommand", "Group items into folder", "Groups all selected items in the explorer into a folder. Grouping items in the tree is currently unsupported"));
                list.Add(new SeparatorEntry());
                list.Add(new CommandContextEntry("commands.resources.EnableResourcesCommand", "Set All Online"));
                list.Add(new CommandContextEntry("commands.resources.DisableResourcesCommand", "Set All Offline"));
                list.Add(new SeparatorEntry());
                list.Add(new CommandContextEntry("commands.resources.DeleteResourcesCommand", "Delete Resources"));
            }
        }

        private static void ChangeResourceImagePath(IDataContext ctx) {
            if (GetSingleSelection(ctx, out BaseResource resource) && resource is ResourceImage image) {
                string filePath = IoC.FilePickService.OpenFile("Select a new image file for this resource", Filters.ImageTypesAndAll);
                if (filePath == null) {
                    return;
                }

                if (image.IsRawBitmapMode) {
                    if (IoC.MessageService.ShowMessage("Clear Raw Bitmap", "This image stores a pure bitmap instead of being file-based. Do you want to clear the bitmap?", MessageBoxButton.OKCancel) != MessageBoxResult.OK) {
                        return;
                    }

                    image.ClearRawBitmapImage();
                }

                image.Disable(true);
                image.FilePath = filePath;
                ResourceLoaderDialog.TryLoadResources(image);
            }
        }

        public static void GenerateNewResourceEntries(List<IContextEntry> list) {
            List<IContextEntry> toAdd = new List<IContextEntry>();
            toAdd.Add(new EventContextEntry(AddColourResource, "Colour"));
            toAdd.Add(new EventContextEntry(AddCompositionResource, "Composition Timeline"));

            list.Add(new GroupContextEntry("Add new...", toAdd));
        }

        private static void AddColourResource(IDataContext ctx) {
            if (GetTargetFolder(ctx, out ResourceFolder folder)) {
                AddNewResource(folder, new ResourceColour() {Colour = RenderUtils.RandomColour(), DisplayName = "New Colour"});
            }
        }

        private static void AddCompositionResource(IDataContext ctx) {
            if (GetTargetFolder(ctx, out ResourceFolder folder)) {
                AddNewResource(folder, new ResourceComposition() {DisplayName = "New Composition"});
            }
        }

        private static void AddNewResource(ResourceFolder folder, BaseResource resource) {
            folder.AddItem(resource);
            ResourceLoaderDialog.TryLoadResources(resource);
        }
    }
}