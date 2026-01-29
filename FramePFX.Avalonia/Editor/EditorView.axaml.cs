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

using Avalonia;
using Avalonia.Controls;
using FramePFX.Editing;
using FramePFX.Editing.ViewStates;
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
        }
    }

    private void OnProjectLoaded(object? sender, ProjectLoadedEventArgs e) {
        this.lazyTimeline.Value1 = TimelineViewState.GetInstance(e.Project.MainTimeline, this.TopLevelIdentifier);
    }

    private void OnProjectUnloaded(object? sender, ProjectUnloadedEventArgs e) {
        this.lazyTimeline.Value1 = default;
    }
}