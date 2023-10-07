using System.Collections.Generic;
using FramePFX.Editor.Timelines;

namespace FramePFX.Editor {
    public class VideoEditor {
        private readonly List<Timeline> activeTimelines;
        public volatile bool IsProjectSaving;
        public volatile bool IsProjectChanging;

        public bool CanRender => !this.IsProjectSaving && !this.IsProjectChanging;

        /// <summary>
        /// Gets the editor playback instance for this video editor
        /// </summary>
        public EditorPlayback Playback { get; }

        /// <summary>
        /// Gets the active project for this editor. Only 1 project can be opened at a time
        /// </summary>
        public Project ActiveProject { get; private set; }

        /// <summary>
        /// A list of timelines that are currently being viewed in the UI
        /// </summary>
        public IReadOnlyList<Timeline> ActiveTimelines => this.activeTimelines;

        /// <summary>
        /// Gets or sets the timeline that is currently active in the UI. This may be null if
        /// there is no active project or the user closes the main timeline for some reason
        /// </summary>
        public Timeline ActiveTimeline { get; set; }

        public VideoEditor() {
            this.Playback = new EditorPlayback(this);
            this.activeTimelines = new List<Timeline>();
        }

        static VideoEditor() {
        }

        public void SetProject(Project project) {
            if (this.ActiveProject != null) {
                this.activeTimelines.Clear();
            }

            this.ActiveProject = project;
            if (project != null) {
                this.activeTimelines.Add(project.Timeline);
                this.ActiveTimeline = this.activeTimelines[0];
            }
        }

        public void ClearTimelines() {
            this.ActiveTimeline = null;
            this.activeTimelines.Clear();
        }
    }
}