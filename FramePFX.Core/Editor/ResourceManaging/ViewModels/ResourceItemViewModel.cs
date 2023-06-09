using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public abstract class ResourceItemViewModel : BaseViewModel, IDisposable {
        private readonly ResourceItemEventHandler onlineStateChangedHandler;

        public ResourceItem Model { get; }

        public ResourceManagerViewModel Manager { get; }

        public string UniqueId => this.Model.UniqueId;

        public bool IsOnline {
            get => this.Model.IsOnline;
            private set {
                if (this.IsOnline == value)
                    return;
                this.Model.IsOnline = value;
                this.Model.OnIsOnlineStateChanged();
            }
        }

        public AsyncRelayCommand RenameCommand { get; }
        public AsyncRelayCommand DeleteCommand { get; }

        public AsyncRelayCommand SetOfflineCommand { get; }
        public AsyncRelayCommand SetOnlineCommand { get; }

        protected ResourceItemViewModel(ResourceManagerViewModel manager, ResourceItem model) {
            this.Manager = manager;
            this.Model = model;
            this.RenameCommand = new AsyncRelayCommand(async () => await this.Manager.RenameResourceAction(this));
            this.DeleteCommand = new AsyncRelayCommand(async () => await this.Manager.DeleteResourceAction(this));
            this.onlineStateChangedHandler = (a, b) => this.RaisePropertyChanged(nameof(this.IsOnline));
            model.OnlineStateChanged += this.onlineStateChangedHandler;

            this.SetOfflineCommand = new AsyncRelayCommand(async () => {
                using (ExceptionStack stack = new ExceptionStack(false)) {
                    await this.Model.SetOfflineAsync(stack);
                    if (stack.TryGetException(out Exception exception)) {
                        await IoC.MessageDialogs.ShowMessageExAsync("Exception setting offline", "An exception occurred while setting resource to offline", exception.GetToString());
                    }
                }
            }, () => this.IsOnline != false);

            this.SetOnlineCommand = new AsyncRelayCommand(async () => {
                await ResourceCheckerViewModel.ProcessResources(new List<ResourceItemViewModel>() {this}, true);
            }, () => this.IsOnline != true);
        }

        public virtual void Dispose() {
            this.Model.OnlineStateChanged -= this.onlineStateChangedHandler;
            this.Model.Dispose();
        }

        /// <summary>
        /// Refreshes the state of this resource which is used to determine if this resource is online or not
        /// <para>
        /// When the resource is not in a valid state, an <see cref="InvalidResourceViewModel"/> can be added to the
        /// given <see cref="ResourceCheckerViewModel"/> in which the user can attempt to fix the resource, or ignore and keep it offline
        /// </para>
        /// <para>
        /// This method may also dispose resources that this resource uses (e.g. dispose image or media resources so the file is no longer in use)
        /// </para>
        /// </summary>
        /// <param name="checker">The checker</param>
        /// <param name="stack">An exception stack. Add any errors encountered while checking the online state or while disposing of resources</param>
        /// <returns>The online state of the resource. This method typically should not directly set the property</returns>
        public virtual Task<bool> ValidateOnlineState(ResourceCheckerViewModel checker, ExceptionStack stack) {
            return Task.FromResult(true);
        }
    }
}