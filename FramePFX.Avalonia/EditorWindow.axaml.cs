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
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Avalonia.Themes.Controls;
using FramePFX.Configurations;
using FramePFX.Editing;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.PropertyEditing;
using FramePFX.Tasks;
using FramePFX.Utils;
using FramePFX.Utils.RDA;
using SkiaSharp;

namespace FramePFX.Avalonia;

public partial class EditorWindow : WindowEx, ITopLevel, IVideoEditorUI
{
    public static readonly StyledProperty<VideoEditor?> VideoEditorProperty = AvaloniaProperty.Register<EditorWindow, VideoEditor?>(nameof(VideoEditor));

    public VideoEditor? VideoEditor
    {
        get => this.GetValue(VideoEditorProperty)!;
        set => this.SetValue(VideoEditorProperty, value);
    }

    ITimelineElement IVideoEditorUI.TimelineElement => this.TheTimeline;

    public ProjectConfigurationManager? ActiveProjectConfigurationManager
    {
        get
        {
            if (this.myActiveProjectConfiguration != null)
            {
                return this.myActiveProjectConfiguration;
            }
            
            if (this.activeProject != null)
                return this.myActiveProjectConfiguration = new ProjectConfigurationManager(this.activeProject, this);

            return null;
        }
    }

    private readonly ContextData contextData = new ContextData();
    private bool doNotInvalidateContext;
    private readonly NumberAverager renderTimeAverager;
    private ActivityTask? primaryActivity;
    private readonly ContextData timelineGroupBoxContextData;
    private RateLimitedDispatchAction<Timeline> updateFpsInfoRlda;
    private ProjectConfigurationManager? myActiveProjectConfiguration;
    private Project? activeProject;

    public EditorWindow()
    {
        this.InitializeComponent();

        // average 5 samples. Will take a second to catch up when playing at 5 fps but meh
        this.renderTimeAverager = new NumberAverager(5);
        this.TheTimeline.EditorOwner = this;

        this.updateFpsInfoRlda = BaseRateLimitedDispatchAction.ForDispatcherSync<Timeline>((t) =>
        {
            if (t.Project?.Editor is VideoEditor editor)
            {
                double avgPlaybackMillis = editor.Playback.AveragePlaybackIntervalMillis;
                this.PART_AvgFPSBlock.Text = $"PB: {((int) Math.Round(1000.0 / avgPlaybackMillis)).ToString(),3} FPS ({Math.Round(avgPlaybackMillis, 2).ToString(),5} ms)";
            }

            double avgRenderMillis = this.renderTimeAverager.GetAverage();
            this.PART_AvgRenderTimeBlock.Text = $"RT: {Math.Round(avgRenderMillis, 2).ToString(),5} ms ({((int) Math.Round(1000.0 / avgRenderMillis)).ToString(),3} FPS)";
        }, TimeSpan.FromSeconds(0.05), DispatchPriority.Loaded);
        
        DataManager.SetContextData(this, this.contextData.Set(DataKeys.TopLevelHostKey, this).Set(DataKeys.VideoEditorUIKey, this));
        DataManager.SetContextData(this.PART_TimelinePresenterGroupBox, this.timelineGroupBoxContextData = new ContextData().Set(DataKeys.TimelineUIKey, this.TheTimeline));

        TaskManager taskManager = IoC.TaskManager;
        taskManager.TaskStarted += this.OnTaskStarted;
        taskManager.TaskCompleted += this.OnTaskCompleted;
    }

    private void OnTimelineClipSelectionChanged(ILightSelectionManager<IClipElement> sender)
    {
        this.PART_ViewPort.OnClipSelectionChanged();
    }

    static EditorWindow()
    {
        VideoEditorProperty.Changed.AddClassHandler<EditorWindow, VideoEditor?>((d, e) => d.OnVideoEditorChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        this.contextData.Set(DataKeys.ResourceManagerUIKey, this.PART_ResourcePanelControl);

        DataManager.InvalidateInheritedContext(this);

        this.ThePropertyEditor.ApplyTemplate();
        this.ThePropertyEditor.PropertyEditor = VideoEditorPropertyEditor.Instance;
        this.PART_ActiveBackgroundTaskGrid.IsVisible = false;
        ((ILightSelectionManager<IClipElement>) this.TheTimeline.ClipSelectionManager!).SelectionChanged += this.OnTimelineClipSelectionChanged;
        this.PART_ViewPort.OnClipSelectionChanged();

        EditorConfigurationOptions.Instance.TitleBarPrefixChanged += this.OnApplicationTitleBarPrefixChanged;
        EditorConfigurationOptions.Instance.TitleBarBrushChanged += this.OnApplicationTitleBarBrushChanged;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        
        // Prevent semantic memory leak
        EditorConfigurationOptions.Instance.TitleBarPrefixChanged -= this.OnApplicationTitleBarPrefixChanged;
        EditorConfigurationOptions.Instance.TitleBarBrushChanged -= this.OnApplicationTitleBarBrushChanged;
    }

    private void OnApplicationTitleBarPrefixChanged(EditorConfigurationOptions sender)
    {
        this.UpdateWindowTitle(this.VideoEditor?.Project);
    }
    
    private void OnApplicationTitleBarBrushChanged(EditorConfigurationOptions sender)
    {
        SKColor c = sender.TitleBarBrush;
        this.TitleBarBrush = new ImmutableSolidColorBrush(new Color(c.Alpha, c.Red, c.Green, c.Blue));
    }

    private void OnVideoEditorChanged(VideoEditor? oldEditor, VideoEditor? newEditor)
    {
        this.doNotInvalidateContext = true;
        if (oldEditor != null)
        {
            oldEditor.ProjectChanged -= this.OnProjectChanged;
            Project? oldProject = oldEditor.Project;
            if (oldProject != null)
                this.OnProjectChanged(oldProject, null);

            this.PART_ViewPort.VideoEditor = null;
            oldEditor.IsExportingChanged -= this.OnIsExportingChanged;
        }

        if (newEditor != null)
        {
            newEditor.ProjectChanged += this.OnProjectChanged;
            newEditor.IsExportingChanged += this.OnIsExportingChanged;
            this.PART_ViewPort.Owner = this;
            this.PART_ViewPort.VideoEditor = newEditor;
            this.OnProjectChanged(null, newEditor.Project);
            this.OnIsExportingChanged(newEditor);
        }

        this.doNotInvalidateContext = false;
        this.contextData.Set(DataKeys.VideoEditorKey, newEditor);
        DataManager.InvalidateInheritedContext(this);
    }

    private void OnIsExportingChanged(VideoEditor editor)
    {
        this.PART_EditorWindowContent.IsEnabled = !editor.IsExporting;
    }

    private void OnProjectChanged(VideoEditor editor, Project oldproject, Project newproject)
    {
        this.OnProjectChanged(oldproject, newproject);
    }

    private void OnProjectChanged(Project? oldProject, Project? newProject)
    {
        if (oldProject != null)
        {
            oldProject.ActiveTimelineChanged -= this.OnActiveTimelineChanged;
            oldProject.ProjectFilePathChanged -= this.OnProjectFilePathChanged;
            oldProject.ProjectNameChanged -= this.OnProjectNameChanged;
            oldProject.IsModifiedChanged -= this.OnProjectModifiedChanged;
        }

        if (newProject != null)
        {
            newProject.ActiveTimelineChanged += this.OnActiveTimelineChanged;
            newProject.ProjectFilePathChanged += this.OnProjectFilePathChanged;
            newProject.ProjectNameChanged += this.OnProjectNameChanged;
            newProject.IsModifiedChanged += this.OnProjectModifiedChanged;
        }

        this.activeProject = newProject;
        this.myActiveProjectConfiguration?.Destroy();
        this.myActiveProjectConfiguration = null;
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

        IoC.Dispatcher.InvokeAsync(() =>
        {
            this.PART_ViewPort?.PART_FreeMoveViewPort?.FitContentToCenter();
        }, DispatchPriority.Background);
    }

    private void OnActiveTimelineChanged(Project project, Timeline? oldTimeline, Timeline? newTimeline)
    {
        this.OnActiveTimelineChanged(oldTimeline, newTimeline);
    }

    private void OnActiveTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline)
    {
        if (oldTimeline != null)
        {
            oldTimeline.RenderManager.FrameRendered -= this.UpdateFrameRenderInterval;
            if (oldTimeline is CompositionTimeline oldComposition)
            {
                oldComposition.Resource.DisplayNameChanged -= this.OnCompositionTimelineDisplayNameChanged;
            }
        }

        if (newTimeline != null)
        {
            newTimeline.RenderManager.FrameRendered += this.UpdateFrameRenderInterval;
            if (newTimeline is CompositionTimeline newComposition)
            {
                newComposition.Resource.DisplayNameChanged += this.OnCompositionTimelineDisplayNameChanged;
            }
        }

        this.TheTimeline.Timeline = newTimeline;
        this.timelineGroupBoxContextData.Set(DataKeys.TimelineKey, newTimeline);
        DataManager.InvalidateInheritedContext(this.PART_TimelinePresenterGroupBox);
        this.UpdateTimelineName();
        this.PART_CloseTimelineButton.IsEnabled = newTimeline is Timeline timeline && timeline is CompositionTimeline;
    }

    private void UpdateFrameRenderInterval(RenderManager manager)
    {
        this.renderTimeAverager.PushValue(manager.AverageVideoRenderTimeMillis);
        this.updateFpsInfoRlda.InvokeAsync(manager.Timeline);
    }

    private void OnCompositionTimelineDisplayNameChanged(IDisplayName sender, string? oldName, string? newName) => this.UpdateTimelineName();

    private void OnProjectFilePathChanged(Project project) => this.UpdateWindowTitle(project);

    private void OnProjectNameChanged(Project project) => this.UpdateWindowTitle(project);

    private void OnProjectModifiedChanged(Project project) => this.UpdateWindowTitle(project);

    private void UpdateTimelineName()
    {
        Timeline? timeline = this.TheTimeline?.Timeline;
        if (timeline == null)
        {
            this.PART_TimelineName.Text = "No timeline loaded";
        }
        else if (timeline is CompositionTimeline composition)
        {
            string? text = composition.Resource.DisplayName;
            this.PART_TimelineName.Text = string.IsNullOrWhiteSpace(text) ? "Unnamed Composition Timeline" : text;
        }
        else
        {
            this.PART_TimelineName.Text = "Project Timeline";
        }
    }

    private void UpdateWindowTitle(Project? project)
    {
        EditorConfigurationOptions options = EditorConfigurationOptions.Instance;
        if (project == null)
        {
            this.Title = options.TitleBarPrefix;
        }
        else
        {
            StringBuilder sb = new StringBuilder().Append(options.TitleBarPrefix);
            if (!string.IsNullOrEmpty(project.ProjectFilePath))
                sb.Append(" - ").Append(project.ProjectFilePath);

            if (!string.IsNullOrWhiteSpace(project.ProjectName))
                sb.Append(" [").Append(project.ProjectName).Append("]");

            if (project.IsModified)
                sb.Append("*");

            this.Title = sb.ToString();
        }
    }

    private void FitToScale_Click(object? sender, RoutedEventArgs e)
    {
        this.PART_ViewPort?.PART_FreeMoveViewPort?.FitContentToCenter();
    }

    #region Task Manager and Activity System

    private void OnTaskStarted(TaskManager manager, ActivityTask task, int index)
    {
        if (this.primaryActivity == null || this.primaryActivity.IsCompleted)
        {
            this.SetActivityTask(task);
        }
    }

    private void OnTaskCompleted(TaskManager manager, ActivityTask task, int index)
    {
        if (task == this.primaryActivity)
        {
            // try to access next task
            this.SetActivityTask(manager.ActiveTasks.Count > 0 ? manager.ActiveTasks[0] : null);
        }
    }

    private void SetActivityTask(ActivityTask? task)
    {
        IActivityProgress? prog = null;
        if (this.primaryActivity != null)
        {
            prog = this.primaryActivity.Progress;
            prog.CurrentActionChanged -= this.OnPrimaryActivityCurrentActionChanged;
            prog.CompletionState.CompletionValueChanged -= this.OnPrimaryActionCompletionValueChanged;
            prog.IsIndeterminateChanged -= this.OnPrimaryActivityIndeterminateChanged;
            prog = null;
        }

        this.primaryActivity = task;
        if (task != null)
        {
            prog = task.Progress;
            prog.CurrentActionChanged += this.OnPrimaryActivityCurrentActionChanged;
            prog.CompletionState.CompletionValueChanged += this.OnPrimaryActionCompletionValueChanged;
            prog.IsIndeterminateChanged += this.OnPrimaryActivityIndeterminateChanged;
            this.PART_ActiveBackgroundTaskGrid.IsVisible = true;
        }
        else
        {
            this.PART_ActiveBackgroundTaskGrid.IsVisible = false;
        }

        this.OnPrimaryActivityCurrentActionChanged(prog);
        this.OnPrimaryActionCompletionValueChanged(prog?.CompletionState);
        this.OnPrimaryActivityIndeterminateChanged(prog);
    }

    private void OnPrimaryActivityCurrentActionChanged(IActivityProgress? tracker)
    {
        IoC.Dispatcher.Invoke(() => this.PART_TaskCaption.Text = tracker?.CurrentAction ?? "", DispatchPriority.Loaded);
    }

    private void OnPrimaryActionCompletionValueChanged(CompletionState? state)
    {
        IoC.Dispatcher.Invoke(() => this.PART_ActiveBgProgress.Value = state?.TotalCompletion ?? 0.0, DispatchPriority.Loaded);
    }

    private void OnPrimaryActivityIndeterminateChanged(IActivityProgress? tracker)
    {
        IoC.Dispatcher.Invoke(() => this.PART_ActiveBgProgress.IsIndeterminate = tracker?.IsIndeterminate ?? false, DispatchPriority.Loaded);
    }

    #endregion

    private void CloseTimelineClick(object? sender, RoutedEventArgs e)
    {
        if (this.VideoEditor?.Project is Project project)
        {
            project.ActiveTimeline = project.MainTimeline;
        }
    }
    
    public void CenterViewPort()
    {
        IoC.Dispatcher.InvokeAsync(() => this.PART_ViewPort?.PART_FreeMoveViewPort?.FitContentToCenter(), DispatchPriority.Background);
    }
}