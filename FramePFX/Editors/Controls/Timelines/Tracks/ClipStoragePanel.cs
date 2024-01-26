using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Controls.Timelines.Tracks.Clips;
using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Controls.Timelines.Tracks {
    /// <summary>
    /// A panel which stores a track's clip items. This is pretty much just a canvas
    /// </summary>
    public class ClipStoragePanel : Panel {
        public TimelineTrackControl Track { get; set; }

        private readonly Stack<TimelineClipControl> itemCache;

        public UIElementCollection MyInternalChildren => this.InternalChildren;

        public ClipStoragePanel() {
            this.itemCache = new Stack<TimelineClipControl>();
        }

        public IEnumerable<TimelineClipControl> GetClips() => this.InternalChildren.Cast<TimelineClipControl>();

        public void InsertClip(Clip clip, int index) {
            this.InsertClip(this.itemCache.Count > 0 ? this.itemCache.Pop() : new TimelineClipControl(), clip, index);
        }

        public void InsertClip(TimelineClipControl control, Clip clip, int index) {
            if (this.Track == null)
                throw new InvalidOperationException("Cannot insert clips without a track associated");
            control.OnAdding(this.Track, clip);
            this.InternalChildren.Insert(index, control);
            // control.InvalidateMeasure();
            // control.UpdateLayout();
            control.ApplyTemplate();
            control.OnAdded();
            this.Track.OwnerPanel.TimelineControl.UpdateClipAutomationVisibility(control);
        }

        public void RemoveClipInternal(int index, bool canCache = true) {
            TimelineClipControl control = (TimelineClipControl) this.InternalChildren[index];
            control.OnRemoving();
            this.InternalChildren.RemoveAt(index);
            control.OnRemoved();
            if (canCache && this.itemCache.Count < 16)
                this.itemCache.Push(control);
        }

        public void ClearClipsInternal(bool canCache = true) {
            int count = this.InternalChildren.Count;
            for (int i = count - 1; i >= 0; i--) {
                this.RemoveClipInternal(i, canCache);
            }
        }

        protected override Size MeasureOverride(Size availableSize) {
            if (this.Track != null) {
                availableSize.Height = this.Track.Track.Height;
            }

            Size total = new Size();
            UIElementCollection items = this.InternalChildren;
            int count = items.Count;
            for (int i = 0; i < count; i++) {
                UIElement item = items[i];
                item.Measure(availableSize);
                Size size = item.DesiredSize;
                total.Width = Math.Max(total.Width, size.Width);
                total.Height = Math.Max(total.Height, size.Height);
            }

            return new Size(total.Width, availableSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            UIElementCollection items = this.InternalChildren;
            for (int i = 0, count = items.Count; i < count; i++) {
                TimelineClipControl clip = (TimelineClipControl) items[i];
                clip.Arrange(new Rect(clip.PixelBegin, 0, clip.PixelWidth, finalSize.Height));
            }

            return finalSize;
        }
    }
}