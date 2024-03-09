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
using System.IO;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.RBC;
using FramePFX.Tasks;
using FramePFX.Utils;
using FramePFX.Utils.Destroying;

namespace FramePFX.Editors
{
    public delegate void ProjectEventHandler(Project project);

    public delegate void ActiveTimelineChangedEventHandler(Project project, Timeline oldTimeline, Timeline newTimeline);

    public class Project : IDestroy
    {
        private string projectName;
        private Timeline activeTimeline;
        private volatile bool isSaving;

        /// <summary>
        /// Gets or sets the active timeline in the UI. This is the timeline that all timeline actions are applied
        /// on (e.g. cutting clips) and is also the timeline that is rendered to the UI
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public Timeline ActiveTimeline {
            get => this.activeTimeline;
            set
            {
                if (value == null)
                    value = this.MainTimeline;
                Timeline oldTimeline = this.activeTimeline;
                if (oldTimeline == value)
                    return;
                if (value.Project != this)
                    throw new InvalidOperationException("The new active timeline's project does not match the current instance");

                this.activeTimeline = value;
                Timeline.InternalOnActiveTimelineChanged(oldTimeline, value);
                if (this.Editor != null)
                    VideoEditor.InternalOnActiveTimelineChanged(this.Editor, oldTimeline, value);

                value.RenderManager.UpdateFrameInfo(this.Settings);
                value.RenderManager.InvalidateRender();

                this.ActiveTimelineChanged?.Invoke(this, oldTimeline, value);
            }
        }

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
        /// Gets or sets if a video is being exported. Used by the view port to optimise the UI for rendering
        /// </summary>
        public bool IsExporting { get; set; }

        /// <summary>
        /// Gets or sets the readable name of this project. This may be differently named from the saved file path.
        /// This can be changed at any time and fires the <see cref="ProjectNameChanged"/>
        /// </summary>
        public string ProjectName {
            get => this.projectName;
            set
            {
                if (this.projectName == value)
                    return;
                this.projectName = value;
                this.ProjectNameChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Returns true if this project has been modified since being created or loaded
        /// </summary>
        public bool IsModified { get; private set; }

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

        public bool IsSaving {
            get => this.isSaving;
            set
            {
                if (this.isSaving == value)
                    return;
                this.isSaving = value;
                this.IsSavingChanged?.Invoke(this);
            }
        }

        public event ProjectEventHandler ProjectNameChanged;
        public event ProjectEventHandler ProjectFilePathChanged;
        public event ProjectEventHandler IsModifiedChanged;
        public event ProjectEventHandler IsSavingChanged;

        /// <summary>
        /// An event fired when our <see cref="ActiveTimeline"/> changes.
        /// The old and new timeline values will always be non-null
        /// </summary>
        public event ActiveTimelineChangedEventHandler ActiveTimelineChanged;

        public Project()
        {
            this.projectName = "Unnamed Project";
            this.Settings = ProjectSettings.CreateDefault(this);
            this.ResourceManager = new ResourceManager(this);
            this.MainTimeline = new Timeline();
            this.activeTimeline = this.MainTimeline;
            Timeline.InternalSetMainTimelineProjectReference(this.MainTimeline, this);
        }

        public void ReadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid file path", nameof(filePath));

            if (this.ResourceManager.EntryMap.Count > 0 || this.ResourceManager.RootContainer.Items.Count > 0 || this.MainTimeline.Tracks.Count > 0)
                throw new InvalidOperationException("Cannot read RBE data on a project that already has data");

            RBEBase root;
            try
            {
                root = RBEUtils.ReadFromFilePacked(filePath);
            }
            catch (Exception e)
            {
                throw new Exception("File contained invalid data", e);
            }

            if (!(root is RBEDictionary dictionary))
            {
                throw new Exception("File contained invalid data: root object was not an RBE Dictionary");
            }

            this.ReadProjectData(dictionary, filePath);
        }

        private void ReadProjectData(RBEDictionary data, string filePath)
        {
            // just in case the deserialise methods access these, which they shouldn't anyway
            this.ProjectFilePath = filePath;
            this.DataFolderPath = Path.GetDirectoryName(filePath);

            try
            {
                RBEDictionary manager = data.GetDictionary("ResourceManager");
                RBEDictionary timeline = data.GetDictionary("Timeline");
                RBEDictionary settings = data.GetDictionary("Settings");

                this.ProjectName = data.GetString(nameof(this.ProjectName), "Unnamed project");

                // TODO: video editor specific settings that can be applied when this project is loaded

                this.Settings.ReadFromRBE(settings);
                this.ResourceManager.ReadFromRBE(manager);
                this.MainTimeline.ReadFromRBE(timeline);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to deserialise project data", e);
            }

            // Just in case anything is listening
            this.HasSavedOnce = true;
            this.ProjectFilePathChanged?.Invoke(this);

            if (data.TryGetULong("ActiveTimelineResourceId", out ulong resourceId))
            {
                if (this.ResourceManager.TryGetEntryItem(resourceId, out ResourceItem resource) && resource is ResourceComposition composition)
                {
                    this.ActiveTimeline = composition.Timeline;
                }
            }
        }

        public void SaveToFileAndSetPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid file path", nameof(filePath));

            string newDataFolder = null;
            if (this.ProjectFilePath != filePath)
            {
                newDataFolder = Path.GetDirectoryName(filePath) ?? throw new Exception("Invalid file path: could not get directory path");
            }

            RBEDictionary dictionary = new RBEDictionary();
            this.WriteProjectData(dictionary);

            try
            {
                RBEUtils.WriteToFilePacked(dictionary, filePath);
            }
            catch (Exception e)
            {
                throw new IOException("Failed to write RBE data to file", e);
            }

            this.HasSavedOnce = true;
            if (newDataFolder != null)
            {
                this.DataFolderPath = newDataFolder;
                this.ProjectFilePath = filePath;
            }
        }

        private void WriteProjectData(RBEDictionary data)
        {
            try
            {
                this.Settings.WriteToRBE(data.CreateDictionary("Settings"));
                this.ResourceManager.WriteToRBE(data.CreateDictionary("ResourceManager"));
                this.MainTimeline.WriteToRBE(data.CreateDictionary("Timeline"));
                data.SetString(nameof(this.ProjectName), this.ProjectName);

                if (this.ActiveTimeline is CompositionTimeline timeline)
                {
                    data.SetULong("ActiveTimelineResourceId", timeline.Resource.UniqueId);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to serialise project data", e);
            }
        }

        /// <summary>
        /// Destroys all of this project's resources, timeline, tracks, clips, etc., allowing for it to be safely garbage collected.
        /// This is called when closing a project, or loading a new project (old project destroyed, new one is loaded)
        /// </summary>
        public void Destroy()
        {
            this.MainTimeline.Destroy();
            this.ResourceManager.Clear();
        }

        public static Project ReadProjectAt(string filePath)
        {
            Project project = new Project();
            using (project.MainTimeline.RenderManager.SuspendRenderInvalidation())
            {
                try
                {
                    project.ReadFromFile(filePath);
                }
                catch
                {
                    try
                    {
                        project.Destroy();
                    }
                    catch
                    { /* ignored */
                    }

                    throw;
                }
            }

            return project;
        }

        internal static void OnOpened(VideoEditor editor, Project project)
        {
            project.Editor = editor;
        }

        internal static void OnClosed(VideoEditor editor, Project project)
        {
            project.Editor = null;
        }

        /// <summary>
        /// Notifies the project that it has unsaved data
        /// </summary>
        public void MarkModified()
        {
            if (this.IsModified)
                return;
            this.IsModified = true;
            this.IsModifiedChanged?.Invoke(this);
        }

        public void SetUnModified()
        {
            if (!this.IsModified)
                return;
            this.IsModified = false;
            this.IsModifiedChanged?.Invoke(this);
        }

        public static bool? SaveProject(Project project, IActivityProgress progress)
        {
            if (project.HasSavedOnce && !string.IsNullOrEmpty(project.ProjectFilePath))
            {
                return SaveProjectInternal(project, project.ProjectFilePath, progress);
            }
            else
            {
                return SaveProjectAs(project, progress);
            }
        }

        public static bool? SaveProjectAs(Project project, IActivityProgress progress)
        {
            const string message = "Specify a file path for the project file. Any project data will be stored in the same folder, so it's best to create a project-specific folder";
            string filePath = IoC.FilePickService.SaveFile(message, Filters.ProjectType, project.ProjectFilePath);
            if (filePath == null)
            {
                return null;
            }

            progress.OnProgress(0.1);
            using (progress.PushCompletionRange(0.1, 0.8))
            {
                return SaveProjectInternal(project, filePath, progress);
            }
        }

        private static bool SaveProjectInternal(Project project, string filePath, IActivityProgress progress)
        {
            if (project.IsSaving)
            {
                throw new InvalidOperationException("Already saving!");
            }

            if (progress == null)
                progress = EmptyActivityProgress.Instance;

            project.IsSaving = true;
            try
            {
                project.Editor?.Playback.Pause();

                progress.Text = "Serialising project...";
                progress.OnProgress(0.5);
                try
                {
                    project.SaveToFileAndSetPath(filePath);
                    progress.OnProgress(0.5);
                    return true;
                }
                catch (Exception e)
                {
                    IoC.MessageService.ShowMessage("Save Error", "An exception occurred while saving project", e.GetToString());
                    progress.OnProgress(0.5);
                    return false;
                }
            }
            finally
            {
                project.IsSaving = false;
            }
        }
    }
}