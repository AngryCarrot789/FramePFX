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
using FramePFX.Utils;

namespace FramePFX.Editing;

/// <summary>
/// A class which manages the behaviour for when a user tries to drop a resource into a timeline
/// </summary>
public sealed class ResourceToClipDropRegistry
{
    public static ResourceToClipDropRegistry Instance => Application.Instance.Services.GetService<ResourceToClipDropRegistry>();
    
    private readonly Dictionary<Type, IResourceDropInformation> information;
    
    public ResourceToClipDropRegistry()
    {
        this.information = new Dictionary<Type, IResourceDropInformation>();
    }

    public void RegisterStandard()
    {
        this.Register(typeof(ResourceAVMedia), new AVMediaDropInformation());
        this.Register(typeof(ResourceColour), new ResourceColourDropInformation());
        this.Register(typeof(ResourceImage), new ResourceImageDropInformation());
        this.Register(typeof(ResourceComposition), new CompositionResourceDropInformation());
    }

    public void Register(Type resourceType, IResourceDropInformation info)
    {
        Validate.NotNull(resourceType);
        Validate.NotNull(info);

        if (!typeof(ResourceItem).IsAssignableFrom(resourceType))
            throw new ArgumentException("Resource type is not an instance of " + nameof(ResourceItem));
        
        this.information[resourceType] = info;
    }

    public bool TryGetValue(Type key, [NotNullWhen(true)] out IResourceDropInformation? value) => this.information.TryGetValue(key, out value);
    
    private class AVMediaDropInformation : IResourceDropInformation
    {
        public long GetClipDurationForDrop(Track track, ResourceItem resource)
        {
            if (resource.Manager == null)
                return -1;

            TimeSpan duration = ((ResourceAVMedia) resource).GetDuration();
            double fps = resource.Manager.Project.Settings.FrameRate.AsDouble;

            return (long) (duration.TotalSeconds * fps);
        }

        public async Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span)
        {
            if (!await HandleGeneralCanDropResource(resource))
                return;

            ResourceAVMedia media = (ResourceAVMedia) resource;
            AVMediaVideoClip clip = new AVMediaVideoClip();
            clip.FrameSpan = span;
            await clip.ResourceHelper.SetResourceHelper(AVMediaVideoClip.MediaKey, media);

            track.AddClip(clip);
        }
    }

    private class ResourceImageDropInformation : IResourceDropInformation
    {
        public long GetClipDurationForDrop(Track track, ResourceItem resource) => 300;

        public async Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span)
        {
            if (!await HandleGeneralCanDropResource(resource))
                return;

            ResourceImage media = (ResourceImage) resource;
            ImageVideoClip clip = new ImageVideoClip();
            clip.FrameSpan = span;
            clip.ResourceHelper.SetResource(ImageVideoClip.ResourceImageKey, media);

            track.AddClip(clip);
        }
    }

    private class ResourceColourDropInformation : IResourceDropInformation
    {
        public long GetClipDurationForDrop(Track track, ResourceItem resource) => 300;

        public async Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span)
        {
            if (!await HandleGeneralCanDropResource(resource))
                return;

            ResourceColour colourRes = (ResourceColour) resource;
            VideoClipShape shape = new VideoClipShape();
            shape.FrameSpan = span;
            shape.ResourceHelper.SetResource(VideoClipShape.ColourKey, colourRes);

            track.AddClip(shape);
        }
    }

    private class CompositionResourceDropInformation : IResourceDropInformation
    {
        public long GetClipDurationForDrop(Track track, ResourceItem resource)
        {
            if (resource.Manager == null)
                return -1;

            return ((ResourceComposition) resource).Timeline.LargestFrameInUse;
        }

        public async Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span)
        {
            if (!await HandleGeneralCanDropResource(resource))
                return;

            ResourceComposition comp = (ResourceComposition) resource;
            CompositionVideoClip clip = new CompositionVideoClip();
            clip.FrameSpan = span;
            await clip.ResourceHelper.SetResourceHelper(CompositionVideoClip.ResourceCompositionKey, comp);

            track.AddClip(clip);
        }
    }
    
    private static async ValueTask<bool> HandleGeneralCanDropResource(ResourceItem item)
    {
        if (item.HasReachedResourceLimit())
        {
            int count = item.ResourceLinkLimit;
            await IoC.MessageService.ShowMessage("Resource Limit", $"This resource cannot be used by more than {count} clip{Lang.S(count)}");
            return false;
        }

        return true;
    }
}

public interface IResourceDropInformation
{
    long GetClipDurationForDrop(Track track, ResourceItem resource);
    
    Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span);
}