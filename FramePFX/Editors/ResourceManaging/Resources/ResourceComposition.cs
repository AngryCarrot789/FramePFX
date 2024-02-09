using FramePFX.RBC;

namespace FramePFX.Editors.ResourceManaging.Resources {
    /// <summary>
    /// A resource that represents a composition timeline/sequence
    /// </summary>
    public sealed class ResourceComposition : ResourceItem {
        public CompositionTimeline Timeline { get; }

        public ResourceComposition() {
            this.Timeline = new CompositionTimeline();
            CompositionTimeline.InternalConstructCompositionTimeline(this);
        }

        protected internal override void OnAttachedToManager() {
            base.OnAttachedToManager();
            Timelines.Timeline.InternalSetCompositionTimelineProjectReference(this.Timeline, this.Manager.Project);
        }

        protected internal override void OnDetatchedFromManager() {
            base.OnDetatchedFromManager();
            Project project = this.Manager.Project;
            if (ReferenceEquals(project.ActiveTimeline, this.Timeline))
                project.ActiveTimeline = null; // sets to main timeline when assigning null
            Timelines.Timeline.InternalSetCompositionTimelineProjectReference(this.Timeline, null);
        }

        public override void Destroy() {
            base.Destroy();
            this.Timeline.Destroy();
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Timeline.ReadFromRBE(data.GetDictionary(nameof(this.Timeline)));
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            this.Timeline.WriteToRBE(data.CreateDictionary(nameof(this.Timeline)));
        }

        protected override void LoadDataIntoClone(BaseResource clone) {
            base.LoadDataIntoClone(clone);
            this.Timeline.LoadDataIntoClone(((ResourceComposition) clone).Timeline);
        }
    }
}