using System;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.ResourceManaging;

namespace FramePFX.Core.Editor {
    public class ProjectModel {
        public ProjectSettingsModel Settings { get; set; }
        public string ProjectDir { get; set; }

        public volatile bool IsSaving;

        /// <summary>
        /// The video editor that this project is currently in
        /// </summary>
        public VideoEditorModel Editor { get; set; }

        public TimelineModel Timeline { get; }

        public ResourceManager ResourceManager { get; }

        public ProjectModel() {
            this.Timeline = new TimelineModel(this);
            this.ResourceManager = new ResourceManager(this);
        }

        public void SaveProject(string file) {

        }

        public void LoadProject(string file) {

        }
    }
}