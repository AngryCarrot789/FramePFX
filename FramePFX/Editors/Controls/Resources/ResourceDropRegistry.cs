using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Interactivity;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Resources {
    public static class ResourceDropRegistry {
        public static DragDropRegistry<BaseResource> DropRegistry { get; }

        /// <summary>
        /// The drag-drop identifier for a resource drag-drop
        /// </summary>
        public const string ResourceDropType = "PFXResource_DropType";

        static ResourceDropRegistry() {
            DropRegistry = new DragDropRegistry<BaseResource>();

            DropRegistry.Register<ResourceFolder, List<BaseResource>>((target, items, dropType, c) => {
                if (dropType == EnumDropType.None || dropType == EnumDropType.Link) {
                    return EnumDropType.None;
                }

                if (items.Count == 1) {
                    BaseResource item = items[0];
                    if (item is ResourceFolder folder && folder.IsParentInHierarchy(target)) {
                        return EnumDropType.None;
                    }
                    else if (dropType != EnumDropType.Copy) {
                        if (target.Contains(item)) {
                            return EnumDropType.None;
                        }
                    }
                }

                return dropType;
            }, (folder, resources, dropType, c) => {
                if (dropType != EnumDropType.Copy && dropType != EnumDropType.Move) {
                    return Task.CompletedTask;
                }

                List<ResourceItem> items = new List<ResourceItem>();
                foreach (BaseResource resource in resources) {
                    if (resource is ResourceFolder group && @group.IsParentInHierarchy(folder)) {
                        continue;
                    }

                    if (dropType == EnumDropType.Copy) {
                        BaseResource clone = BaseResource.Clone(resource);
                        if (!TextIncrement.GetIncrementableString(folder.IsNameFree, clone.DisplayName, out string name))
                            name = clone.DisplayName;
                        clone.DisplayName = name;
                        folder.AddItem(clone);
                        if (clone is ResourceItem)
                            items.Add((ResourceItem) clone);
                    }
                    else if (resource.Parent != null) {
                        if (resource.Parent != folder) {
                            // drag dropped a resource into the same folder
                            resource.Parent.MoveItemTo(folder, resource);
                        }
                    }
                    else {
                        // ???
                        AppLogger.Instance.WriteLine("A resource was dropped with a null parent???");
                    }
                }

                ResourceLoaderDialog.TryLoadResources(items);

                return Task.CompletedTask;
            });

            DropRegistry.RegisterNative<ResourceFolder>(NativeDropTypes.FileDrop, (folder, objekt, dropType, c) => {
                return objekt.GetData(NativeDropTypes.FileDrop) is string[] files && files.Length > 0 ? EnumDropType.Copy : EnumDropType.None;
            }, (folder, objekt, dropType, c) => {
                if (objekt.GetData(NativeDropTypes.FileDrop) is string[] files) {
                    foreach (string path in files) {
                        switch (Path.GetExtension(path).ToLower()) {
                            case ".gif":
                            case ".mp3":
                            case ".wav":
                            case ".ogg":
                            case ".mp4":
                            case ".wmv":
                            case ".avi":
                            case ".avchd":
                            case ".f4v":
                            case ".swf":
                            case ".mov":
                            case ".mkv":
                            case ".qt":
                            case ".webm":
                            case ".flv": {
                                ResourceAVMedia media = new ResourceAVMedia() {
                                    FilePath = path, DisplayName = Path.GetFileName(path)
                                };

                                if (!ResourceLoaderDialog.TryLoadResources(media)) {
                                    break;
                                }

                                folder.AddItem(media);

                                break;
                            }
                            case ".png":
                            case ".bmp":
                            case ".jpg":
                            case ".jpeg": {
                                ResourceImage image = new ResourceImage() {FilePath = path, DisplayName = Path.GetFileName(path)};
                                if (!ResourceLoaderDialog.TryLoadResources(image)) {
                                    return Task.CompletedTask;
                                }

                                folder.AddItem(image);
                                break;
                            }
                        }
                    }
                }

                return Task.CompletedTask;
            });
        }
    }
}