using System;
using System.IO;
using System.Threading;
using FramePFX.Destroying;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.RBC;

namespace FramePFX.Editors {
    public delegate void ProjectEventHandler(Project project);

    public class Project : IDestroy {
        private string projectName;

        /// <summary>
        /// Gets this project's primary timeline. This does not change
        /// </summary>
        public Timeline MainTimeline { get; }

        /// <summary>
        /// Gets this project's resource manager. This does not change
        /// </summary>
        public ResourceManager ResourceManager { get; }

        /// <summary>
        /// Gets a reference to the video editor that this project is currently loaded in
        /// </summary>
        public VideoEditor Editor { get; private set; }

        public ProjectSettings Settings { get; }

        /// <summary>
        /// Gets this project's render manager, which handles rendering of video and audio
        /// </summary>
        public RenderManager RenderManager { get; }

        /// <summary>
        /// Gets or sets if a video is being exported. Used by the view port to optimise the UI for rendering
        /// </summary>
        public bool IsExporting { get; set; }

        /// <summary>
        /// Gets or sets the readable name of this project. This may be differently named from the saved file path.
        /// This can be changed at any time and fires the <see cref="ProjectNameChanged"/>
        /// </summary>
        public string ProjectName {
            get => this.projectName;
            set {
                if (this.projectName == value)
                    return;
                this.projectName = value;
                this.ProjectNameChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets if this project has been saved at least once
        /// </summary>
        public bool HasSavedOnce { get; set; }

        /// <summary>
        /// Gets the path of the .fpfx file. Will be null for an empty/default project. This changes when the user
        /// saves the project to a new location and fires the <see cref="ProjectFilePathChanged"/> changed event
        /// </summary>
        public string ProjectFilePath { get; private set; }

        /// <summary>
        /// Gets the data folder path, that is, the folder that contains the project file (at <see cref="ProjectFilePath"/>)
        /// and any files and folders that resources and clips have saved to the disk
        /// </summary>
        public string DataFolderPath { get; private set; }

        public event ProjectEventHandler ProjectNameChanged;
        public event ProjectEventHandler ProjectFilePathChanged;

        public Project() {
            this.Settings = ProjectSettings.CreateDefault(this);
            this.RenderManager = new RenderManager(this);
            this.ResourceManager = new ResourceManager(this);
            this.MainTimeline = new Timeline();
            Timeline.InternalSetMainTimelineProjectReference(this.MainTimeline, this);
        }

        public void ReadFromFile(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid file path", nameof(filePath));

            if (this.ResourceManager.EntryMap.Count > 0 || this.ResourceManager.RootContainer.Items.Count > 0 || this.MainTimeline.Tracks.Count > 0)
                throw new InvalidOperationException("Cannot read RBE data on a project that already has data");

            RBEBase root;
            try {
                root = RBEUtils.ReadFromFilePacked(filePath);
            }
            catch (Exception e) {
                throw new Exception("File contained invalid data", e);
            }

            if (!(root is RBEDictionary dictionary)) {
                throw new Exception("File contained invalid data: root object was not an RBE Dictionary");
            }

            this.ReadProjectData(dictionary, filePath);
        }

        private void ReadProjectData(RBEDictionary data, string filePath) {
            // just in case the deserialise methods access these, which they shouldn't anyway
            this.ProjectFilePath = filePath;
            this.DataFolderPath = Path.GetDirectoryName(filePath);

            try {
                RBEDictionary manager = data.GetDictionary("ResourceManager");
                RBEDictionary timeline = data.GetDictionary("Timeline");

                this.ProjectName = data.GetString(nameof(this.ProjectName), "Unnamed project");

                // TODO: video editor specific settings that can be applied when this project is loaded

                this.ResourceManager.ReadFromRBE(manager);
                this.MainTimeline.ReadFromRBE(timeline);
            }
            catch (Exception e) {
                throw new Exception("Failed to deserialise project data", e);
            }

            // Just in case anything is listening
            this.HasSavedOnce = true;
            this.ProjectFilePathChanged?.Invoke(this);
        }

        public void WriteToFile(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid file path", nameof(filePath));

            string newDataFolder = null;
            if (this.ProjectFilePath != filePath) {
                newDataFolder = Path.GetDirectoryName(filePath) ?? throw new Exception("Invalid file path: could not get directory path");
            }

            RBEDictionary dictionary = new RBEDictionary();
            this.WriteProjectData(dictionary);

            try {
                RBEUtils.WriteToFilePacked(dictionary, filePath);
            }
            catch (Exception e) {
                throw new IOException("Failed to write RBE data to file", e);
            }

            this.HasSavedOnce = true;
            if (newDataFolder != null) {
                this.DataFolderPath = newDataFolder;
                this.ProjectFilePath = filePath;
            }
        }

        private void WriteProjectData(RBEDictionary data) {
            try {
                this.ResourceManager.WriteToRBE(data.CreateDictionary("ResourceManager"));
                this.MainTimeline.WriteToRBE(data.CreateDictionary("Timeline"));
                data.SetString(nameof(this.ProjectName), this.ProjectName);
            }
            catch (Exception e) {
                throw new Exception("Failed to serialise project data", e);
            }
        }

        /// <summary>
        /// Destroys all of this project's resources, timeline, tracks, clips, etc., allowing for it to be safely garbage collected.
        /// This is called when closing a project, or loading a new project (old project destroyed, new one is loaded)
        /// </summary>
        public void Destroy() {
            // TODO: this is no good
            while (this.RenderManager.IsRendering)
                Thread.Sleep(1);
            using (this.RenderManager.SuspendRenderInvalidation()) {
                this.MainTimeline.Destroy();
                this.ResourceManager.ClearEntries();
            }

            this.RenderManager.Dispose();
        }

        internal static void OnOpened(VideoEditor editor, Project project) {
            project.Editor = editor;
        }

        internal static void OnClosed(VideoEditor editor, Project project) {
            project.Editor = null;
        }
    }
}