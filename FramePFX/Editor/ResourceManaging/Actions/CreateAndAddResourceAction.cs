using System;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Editor.ViewModels;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.Actions {
    public static class CreateResourceUtils {
        public static bool GetSelectedFolder(IDataContext ctx, out ResourceFolderViewModel folder) {
            if (ctx.TryGetContext(out folder)) {
                return folder.Manager != null;
            }

            if (ctx.TryGetContext(out ResourceManagerViewModel manager)) {
                folder = manager.CurrentFolder;
                return folder.Manager != null;
            }

            if (ctx.TryGetContext(out ProjectViewModel project)) {
                return (folder = project.ResourceManager.CurrentFolder).Manager != null;
            }

            if (ctx.TryGetContext(out VideoEditorViewModel editor)) {
                return editor.ActiveProject != null && (folder = editor.ActiveProject.ResourceManager.CurrentFolder).Manager != null;
            }

            return false;
        }
    }

    public abstract class CreateAndAddResourceAction<T> : ExecutableAction where T : BaseResource {
        public string RegistryTypeId { get; }

        protected CreateAndAddResourceAction() {
            this.RegistryTypeId = ResourceTypeFactory.Instance.GetTypeIdForModel(typeof(T));
        }

        public override bool CanExecute(ActionEventArgs e) {
            return CreateResourceUtils.GetSelectedFolder(e.DataContext, out _);
        }

        public virtual string GetDefaultDisplayName() {
            return "New Resource";
        }

        public sealed override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!CreateResourceUtils.GetSelectedFolder(e.DataContext, out ResourceFolderViewModel folder)) {
                return false;
            }

            BaseResource model;
            try {
                model = ResourceTypeFactory.Instance.CreateModel(this.RegistryTypeId);
            }
            catch (Exception ex) {
                string str = ex.GetToString();
                AppLogger.WriteLine("Failed to create resource: " + str);
                await IoC.DialogService.ShowMessageExAsync("Resource Failure", "Failed to create resource", str);
                return true;
            }

            if (model is T resource) {
                resource.DisplayName = this.GetDefaultDisplayName();
                folder.Model.AddItem(model);
                if (await this.PreProcessCanKeepInFolder(folder, resource, e)) {
                    await this.PostProcessItemCreation(folder, resource, e);
                }
                else {
                    folder.Model.UnregisterRemoveAndDisposeItem(model);
                }
            }
            else {
                await IoC.DialogService.ShowMessageAsync("Resource Failure", "Resource type error... this should not have occured XD.\nID = " + this.RegistryTypeId + ", actual type = " + model.GetType().Name);
            }

            return true;
        }

        public virtual async Task<bool> PreProcessCanKeepInFolder(ResourceFolderViewModel folder, T tItem, ActionEventArgs e) {
            if (!(tItem is ResourceItem resource)) {
                return true;
            }

            ResourceItemViewModel item = (ResourceItemViewModel) folder.LastItem;
            resource.Manager.RegisterEntry(resource);
            if (item.IsOnline || await ResourceItemViewModel.TryLoadResource(item, null)) {
                AppLogger.WriteLine($"Loaded new resource '{item.GetType().Name}'");
                return true;
            }
            else {
                AppLogger.WriteLine($"Failed to load new resource '{item.GetType().Name}'");
                return false;
            }
        }

        public virtual Task PostProcessItemCreation(ResourceFolderViewModel folder, T resource, ActionEventArgs e) {
            return Task.CompletedTask;
        }
    }

    [ActionRegistration("action.create.new.resource.ResourceFolder")]
    public class CreateResourceFolderAction : CreateAndAddResourceAction<ResourceFolder> {
        public override string GetDefaultDisplayName() => "New Folder";
    }

    [ActionRegistration("action.create.new.resource.ResourceColour")]
    public class CreateResourceColourAction : CreateAndAddResourceAction<ResourceColour> {
        public override string GetDefaultDisplayName() => "New Colour Resource";
    }

    [ActionRegistration("action.create.new.resource.ResourceImage")]
    public class CreateResourceImageAction : CreateAndAddResourceAction<ResourceImage> {
        public override string GetDefaultDisplayName() => "New Image Resource";
    }

    [ActionRegistration("action.create.new.resource.ResourceTextFile")]
    public class CreateResourceTextFileAction : CreateAndAddResourceAction<ResourceTextFile> {
        public override string GetDefaultDisplayName() => "New Text File Resource";
    }

    [ActionRegistration("action.create.new.resource.ResourceTextStyle")]
    public class CreateResourceTextStyleAction : CreateAndAddResourceAction<ResourceTextStyle> {
        public static CreateResourceTextStyleAction Instance { get; private set; }

        public CreateResourceTextStyleAction() => Instance = this;

        public override string GetDefaultDisplayName() => "New Text Style Resource";
    }

    [ActionRegistration("action.create.new.resource.ResourceComposition")]
    public class CreateCompositionResourceAction : CreateAndAddResourceAction<ResourceComposition> {
        public override Task PostProcessItemCreation(ResourceFolderViewModel folder, ResourceComposition resource, ActionEventArgs e) {
            resource.Timeline.DisplayName = "Composition timeline";
            resource.Timeline.AddTrack(new VideoTrack() {
                DisplayName = "Track 1"
            });

            resource.Timeline.AddTrack(new VideoTrack() {
                DisplayName = "Track 2"
            });

            ResourceCompositionViewModel item = (ResourceCompositionViewModel) folder.LastItem;
            folder.Manager.Project?.Editor?.OpenAndSelectTimeline(item.Timeline);
            return base.PostProcessItemCreation(folder, resource, e);
        }
    }
}