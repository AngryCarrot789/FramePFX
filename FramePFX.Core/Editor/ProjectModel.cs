using System;

namespace FramePFX.Core.Editor {
    public class ProjectModel {
        public ProjectSettingsModel Settings { get; set; }
        public string ProjectDir { get; set; }

        /// <summary>
        /// The video editor that this project is currently in
        /// </summary>
        public VideoEditorModel Editor { get; set; }

        public ProjectModel() {

        }
    }
}