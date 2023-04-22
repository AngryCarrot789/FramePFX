using System;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Layer.Clips;
using FramePFX.Editor.Timeline.ViewModels.Layer;

namespace FramePFX.Editor.Timeline.ViewModels.Clips {
    public abstract class BaseTimelineClip : BaseViewModel, IDisposable {
        private string header;
        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        /// <summary>
        /// A reference to the actual UI element clip container
        /// </summary>
        public IClipHandle Handle { get; set; }

        /// <summary>
        /// The layer that this clip container is currently in. Should be null if the clip is not yet in a layer or it was removed from the layer
        /// </summary>
        public PFXTimelineLayer Layer { get; set; }

        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }

        private bool isDisposed;
        public bool IsDisposed {
            get => this.isDisposed;
            private set => this.RaisePropertyChanged(ref this.isDisposed, value);
        }

        /// <summary>
        /// Whether this clip is currently in the process of being removed from the timeline
        /// </summary>
        public bool IsRemoving { get; private set; }

        protected bool isTimelinePlaying;
        private bool isProjectRendering;

        protected BaseTimelineClip() {
            this.RenameCommand = new RelayCommand(this.RenameAction);
            this.DeleteCommand = new RelayCommand(() => {
                this.Layer.RemoveClip(this);
            });
        }

        /// <summary>
        /// Returns whether this clip intersects the given video frame. This supports audio clips (or even other clips...?)
        /// </summary>
        public abstract bool IntersectsFrameAt(long frame);

        public virtual void OnTimelinePlayBegin() {
            this.isTimelinePlaying = true;
            // Saves every clip accessing this per tick. Helps performance a bit
            this.isProjectRendering = this.Layer.Timeline.Project.IsRendering;
        }

        public virtual void OnTimelinePlayEnd() {
            this.isTimelinePlaying = false;
            this.isProjectRendering = false;
        }

        private void RenameAction() {
            string newName = CoreIoC.UserInput.ShowSingleInputDialog("Rename clip", "Input a new clip name:", this.Header ?? "");
            if (newName != null) {
                this.Header = newName;
            }
        }

        public void Dispose() {
            this.EnsureNotDisposed("Clip is already disposed. It cannot be disposed again");
            try {
                using (ExceptionStack stack = ExceptionStack.Push("Exception while disposing clip")) {
                    if (this.isTimelinePlaying) {
                        try {
                            this.OnTimelinePlayEnd();
                        }
                        catch (Exception e) {
                            stack.Add(new Exception($"Failed to call {nameof(this.OnTimelinePlayEnd)} while disposing", e));
                        }
                    }

                    this.DisposeClip(stack);
                }
            }
            finally {
                this.IsDisposed = true;
            }
        }

        protected virtual void DisposeClip(ExceptionStack stack) {

        }

        protected void EnsureNotDisposed(string msg = "Clip is disposed; it cannot be used") {
            if (this.IsDisposed) {
                throw new ObjectDisposedException(this.GetType().Name, msg);
            }
        }

        /// <summary>
        /// Clones this clip's data into a new clip
        /// </summary>
        /// <returns></returns>
        public virtual BaseTimelineClip NewInstanceOverride() {
            throw new InvalidOperationException($"{nameof(this.NewInstanceOverride)} is not allowed to be directly called. Use {nameof(this.Clone)} instead");
        }

        public virtual void LoadDataIntoClone(BaseTimelineClip clone) {
            clone.Header = this.Header;
        }

        public virtual BaseTimelineClip Clone() {
            BaseTimelineClip clip = this.NewInstanceOverride();
            this.LoadDataIntoClone(clip);
            return clip;
        }

        /// <summary>
        /// The main function for setting this clip into a removed state
        /// </summary>
        /// <param name="layer"></param>
        public virtual void OnRemoving(PFXTimelineLayer layer) {
            this.IsRemoving = true;
            try {
                if (this.isTimelinePlaying) {
                    this.OnTimelinePlayEnd();
                }

                this.OnRemovingOverride(layer);
                if (!this.isDisposed)
                    this.Dispose();
            }
            finally {
                this.IsRemoving = false;
            }
        }

        /// <summary>
        /// The overridable function for setting this clip into a removed state
        /// </summary>
        /// <param name="layer"></param>
        protected virtual void OnRemovingOverride(PFXTimelineLayer layer) {

        }
    }
}