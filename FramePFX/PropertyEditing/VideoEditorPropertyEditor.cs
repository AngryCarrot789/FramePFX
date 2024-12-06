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

using FramePFX.Editing.PropertyEditors;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Interactivity.Formatting;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.Core;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.PropertyEditing;

/// <summary>
/// A class which stores the video editor's general property editor information
/// </summary>
public class VideoEditorPropertyEditor : PropertyEditor {
    public static VideoEditorPropertyEditor Instance { get; } = new VideoEditorPropertyEditor();

    public SimplePropertyEditorGroup ClipGroup { get; }

    public SimplePropertyEditorGroup TrackGroup { get; }

    public EffectListPropertyEditorGroup ClipEffectListGroup { get; }

    public EffectListPropertyEditorGroup TrackEffectListGroup { get; }

    private VideoEditorPropertyEditor() {
        {
            this.ClipGroup = new SimplePropertyEditorGroup(typeof(Clip)) {
                DisplayName = "Clip", IsExpanded = true
            };

            this.ClipGroup.AddItem(new DisplayNamePropertyEditorSlot());
            this.ClipGroup.AddItem(new ParameterDoublePropertyEditorSlot(VideoClip.OpacityParameter, typeof(VideoClip), "Opacity", DragStepProfile.UnitOne){ ValueFormatter = UnitToPercentFormatter.Standard });
            this.ClipGroup.AddItem(new VideoClipMediaFrameOffsetPropertyEditorSlot());

            {
                SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(VideoClip), GroupType.SecondaryExpander) { DisplayName = "Motion/Transformation" };
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClip.MediaPositionParameter, typeof(VideoClip), "Pos", DragStepProfile.InfPixelRange) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClip.MediaScaleParameter, typeof(VideoClip), "Scale", DragStepProfile.UnitOne));
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClip.MediaScaleOriginParameter, typeof(VideoClip), "Scale Origin", DragStepProfile.InfPixelRange) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                group.AddItem(new ParameterDoublePropertyEditorSlot(VideoClip.MediaRotationParameter, typeof(VideoClip), "Rotation", DragStepProfile.Rotation) { ValueFormatter = SuffixValueFormatter.StandardDegrees });
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClip.MediaRotationOriginParameter, typeof(VideoClip), "Rotation Origin", DragStepProfile.InfPixelRange) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                this.ClipGroup.AddItem(group);
            }

            {
                SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(VideoClipShape), GroupType.SecondaryExpander) { DisplayName = "Shape Info" };
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClipShape.SizeParameter, typeof(VideoClipShape), "Size", DragStepProfile.InfPixelRange) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                this.ClipGroup.AddItem(group);
            }
            
            {
                SimplePropertyEditorGroup textGroup = new SimplePropertyEditorGroup(typeof(TextVideoClip), GroupType.SecondaryExpander) { DisplayName = "Text" };
                textGroup.AddItem(new DataParameterStringPropertyEditorSlot(TextVideoClip.FontFamilyParameter, typeof(TextVideoClip), "Font Family"));
                textGroup.AddItem(new ParameterFloatPropertyEditorSlot(TextVideoClip.FontSizeParameter, typeof(TextVideoClip), "Font Size", DragStepProfile.FontSize) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                textGroup.AddItem(new ParameterFloatPropertyEditorSlot(TextVideoClip.BorderThicknessParameter, typeof(TextVideoClip), "Stroke Width", DragStepProfile.Pixels) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                textGroup.AddItem(new ParameterFloatPropertyEditorSlot(TextVideoClip.SkewXParameter, typeof(TextVideoClip), "Skew X", DragStepProfile.Pixels) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                textGroup.AddItem(new ParameterBoolPropertyEditorSlot(TextVideoClip.IsAntiAliasedParameter, typeof(TextVideoClip), "Anti Alias"));
                textGroup.AddItem(new ParameterFloatPropertyEditorSlot(TextVideoClip.LineSpacingParameter, typeof(TextVideoClip), "Line Spacing", DragStepProfile.Pixels) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                textGroup.AddItem(new DataParameterStringPropertyEditorSlot(TextVideoClip.TextParameter, typeof(TextVideoClip), "Text") { AnticipatedLineCount = 8 });
                this.ClipGroup.AddItem(textGroup);
            }

            {
                SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(TimecodeClip), GroupType.SecondaryExpander) { DisplayName = "Timecode Info" };
                group.AddItem(new TimecodeFontFamilyPropertyEditorSlot());
                group.AddItem(new ParameterDoublePropertyEditorSlot(TimecodeClip.FontSizeParameter, typeof(TimecodeClip), "Font Size", DragStepProfile.FontSize) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                group.AddItem(new DataParameterDoublePropertyEditorSlot(TimecodeClip.StartTimeParameter, TimecodeClip.UseClipStartTimeParameter, true, typeof(TimecodeClip), "Start secs", DragStepProfile.Percentage) { ValueFormatter = UnitToPercentFormatter.Standard });
                group.AddItem(new DataParameterDoublePropertyEditorSlot(TimecodeClip.EndTimeParameter, TimecodeClip.UseClipEndTimeParameter, true, typeof(TimecodeClip), "End secs", DragStepProfile.Percentage) { ValueFormatter = UnitToPercentFormatter.Standard });
                this.ClipGroup.AddItem(group);
            }

            this.ClipEffectListGroup = new EffectListPropertyEditorGroup();
            this.ClipGroup.AddItem(this.ClipEffectListGroup);
        }

        this.Root.AddItem(this.ClipGroup);

        {
            this.TrackGroup = new SimplePropertyEditorGroup(typeof(Track)) {
                DisplayName = "Track"
            };

            this.TrackGroup.AddItem(new DisplayNamePropertyEditorSlot());
            this.TrackGroup.AddItem(new ParameterDoublePropertyEditorSlot(VideoTrack.OpacityParameter, typeof(VideoTrack), "Opacity", DragStepProfile.UnitOne) { ValueFormatter = UnitToPercentFormatter.Standard });

            {
                SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(VideoTrack), GroupType.SecondaryExpander) { DisplayName = "Motion/Transformation" };
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoTrack.MediaPositionParameter, typeof(VideoTrack), "Pos", DragStepProfile.InfPixelRange) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoTrack.MediaScaleParameter, typeof(VideoTrack), "Scale", DragStepProfile.UnitOne));
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoTrack.MediaScaleOriginParameter, typeof(VideoTrack), "Scale Origin", DragStepProfile.InfPixelRange) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                group.AddItem(new ParameterDoublePropertyEditorSlot(VideoTrack.MediaRotationParameter, typeof(VideoTrack), "Rotation", DragStepProfile.Rotation) { ValueFormatter = SuffixValueFormatter.StandardDegrees });
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoTrack.MediaRotationOriginParameter, typeof(VideoTrack), "Rotation Origin", DragStepProfile.InfPixelRange) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                this.TrackGroup.AddItem(group);
            }

            this.TrackEffectListGroup = new EffectListPropertyEditorGroup();
            this.TrackGroup.AddItem(this.TrackEffectListGroup);
        }

        this.Root.AddItem(this.TrackGroup);
    }

    public void OnProjectChanged() {
        this.ClipEffectListGroup.ClearHierarchy();
        this.TrackEffectListGroup.ClearHierarchy();
        this.ClipGroup.ClearHierarchy();
        this.TrackGroup.ClearHierarchy();
    }
}