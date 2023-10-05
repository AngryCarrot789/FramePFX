using System;
using System.IO;
using FramePFX.Automation;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.ZSystem;
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor
{
    public class Project : ZObject
    {
        public volatile bool IsSaving;

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

        public bool IsExporting { get; set; }

        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets or sets this project's settings' rendering quality
        /// </summary>
        public EnumRenderQuality RenderQuality
        {
            get => this.Settings.Quality;
            set => this.Settings.Quality = value;
        }

        /// <summary>
        /// This project's data folder, which is where file-based data is stored (that isn't stored using an absolute path).
        /// <para>
        /// This will return null if a data folder has not been generated, in which case,
        /// calling <see cref="GetDataFolderPath"/> or any similar function will generate it
        /// </para>
        /// </summary>
        public string DataFolder { get; set; }

        /// <summary>
        /// Gets the name of the project file (without directory info), relative to the <see cref="DataFolder"/>. This will not be null, and defaults to "Project.fpx"
        /// </summary>
        public string ProjectFileName { get; set; } = "Project" + Filters.DotFrameFPXExtension;

        public string FullProjectFilePath { get; set; }

        /// <summary>
        /// Whether or not <see cref="DataFolder"/> is located in the tmp folder
        /// </summary>
        public bool IsTempDataFolder { get; private set; }

        public Project()
        {
            this.Settings = new ProjectSettings()
            {
                Resolution = new Resolution(1920, 1080)
            };

            this.ResourceManager = new ResourceManager(this);
            this.Timeline = new Timeline()
            {
                MaxDuration = 5000L,
                DisplayName = "Project Timeline"
            };

            this.Timeline.SetProject(this);
        }

        public void WriteToRBE(RBEDictionary data)
        {
            this.Settings.WriteToRBE(data.CreateDictionary(nameof(this.Settings)));
            this.ResourceManager.WriteToRBE(data.CreateDictionary(nameof(this.ResourceManager)));
            this.Timeline.WriteToRBE(data.CreateDictionary(nameof(this.Timeline)));
        }

        public void ReadFromRBE(RBEDictionary data, string dataFolder, string projectFileName)
        {
            if (this.DataFolder != null)
            {
                throw new Exception("Our data folder is not invalid; cannot replace it");
            }

            this.DataFolder = dataFolder;
            this.ProjectFileName = projectFileName;
            try
            {
                this.FullProjectFilePath = Path.Combine(dataFolder, projectFileName);
            }
            catch (Exception e)
            {
                throw new Exception($"Data folder or project file name contain invalid chars.\n'{dataFolder}', {projectFileName}", e);
            }

            this.Settings.ReadFromRBE(data.GetDictionary(nameof(this.Settings)));
            this.ResourceManager.ReadFromRBE(data.GetDictionary(nameof(this.ResourceManager)));
            this.Timeline.ReadFromRBE(data.GetDictionary(nameof(this.Timeline)));
            this.UpdateTimelineBackingStorage();
        }

        /// <summary>
        /// Sets this project in a loaded state. The OpenGL context must be current before calling this
        /// </summary>
        public void OnLoaded()
        {
            if (this.IsLoaded)
                return;
            AppLogger.WriteLine("Load project internal");
            this.IsLoaded = true;
            this.ResourceManager.OnProjectLoaded();
        }

        /// <summary>
        /// Sets the project in an unloaded state. The OpenGL context must be current before calling this
        /// </summary>
        public void OnUnloaded()
        {
            if (!this.IsLoaded)
                return;
            AppLogger.WriteLine("Unload project internal");
            this.IsLoaded = false;
            this.ResourceManager.OnProjectUnloaded();
        }

        /// <summary>
        /// Gets the absolute file path from a project path
        /// </summary>
        /// <param name="file">Input relative path</param>
        /// <returns>Output absolute filepath that may exist on the system</returns>
        /// <exception cref="ArgumentException">The path's file path is null or empty... somehow</exception>
        public string GetFilePath(ProjectPath file)
        {
            if (string.IsNullOrEmpty(file.FilePath))
                throw new ArgumentException("File's path cannot be null or empty (corrupted project path)", nameof(file));

            if (file.IsAbsolute)
                return Path.GetFullPath(file.FilePath);
            else
                return Path.Combine(this.GetDataFolderPath(out _), file.FilePath);
        }

        public string GetDataFolderPath(out bool isTemp)
        {
            if (string.IsNullOrEmpty(this.DataFolder))
            {
                this.IsTempDataFolder = isTemp = true;
                this.DataFolder = RandomUtils.RandomLettersWhere(Path.GetTempPath(), 16, x => !Directory.Exists(x));
                this.FullProjectFilePath = Path.Combine(this.DataFolder, this.ProjectFileName);
            }
            else
            {
                isTemp = false;
            }

            return this.DataFolder;
        }

        public void SetProjectFileLocation(string dataFolder, string projectFileName, string projectFilePath)
        {
            this.DataFolder = dataFolder;
            this.ProjectFileName = projectFileName;
            this.FullProjectFilePath = projectFilePath;
        }

        /// <summary>
        /// Gets or creates the project's data folder
        /// </summary>
        /// <param name="isTemp">Whether or not the current data folder is in the temp directory</param>
        /// <returns>The directory info for the data folder</returns>
        public DirectoryInfo GetDataFolder(out bool isTemp)
        {
            // Cleaner and also faster than manual existence check (exists() ? new DirectoryInfo(dir) : create())
            return Directory.CreateDirectory(this.GetDataFolderPath(out isTemp));
        }

        /// <summary>
        /// Gets or creates the project's data folder
        /// </summary>
        /// <returns>The directory info for the data folder</returns>
        public DirectoryInfo GetDataFolder() => this.GetDataFolder(out _);

        /// <summary>
        /// Updates the backing storage of the timeline, all tracks and all clips
        /// </summary>
        public void UpdateTimelineBackingStorage() => AutomationEngine.UpdateBackingStorage(this.Timeline);

        public void OnDisconnectedFromEditor()
        {
            this.Editor = null;
        }

        public void OnConnectedToEditor(VideoEditor editor)
        {
            this.Editor = editor;
        }
    }
}