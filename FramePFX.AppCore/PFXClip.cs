using System;
using FramePFX.Core.Utils;

namespace FramePFX.Editor.Timeline.New {
    public abstract class PFXClip : IDisposable {
        public PFXTimelineLayer Layer { get; set; }

        public string Header { get; set; }

        public bool IsDisposed { get; private set;  }

        protected bool isTimelinePlaying;
        protected bool isProjectRendering;
        protected bool isRemoving;

        protected PFXClip() {

        }

        /// <summary>
        /// Returns whether this clip intersects the given video frame. This also supports audio clips
        /// </summary>
        public abstract bool IntersectsFrameAt(long frame);

        public virtual void OnTimelinePlayBegin() {
            this.isTimelinePlaying = true;
            this.isProjectRendering = this.Layer.Timeline.Project.IsRendering;
        }

        public virtual void OnTimelinePlayEnd() {
            this.isTimelinePlaying = false;
            this.isProjectRendering = false;
        }

        public void Dispose() {
            this.EnsureNotDisposed("Clip is already disposed. It cannot be disposed again");
            try {
                using (ExceptionStack stack = ExceptionStack.Push()) {
                    if (this.isTimelinePlaying) {
                        try {
                            this.OnTimelinePlayEnd();
                        }
                        catch (Exception e) {
                            stack.Add(new Exception($"Failed to call {nameof(this.OnTimelinePlayEnd)} while disposing", e));
                        }
                    }

                    this.DisposeResource(stack);
                }
            }
            finally {
                this.IsDisposed = true;
            }
        }

        protected virtual void DisposeResource(ExceptionStack stack) {

        }

        protected void EnsureNotDisposed(string msg = "Clip is disposed; it cannot be used") {
            if (this.IsDisposed) {
                throw new ObjectDisposedException(this.GetType().Name, msg);
            }
        }

        /// <summary>
        /// Creates a new instance of this clip. Care must be taken to override this when a
        /// derived class already overrides this function
        /// <para>
        /// This method doesn't need to be overridden if <see cref="Clone"/> is manually overridden. This just
        /// simplified the cloning process (instead of having to call <see cref="LoadDataIntoClone"/>) each time
        /// </para>
        /// </summary>
        /// <returns></returns>
        protected virtual PFXClip NewInstanceCore() {
            throw new InvalidOperationException($"{nameof(this.Clone)} should be used instead of {nameof(this.NewInstanceCore)}");
        }

        /// <summary>
        /// Creates a new clone of this clip
        /// </summary>
        /// <returns></returns>
        public virtual PFXClip Clone() {
            PFXClip clip = this.NewInstanceCore();
            this.LoadDataIntoClone(clip);
            return clip;
        }

        public virtual void LoadDataIntoClone(PFXClip clone) {
            clone.Header = this.Header;
        }

        public void OnRemovingCore(PFXTimelineLayer layer) {
            this.isRemoving = true;
            try {
                if (this.isTimelinePlaying) {
                    this.OnTimelinePlayEnd();
                }

                this.OnRemoving(layer);
                if (!this.IsDisposed)
                    this.Dispose();
            }
            finally {
                this.isRemoving = false;
            }
        }

        protected virtual void OnRemoving(PFXTimelineLayer layer) {

        }
    }
}