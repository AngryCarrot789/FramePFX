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

using System.Diagnostics;
using System.Numerics;
using Fractions;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Editing.Timelines.Tracks;
using PFXToolKitUI;
using PFXToolKitUI.History;
using PFXToolKitUI.Services;
using PFXToolKitUI.Utils.Destroying;

namespace FramePFX.Editing;

/// <summary>
/// The model class for an editor window
/// </summary>
public class VideoEditor : IServiceable, IDestroy {
    private bool isExporting;

    /// <summary>
    /// Gets this video editor's history manager, which manages all history actions
    /// </summary>
    public HistoryManager HistoryManager { get; }

    /// <summary>
    /// Gets the project that is currently loaded
    /// </summary>
    public Project? Project { get; private set; }

    public PlaybackManager Playback { get; }

    public ServiceManager ServiceManager { get; }

    /// <summary>
    /// Gets or sets if this editor is being used to export at the current moment
    /// </summary>
    public bool IsExporting {
        get => this.isExporting;
        set {
            if (this.isExporting == value)
                return;

            this.isExporting = value;
            VideoEditorListener.GetInstance(this).InternalOnIsExportingChanged(this);
        }
    }

    public VideoEditor() {
        this.HistoryManager = new HistoryManager();
        this.ServiceManager = new ServiceManager();
        this.Playback = new PlaybackManager(this);
        this.Playback.SetFrameRate(new Fraction(1, 1));
        this.Playback.StartTimer();
    }

    public void LoadDefaultProject() {
        if (this.Project != null) {
            throw new Exception("A project is already loaded");
        }

        Project project = new Project() {
            ProjectName = "Default Project"
        };

        ResourceManager manager = project.ResourceManager;
        ResourceColour id_r = manager.RootContainer.AddItemAndRet(new ResourceColour(220, 25, 25) { DisplayName = "colour_red" });
        ResourceColour id_g = manager.RootContainer.AddItemAndRet(new ResourceColour(25, 220, 25) { DisplayName = "colour_green" });
        ResourceColour id_b = manager.RootContainer.AddItemAndRet(new ResourceColour(25, 25, 220) { DisplayName = "colour_blue" });

        ResourceFolder folder = new ResourceFolder("Extra Colours");
        manager.RootContainer.AddItem(folder);
        ResourceColour id_w = folder.AddItemAndRet(new ResourceColour(220, 220, 220) { DisplayName = "white colour" });
        ResourceColour id_d = folder.AddItemAndRet(new ResourceColour(50, 100, 220) { DisplayName = "idek" });

        {
            VideoTrack track = new VideoTrack() {
                DisplayName = "Track 1 with stuff"
            };
            project.MainTimeline.AddTrack(track);
            track.AutomationData[VideoTrack.OpacityParameter].AddKeyFrame(new KeyFrameDouble(0, 0.3d));
            track.AutomationData[VideoTrack.OpacityParameter].AddKeyFrame(new KeyFrameDouble(50, 0.5d));
            track.AutomationData[VideoTrack.OpacityParameter].AddKeyFrame(new KeyFrameDouble(100, 1d));
            track.AutomationData.ActiveParameter = VideoTrack.OpacityParameter.Key;

            VideoClipShape clip1 = new VideoClipShape {
                FrameSpan = new FrameSpan(0, 120),
                DisplayName = "Clip colour_red"
            };

            clip1.SetDefaultValue(VideoClipShape.SizeParameter, 200, 200);

            clip1.ResourceHelper.SetResource(VideoClipShape.ColourKey, id_r);
            clip1.AutomationData.UpdateBackingStorage();
            track.AddClip(clip1);

            VideoClipShape clip2 = new VideoClipShape {
                FrameSpan = new FrameSpan(150, 30),
                DisplayName = "Clip colour_green"
            };

            clip2.SetDefaultValue(VideoClipShape.SizeParameter, new Vector2(200, 200));
            VideoClip.MediaScaleOriginParameter.SetValue(clip2, new Vector2(100, 100));

            clip2.ResourceHelper.SetResource(VideoClipShape.ColourKey, id_g);
            clip2.AutomationData.UpdateBackingStorage();
            track.AddClip(clip2);
        }
        {
            VideoTrack track = new VideoTrack() { DisplayName = "Track 2" };
            project.MainTimeline.AddTrack(track);

            VideoClipShape clip1 = new VideoClipShape {
                FrameSpan = new FrameSpan(300, 90),
                DisplayName = "Clip colour_blue"
            };

            clip1.SetDefaultValue(VideoClipShape.SizeParameter, 400, 400);

            clip1.ResourceHelper.SetResource(VideoClipShape.ColourKey, id_b);
            clip1.AutomationData.UpdateBackingStorage();
            track.AddClip(clip1);
            VideoClipShape clip2 = new VideoClipShape {
                FrameSpan = new FrameSpan(15, 130),
                DisplayName = "Clip blueish"
            };

            clip2.SetDefaultValue(VideoClipShape.SizeParameter, 100, 1000);
            clip2.AutomationData[VideoClip.MediaPositionParameter].AddKeyFrame(new KeyFrameVector2(10L, Vector2.Zero));
            clip2.AutomationData[VideoClip.MediaPositionParameter].AddKeyFrame(new KeyFrameVector2(75L, new Vector2(100, 200)));
            clip2.AutomationData[VideoClip.MediaPositionParameter].AddKeyFrame(new KeyFrameVector2(90L, new Vector2(400, 400)));
            clip2.AutomationData[VideoClip.MediaPositionParameter].AddKeyFrame(new KeyFrameVector2(115L, new Vector2(100, 700)));
            clip2.AutomationData.ActiveParameter = VideoClip.MediaPositionParameter.Key;
            clip2.ResourceHelper.SetResource(VideoClipShape.ColourKey, id_d);
            clip2.AutomationData.UpdateBackingStorage();
            track.AddClip(clip2);
        }

        {
            VideoTrack empty = new VideoTrack() {
                DisplayName = "Empty Track"
            };
            project.MainTimeline.AddTrack(empty);

            // AudioTrack audio = new AudioTrack() {
            //     DisplayName = "Audio!!!"
            // };
            // audio.AddClip(new AudioClip() { FrameSpan = new FrameSpan(0, 200), DisplayName = "An audio clip" });
            // project.MainTimeline.AddTrack(audio);
        }

        project.SetUnModified();
        this.SetProject(project);
        Debug.Assert(project.IsModified == false, "Expected editor not to modify project while setting it as the active project");
    }

    public void SetProject(Project project) {
        if (this.Project != null)
            throw new Exception("A project is already loaded; it must be unloaded first (CloseProject)");
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        VideoEditorListener listener = VideoEditorListener.GetInstance(this);
        this.Project = project;
        listener.InternalOnProjectLoading(this, project);

        project.Settings.FrameRateChanged += this.OnProjectFrameRateChanged;

        Project.OnOpened(this, project);
        PlaybackManager.InternalOnActiveTimelineChanged(this.Playback, null, project.ActiveTimeline);

        ProjectSettings settings = project.Settings;
        project.ActiveTimeline.RenderManager.UpdateFrameInfo(settings);

        this.Playback.SetFrameRate(settings.FrameRate);

        listener.InternalOnProjectLoaded(this, project);

        ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
            this.Project?.ActiveTimeline.InvalidateRender();
        }, DispatchPriority.Background);
    }

    public void CloseProject() {
        Project? myProject = this.Project;
        if (myProject == null)
            throw new Exception("There is no project opened");

        VideoEditorListener listener = VideoEditorListener.GetInstance(this);
        listener.InternalOnProjectUnloading(this, myProject);

        if (this.Playback.PlayState != PlayState.Stop) {
            this.Playback.Stop();
        }

        PlaybackManager.InternalOnActiveTimelineChanged(this.Playback, myProject.ActiveTimeline, null);
        myProject.Settings.FrameRateChanged -= this.OnProjectFrameRateChanged;
        this.HistoryManager.Clear();
        listener.InternalOnProjectUnloaded(this, myProject);

        this.Project = null;
        Project.OnClosed(this, myProject);
    }

    private void OnProjectFrameRateChanged(ProjectSettings settings) {
        this.Playback.SetFrameRate(settings.FrameRate);
    }

    internal static void InternalOnActiveTimelineChanged(VideoEditor editor, Timeline oldTimeline, Timeline newTimeline) {
        PlaybackManager.InternalOnActiveTimelineChanged(editor.Playback, oldTimeline, newTimeline);
    }

    public void Destroy() {
        this.Playback.Stop();
        this.Playback.StopTimer();
    }
}