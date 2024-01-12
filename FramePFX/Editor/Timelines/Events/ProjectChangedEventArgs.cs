using System;

namespace FramePFX.Editor.Timelines.Events {
    public class ProjectChangedEventArgs {
        public Project OldProject { get; }

        public Project NewProject { get; }

        public ProjectChangedEventArgs(Project oldProject, Project newProject) {
            if (oldProject == null && newProject == null)
                throw new ArgumentException("Old and new projects cannot both be null");
            this.OldProject = oldProject;
            this.NewProject = newProject;
        }
    }
}