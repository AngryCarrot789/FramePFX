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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Editors;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;

namespace FramePFX.PropertyEditing.Controls.Automation {
    /// <summary>
    /// A control which contains the common 3 buttons for automation: toggle override, insert key frame and reset value
    /// </summary>
    [TemplatePart(Name = "PART_ToggleOverride", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_InsertKeyFrame", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ResetValue", Type = typeof(Button))]
    public class KeyFrameToolsControl : Control {
        public static readonly DependencyProperty AutomationSequenceProperty = DependencyProperty.Register("AutomationSequence", typeof(AutomationSequence), typeof(KeyFrameToolsControl), new PropertyMetadata(null, OnAutomationSequenceChanged));

        public AutomationSequence AutomationSequence {
            get => (AutomationSequence) this.GetValue(AutomationSequenceProperty);
            set => this.SetValue(AutomationSequenceProperty, value);
        }

        private bool isUpdatingToggleOverride;
        
        private ToggleButton PART_ToggleOverride;
        private Button PART_InsertKeyFrame;
        private Button PART_ResetValue;

        private IStrictFrameRange strictFrameRange;

        public KeyFrameToolsControl() {

        }

        static KeyFrameToolsControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(KeyFrameToolsControl), new FrameworkPropertyMetadata(typeof(KeyFrameToolsControl)));

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_ToggleOverride = this.GetTemplateChild(nameof(this.PART_ToggleOverride)) as ToggleButton ?? throw new Exception("Missing " + nameof(this.PART_ToggleOverride));
            this.PART_InsertKeyFrame = this.GetTemplateChild(nameof(this.PART_InsertKeyFrame)) as Button ?? throw new Exception("Missing " + nameof(this.PART_InsertKeyFrame));
            this.PART_ResetValue = this.GetTemplateChild(nameof(this.PART_ResetValue)) as Button ?? throw new Exception("Missing " + nameof(this.PART_ResetValue));

            this.PART_ToggleOverride.Checked += this.OnToggleOverrideChanged;
            this.PART_ToggleOverride.Unchecked += this.OnToggleOverrideChanged;
            this.PART_InsertKeyFrame.Click += this.OnInsertKeyFrameClicked;
            this.PART_ResetValue.Click += this.OnResetValueClicked;
        }

        private void OnToggleOverrideChanged(object sender, RoutedEventArgs e) {
            this.isUpdatingToggleOverride = true;
            if (this.AutomationSequence is AutomationSequence sequence) {
                sequence.IsOverrideEnabled = this.PART_ToggleOverride.IsChecked == true;
            }
            this.isUpdatingToggleOverride = false;
        }

        private void OnInsertKeyFrameClicked(object sender, RoutedEventArgs e) {
            if (this.AutomationSequence is AutomationSequence sequence) {
                AutomatedUtils.TryAddKeyFrameAtLocation(sequence, out _);
                sequence.UpdateValue();
            }
        }

        private void OnResetValueClicked(object sender, RoutedEventArgs e) {
            if (this.AutomationSequence is AutomationSequence sequence) {
                AutomatedUtils.GetDefaultKeyFrameOrAddNew(sequence, out KeyFrame keyFrame);
                keyFrame.AssignDefaultValue(sequence.Parameter.Descriptor);
                sequence.UpdateValue();
                // sequence.InvalidateTimelineRender();
            }
        }

        private static bool GetClipForAutomatable(IAutomatable automatable, out Clip clip) {
            if (automatable == null) {
                clip = null;
                return false;
            }
            else if (automatable is Clip automatableClip) {
                clip = automatableClip;
            }
            else if (automatable is BaseEffect effect && effect.Owner is Clip effectOwnerClip) {
                clip = effectOwnerClip;
            }
            else {
                clip = null;
                return false;
            }

            return true;
        }

        private static void OnAutomationSequenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            KeyFrameToolsControl control = (KeyFrameToolsControl) d;
            if (e.OldValue is AutomationSequence oldSequence) {
                oldSequence.OverrideStateChanged -= control.OnOverrideStateChanged;

                control.strictFrameRange = null;
                IAutomatable oldOwner = oldSequence.AutomationData.Owner;
                oldOwner.TimelineChanged -= control.OnClipTimelineChanged;
                if (GetClipForAutomatable(oldOwner, out Clip clip))
                    clip.FrameSpanChanged -= control.OnOwnerClipFrameSpanChanged;
            }

            if (e.NewValue is AutomationSequence newSequence) {
                newSequence.OverrideStateChanged += control.OnOverrideStateChanged;

                IAutomatable newOwner = newSequence.AutomationData.Owner;
                control.strictFrameRange = newOwner as IStrictFrameRange;
                newOwner.TimelineChanged += control.OnClipTimelineChanged;
                if (GetClipForAutomatable(newOwner, out Clip clip))
                    clip.FrameSpanChanged += control.OnOwnerClipFrameSpanChanged;

                control.UpdateInsertKeyFrame(newOwner);

                Timeline timeline = newOwner.Timeline;
                if (timeline != null)
                    control.OnClipTimelineChanged(newOwner, null, timeline);
            }
            else {
                control.UpdateInsertKeyFrame(null);
            }
        }

        private void OnClipTimelineChanged(IHaveTimeline owner, Timeline oldtimeline, Timeline newtimeline) {
            if (oldtimeline != null)
                oldtimeline.PlayHeadChanged -= this.OnTimelinePlayHeadChanged;
            if (newtimeline != null)
                newtimeline.PlayHeadChanged += this.OnTimelinePlayHeadChanged;
        }

        private void OnTimelinePlayHeadChanged(Timeline timeline, long oldvalue, long newvalue) {
            this.UpdateInsertKeyFrame(newvalue);
        }

        private void OnOwnerClipFrameSpanChanged(Clip clip, FrameSpan oldspan, FrameSpan newspan) {
            this.UpdateInsertKeyFrame(clip);
        }

        private void UpdateInsertKeyFrame(long timelinePlayHeadFrame) {
            bool isInRange = this.strictFrameRange == null || this.strictFrameRange.IsTimelineFrameInRange(timelinePlayHeadFrame);
            if (isInRange != this.PART_InsertKeyFrame.IsEnabled) {
                this.PART_InsertKeyFrame.IsEnabled = isInRange;
            }
        }

        private void UpdateInsertKeyFrame(IHaveTimeline owner) {
            if (owner?.Timeline is Timeline timeline) {
                this.UpdateInsertKeyFrame(timeline.PlayHeadPosition);
            }
            else if (this.PART_InsertKeyFrame.IsEnabled) {
                this.PART_InsertKeyFrame.IsEnabled = false;
            }
        }

        private void OnOverrideStateChanged(AutomationSequence sequence) {
            if (this.isUpdatingToggleOverride)
                return;
            this.PART_ToggleOverride.IsChecked = sequence.IsOverrideEnabled;
        }
    }
}