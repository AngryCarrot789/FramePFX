using System;
using FramePFX.Editor.Timelines;
using FramePFX.RBC;

namespace FramePFX.Editor.ResourceManaging.Resources {
    /// <summary>
    /// A resource for storing information about a composition sequence,
    /// which is a separate timeline that can be used as a clip
    /// </summary>
    public class ResourceCompositionSeq : ResourceItem {
        /// <summary>
        /// This composition sequence's timeline
        /// </summary>
        public Timeline Timeline { get; }

        public ResourceCompositionSeq() : this(new Timeline()) {

        }

        public ResourceCompositionSeq(Timeline timeline) {
            this.Timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
        }

        public override void SetManager(ResourceManager manager) {
            base.SetManager(manager);
            this.Timeline.Project = manager?.Project;
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Timeline.ReadFromRBE(data.GetDictionary(nameof(this.Timeline)));
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            this.Timeline.WriteToRBE(data.CreateDictionary(nameof(this.Timeline)));
        }
    }
}