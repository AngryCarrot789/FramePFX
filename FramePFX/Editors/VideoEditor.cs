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

using System;
using System.Diagnostics;
using System.Numerics;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.History;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors {
    public delegate void ProjectChangedEventHandler(VideoEditor editor, Project oldProject, Project newProject);

    /// <summary>
    /// A general delegate for parameter-less events containing only a reference to the video editor that fired the event
    /// </summary>
    public delegate void VideoEditorEventHandler(VideoEditor editor);

    /// <summary>
    /// The class which stores all of the data for the video editor application
    /// </summary>
    public class VideoEditor {
        private bool showClipAutomation;
        private bool showTrackAutomation;

        /// <summary>
        /// Gets this video editor's history manager, which manages all history actions
        /// </summary>
        public HistoryManager HistoryManager { get; }

        /// <summary>
        /// Gets the project that is currently loaded
        /// </summary>
        public Project Project { get; private set; }

        public PlaybackManager Playback { get; }

        public bool ShowClipAutomation {
            get => this.showClipAutomation;
            set {
                if (this.showClipAutomation == value)
                    return;
                this.showClipAutomation = value;
                this.ShowClipAutomationChanged?.Invoke(this);
            }
        }

        public bool ShowTrackAutomation {
            get => this.showTrackAutomation;
            set {
                if (this.showTrackAutomation == value)
                    return;
                this.showTrackAutomation = value;
                this.ShowTrackAutomationChanged?.Invoke(this);
            }
        }

        public event ProjectChangedEventHandler ProjectChanged;
        public event VideoEditorEventHandler ShowClipAutomationChanged;
        public event VideoEditorEventHandler ShowTrackAutomationChanged;

        public VideoEditor() {
            this.HistoryManager = new HistoryManager();
            this.Playback = new PlaybackManager(this);
            this.Playback.SetFrameRate(new Rational(1, 1));
            this.Playback.StartTimer();
            this.showClipAutomation = true;
        }

        public void LoadDefaultProject() {
            if (this.Project != null) {
                throw new Exception("A project is already loaded");
            }

            Project project = new Project();

            ResourceManager manager = project.ResourceManager;
            ResourceColour id_r = manager.RootContainer.AddItemAndRet(new ResourceColour(220, 25, 25) {DisplayName = "colour_red"});
            ResourceColour id_g = manager.RootContainer.AddItemAndRet(new ResourceColour(25, 220, 25) {DisplayName = "colour_green"});
            ResourceColour id_b = manager.RootContainer.AddItemAndRet(new ResourceColour(25, 25, 220) {DisplayName = "colour_blue"});

            ResourceFolder folder = new ResourceFolder("Extra Colours");
            manager.RootContainer.AddItem(folder);
            ResourceColour id_w = folder.AddItemAndRet(new ResourceColour(220, 220, 220) {DisplayName = "white colour"});
            ResourceColour id_d = folder.AddItemAndRet(new ResourceColour(50, 100, 220) {DisplayName = "idek"});

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

                clip1.ColourKey.SetTargetResourceId(id_r.UniqueId);
                clip1.AutomationData.UpdateBackingStorage();
                track.AddClip(clip1);

                VideoClipShape clip2 = new VideoClipShape {
                    FrameSpan = new FrameSpan(150, 30),
                    DisplayName = "Clip colour_green"
                };

                clip2.SetDefaultValue(VideoClipShape.SizeParameter, new Vector2(200, 200));
                clip2.SetDefaultValue(VideoClip.MediaScaleOriginParameter, new Vector2(100, 100));

                clip2.ColourKey.SetTargetResourceId(id_g.UniqueId);
                clip2.AutomationData.UpdateBackingStorage();
                track.AddClip(clip2);
            }
            {
                VideoTrack track = new VideoTrack() {DisplayName = "Track 2"};
                project.MainTimeline.AddTrack(track);

                VideoClipShape clip1 = new VideoClipShape {
                    FrameSpan = new FrameSpan(300, 90),
                    DisplayName = "Clip colour_blue"
                };

                clip1.SetDefaultValue(VideoClipShape.SizeParameter, 400, 400);

                clip1.ColourKey.SetTargetResourceId(id_b.UniqueId);
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
                clip2.ColourKey.SetTargetResourceId(id_d.UniqueId);
                clip2.AutomationData.UpdateBackingStorage();
                track.AddClip(clip2);
            }

            {
                VideoTrack empty = new VideoTrack() {
                    DisplayName = "Empty Track"
                };
                project.MainTimeline.AddTrack(empty);

                AudioTrack audio = new AudioTrack() {
                    DisplayName = "Audio!!!"
                };
                audio.AddClip(new AudioClip() {FrameSpan = new FrameSpan(100, 200), DisplayName = "An audio clip"});
                project.MainTimeline.AddTrack(audio);
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

            this.Project = project;
            project.Settings.FrameRateChanged += this.OnProjectFrameRateChanged;

            Project.OnOpened(this, project);
            PlaybackManager.InternalOnActiveTimelineChanged(this.Playback, null, project.ActiveTimeline);

            ProjectSettings settings = project.Settings;
            project.ActiveTimeline.RenderManager.UpdateFrameInfo(settings);
            project.ActiveTimeline.RenderManager.InvalidateRender();

            this.Playback.SetFrameRate(settings.FrameRate);
            this.ProjectChanged?.Invoke(this, null, project);
            VideoEditorPropertyEditor.Instance.OnProjectChanged();
        }

        public void CloseProject() {
            Project oldProject = this.Project;
            if (oldProject == null) {
                throw new Exception("There is no project opened");
            }

            PlaybackManager.InternalOnActiveTimelineChanged(this.Playback, oldProject.ActiveTimeline, null);
            oldProject.Settings.FrameRateChanged -= this.OnProjectFrameRateChanged;
            this.Project = null;
            this.ProjectChanged?.Invoke(this, oldProject, null);
            Project.OnClosed(this, oldProject);
            VideoEditorPropertyEditor.Instance.OnProjectChanged();
            this.HistoryManager.Clear();
        }

        private void OnProjectFrameRateChanged(ProjectSettings settings) {
            this.Playback.SetFrameRate(settings.FrameRate);
        }

        internal static void InternalOnActiveTimelineChanged(VideoEditor editor, Timeline oldTimeline, Timeline newTimeline) {
            PlaybackManager.InternalOnActiveTimelineChanged(editor.Playback, oldTimeline, newTimeline);
        }
    }
}