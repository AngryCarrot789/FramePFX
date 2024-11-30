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
using System.Text;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Avalonia.Themes.Controls;
using FramePFX.Editing;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.PropertyEditing;
using FramePFX.Utils;

namespace FramePFX.Avalonia;

public partial class EditorWindow : WindowEx, ITopLevel, IVideoEditorUI {
    public static readonly StyledProperty<VideoEditor?> VideoEditorProperty = AvaloniaProperty.Register<EditorWindow, VideoEditor?>(nameof(VideoEditor));

    public VideoEditor? VideoEditor {
        get => this.GetValue(VideoEditorProperty);
        set => this.SetValue(VideoEditorProperty, value);
    }

    public ITimelineElement? ActiveTimeline => this.isFakedActiveTimelineNull ? null : this.TheTimeline;
    
    public event VideoEditorActiveTimelineChanged? ActiveTimelineChanged;

    private readonly ContextData contextData = new ContextData();
    private bool doNotInvalidateContext;
    private bool isFakedActiveTimelineNull = true;
    private readonly NumberAverager renderTimeAverager;

    public EditorWindow() {
        this.InitializeComponent();
        
        // average 5 samples. Will take a second to catch up when playing at 5 fps but meh
        this.renderTimeAverager = new NumberAverager(5);
        this.TheTimeline.VideoEditor = this;
        DataManager.SetContextData(this, this.contextData.
                                              Set(DataKeys.TopLevelHostKey, this).
                                              Set(DataKeys.VideoEditorUIKey, this).
                                              Set(DataKeys.TimelineUIKey, this.TheTimeline));
    }

    static EditorWindow() {
        VideoEditorProperty.Changed.AddClassHandler<EditorWindow, VideoEditor?>((d, e) => d.OnVideoEditorChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.contextData.Set(DataKeys.ResourceManagerUIKey, this.PART_ResourcePanelControl);
        this.contextData.Set(DataKeys.ResourceTreeUIKey, this.PART_ResourcePanelControl.ResourceTreeView);
        this.contextData.Set(DataKeys.ResourceListUIKey, this.PART_ResourcePanelControl.ResourceListBox);
        
        DataManager.InvalidateInheritedContext(this);
        
        this.ThePropertyEditor.ApplyTemplate();
        this.ThePropertyEditor.PropertyEditor = VideoEditorPropertyEditor.Instance;
    }

    private void OnVideoEditorChanged(VideoEditor? oldEditor, VideoEditor? newEditor) {
        this.doNotInvalidateContext = true;
        if (oldEditor != null) {
            oldEditor.ProjectChanged -= this.OnProjectChanged;
            Project? oldProject = oldEditor.Project;
            if (oldProject != null)
                this.OnProjectChanged(oldProject, null);

            this.PART_ViewPort.VideoEditor = null;
        }

        if (newEditor != null) {
            newEditor.ProjectChanged += this.OnProjectChanged;
            this.PART_ViewPort.Owner = this;
            this.PART_ViewPort.VideoEditor = newEditor;
            this.OnProjectChanged(null, newEditor?.Project);
        }

        this.doNotInvalidateContext = false;
        this.contextData.Set(DataKeys.VideoEditorKey, newEditor);
        DataManager.InvalidateInheritedContext(this);
    }

    private void OnProjectChanged(VideoEditor editor, Project oldproject, Project newproject) {
        this.OnProjectChanged(oldproject, newproject);
    }

    private void OnProjectChanged(Project? oldProject, Project? newProject) {
        if (oldProject != null) {
            oldProject.ActiveTimelineChanged -= this.OnActiveTimelineChanged;
            oldProject.ProjectFilePathChanged -= this.OnProjectFilePathChanged;
            oldProject.ProjectNameChanged -= this.OnProjectNameChanged;
            oldProject.IsModifiedChanged -= this.OnProjectModifiedChanged;
        }

        if (newProject != null) {
            newProject.ActiveTimelineChanged += this.OnActiveTimelineChanged;
            newProject.ProjectFilePathChanged += this.OnProjectFilePathChanged;
            newProject.ProjectNameChanged += this.OnProjectNameChanged;
            newProject.IsModifiedChanged += this.OnProjectModifiedChanged;
        }

        this.contextData.Set(DataKeys.ProjectKey, newProject);
        this.PART_ResourcePanelControl.ResourceManager = newProject?.ResourceManager;
        this.OnActiveTimelineChanged(oldProject?.ActiveTimeline, newProject?.ActiveTimeline);
        this.UpdateWindowTitle(newProject);
        
        // this.contextData.Set(DataKeys.ResourceTreeSelectionManagerKey, this.PART_ResourcePanelControl.MultiSelectionManager);
        // this.contextData.Set(DataKeys.TrackSelectionManagerKey, newProject);
        // this.contextData.Set(DataKeys.ClipSelectionManagerKey, newProject);
        // this.contextData.Set(DataKeys.TimelineClipSelectionManagerKey, newProject);
        if (!this.doNotInvalidateContext)
            DataManager.InvalidateInheritedContext(this);
        
        VideoEditorPropertyEditorHelper.OnProjectChanged();
    }

    private void OnActiveTimelineChanged(Project project, Timeline? oldTimeline, Timeline? newTimeline) {
        this.OnActiveTimelineChanged(oldTimeline, newTimeline);
    }

    private void OnActiveTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        if (oldTimeline != null) {
            oldTimeline.RenderManager.FrameRendered -= this.UpdateFrameRenderInterval;
            if (oldTimeline is CompositionTimeline oldComposition) {
                oldComposition.Resource.DisplayNameChanged -= this.OnCompositionTimelineDisplayNameChanged;
            }

            this.isFakedActiveTimelineNull = true;
            this.ActiveTimelineChanged?.Invoke(this, this.ActiveTimeline, null);
        }

        if (newTimeline != null) {
            newTimeline.RenderManager.FrameRendered += this.UpdateFrameRenderInterval;
            if (newTimeline is CompositionTimeline newComposition) {
                newComposition.Resource.DisplayNameChanged += this.OnCompositionTimelineDisplayNameChanged;
            }
            
            this.isFakedActiveTimelineNull = false;
            this.ActiveTimelineChanged?.Invoke(this, null, this.ActiveTimeline);
        }

        this.TheTimeline.Timeline = newTimeline;
        this.PART_CloseTimelineButton.IsEnabled = newTimeline is Timeline timeline && timeline is CompositionTimeline;
    }

    private void UpdateFrameRenderInterval(RenderManager manager) {
        this.UpdateFrameRenderInterval(manager.Timeline.Project?.Editor, manager.Timeline);
    }

    private void UpdateFrameRenderInterval(VideoEditor? editor, Timeline? timeline) {
        if (editor != null) {
            double avgPlaybackMillis = editor.Playback.AveragePlaybackIntervalMillis;
            this.PART_AvgFPSBlock.Text = $"{Math.Round(avgPlaybackMillis, 2).ToString(),5} ms ({((int) Math.Round(1000.0 / avgPlaybackMillis)).ToString(),3} FPS)";
        }

        if (timeline != null) {
            this.renderTimeAverager.PushValue(timeline.RenderManager.AverageVideoRenderTimeMillis);
            double avgRenderMillis = this.renderTimeAverager.GetAverage();
            this.PART_AvgRenderTimeBlock.Text = $"{Math.Round(avgRenderMillis, 2).ToString(),5} ms ({((int) Math.Round(1000.0 / avgRenderMillis)).ToString(),3} FPS)";
        }
    }

    private void OnCompositionTimelineDisplayNameChanged(IDisplayName sender, string oldName, string newName) {
        // this.UpdateTimelineName();
    }

    private void OnProjectFilePathChanged(Project project) {
        this.UpdateWindowTitle(project);
    }

    private void OnProjectNameChanged(Project project) {
        this.UpdateWindowTitle(project);
    }

    private void OnProjectModifiedChanged(Project project) {
        this.UpdateWindowTitle(project);
    }

    private void UpdateWindowTitle(Project? project) {
        const string DefaultTitle = "Bootleg song vegas (FramePFX v2.0.0)";
        if (project == null) {
            this.Title = DefaultTitle;
        }
        else {
            StringBuilder sb = new StringBuilder().Append(DefaultTitle);
            if (!string.IsNullOrEmpty(project.ProjectFilePath))
                sb.Append(" - ").Append(project.ProjectFilePath);

            if (!string.IsNullOrWhiteSpace(project.ProjectName))
                sb.Append(" [").Append(project.ProjectName).Append("]");

            if (project.IsModified)
                sb.Append("*");

            this.Title = sb.ToString();
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
    }
}