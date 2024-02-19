//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

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