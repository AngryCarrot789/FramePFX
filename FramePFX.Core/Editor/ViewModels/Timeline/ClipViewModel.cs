using System;
using System.Collections.Generic;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    /// <summary>
    /// The base view model for all types of clips (video, audio, etc)
    /// </summary>
    public abstract class ClipViewModel : BaseViewModel, IDisposable {
        /// <summary>
        /// The clip's display/readable name, editable by a user
        /// </summary>
        public string DisplayName {
            get => this.Model.DisplayName;
            set {
                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The layer this clip is located in
        /// </summary>
        public TimelineLayerViewModel Layer { get; private set; }

        public AsyncRelayCommand EditDisplayNameCommand { get; }

        public RelayCommand RemoveClipCommand { get; }

        public ClipModel Model { get; }

        public bool IsDisposing { get; private set; }

        protected ClipViewModel(ClipModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.EditDisplayNameCommand = new AsyncRelayCommand(async () => {
                string name = await IoC.UserInput.ShowSingleInputDialogAsync("Input a new name", "Input a new display name for this clip", this.DisplayName);
                if (name != null) {
                    this.DisplayName = name;
                }
            });

            this.RemoveClipCommand = new RelayCommand(() => {
                this.Layer?.DisposeAndRemoveItemsAction(new List<ClipViewModel>() {this});
            });
        }

        public static void SetLayer(ClipViewModel viewModel, TimelineLayerViewModel layer, bool fireLayerChangedEvent = true) {
            ClipModel.SetLayer(viewModel.Model, layer?.Model, fireLayerChangedEvent);
            viewModel.Layer = layer;
        }

        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack()) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Push(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
                }
            }
        }

        public virtual void OnBeginDispose() {
            this.IsDisposing = true;
        }

        protected virtual void DisposeCore(ExceptionStack stack) {
            if (this.Model is IDisposable disposable) {
                try {
                    disposable.Dispose();
                }
                catch (Exception e) {
                    stack.Push(new Exception("Exception disposing model", e));
                }
            }
        }

        public virtual void OnEndDispose() {
            this.IsDisposing = false;
        }

        public bool IntersectsFrameAt(long frame) => this.Model.IntersectsFrameAt(frame);

        public virtual void OnTimelinePlayBegin() {

        }

        public virtual void OnTimelinePlayEnd() {

        }
    }
}