using System;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.ViewModels {
    public abstract class ResourceItemViewModel : BaseResourceViewModel {
        public new ResourceItem Model => (ResourceItem) base.Model;

        public ulong UniqueId => this.Model.UniqueId;

        public bool IsOnline => this.Model.IsOnline;

        public bool IsOfflineByUser {
            get => this.Model.IsOfflineByUser;
            set {
                if (this.IsOfflineByUser == value)
                    return;
                this.Model.IsOfflineByUser = value;
                this.RaisePropertyChanged();
            }
        }

        public AsyncRelayCommand SetOfflineCommand { get; }

        public AsyncRelayCommand SetOnlineCommand { get; }

        public int ReferenceCount => this.Model.ReferenceCount;

        protected ResourceItemViewModel(ResourceItem model) : base(model) {
            model.OnlineStateChanged += (a, b) => {
                this.RaisePropertyChanged(nameof(this.IsOnline));
                this.RaisePropertyChanged(nameof(this.IsOfflineByUser));
            };

            model.ReferenceCountChanged += (item, reference, added) => this.RaisePropertyChanged(nameof(this.ReferenceCount));
            this.SetOfflineCommand = new AsyncRelayCommand(() => this.SetOfflineAsync(true), () => this.IsOnline);
            this.SetOnlineCommand = new AsyncRelayCommand(async () => {
                ResourceCheckerViewModel checker = new ResourceCheckerViewModel() {
                    Caption = "This resource could not be loaded"
                };

                await ResourceCheckerViewModel.LoadResources(checker, CollectionUtils.Singleton(this), true);
            }, () => !this.IsOnline);
        }

        public virtual async Task SetOfflineAsync(bool user) {
            using (ErrorList stack = new ErrorList("Failed to set resource to offline")) {
                // TODO: remove ErrorList usage and replace with something like an exception viewer
                this.Model.Disable(stack, user);
                if (stack.TryGetException(out Exception exception)) {
                    await Services.DialogService.ShowMessageExAsync("Exception setting offline", "An exception occurred while setting resource to offline", exception.GetToString());
                }
            }
        }

        /// <summary>
        /// Attempts to load this resource. This just invokes <see cref="TryLoadResource"/> passing the current instance as a parameter
        /// </summary>
        /// <param name="checker">An optional checker to use</param>
        /// <returns>The value of <see cref="IsOnline"/></returns>
        public Task<bool> LoadResourceAsync(ResourceCheckerViewModel checker = null, bool reloadIfOnline = false) {
            return TryLoadResource(this, checker, reloadIfOnline);
        }

        // TODO: maybe move these 2 functions into the model at some point, instead of
        //       giving the view model the responsibility of loading the resource?

        /// <summary>
        /// Attempt to load this resource's data; make this resource "online". If the resource could not load
        /// its data (e.g. file does not exist), then this function returns false, and you can optionally add
        /// an instance of <see cref="InvalidResourceViewModel"/> to the <see cref="checker"/> parameter
        /// <para>
        /// The <see cref="checker"/> parameter may be null. When non-null, a dialog may be shown if the checker contains
        /// any <see cref="InvalidResourceViewModel"/> instances, allowing the user to fix any problems that prevented
        /// the resource from coming online
        /// </para>
        /// <para>
        /// This function should not throw, but doing so may result in the app crashing
        /// </para>
        /// <para>
        /// This will only be called if the resource is actually offline (<see cref="IsOnline"/> is false)
        /// </para>
        /// </summary>
        /// <param name="checker">[optional] checker instance</param>
        /// <param name="list"></param>
        /// <returns>
        /// True if the resource was loaded successfully and can be used (online), otherwise false (offline)
        /// </returns>
        protected virtual Task<bool> LoadResource(ResourceCheckerViewModel checker, ErrorList list) {
            return Task.FromResult(true);
        }

        // TODO: maybe auto-save before calling this? maybe add autosave function in the project that can easily be called anywhere

        /// <summary>
        /// Attempts to load the resource (by calling <see cref="LoadResource"/>), and then sets the resource's online state accordingly
        /// <para>
        /// (this behaviour will change in the future) Currently, when the resource is loaded, no message is shown unless an error
        /// is also encountered (which should not really happen), but when the resource could not be loaded, errors are only logged
        /// to the app logger (no dialog). When an unexpected exception is encountered, a message is shown then an exception is thrown
        /// </para>
        /// </summary>
        /// <param name="resource">The resource to load</param>
        /// <param name="checker">An optional resource checker, to show the user a list of possible resolvable errors</param>
        /// <param name="reloadIfOnline">True to disable and then try to load the resource, False to return if already online. Default value is false</param>
        /// <returns>True if the resource was loaded and set online, otherwise false meaning it is offline</returns>
        /// <exception cref="ArgumentNullException">The resource was null</exception>
        /// <exception cref="Exception">An unexpected error occurred while loading the resource</exception>
        public static async Task<bool> TryLoadResource(ResourceItemViewModel resource, ResourceCheckerViewModel checker, bool reloadIfOnline = false) {
            if (resource == null) {
                throw new ArgumentNullException(nameof(resource));
            }

            if (resource.IsOnline) {
                if (reloadIfOnline) {
                    await resource.SetOfflineAsync(false);
                }
                else {
                    return true;
                }
            }

            // TODO: as per the SetOfflineAsync function comments, really gotta remove the ErrorList usage

            bool isOnline;
            using (ErrorList list = new ErrorList("Failed to load resource", false)) {
                try {
                    isOnline = await resource.LoadResource(checker, list);
                }
                catch (Exception e) {
                    string msg = e.GetToString();
                    string typeName = resource.GetType().Name;
                    AppLogger.WriteLine($"[FATAL] An unexpected exception occurred while loading resource: {typeName}\n{msg}");
                    await Services.DialogService.ShowMessageExAsync("Resource load error", $"An unexpected error occurred while loading resource '{typeName}' :(", msg);
                    throw new Exception($"Exception occurred while loading resource '{typeName}'", e);
                }

                ResourceItem.SetOnlineState(resource.Model, isOnline);
                if (list.TryGetException(out Exception exception)) {
                    if (isOnline) {
                        await Services.DialogService.ShowMessageExAsync("Resource warning", $"Resource loaded with one or more errors", exception.GetToString());
                    }
                    else {
                        AppLogger.WriteLine("Resource could not be loaded due to one or more errors: " + exception.GetToString());
                    }
                }
            }

            return isOnline;
        }

        /// <summary>
        /// A helper function that adds a resource to a target group, registers it with the manager, and then
        /// loads it. If it fails to load, it removes it from the group and unregisters it
        /// </summary>
        /// <param name="folder">The group in which the resource is added to. This group must have a manager associated with it</param>
        /// <param name="resource">The resource to add and register</param>
        /// <param name="checker">An optional checker passed to <see cref="TryLoadResource"/></param>
        /// <returns>True if the resource was loaded and set online, otherwise false</returns>
        /// <exception cref="ArgumentNullException">The group or resource was null</exception>
        /// <exception cref="Exception">No manager associated with the group, or <see cref="TryLoadResource"/> encountered an unexpected exception</exception>
        public static async Task<bool> TryAddAndLoadNewResource(ResourceFolderViewModel folder, ResourceItemViewModel resource, ResourceCheckerViewModel checker = null, bool keepInHierarchyOnLoadFailure = false) {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));

            ResourceManagerViewModel manager = folder.Manager;
            if (manager == null) {
                throw new Exception("Group has no manager associated with it");
            }

            bool result = false;
            folder.AddItem(resource);
            ulong id = resource.Model.IsRegistered() ? resource.UniqueId : manager.Model.RegisterEntry(resource.Model);
            if (resource.IsOnline || await TryLoadResource(resource, checker)) {
                result = true;
            }
            else {
                if (!keepInHierarchyOnLoadFailure) {
                    folder.Model.UnregisterAndRemoveItem(resource.Model);
                }
            }

            AppLogger.WriteLine($"Loaded new resource '{resource.GetType().Name}': {(result ? $"Success (ID '{id}')" : "Failed")}");
            return result;
        }
    }
}