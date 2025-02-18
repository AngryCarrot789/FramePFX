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

using FramePFX.Editing;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Effects;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Interactivity.Contexts;

namespace FramePFX;

/// <summary>
/// A class which contains all of the general FramePFX data keys
/// </summary>
public static class DataKeys {
    public static readonly DataKey<ITopLevel> TopLevelHostKey = DataKey<ITopLevel>.Create("TopLevel");
    public static readonly DataKey<VideoEditor> VideoEditorKey = DataKey<VideoEditor>.Create("VideoEditor");
    public static readonly DataKey<Project> ProjectKey = DataKey<Project>.Create("Project");
    public static readonly DataKey<Timeline> TimelineKey = DataKey<Timeline>.Create("Timeline");
    public static readonly DataKey<Track> TrackKey = DataKey<Track>.Create("Track");
    public static readonly DataKey<Clip> ClipKey = DataKey<Clip>.Create("Clip");
    public static readonly DataKey<BaseEffect> EffectKey = DataKey<BaseEffect>.Create("Effect");

    public static readonly DataKey<IVideoEditorWindow> VideoEditorUIKey = DataKey<IVideoEditorWindow>.Create("VideoEditorUI");
    public static readonly DataKey<ITimelineElement> TimelineUIKey = DataKey<ITimelineElement>.Create("TimelineUI");
    public static readonly DataKey<ITrackElement> TrackUIKey = DataKey<ITrackElement>.Create("TrackUI");
    public static readonly DataKey<IClipElement> ClipUIKey = DataKey<IClipElement>.Create("ClipUI");

    /// <summary>
    /// A data key for the location of the mouse cursor, in frames, when a context menu
    /// was opened (well, specifically when the track was right clicked)
    /// </summary>
    public static readonly DataKey<long> TrackContextMouseFrameKey = DataKey<long>.Create("TrackFrameContextMousePos");

    /// <summary>
    /// A data key for the data object drop location, in frames. This is basically where the mouse
    /// cursor was when the drop occurred converted into frames
    /// </summary>
    public static readonly DataKey<long> TrackDropFrameKey = DataKey<long>.Create("TrackFrameDropPos");

    public static readonly DataKey<ResourceManager> ResourceManagerKey = DataKey<ResourceManager>.Create("ResourceManager");
    public static readonly DataKey<BaseResource> ResourceObjectKey = DataKey<BaseResource>.Create("ResourceObject");
    public static readonly DataKey<IResourceManagerElement> ResourceManagerUIKey = DataKey<IResourceManagerElement>.Create("ResourceManagerUI");
    public static readonly DataKey<IResourceTreeElement> ResourceTreeUIKey = DataKey<IResourceTreeElement>.Create("ResourceTreeUI");
    public static readonly DataKey<IResourceListElement> ResourceListUIKey = DataKey<IResourceListElement>.Create("ResourceListUI");
    public static readonly DataKey<IResourceTreeNodeElement> ResourceNodeUIKey = DataKey<IResourceTreeNodeElement>.Create("ResourceNode");
}