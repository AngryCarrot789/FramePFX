// 
// Copyright (c) 2024-2024 REghZy
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

using System.Diagnostics.CodeAnalysis;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Services.Messaging;
using FramePFX.Utils;

namespace FramePFX.Editing;

/// <summary>
/// A class which manages the behaviour for when a user tries to drop a resource into a timeline
/// </summary>
public sealed class ResourceDropOnTimelineService {
    public static ResourceDropOnTimelineService Instance => Application.Instance.ServiceManager.GetService<ResourceDropOnTimelineService>();

    private readonly Dictionary<Type, IResourceDropHandler> information;

    public ResourceDropOnTimelineService() {
        this.information = new Dictionary<Type, IResourceDropHandler>();
        this.Register(typeof(ResourceColour), new ResourceColourDropHandler());
        this.Register(typeof(ResourceImage), new ResourceImageDropHandler());
        this.Register(typeof(ResourceComposition), new CompositionResourceDropHandler());
    }

    public void Register(Type resourceType, IResourceDropHandler info) {
        Validate.NotNull(resourceType);
        Validate.NotNull(info);

        if (!typeof(ResourceItem).IsAssignableFrom(resourceType))
            throw new ArgumentException("Resource type is not an instance of " + nameof(ResourceItem));

        this.information[resourceType] = info;
    }

    public bool TryGetHandler(Type key, [NotNullWhen(true)] out IResourceDropHandler? value) => this.information.TryGetValue(key, out value);

    private class ResourceImageDropHandler : IResourceDropHandler {
        public long GetClipDurationForDrop(Track track, ResourceItem resource) => 300;

        public async Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span) {
            if (!await HandleGeneralCanDropResource(resource))
                return;

            ResourceImage media = (ResourceImage) resource;
            ImageVideoClip clip = new ImageVideoClip();
            clip.FrameSpan = span;
            clip.ResourceHelper.SetResource(ImageVideoClip.ResourceImageKey, media);

            track.AddClip(clip);
        }
    }

    private class ResourceColourDropHandler : IResourceDropHandler {
        public long GetClipDurationForDrop(Track track, ResourceItem resource) => 300;

        public async Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span) {
            if (!await HandleGeneralCanDropResource(resource))
                return;

            ResourceColour colourRes = (ResourceColour) resource;
            VideoClipShape shape = new VideoClipShape();
            shape.FrameSpan = span;
            shape.ResourceHelper.SetResource(VideoClipShape.ColourKey, colourRes);

            track.AddClip(shape);
        }
    }

    private class CompositionResourceDropHandler : IResourceDropHandler {
        public long GetClipDurationForDrop(Track track, ResourceItem resource) {
            if (resource.Manager == null)
                return -1;

            return ((ResourceComposition) resource).Timeline.LargestFrameInUse;
        }

        public async Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span) {
            if (!await HandleGeneralCanDropResource(resource))
                return;

            ResourceComposition comp = (ResourceComposition) resource;
            CompositionVideoClip clip = new CompositionVideoClip();
            clip.FrameSpan = span;
            await clip.ResourceHelper.SetResourceHelper(CompositionVideoClip.ResourceCompositionKey, comp);

            track.AddClip(clip);
        }
    }

    private static async ValueTask<bool> HandleGeneralCanDropResource(ResourceItem item) {
        if (item.HasReachedResourceLimit()) {
            int count = item.ResourceLinkLimit;
            await IMessageDialogService.Instance.ShowMessage("Resource Limit", $"This resource cannot be used by more than {count} clip{Lang.S(count)}");
            return false;
        }

        return true;
    }
}

/// <summary>
/// An object that handles a resource being dropped in a track. 
/// </summary>
public interface IResourceDropHandler {
    long GetClipDurationForDrop(Track track, ResourceItem resource);

    Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span);
}