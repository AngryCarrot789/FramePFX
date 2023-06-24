using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public abstract class ResourceItemViewModel : BaseResourceObjectViewModel, IDisposable {
        private readonly ResourceItemEventHandler onlineStateChangedHandler;

        public new ResourceItem Model => (ResourceItem) base.Model;

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

            this.SetOfflineCommand = new AsyncRelayCommand(async () => {
                using (ExceptionStack stack = new ExceptionStack(false)) {
                    await this.Model.DisableAsync(stack, true);
                    if (stack.TryGetException(out Exception exception)) {
                        await IoC.MessageDialogs.ShowMessageExAsync("Exception setting offline", "An exception occurred while setting resource to offline", exception.GetToString());
                    }
                }
            }, () => this.IsOnline);

            this.SetOnlineCommand = new AsyncRelayCommand(async () => {
                await ResourceCheckerViewModel.LoadResources(new List<ResourceItemViewModel>() {this}, true);
            }, () => !this.IsOnline);
        }

        public override async Task<bool> RenameSelfAction() {
            string newId;
            if (this.Manager == null) {
                newId = await IoC.UserInput.ShowSingleInputDialogAsync("Input a resource ID", "Input a new UUID for the resource", this.UniqueId ?? "Resource ID Here");
            }
            else {
                newId = await this.Manager.SelectNewResourceId("Input a new UUID for the resource", this.UniqueId);
            }

            if (newId == null) {
                return false;
            }
            else if (string.IsNullOrWhiteSpace(newId)) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid UUID", "UUID cannot be an empty string or consist of only whitespaces");
                return false;
            }
            else if (this.Manager != null && !this.Manager.Model.EntryExists(this.UniqueId)) {
                await IoC.MessageDialogs.ShowMessageAsync("Resource no long exists", "This resource is not registered...");
                return false;
            }
            else if (this.Manager != null && this.Manager.Model.EntryExists(newId)) {
                await IoC.MessageDialogs.ShowMessageAsync("Resource already exists", "Resource already exists with the UUID: " + newId);
                return false;
            }
            else {
                if (this.Manager != null) {
                    this.Manager.Model.RenameEntry(this.Model, newId);
                }
                else {
                    ResourceItem.SetUniqueId(this.Model, newId);
                }

                this.RaisePropertyChanged(nameof(this.UniqueId));
                return true;
            }
        }

        public override async Task<bool> DeleteSelfAction() {
            if (this.Parent == null) {
                await IoC.MessageDialogs.ShowMessageExAsync("Invalid item", "This resource is not located anywhere...?", new Exception().GetToString());
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.UniqueId)) {
                await IoC.MessageDialogs.ShowMessageExAsync("Invalid item", "This resource has not been registered yet", new Exception().GetToString());
                return false;
            }

            if (await IoC.MessageDialogs.ShowDialogAsync("Delete resource?", $"Delete resource '{this.UniqueId}'?", MsgDialogType.OKCancel) != MsgDialogResult.OK) {
                return false;
            }

            this.Manager?.Model.DeleteEntryByItem(this.Model);
            this.Parent.RemoveItem(this, true, true);

            try {
                this.Dispose();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error disposing item", "Failed to dispose resource", e.GetToString());
            }

            return true;
        }

        protected override bool CanRename() {
            return this.Model.IsRegistered(out _);
        }

        protected override bool CanDelete() {
            return this.Model.IsRegistered(out _);
        }

        public override void Dispose() {
            this.Model.OnlineStateChanged -= this.onlineStateChangedHandler;
            base.Dispose();
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
    }
}