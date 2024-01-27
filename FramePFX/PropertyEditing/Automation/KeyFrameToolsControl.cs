using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Editors;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using Track = FramePFX.Editors.Timelines.Tracks.Track;

namespace FramePFX.PropertyEditing.Automation {
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
                AutomatedUtils.TryAddKeyFrameAtLocation(sequence, out KeyFrame keyFrame);
            }
        }

        private void OnResetValueClicked(object sender, RoutedEventArgs e) {
            if (this.AutomationSequence is AutomationSequence sequence) {
                if (AutomatedUtils.TryAddKeyFrameAtLocation(sequence, out KeyFrame keyFrame)) {
                    keyFrame.AssignDefaultValue(sequence.Parameter.Descriptor);
                }
                else {
                    sequence.IsOverrideEnabled = true;
                }
            }
        }

        private static bool GetClipForAutomatable(IAutomatable automatable, out Clip clip) {
            return (clip = automatable as Clip) != null || automatable is BaseEffect effect && (clip = effect.OwnerClip) != null;
        }

        private static void OnAutomationSequenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            KeyFrameToolsControl control = (KeyFrameToolsControl) d;
            if (e.OldValue is AutomationSequence oldSequence) {
                oldSequence.OverrideStateChanged -= control.OnOverrideStateChanged;
                oldSequence.AutomationData.ActiveParameterChanged -= control.OnActiveParameterChanged;

                control.strictFrameRange = null;
                IAutomatable oldOwner = oldSequence.AutomationData.Owner;
                oldOwner.TimelineChanged -= control.OnClipTimelineChanged;
                if (GetClipForAutomatable(oldOwner, out Clip clip))
                    clip.FrameSpanChanged -= control.OnOwnerClipFrameSpanChanged;
            }

            if (e.NewValue is AutomationSequence newSequence) {
                newSequence.OverrideStateChanged += control.OnOverrideStateChanged;
                newSequence.AutomationData.ActiveParameterChanged += control.OnActiveParameterChanged;

                IAutomatable newOwner = newSequence.AutomationData.Owner;
                control.strictFrameRange = newOwner as IStrictFrameRange;
                newOwner.TimelineChanged += control.OnClipTimelineChanged;
                if (GetClipForAutomatable(newOwner, out Clip clip))
                    clip.FrameSpanChanged += control.OnOwnerClipFrameSpanChanged;

                control.UpdateInsertKeyFrame(newOwner);
                control.OnClipTimelineChanged(newOwner, null, newOwner.Timeline);
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

        private void OnActiveParameterChanged(AutomationData data, ParameterKey oldkey, ParameterKey newkey) {

        }
    }
}