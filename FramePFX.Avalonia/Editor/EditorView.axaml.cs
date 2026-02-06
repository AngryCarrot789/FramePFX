// 
// Copyright (c) 2026-2026 REghZy
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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using FramePFX.Editing;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI;
using PFXToolKitUI.EventHelpers;
using PFXToolKitUI.Interactivity.Windowing;

namespace FramePFX.Avalonia.Editor;

public partial class EditorView : UserControl {
    public static readonly DirectProperty<EditorView, VideoEditor?> VideoEditorProperty = AvaloniaProperty.RegisterDirect<EditorView, VideoEditor?>(nameof(VideoEditor), o => o.VideoEditor, (o, v) => o.VideoEditor = v);

    public VideoEditor? VideoEditor {
        get => field;
        set => this.SetAndRaise(VideoEditorProperty, ref field, value);
    }

    public TopLevelIdentifier TopLevelIdentifier { get; }

    private readonly LazyHelper2<TimelineViewState, bool> lazyTimeline;
    private bool isUpdatingToolBarCentering;

    public EditorView(TopLevelIdentifier topLevelIdentifier) {
        this.TopLevelIdentifier = topLevelIdentifier;
        this.InitializeComponent();
        this.lazyTimeline = new LazyHelper2<TimelineViewState, bool>((a, b, both) => this.PART_Timeline.Timeline = b && both ? a : null);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        this.lazyTimeline.Value2 = true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        this.lazyTimeline.Value2 = false;
    }

    static EditorView() {
        VideoEditorProperty.Changed.AddClassHandler<EditorView, VideoEditor?>((s, e) => s.OnVideoEditorChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        
        this.VideoEditor?.Project?.MainTimeline.RenderManager.InvalidateRender();
    }

    private void OnVideoEditorChanged(VideoEditor? oldValue, VideoEditor? newValue) {
        if (oldValue != null) {
            oldValue.ProjectLoaded -= this.OnProjectLoaded;
            oldValue.ProjectUnloaded -= this.OnProjectUnloaded;
            if (oldValue.Project != null) {
                this.OnProjectUnloaded(oldValue, new ProjectUnloadedEventArgs(oldValue.Project));
            }
        }

        if (newValue != null) {
            newValue.ProjectLoaded += this.OnProjectLoaded;
            newValue.ProjectUnloaded += this.OnProjectUnloaded;
            if (newValue.Project != null) {
                this.OnProjectLoaded(newValue, new ProjectLoadedEventArgs(newValue.Project));
            }

            this.PART_ViewPort.VideoEditor = VideoEditorViewState.GetInstance(newValue, this.TopLevelIdentifier);
            
            ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
                this.PART_ViewPort?.PART_FreeMoveViewPort?.FitContentToCenter();
            }, DispatchPriority.Background);
        }
        else {
            this.PART_ViewPort.VideoEditor = null;
        }
    }

    private void OnProjectLoaded(object? sender, ProjectLoadedEventArgs e) {
        this.lazyTimeline.Value1 = TimelineViewState.GetInstance(e.Project.MainTimeline, this.TopLevelIdentifier);
    }

    private void OnProjectUnloaded(object? sender, ProjectUnloadedEventArgs e) {
        this.lazyTimeline.Value1 = default;
    }
    
    private void FitToScale_Click(object? sender, RoutedEventArgs e) {
        this.PART_ViewPort?.PART_FreeMoveViewPort?.FitContentToCenter();
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