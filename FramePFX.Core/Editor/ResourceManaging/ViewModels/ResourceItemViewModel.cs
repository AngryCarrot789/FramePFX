using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public abstract class ResourceItemViewModel : BaseResourceObjectViewModel {
        private readonly ResourceItemEventHandler onlineStateChangedHandler;

        public new ResourceItem Model => (ResourceItem) base.Model;

        public ulong UniqueId => this.Model.UniqueId;

        public bool IsOnline {
            get => this.Model.IsOnline;
            private set {
                if (this.IsOnline == value)
                    return;
                this.Model.IsOnline = value;
                this.Model.OnIsOnlineStateChanged();
            }
        }

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

        protected ResourceItemViewModel(ResourceItem model) : base(model) {
            this.onlineStateChangedHandler = (a, b) => {
                this.RaisePropertyChanged(nameof(this.IsOnline));
                this.RaisePropertyChanged(nameof(this.IsOfflineByUser));
            };

            model.OnlineStateChanged += this.onlineStateChangedHandler;
            this.SetOfflineCommand = new AsyncRelayCommand(() => this.SetOfflineAsync(true), () => this.IsOnline);
            this.SetOnlineCommand = new AsyncRelayCommand(async () => {
                await ResourceCheckerViewModel.LoadResources(new List<ResourceItemViewModel>() {this}, true);
            }, () => !this.IsOnline);
        }

        public virtual async Task SetOfflineAsync(bool user) {
            using (ExceptionStack stack = new ExceptionStack(false)) {
                this.Model.Disable(stack, user);
                if (stack.TryGetException(out Exception exception)) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exception setting offline", "An exception occurred while setting resource to offline", exception.GetToString());
                }
            }
        }

        public override async Task<bool> DeleteSelfAction() {
            if (this.Parent == null) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid item", "This resource is not located anywhere...?");
                return false;
            }

            if (await IoC.MessageDialogs.ShowDialogAsync("Delete resource?", $"Delete resource{(this.DisplayName != null ? $"'{this.DisplayName}'" : "")}?", MsgDialogType.OKCancel) != MsgDialogResult.OK) {
                return false;
            }

            if (this.UniqueId != ResourceManager.EmptyId && this.Manager != null) {
                if (this.Manager.Manager.TryGetEntryItem(this.UniqueId, out ResourceItem item)) {
                    if (!ReferenceEquals(this.Model, item)) {
                        #if DEBUG
                        System.Diagnostics.Debugger.Break();
                        #endif
                        await IoC.MessageDialogs.ShowMessageAsync("Application Corrupted", "This resource is registered but the ID is associated with another resource");
                        return false;
                    }
                }

                this.Manager.Manager.DeleteEntryByItem(this.Model);
            }

            this.Parent.RemoveItem(this, true, true);

            try {
                base.Model.Dispose();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error disposing item", "Failed to dispose resource", e.GetToString());
            }

            return true;
        }

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
        /// The exception stack is optional, but can be used to show any errors to the user. These errors are accumulated
        /// into a single final exception which is presented to the user, so it may be mixed with multiple resource errors
        /// </para>
        /// </summary>
        /// <param name="checker">[nullable] checker instance</param>
        /// <param name="stack">The stack of exceptions for user-presentable errors</param>
        /// <returns></returns>
        public virtual Task<bool> LoadResource(ResourceCheckerViewModel checker, ExceptionStack stack) {
            return Task.FromResult(true);
        }

        protected override void OnDisposing() {
            this.Model.OnlineStateChanged -= this.onlineStateChangedHandler;
        }
    }
}