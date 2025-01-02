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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FramePFX.Avalonia.Editing.Toolbars;
using FramePFX.BaseFrontEnd.AvControls;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.BaseFrontEnd.Interactivity.Contexts;
using FramePFX.BaseFrontEnd.Themes.Controls;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Editing;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Toolbars;
using FramePFX.Editing.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Persistence;
using FramePFX.PropertyEditing;
using FramePFX.Tasks;
using FramePFX.Themes;
using FramePFX.Toolbars;
using FramePFX.Utils;
using FramePFX.Utils.Collections.Observable;
using FramePFX.Utils.RDA;
using SkiaSharp;

namespace FramePFX.Avalonia;

public partial class EditorWindow : WindowEx, ITopLevel, IVideoEditorWindow {
    public VideoEditor VideoEditor { get; }
    
    public VideoEditorPropertyEditor PropertyEditor { get; }
    
    public bool IsClosing { get; private set; }
    
    public bool IsClosed { get; private set; }

    ITimelineElement IVideoEditorWindow.TimelineElement => this.TheTimeline;

    IResourceManagerElement IVideoEditorWindow.ResourceManager => this.PART_ResourcePanelControl;

    IViewPortElement IVideoEditorWindow.ViewPort => this.PART_ViewPort;
    
    private readonly NumberAverager renderTimeAverager;
    private readonly RateLimitedDispatchAction<Timeline> updateFpsInfoRlda;
    private ActivityTask? primaryActivity;
    private Project? activeProject;
    private ObservableItemProcessorIndexing<Theme>? themeListHandler;
    private ObservableItemProcessorIndexing<ToolBarButton>? disposeWestButtons;
    private ObservableItemProcessorIndexing<ToolBarButton>? disposeCenterButtons;
    private ObservableItemProcessorIndexing<ToolBarButton>? disposeEastButtons;
    private bool isUpdatingToolBarCentering;

    public EditorWindow(VideoEditor videoEditor) {
        this.VideoEditor = videoEditor;
        this.PropertyEditor = new VideoEditorPropertyEditor();
        this.InitializeComponent();
        // TemplateUtils.ApplyRecursive(this);
        // this.Measure(default);

        // average 5 samples. Will take a second to catch up when playing at 5 fps but meh
        this.renderTimeAverager = new NumberAverager(5);
        this.TheTimeline.OnConnectedToEditor(this);

        this.updateFpsInfoRlda = RateLimitedDispatchActionBase.ForDispatcherSync<Timeline>((t) => {
            if (t.Project?.Editor is VideoEditor editor) {
                double avgPlaybackMillis = editor.Playback.AveragePlaybackIntervalMillis;
                this.PART_AvgFPSBlock.Text = $"PB: {((int) Math.Round(1000.0 / avgPlaybackMillis)).ToString(),3} FPS ({Math.Round(avgPlaybackMillis, 2).ToString(),5} ms)";
            }

            double avgRenderMillis = this.renderTimeAverager.GetAverage();
            this.PART_AvgRenderTimeBlock.Text = $"RT: {Math.Round(avgRenderMillis, 2).ToString(),5} ms ({((int) Math.Round(1000.0 / avgRenderMillis)).ToString(),3} FPS)";
        }, TimeSpan.FromSeconds(0.05), DispatchPriority.Loaded);

        using (MultiChangeToken batch = DataManager.GetContextData(this).BeginChange()) {
            batch.Context.Set(DataKeys.TopLevelHostKey, this).Set(DataKeys.VideoEditorUIKey, this);
        }

        ActivityManager activityManager = ActivityManager.Instance;
        activityManager.TaskStarted += this.OnTaskStarted;
        activityManager.TaskCompleted += this.OnTaskCompleted;
    }

    private static ObservableItemProcessorIndexing<ToolBarButton> CreateToolbarBinder(IObservableList<ToolBarButton> list, StackPanel stackPanel) {
        int originalCounter = stackPanel.Children.Count;
        return ObservableItemProcessor.MakeIndexable(list, (sender, index, item) => {
            AbstractAvaloniaButtonElement btnImpl = (AbstractAvaloniaButtonElement) item.Button;
            if (btnImpl.Button is IIconButton button) {
                button.IconMaxWidth = 15;
                button.IconMaxHeight = 15;
            }

            btnImpl.Button.MinWidth = 23;
            btnImpl.Button.Height = 23;
            btnImpl.Button.Padding = new Thickness(3);
            btnImpl.Button.BorderThickness = new Thickness(1);
            stackPanel.Children.Insert(index + originalCounter, btnImpl.Button);
            item.UpdateCanExecuteLater();
        }, (sender, index, item) => {
            stackPanel.Children.RemoveAt(index + originalCounter);
        }, (sender, oldIndex, newIndex, item) => {
            stackPanel.Children.MoveItem(oldIndex + originalCounter, newIndex + originalCounter);
        }).ProcessExistingItems();
    }
    
    private void OnTimelineClipSelectionChanged(ILightSelectionManager<IClipElement> sender) {
        this.PART_ViewPort.OnClipSelectionChanged();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.ThePropertyEditor.PropertyEditor = this.PropertyEditor;
        using (MultiChangeToken myDataBatch = DataManager.GetContextData(this).BeginChange()) {
            this.VideoEditor.ProjectChanged += this.OnProjectChanged;
            this.VideoEditor.IsExportingChanged += this.OnIsExportingChanged;
            TemplateUtils.Apply(this.PART_ViewPort);
            this.PART_ViewPort.Owner = this;
            this.PART_ViewPort.VideoEditor = this.VideoEditor;
            this.OnProjectChanged(null, this.VideoEditor.Project);
            this.OnIsExportingChanged(this.VideoEditor);
            myDataBatch.Context.Set(DataKeys.VideoEditorKey, this.VideoEditor);
        }

        DataManager.GetContextData(this.PART_TimelinePresenterGroupBox).Set(DataKeys.TimelineUIKey, this.TheTimeline);
        this.PART_ActiveBackgroundTaskGrid.IsVisible = false;
        ((ILightSelectionManager<IClipElement>) this.TheTimeline.ClipSelectionManager!).SelectionChanged += this.OnTimelineClipSelectionChanged;
        this.PART_ViewPort.OnClipSelectionChanged();

        EditorConfigurationOptions.Instance.TitleBarPrefixChanged += this.OnApplicationTitleBarPrefixChanged;
        EditorConfigurationOptions.Instance.TitleBarBrushChanged += this.OnApplicationTitleBarBrushChanged;
        
        this.themeListHandler = ObservableItemProcessor.MakeIndexable(ThemeManager.Instance.Themes, (sender, index, item) => {
            MenuItem menuItem = new MenuItem() { Header = item.Name, Tag = item };
            menuItem.Click += this.OnSetThemeMenuItemClicked;
            this.PART_ThemesMenuItem.Items.Insert(index, menuItem);
        }, (sender, index, item) => {
            this.PART_ThemesMenuItem.Items.RemoveAt(index);
        }, (sender, oldIndex, newIndex, item) => {
            CollectionUtils.MoveItem(this.PART_ThemesMenuItem.Items, oldIndex, newIndex);
        }).ProcessExistingItems();
        
        ViewPortToolBarManager manager = ViewPortToolBarManager.GetInstance(this.VideoEditor);
        this.disposeWestButtons = CreateToolbarBinder(manager.WestButtons, this.PART_ToolBar_West);
        this.disposeCenterButtons = CreateToolbarBinder(manager.CenterButtons, this.PART_ToolBar_Center);
        this.disposeEastButtons = CreateToolbarBinder(manager.EastButtons, this.PART_ToolBar_East);
    }

    private void OnSetThemeMenuItemClicked(object? sender, RoutedEventArgs e) {
        Theme theme = (Theme) ((MenuItem) sender!).Tag!;
        theme.ThemeManager.SetTheme(theme);
        StartupConfigurationOptions.Instance.StartupTheme = theme.Name;
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        
        this.themeListHandler?.Dispose();
        this.themeListHandler = null;
        this.PART_ThemesMenuItem.Items.Clear();
        
        this.disposeWestButtons!.Dispose();
        this.disposeCenterButtons!.Dispose();
        this.disposeEastButtons!.Dispose();
        this.disposeWestButtons = null;
        this.disposeCenterButtons = null;
        this.disposeEastButtons = null;
        
        // Prevent semantic memory leak
        EditorConfigurationOptions.Instance.TitleBarPrefixChanged -= this.OnApplicationTitleBarPrefixChanged;
        EditorConfigurationOptions.Instance.TitleBarBrushChanged -= this.OnApplicationTitleBarBrushChanged;
        
        if (this.activeProject != null) {
            this.VideoEditor.CloseProject();
        }

        this.TheTimeline.OnDisconnectedFromEditor();
        this.VideoEditor.Destroy();
    }
    
    private void OnApplicationTitleBarPrefixChanged(PersistentConfiguration config, PersistentProperty<string> property, string oldValue, string newValue) {
        this.UpdateWindowTitle(this.VideoEditor.Project);
    }

    private void OnApplicationTitleBarBrushChanged(PersistentConfiguration config, PersistentProperty<ulong> property, ulong oldValue, ulong newValue) {
        SKColor c = ((EditorConfigurationOptions) config).TitleBarBrush;
        this.TitleBarBrush = new ImmutableSolidColorBrush(new Color(c.Alpha, c.Red, c.Green, c.Blue));
    }

    private void OnIsExportingChanged(VideoEditor editor) {
        this.PART_EditorWindowContent.IsEnabled = !editor.IsExporting;
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

        this.activeProject = newProject;
        DataManager.GetContextData(this).Set(DataKeys.ProjectKey, newProject);
        this.PART_ResourcePanelControl.ResourceManager = newProject?.ResourceManager;
        this.OnActiveTimelineChanged(oldProject?.ActiveTimeline, newProject?.ActiveTimeline);
        this.UpdateWindowTitle(newProject);

        // DataManager.GetContextData(this).Set(DataKeys.ResourceTreeSelectionManagerKey, this.PART_ResourcePanelControl.MultiSelectionManager);
        // DataManager.GetContextData(this).Set(DataKeys.TrackSelectionManagerKey, newProject);
        // DataManager.GetContextData(this).Set(DataKeys.ClipSelectionManagerKey, newProject);
        // DataManager.GetContextData(this).Set(DataKeys.TimelineClipSelectionManagerKey, newProject);
        VideoEditorPropertyEditorHelper.OnProjectChanged(this);

        Application.Instance.Dispatcher.InvokeAsync(() => {
            this.PART_ViewPort?.PART_FreeMoveViewPort?.FitContentToCenter();
        }, DispatchPriority.Background);
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
        }

        if (newTimeline != null) {
            newTimeline.RenderManager.FrameRendered += this.UpdateFrameRenderInterval;
            if (newTimeline is CompositionTimeline newComposition) {
                newComposition.Resource.DisplayNameChanged += this.OnCompositionTimelineDisplayNameChanged;
            }
        }

        this.TheTimeline.Timeline = newTimeline;
        DataManager.GetContextData(this.PART_TimelinePresenterGroupBox).Set(DataKeys.TimelineKey, newTimeline);
        this.UpdateTimelineName();
        this.PART_CloseTimelineButton.IsEnabled = newTimeline is Timeline timeline && timeline is CompositionTimeline;
    }

    private void UpdateFrameRenderInterval(RenderManager manager) {
        this.renderTimeAverager.PushValue(manager.AverageVideoRenderTimeMillis);
        this.updateFpsInfoRlda.InvokeAsync(manager.Timeline);
    }

    private void OnCompositionTimelineDisplayNameChanged(IDisplayName sender, string? oldName, string? newName) => this.UpdateTimelineName();

    private void OnProjectFilePathChanged(Project project) => this.UpdateWindowTitle(project);

    private void OnProjectNameChanged(Project project) => this.UpdateWindowTitle(project);

    private void OnProjectModifiedChanged(Project project) => this.UpdateWindowTitle(project);

    private void UpdateTimelineName() {
        Timeline? timeline = this.TheTimeline?.Timeline;
        if (timeline == null) {
            this.PART_TimelineName.Text = "No timeline loaded";
        }
        else if (timeline is CompositionTimeline composition) {
            string? text = composition.Resource.DisplayName;
            this.PART_TimelineName.Text = string.IsNullOrWhiteSpace(text) ? "Unnamed Composition Timeline" : text;
        }
        else {
            this.PART_TimelineName.Text = "Project Timeline";
        }
    }

    private void UpdateWindowTitle(Project? project) {
        EditorConfigurationOptions options = EditorConfigurationOptions.Instance;
        if (project == null) {
            this.Title = options.TitleBarPrefix;
        }
        else {
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

    private void FitToScale_Click(object? sender, RoutedEventArgs e) {
        this.PART_ViewPort?.PART_FreeMoveViewPort?.FitContentToCenter();
    }

    #region Task Manager and Activity System

    private void OnTaskStarted(ActivityManager manager, ActivityTask task, int index) {
        if (this.primaryActivity == null || this.primaryActivity.IsCompleted) {
            this.SetActivityTask(task);
        }
    }

    private void OnTaskCompleted(ActivityManager manager, ActivityTask task, int index) {
        if (task == this.primaryActivity) {
            // try to access next task
            this.SetActivityTask(manager.ActiveTasks.Count > 0 ? manager.ActiveTasks[0] : null);
        }
    }

    private void SetActivityTask(ActivityTask? task) {
        IActivityProgress? prog = null;
        if (this.primaryActivity != null) {
            prog = this.primaryActivity.Progress;
            prog.TextChanged -= this.OnPrimaryActivityTextChanged;
            prog.CompletionState.CompletionValueChanged -= this.OnPrimaryActionCompletionValueChanged;
            prog.IsIndeterminateChanged -= this.OnPrimaryActivityIndeterminateChanged;
            prog = null;
        }

        this.primaryActivity = task;
        if (task != null) {
            prog = task.Progress;
            prog.TextChanged += this.OnPrimaryActivityTextChanged;
            prog.CompletionState.CompletionValueChanged += this.OnPrimaryActionCompletionValueChanged;
            prog.IsIndeterminateChanged += this.OnPrimaryActivityIndeterminateChanged;
            this.PART_ActiveBackgroundTaskGrid.IsVisible = true;
        }
        else {
            this.PART_ActiveBackgroundTaskGrid.IsVisible = false;
        }

        this.OnPrimaryActivityTextChanged(prog);
        this.OnPrimaryActionCompletionValueChanged(prog?.CompletionState);
        this.OnPrimaryActivityIndeterminateChanged(prog);
    }

    private void OnPrimaryActivityTextChanged(IActivityProgress? tracker) {
        Application.Instance.Dispatcher.Invoke(() => this.PART_TaskCaption.Text = tracker?.Text ?? "", DispatchPriority.Loaded);
    }

    private void OnPrimaryActionCompletionValueChanged(CompletionState? state) {
        Application.Instance.Dispatcher.Invoke(() => this.PART_ActiveBgProgress.Value = state?.TotalCompletion ?? 0.0, DispatchPriority.Loaded);
    }

    private void OnPrimaryActivityIndeterminateChanged(IActivityProgress? tracker) {
        Application.Instance.Dispatcher.Invoke(() => this.PART_ActiveBgProgress.IsIndeterminate = tracker?.IsIndeterminate ?? false, DispatchPriority.Loaded);
    }

    #endregion

    private void CloseTimelineClick(object? sender, RoutedEventArgs e) {
        if (this.VideoEditor.Project is Project project) {
            project.ActiveTimeline = project.MainTimeline;
        }
    }

    public void CenterViewPort() {
        Application.Instance.Dispatcher.InvokeAsync(() => this.PART_ViewPort?.PART_FreeMoveViewPort?.FitContentToCenter(), DispatchPriority.Background);
    }

    protected override Task<bool> OnClosingAsync(WindowCloseReason reason) {
        this.IsClosing = true;
        return base.OnClosingAsync(reason);
    }

    protected override void OnClosed(EventArgs e) {
        this.IsClosing = false;
        this.IsClosed = true;
        base.OnClosed(e);
    }

    private void PART_ToolBarPanel_OnSizeChanged(object? sender, SizeChangedEventArgs e) => this.UpdateToolBarCentering();
    private void PART_ToolBar_West_OnSizeChanged(object? sender, SizeChangedEventArgs e) => this.UpdateToolBarCentering();
    private void PART_ToolBar_Center_OnSizeChanged(object? sender, SizeChangedEventArgs e) => this.UpdateToolBarCentering();
    private void PART_ToolBar_East_OnSizeChanged(object? sender, SizeChangedEventArgs e) => this.UpdateToolBarCentering();
    
    private void UpdateToolBarCentering() {
        if (this.isUpdatingToolBarCentering) {
            throw new Exception("Impossible reentrency");
        }

        this.isUpdatingToolBarCentering = true;
        
        // Modified version of https://stackoverflow.com/a/61054009
        StackPanel centerPanel = this.PART_ToolBar_Center!;
        double centerSize = this.PART_ToolBar_Center.Bounds.Width;
        double leftSize = this.PART_ToolBar_West.Bounds.Width;
        double rightSize = this.PART_ToolBar_East.Bounds.Width;
        double width = (this.PART_ToolBarPanel.Bounds.Width / 2) - (centerSize / 2);
        const double panel_gap = 4;

        if (width - leftSize - panel_gap <= 0) {
            centerPanel.HorizontalAlignment = HorizontalAlignment.Left;
            centerPanel.Margin = new Thickness(leftSize + panel_gap, 0, 0, 0);
        }
        else if (width - rightSize - panel_gap <= 0) {
            centerPanel.HorizontalAlignment = HorizontalAlignment.Right;
            centerPanel.Margin = new Thickness(0, 0, rightSize + panel_gap, 0);
        }
        else {
            centerPanel.HorizontalAlignment = HorizontalAlignment.Center;
            centerPanel.Margin = default;
        }

        this.isUpdatingToolBarCentering = false;
    }
}