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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing;
using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Effects;

namespace FramePFX.Avalonia.PropertyEditing.Automation;

/// <summary>
/// A control which contains the common 3 buttons for automation: toggle override, insert key frame and reset value
/// </summary>
[TemplatePart(Name = "PART_ToggleOverride", Type = typeof(ToggleButton))]
[TemplatePart(Name = "PART_InsertKeyFrame", Type = typeof(Button))]
[TemplatePart(Name = "PART_ResetValue", Type = typeof(Button))]
public class KeyFrameToolsControl : TemplatedControl
{
    public static readonly StyledProperty<AutomationSequence?> AutomationSequenceProperty = AvaloniaProperty.Register<KeyFrameToolsControl, AutomationSequence?>(nameof(AutomationSequence));

    public AutomationSequence? AutomationSequence
    {
        get => this.GetValue(AutomationSequenceProperty);
        set => this.SetValue(AutomationSequenceProperty, value);
    }

    private bool isUpdatingToggleOverride;

    private ToggleButton PART_ToggleOverride;
    private Button PART_InsertKeyFrame;
    private Button PART_ResetValue;

    private IStrictFrameRange? strictFrameRange;

    public KeyFrameToolsControl() {
    }

    static KeyFrameToolsControl()
    {
        AutomationSequenceProperty.Changed.AddClassHandler<KeyFrameToolsControl, AutomationSequence?>((d, e) => d.OnAutomationSequenceChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnAutomationSequenceChanged(AutomationSequence? oldValue, AutomationSequence? newValue)
    {
        if (oldValue != null)
        {
            oldValue.OverrideStateChanged -= this.OnOverrideStateChanged;

            this.strictFrameRange = null;
            IAutomatable oldOwner = oldValue.AutomationData.Owner;
            oldOwner.TimelineChanged -= this.OnClipTimelineChanged;
            if (GetClipForAutomatable(oldOwner, out Clip clip))
                clip.FrameSpanChanged -= this.OnOwnerClipFrameSpanChanged;
        }

        if (newValue != null)
        {
            newValue.OverrideStateChanged += this.OnOverrideStateChanged;
            IAutomatable newOwner = newValue.AutomationData.Owner;
            this.strictFrameRange = newOwner as IStrictFrameRange;
            newOwner.TimelineChanged += this.OnClipTimelineChanged;
            if (GetClipForAutomatable(newOwner, out Clip clip))
                clip.FrameSpanChanged += this.OnOwnerClipFrameSpanChanged;

            this.UpdateInsertKeyFrame(newOwner);

            Timeline? timeline = newOwner.Timeline;
            if (timeline != null)
                this.OnClipTimelineChanged(newOwner, null, timeline);
        }
        else
        {
            this.UpdateInsertKeyFrame(null);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.PART_ToggleOverride = e.NameScope.GetTemplateChild<ToggleButton>(nameof(this.PART_ToggleOverride));
        this.PART_InsertKeyFrame = e.NameScope.GetTemplateChild<Button>(nameof(this.PART_InsertKeyFrame));
        this.PART_ResetValue = e.NameScope.GetTemplateChild<Button>(nameof(this.PART_ResetValue));

        this.PART_ToggleOverride.IsCheckedChanged += this.OnToggleOverrideChanged;
        this.PART_InsertKeyFrame.Click += this.OnInsertKeyFrameClicked;
        this.PART_ResetValue.Click += this.OnResetValueClicked;
    }

    private void OnToggleOverrideChanged(object? sender, RoutedEventArgs e)
    {
        this.isUpdatingToggleOverride = true;
        if (this.AutomationSequence is AutomationSequence sequence)
        {
            sequence.IsOverrideEnabled = this.PART_ToggleOverride.IsChecked == true;
        }

        this.isUpdatingToggleOverride = false;
    }

    private void OnInsertKeyFrameClicked(object? sender, RoutedEventArgs e)
    {
        if (this.AutomationSequence is AutomationSequence sequence)
        {
            AutomationUtils.TryAddKeyFrameAtLocation(sequence, out _);
            sequence.UpdateValue();
        }
    }

    private void OnResetValueClicked(object? sender, RoutedEventArgs e)
    {
        if (this.AutomationSequence is AutomationSequence sequence)
        {
            AutomationUtils.GetDefaultKeyFrameOrAddNew(sequence, out KeyFrame keyFrame);
            keyFrame.AssignDefaultValue(sequence.Parameter.Descriptor);
            sequence.UpdateValue();
            // sequence.InvalidateTimelineRender();
        }
    }

    private static bool GetClipForAutomatable(IAutomatable automatable, out Clip clip)
    {
        if (automatable == null)
        {
            clip = null;
            return false;
        }
        else if (automatable is Clip automatableClip)
        {
            clip = automatableClip;
        }
        else if (automatable is BaseEffect effect && effect.Owner is Clip effectOwnerClip)
        {
            clip = effectOwnerClip;
        }
        else
        {
            clip = null;
            return false;
        }

        return true;
    }

    private void OnClipTimelineChanged(IHaveTimeline owner, Timeline? oldTimeline, Timeline? newTimeline)
    {
        if (oldTimeline != null)
            oldTimeline.PlayHeadChanged -= this.OnTimelinePlayHeadChanged;
        if (newTimeline != null)
            newTimeline.PlayHeadChanged += this.OnTimelinePlayHeadChanged;
    }

    private void OnTimelinePlayHeadChanged(Timeline timeline, long oldvalue, long newvalue)
    {
        this.UpdateInsertKeyFrame(newvalue);
    }

    private void OnOwnerClipFrameSpanChanged(Clip clip, FrameSpan oldspan, FrameSpan newspan)
    {
        this.UpdateInsertKeyFrame(clip);
    }

    private void UpdateInsertKeyFrame(long timelinePlayHeadFrame)
    {
        bool isInRange = this.strictFrameRange == null || this.strictFrameRange.IsTimelineFrameInRange(timelinePlayHeadFrame);
        if (isInRange != this.PART_InsertKeyFrame.IsEnabled)
        {
            this.PART_InsertKeyFrame.IsEnabled = isInRange;
        }
    }

    private void UpdateInsertKeyFrame(IHaveTimeline owner)
    {
        if (owner?.Timeline is Timeline timeline)
        {
            this.UpdateInsertKeyFrame(timeline.PlayHeadPosition);
        }
        else if (this.PART_InsertKeyFrame.IsEnabled)
        {
            this.PART_InsertKeyFrame.IsEnabled = false;
        }
    }

    private void OnOverrideStateChanged(AutomationSequence sequence)
    {
        if (this.isUpdatingToggleOverride)
            return;
        this.PART_ToggleOverride.IsChecked = sequence.IsOverrideEnabled;
    }
}