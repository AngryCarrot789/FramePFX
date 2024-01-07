using System;
using System.Collections.Generic;
using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Events;
using FramePFX.PropertyEditing.Editors;

namespace FramePFX.PropertyEditing {
    /// <summary>
    /// A class that helps manage the executable state ICommands associated with a data editor when editing clips
    /// </summary>
    public class ClipAutomatableEditorHelper : IDisposable {
        public BaseAutomatablePropertyEditorViewModel Editor { get; }

        private readonly List<ClipViewModel> clips;

        private readonly FrameSeekedEventHandler Handeler1;
        private readonly ClipMovedOverPlayeHeadEventHandler Handeler2;
        private readonly PlayHeadLeaveClipEventHandler Handeler3;

        public Action<ClipAutomatableEditorHelper> OnPlayHeadIntersectionChanged { get; set; }

        public ClipAutomatableEditorHelper(BaseAutomatablePropertyEditorViewModel editor, Action<ClipAutomatableEditorHelper> updateCommands) {
            this.clips = new List<ClipViewModel>();
            this.Editor = editor;
            this.Editor.Loaded += this.OnEditorLoaded;
            this.Editor.Clearing += this.OnEditorClearing;

            this.Handeler1 = this.OnPlayHeadSeekedOnClip;
            this.Handeler2 = this.OnClipMovedOverPlayHead;
            this.Handeler3 = this.OnPlayHeadLeaveClipArea;
            this.OnPlayHeadIntersectionChanged = updateCommands;
        }

        public static ClipAutomatableEditorHelper Create<T>(T editor, Action<T> onUpdateCommands) where T : BaseAutomatablePropertyEditorViewModel {
            return new ClipAutomatableEditorHelper(editor, (e) => {
                onUpdateCommands((T) e.Editor);
            });
        }

        private void OnEditorLoaded(BasePropertyEditorViewModel sender) {
            foreach (object handler in sender.Handlers) {
                if (handler is ClipViewModel clip) {
                    this.clips.Add(clip);
                    clip.FrameSeeked += this.Handeler1;
                    clip.ClipMovedOverPlayHead += this.Handeler2;
                    clip.PlayHeadLeaveClip += this.Handeler3;
                }
            }
        }

        private void OnEditorClearing(BasePropertyEditorViewModel sender) {
            try {
                foreach (ClipViewModel clip in this.clips) {
                    clip.FrameSeeked -= this.Handeler1;
                    clip.ClipMovedOverPlayHead -= this.Handeler2;
                    clip.PlayHeadLeaveClip -= this.Handeler3;
                }
            }
            finally {
                this.clips.Clear();
            }
        }

        private void OnPlayHeadSeekedOnClip(ClipViewModel sender, long oldframe, long newframe) {
            this.OnPlayHeadIntersectionChanged?.Invoke(this);
        }

        private void OnClipMovedOverPlayHead(ClipViewModel clip, long frame) {
            this.OnPlayHeadIntersectionChanged?.Invoke(this);
        }

        private void OnPlayHeadLeaveClipArea(ClipViewModel clip, bool iscausedbyplayheadmovement) {
            this.OnPlayHeadIntersectionChanged?.Invoke(this);
        }

        public void Dispose() {
            this.Editor.Loaded -= this.OnEditorLoaded;
            this.Editor.Clearing -= this.OnEditorClearing;
        }
    }
}