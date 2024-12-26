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

namespace FramePFX.Editing.ResourceManaging.Resources;

/// <summary>
/// A resource that represents a composition timeline/sequence
/// </summary>
public sealed class ResourceComposition : ResourceItem {
    public CompositionTimeline Timeline { get; }

    public override int ResourceLinkLimit => 1;

    public ResourceComposition() {
        this.Timeline = new CompositionTimeline();
        CompositionTimeline.InternalConstructCompositionTimeline(this);
    }

    static ResourceComposition() {
        SerialisationRegistry.Register<ResourceComposition>(0, (resource, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            resource.Timeline.ReadFromBTE(data.GetDictionary(nameof(resource.Timeline)));
        }, (resource, data, ctx) => {
            ctx.SerialiseBaseType(data);
            resource.Timeline.WriteToBTE(data.CreateDictionary(nameof(resource.Timeline)));
        });
    }

    protected internal override void OnAttachedToManager() {
        base.OnAttachedToManager();
        Timelines.Timeline.InternalSetCompositionTimelineProjectReference(this.Timeline, this.Manager!.Project);
    }

    protected internal override void OnDetachedFromManager() {
        base.OnDetachedFromManager();
        Project project = this.Manager!.Project;
        if (ReferenceEquals(project.ActiveTimeline, this.Timeline))
            project.ActiveTimeline = project.MainTimeline; // sets to main timeline when assigning null
        Timelines.Timeline.InternalSetCompositionTimelineProjectReference(this.Timeline, null);
    }

    public override void Destroy() {
        base.Destroy();
        this.Timeline.Destroy();
    }

    protected override void LoadDataIntoClone(BaseResource clone) {
        base.LoadDataIntoClone(clone);
        this.Timeline.LoadDataIntoClone(((ResourceComposition) clone).Timeline);
    }
}