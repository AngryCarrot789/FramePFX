using System;
using System.Windows;
using System.Windows.Controls;

namespace FramePFX.WPF.Editor.Timeline.Controls {
    /// <summary>
    /// A timeline control, which has a layer rack and a sequence editor
    /// </summary>
    [TemplatePart(Name = "PART_TrackList", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_TimelineEditor", Type = typeof(TimelineEditorControl))]
    public class TimelineControl : Control {
        public ListBox TrackList { get; private set; }
        public TimelineEditorControl TimelineEditor { get; private set; }

        public TimelineControl() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.TrackList = this.GetTemplateChild("PART_TrackList") as ListBox ?? throw new Exception("Missing track list");
            this.TimelineEditor = this.GetTemplateChild("PART_TimelineEditor") as TimelineEditorControl ?? throw new Exception("Missing timeline editor");
        }
    }
}