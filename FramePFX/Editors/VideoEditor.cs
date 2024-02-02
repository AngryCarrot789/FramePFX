using System;
using System.Numerics;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;
using SkiaSharp;

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
        /// <summary>
        /// Gets the singleton instance of the video editor
        /// </summary>
        public static VideoEditor Instance { get; } = new VideoEditor();

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

            ResourceManager manager = project.ResourceManager;
            ResourceColour id_r = manager.RootContainer.AddItemAndRet(new ResourceColour(220, 25, 25) {DisplayName = "colour_red"});
            ResourceColour id_g = manager.RootContainer.AddItemAndRet(new ResourceColour(25, 220, 25) {DisplayName = "colour_green"});
            ResourceColour id_b = manager.RootContainer.AddItemAndRet(new ResourceColour(25, 25, 220) {DisplayName = "colour_blue"});

            ResourceFolder folder = new ResourceFolder("Extra Colours");
            manager.RootContainer.AddItem(folder);
            ResourceColour id_w = folder.AddItemAndRet(new ResourceColour(220, 220, 220) {DisplayName = "white colour"});
            ResourceColour id_d = folder.AddItemAndRet(new ResourceColour(50, 100, 220) {DisplayName = "idek"});

            VideoClipShape CreateShapeAt(Vector2 pos, Vector2 size, FrameSpan span, string name) {
                VideoClipShape shape = new VideoClipShape() {
                    FrameSpan = span
                };
                MotionEffect motion = new MotionEffect();
                motion.AutomationData[MotionEffect.MediaPositionParameter].DefaultKeyFrame.SetVector2Value(pos);
                shape.AddEffect(motion);

                shape.Size = size;
                shape.DisplayName = name;
                motion.AutomationData.UpdateBackingStorage();
                shape.AutomationData.UpdateBackingStorage();
                return shape;
            }

            {
                Track track = new VideoTrack() {DisplayName = "Vid Track 1"};
                track.AddClip(CreateShapeAt(new Vector2(150, 150), new Vector2(100, 30), new FrameSpan(0, 100), "Clip 1"));
                track.AddClip(CreateShapeAt(new Vector2(300, 150), new Vector2(100, 30), new FrameSpan(150, 100), "Clip 2"));
                track.AddClip(CreateShapeAt(new Vector2(450, 150), new Vector2(100, 30), new FrameSpan(300, 250), "Clip 3"));
                project.MainTimeline.AddTrack(track);
            }

            {
                Track track = new VideoTrack() {DisplayName = "Vid Track 2"};
                track.AddClip(CreateShapeAt(new Vector2(150, 300), new Vector2(100, 30), new FrameSpan(100, 50), "Clip 4"));
                track.AddClip(CreateShapeAt(new Vector2(300, 300), new Vector2(100, 30), new FrameSpan(150, 200), "Clip 5"));
                track.AddClip(CreateShapeAt(new Vector2(450, 300), new Vector2(100, 30), new FrameSpan(500, 125), "Clip 6"));
                project.MainTimeline.AddTrack(track);
            }

            {
                Track track = new VideoTrack() {DisplayName = "Vid Track 3!!"};
                track.AddClip(CreateShapeAt(new Vector2(150, 450), new Vector2(100, 30), new FrameSpan(20, 80), "Clip 7"));
                track.AddClip(CreateShapeAt(new Vector2(300, 450), new Vector2(100, 30), new FrameSpan(150, 100), "Clip 8"));
                track.AddClip(CreateShapeAt(new Vector2(450, 450), new Vector2(100, 30), new FrameSpan(350, 200), "Clip 9"));
                project.MainTimeline.AddTrack(track);
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