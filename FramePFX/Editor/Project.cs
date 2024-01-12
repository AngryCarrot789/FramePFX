using System;
using System.IO;
using System.Runtime.CompilerServices;
using FramePFX.Automation;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.Timelines;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor {
    public delegate void ProjectEventHandler(Project project);

    public class Project : IObservableObject {
        private string projectName;
        private bool isSaving;
        private bool isExporting;

        public ProjectSettings Settings { get; }

        public ResourceManager ResourceManager { get; }

        /// <summary>
        /// This project's main timeline
        /// </summary>
        public Timeline Timeline { get; }

        /// <summary>
        /// The video editor that this project is currently in
        /// </summary>
        public VideoEditor Editor { get; set; }

        public bool IsSaving {
            get => this.isSaving;
            set => this.OnPropertyChanged(ref this.isSaving, value);
        }

        public bool IsExporting {
            get => this.isExporting;
            set => this.OnPropertyChanged(ref this.isExporting, value);
        }

        /// <summary>
        /// Gets or sets the active project name
        /// </summary>
        public string ProjectName {
            get => this.projectName;
            set => this.OnPropertyChanged(ref this.projectName, value);
        }

        /// <summary>
        /// Gets or sets the folder in which the project file is located in
        /// </summary>
        public string ProjectFolder { get; private set; }

        /// <summary>
        /// Gets the file name (with extension) of <see cref="ProjectFilePath"/>
        /// </summary>
        public string ProjectFileName { get; private set; }

        /// <summary>
        /// Gets the full path of the project file based on <see cref="ProjectFolder"/> and <see cref="ProjectName"/>
        /// </summary>
        public string ProjectFilePath { get; private set; }

        /// <summary>
        /// Whether or not <see cref="ProjectFolder"/> is located in the temp directory
        /// </summary>
        public bool IsUsingTempFolder { get; private set; }

        public Project() {
            this.projectName = "New Project";
            this.IsUsingTempFolder = true;
            this.ProjectFolder = RandomUtils.RandomPrefixedLettersWhere(Path.GetTempPath(), 16, x => !Directory.Exists(x));
            this.ProjectFileName = this.ProjectName + Filters.DotFrameFPXExtension;
            this.ProjectFilePath = Path.Combine(this.ProjectFolder, this.ProjectFileName);
            this.Settings = new ProjectSettings() {
                Resolution = new Rect2i(1920, 1080)
            };

            this.ResourceManager = new ResourceManager(this);
            this.Timeline = new Timeline() {
                MaxDuration = 5000L,
                DisplayName = "Project Timeline"
            };

            this.Timeline.SetProject(this);
        }

        public void SetProjectPaths(string filePath, string projectFolder) {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException(nameof(filePath));
            if (string.IsNullOrEmpty(projectFolder))
                throw new ArgumentException(nameof(projectFolder));

            this.IsUsingTempFolder = false;
            this.ProjectFolder = projectFolder;
            this.ProjectFileName = Path.GetFileName(filePath);
            this.ProjectFilePath = filePath;
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetString(nameof(this.ProjectName), this.ProjectName);
            this.Settings.WriteToRBE(data.CreateDictionary(nameof(this.Settings)));
            this.ResourceManager.WriteToRBE(data.CreateDictionary(nameof(this.ResourceManager)));
            this.Timeline.WriteToRBE(data.CreateDictionary(nameof(this.Timeline)));
        }

        public void ReadFromRBE(RBEDictionary data, string projectFilePath) {
            if (string.IsNullOrEmpty(projectFilePath)) {
                throw new Exception("Project file path cannot be null or empty");
            }

            try {
                this.ProjectFolder = Path.GetDirectoryName(projectFilePath);
                this.ProjectFileName = Path.GetFileName(projectFilePath);
                this.ProjectFilePath = projectFilePath;
            }
            catch (Exception e) {
                throw new Exception($"Project file contains invalid characters: '{projectFilePath}'", e);
            }

            this.IsUsingTempFolder = false;
            this.ProjectName = data.GetString(nameof(this.ProjectName), "New Project");
            this.Settings.ReadFromRBE(data.GetDictionary(nameof(this.Settings)));
            this.ResourceManager.ReadFromRBE(data.GetDictionary(nameof(this.ResourceManager)));
            this.Timeline.ReadFromRBE(data.GetDictionary(nameof(this.Timeline)));
            this.UpdateTimelineBackingStorage();
        }

        /// <summary>
        /// Gets the absolute file path from a project path
        /// </summary>
        /// <param name="file">Input relative path</param>
        /// <returns>Output absolute filepath that may exist on the system</returns>
        /// <exception cref="ArgumentException">The path's file path is null or empty... somehow</exception>
        public string GetFilePath(ProjectPath file) {
            if (string.IsNullOrEmpty(file.FilePath))
                throw new ArgumentException("File's path cannot be null or empty (corrupted project path)", nameof(file));

            if (file.IsAbsolute)
                return Path.GetFullPath(file.FilePath);
            return Path.Combine(this.ProjectFolder, file.FilePath);
        }

        /// <summary>
        /// Gets or creates the project's data folder
        /// </summary>
        /// <returns>The directory info for the data folder</returns>
        public DirectoryInfo GetDataFolder() {
            // Cleaner and also faster than manual existence check (exists() ? new DirectoryInfo(dir) : create())
            return Directory.CreateDirectory(this.ProjectFolder);
        }

        /// <summary>
        /// Updates the backing storage of the timeline, all tracks and all clips
        /// </summary>
        public void UpdateTimelineBackingStorage() => AutomationEngine.UpdateBackingStorage(this.Timeline);

        public void OnDisconnectedFromEditor() {
            this.Editor = null;
        }

        public void OnConnectedToEditor(VideoEditor editor) {
            this.Editor = editor;
        }

        private void OnPropertyChanged<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null) {
            field = newValue;
            this.PropertyChanged?.Invoke(this, propertyName);
        }

        public event ObservablePropertyChangedEventHandler PropertyChanged;
    }
}