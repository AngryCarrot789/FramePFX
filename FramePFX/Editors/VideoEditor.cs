using System;
using System.Numerics;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Utils;

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

            // Old default project
            // ResourceManager manager = project.ResourceManager;
            // ResourceColour id_r = manager.RootContainer.AddItemAndRet(new ResourceColour(220, 25, 25) {DisplayName = "colour_red"});
            // ResourceColour id_g = manager.RootContainer.AddItemAndRet(new ResourceColour(25, 220, 25) {DisplayName = "colour_green"});
            // ResourceColour id_b = manager.RootContainer.AddItemAndRet(new ResourceColour(25, 25, 220) {DisplayName = "colour_blue"});
            //
            // ResourceFolder folder = new ResourceFolder("Extra Colours");
            // manager.RootContainer.AddItem(folder);
            // ResourceColour id_w = folder.AddItemAndRet(new ResourceColour(220, 220, 220) {DisplayName = "white colour"});
            // ResourceColour id_d = folder.AddItemAndRet(new ResourceColour(50, 100, 220) {DisplayName = "idek"});
            //
            // VideoClipShape CreateShapeAt(Vector2 pos, Vector2 size, FrameSpan span, string name) {
            //     VideoClipShape shape = new VideoClipShape() {
            //         FrameSpan = span
            //     };
            //     MotionEffect motion = new MotionEffect();
            //     motion.AutomationData[MotionEffect.MediaPositionParameter].DefaultKeyFrame.SetVector2Value(pos);
            //     shape.AddEffect(motion);
            //
            //     shape.Size = size;
            //     shape.DisplayName = name;
            //     motion.AutomationData.UpdateBackingStorage();
            //     shape.AutomationData.UpdateBackingStorage();
            //     return shape;
            // }
            //
            // {
            //     Track track = new VideoTrack() {DisplayName = "Vid Track 1"};
            //     track.AddClip(CreateShapeAt(new Vector2(150, 150), new Vector2(100, 30), new FrameSpan(0, 100), "Clip 1"));
            //     track.AddClip(CreateShapeAt(new Vector2(300, 150), new Vector2(100, 30), new FrameSpan(150, 100), "Clip 2"));
            //     track.AddClip(CreateShapeAt(new Vector2(450, 150), new Vector2(100, 30), new FrameSpan(300, 250), "Clip 3"));
            //     track.AddEffect(new MotionEffect());
            //     project.MainTimeline.AddTrack(track);
            // }
            //
            // {
            //     Track track = new VideoTrack() {DisplayName = "Vid Track 2"};
            //     track.AddClip(CreateShapeAt(new Vector2(150, 300), new Vector2(100, 30), new FrameSpan(100, 50), "Clip 4"));
            //     track.AddClip(CreateShapeAt(new Vector2(300, 300), new Vector2(100, 30), new FrameSpan(150, 200), "Clip 5"));
            //     track.AddClip(CreateShapeAt(new Vector2(450, 300), new Vector2(100, 30), new FrameSpan(500, 125), "Clip 6"));
            //     track.AddEffect(new MotionEffect());
            //     project.MainTimeline.AddTrack(track);
            // }
            //
            // {
            //     Track track = new VideoTrack() {DisplayName = "Vid Track 3!!"};
            //     track.AddClip(CreateShapeAt(new Vector2(150, 450), new Vector2(100, 30), new FrameSpan(20, 80), "Clip 7"));
            //     track.AddClip(CreateShapeAt(new Vector2(300, 450), new Vector2(100, 30), new FrameSpan(150, 100), "Clip 8"));
            //     track.AddClip(CreateShapeAt(new Vector2(450, 450), new Vector2(100, 30), new FrameSpan(350, 200), "Clip 9"));
            //     track.AddEffect(new MotionEffect());
            //     project.MainTimeline.AddTrack(track);
            // }

            ResourceManager manager = project.ResourceManager;
            ResourceColour id_r = manager.RootContainer.AddItemAndRet(new ResourceColour(220, 25, 25) {DisplayName = "colour_red"});
            ResourceColour id_g = manager.RootContainer.AddItemAndRet(new ResourceColour(25, 220, 25) {DisplayName = "colour_green"});
            ResourceColour id_b = manager.RootContainer.AddItemAndRet(new ResourceColour(25, 25, 220) {DisplayName = "colour_blue"});

            ResourceFolder folder = new ResourceFolder("Extra Colours");
            manager.RootContainer.AddItem(folder);
            ResourceColour id_w = folder.AddItemAndRet(new ResourceColour(220, 220, 220) {DisplayName = "white colour"});
            ResourceColour id_d = folder.AddItemAndRet(new ResourceColour(50, 100, 220) {DisplayName = "idek"});

            MotionEffect motion;
            {
                VideoTrack track = new VideoTrack() {
                    DisplayName = "Track 1 with stuff"
                };
                track.AddEffect(new MotionEffect());
                project.MainTimeline.AddTrack(track);
                track.AutomationData[VideoTrack.OpacityParameter].AddKeyFrame(new KeyFrameDouble(0, 0.3d));
                track.AutomationData[VideoTrack.OpacityParameter].AddKeyFrame(new KeyFrameDouble(50, 0.5d));
                track.AutomationData[VideoTrack.OpacityParameter].AddKeyFrame(new KeyFrameDouble(100, 1d));
                track.AutomationData.ActiveParameter = VideoTrack.OpacityParameter.Key;

                VideoClipShape clip1 = new VideoClipShape {
                    FrameSpan = new FrameSpan(0, 120),
                    DisplayName = "Clip colour_red"
                };

                clip1.AutomationData[VideoClipShape.SizeParameter].DefaultKeyFrame.SetVector2Value(200, 200, VideoClipShape.SizeParameter.Descriptor);

                clip1.AddEffect(motion = new MotionEffect());

                clip1.ColourKey.SetTargetResourceId(id_r.UniqueId);
                clip1.AutomationData.UpdateBackingStorage();
                track.AddClip(clip1);

                VideoClipShape clip2 = new VideoClipShape {
                    FrameSpan = new FrameSpan(150, 30),
                    DisplayName = "Clip colour_green"
                };

                clip2.AutomationData[VideoClipShape.SizeParameter].DefaultKeyFrame.SetVector2Value(200, 200, VideoClipShape.SizeParameter.Descriptor);

                clip2.AddEffect(motion = new MotionEffect());
                motion.MediaScaleOrigin = new Vector2(100, 100);

                clip2.ColourKey.SetTargetResourceId(id_g.UniqueId);
                clip2.AutomationData.UpdateBackingStorage();
                track.AddClip(clip2);
            }
            {
                VideoTrack track = new VideoTrack() {DisplayName = "Track 2"};
                track.AddEffect(new MotionEffect());
                project.MainTimeline.AddTrack(track);

                VideoClipShape clip1 = new VideoClipShape {
                    FrameSpan = new FrameSpan(300, 90),
                    DisplayName = "Clip colour_blue"
                };

                clip1.AutomationData[VideoClipShape.SizeParameter].DefaultKeyFrame.SetVector2Value(400, 400, VideoClipShape.SizeParameter.Descriptor);
                clip1.AddEffect(motion = new MotionEffect());

                clip1.ColourKey.SetTargetResourceId(id_b.UniqueId);
                clip1.AutomationData.UpdateBackingStorage();
                track.AddClip(clip1);
                VideoClipShape clip2 = new VideoClipShape {
                    FrameSpan = new FrameSpan(15, 130),
                    DisplayName = "Clip blueish"
                };

                clip2.AutomationData[VideoClipShape.SizeParameter].DefaultKeyFrame.SetVector2Value(100, 1000, VideoClipShape.SizeParameter.Descriptor);
                clip2.AddEffect(motion = new MotionEffect());
                motion.AutomationData[MotionEffect.MediaPositionParameter].AddKeyFrame(new KeyFrameVector2(10L, Vector2.Zero));
                motion.AutomationData[MotionEffect.MediaPositionParameter].AddKeyFrame(new KeyFrameVector2(75L, new Vector2(100, 200)));
                motion.AutomationData[MotionEffect.MediaPositionParameter].AddKeyFrame(new KeyFrameVector2(90L, new Vector2(400, 400)));
                motion.AutomationData[MotionEffect.MediaPositionParameter].AddKeyFrame(new KeyFrameVector2(115L, new Vector2(100, 700)));
                motion.AutomationData.ActiveParameter = MotionEffect.MediaPositionParameter.Key;
                clip2.ColourKey.SetTargetResourceId(id_d.UniqueId);
                clip2.AutomationData.UpdateBackingStorage();
                track.AddClip(clip2);
            }

            {
                VideoTrack empty = new VideoTrack() {
                    DisplayName = "Empty Track"
                };
                empty.AddEffect(new MotionEffect());
                project.MainTimeline.AddTrack(empty);
            }

            this.SetProject(project);
        }

        public void SetProject(Project project) {
            if (this.Project != null) {
                throw new Exception("A project is already loaded; it must be unloaded first");
            }

            project.Settings.FrameRateChanged += this.OnProjectFrameRateChanged;
            this.Project = project;

            Project.OnOpened(this, project);
            this.ProjectChanged?.Invoke(this, null, project);

            ProjectSettings settings = project.Settings;
            project.RenderManager.UpdateFrameInfo();
            project.RenderManager.InvalidateRender();

            // TODO: add event for FrameRate
            this.Playback.SetFrameRate(settings.FrameRate);
        }

        public void CloseProject() {
            Project oldProject = this.Project;
            if (oldProject == null) {
                throw new Exception("There is no project opened");
            }

            oldProject.Settings.FrameRateChanged -= this.OnProjectFrameRateChanged;
            oldProject.Destroy();
            this.Project = null;
            this.ProjectChanged?.Invoke(this, oldProject, null);
            Project.OnClosed(this, oldProject);
        }

        private void OnProjectFrameRateChanged(ProjectSettings settings) {
            this.Playback.SetFrameRate(settings.FrameRate);
        }
    }
}