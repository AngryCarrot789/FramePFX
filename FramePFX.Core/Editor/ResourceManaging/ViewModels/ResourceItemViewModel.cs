using System;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceManaging.Events;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public abstract class ResourceItemViewModel : BaseViewModel, IDisposable {
        private readonly ResourceItemEventHandler onlineStateChangedHandler;

        public ResourceItem Model { get; }

        public ResourceManagerViewModel Manager { get; }

        public string UniqueId => this.Model.UniqueId;

        public bool IsRegistered => this.Model.IsRegistered;

        public bool IsOnline {
            get => this.Model.IsOnline;
            protected set => this.Model.IsOnline = value;
        }

        public AsyncRelayCommand RenameCommand { get; }
        public AsyncRelayCommand DeleteCommand { get; }

        protected ResourceItemViewModel(ResourceManagerViewModel manager, ResourceItem model) {
            this.Manager = manager;
            this.Model = model;
            this.RenameCommand = new AsyncRelayCommand(async () => await this.Manager.RenameResourceAction(this));
            this.DeleteCommand = new AsyncRelayCommand(async () => await this.Manager.DeleteResourceAction(this));
            this.onlineStateChangedHandler = (a, b) => this.RaisePropertyChanged(nameof(this.IsOnline));
            model.OnlineStateChanged += this.onlineStateChangedHandler;
        }

        public virtual void Dispose() {
            this.Model.OnlineStateChanged -= this.onlineStateChangedHandler;
            this.Model.Dispose();
        }

        /// <summary>
        /// Validates the state of this resource, setting the <see cref="ResourceItem.IsOnline"/> property to true or false
        /// <para>
        /// When the resource is not in a valid state, an <see cref="InvalidResourceViewModel"/> can be added to the
        /// given <see cref="ResourceCheckerViewModel"/> in which the user can attempt to fix the resource, or ignore and keep it offline
        /// </para>
        /// </summary>
        /// <param name="checker">The checker</param>
        public virtual void Validate(ResourceCheckerViewModel checker) {
            this.Model.IsOnline = true;
        }
    }
}