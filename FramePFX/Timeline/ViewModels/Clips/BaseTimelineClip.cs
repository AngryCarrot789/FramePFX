using System;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Timeline.Layer.Clips;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline.ViewModels.Clips {
    public abstract class BaseTimelineClip : BaseViewModel, IDisposable {
        private string name;
        public string Name {
            get => this.name;
            set => this.RaisePropertyChanged(ref this.name, value);
        }

        /// <summary>
        /// A reference to the actual UI element clip container
        /// </summary>
        public IClipHandle Handle { get; set; }

        /// <summary>
        /// The layer that this clip container is currently in. Should be null if the clip is not yet in a layer or it was removed from the layer
        /// </summary>
        public BaseTimelineLayer Layer { get; set; }

        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }

        private bool isDisposed;
        public bool IsDisposed {
            get => this.isDisposed;
            private set => this.RaisePropertyChanged(ref this.isDisposed, value);
        }

        private bool isDisposing;
        public bool IsDisposing {
            get => this.isDisposing;
            private set => this.RaisePropertyChanged(ref this.isDisposing, value);
        }

        protected bool isTimelinePlaying;
        protected bool isRemoving;

        protected BaseTimelineClip() {
            this.RenameCommand = new RelayCommand(this.RenameAction);
            this.DeleteCommand = new RelayCommand(() => {
                this.Layer.RemoveClip(this);
            });
        }

        /// <summary>
        /// Returns whether this clip intersects the given video frame. This also supports audio clips
        /// </summary>
        public abstract bool IntersectsFrameAt(long frame);

        public virtual void OnTimelinePlayBegin() {
            this.isTimelinePlaying = true;
        }

        public virtual void OnTimelinePlayEnd() {
            this.isTimelinePlaying = false;
        }

        private void RenameAction() {
            string newName = CoreIoC.UserInput.ShowSingleInputDialog("Rename clip", "Input a new clip name:", this.Name ?? "");
            if (newName != null) {
                this.Name = newName;
            }
        }

        public void Dispose() {
            this.ThrowIfDisposed();
            try {
                this.IsDisposing = true;
                this.DisposeClip();
            }
            finally {
                this.IsDisposed = true;
                this.IsDisposing = false;
            }
        }

        protected virtual void DisposeClip() {

        }

        protected void ThrowIfDisposed() {
            if (this.IsDisposed) {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        /// <summary>
        /// Clones this clip's data into a new clip
        /// </summary>
        /// <returns></returns>
        public abstract BaseTimelineClip CloneInstance();

        public virtual void LoadDataIntoClone(BaseTimelineClip clone) {
            clone.Name = this.Name;
        }

        public void OnRemovingCore(BaseTimelineLayer layer) {
            this.isRemoving = true;
            try {
                if (this.isTimelinePlaying) {
                    this.OnTimelinePlayEnd();
                }

                this.OnRemoving(layer);
                this.Dispose();
            }
            finally {
                this.isRemoving = false;
            }
        }

        protected virtual void OnRemoving(BaseTimelineLayer layer) {

        }
    }
}