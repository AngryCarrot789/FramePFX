using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Editor.Project;
using FramePFX.Render;

namespace FramePFX.Editor.Timeline.New {
    public class PFXTimeline {
        public PFXProject Project { get; }
        public List<PFXTimelineLayer> Layers { get; }

        public long MaxDuration { get; set; }

        public long PlayHeadFrame { get; private set; }

        public bool IsAtLastFrame {
            get => this.PlayHeadFrame >= (this.MaxDuration - 1);
        }

        public bool IsRenderDirty { get; set; }

        public delegate void StepFrameEventHandler(long oldFrame, long newFrame);
        public event StepFrameEventHandler OnStepFrame;

        public PFXTimeline(PFXProject project) {
            this.Project = project;
            this.Layers = new List<PFXTimelineLayer>();
        }

        public bool CanRender() {
            return this.Project.Editor.Playback.IsReadyForRender();
        }

        public bool Render() {
            if (this.CanRender()) {
                this.IsRenderDirty = true;
                this.Project.Editor.Playback.RenderTimeline(this);
                this.IsRenderDirty = false;
                return true;
            }

            return false;
        }

        public IEnumerable<PFXClip> GetClipsAtFrame() {
            return this.GetClipsAtFrame(this.PlayHeadFrame);
        }

        public IEnumerable<PFXClip> GetClipsAtFrame(long frame) {
            return this.Layers.SelectMany(layer => layer.GetClipsAtFrame(frame));
        }

        public static long WrapIndex(long index, long endIndex) {
            // only works properly if index is less than (endIndex * 2)
            // e.g. if index is 2005 and endIndex is 1000, this function will return 1005, not 5
            // Assume that will never be the case though...
            return index >= endIndex ? (index - endIndex) : index;
        }

        public void StepFrame(long change = 1L) {
            long duration = this.MaxDuration;
            long oldFrame = this.PlayHeadFrame;
            // Clamp between 0 and max duration. also clamp change in safe duration range
            long newFrame = Math.Max(oldFrame + Maths.Clamp(change, -duration, duration), 0);
            this.PlayHeadFrame = WrapIndex(newFrame, duration);
            this.OnStepFrame?.Invoke(oldFrame, this.PlayHeadFrame);
        }
    }
}