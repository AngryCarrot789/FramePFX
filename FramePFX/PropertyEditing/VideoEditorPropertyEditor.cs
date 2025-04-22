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
using FramePFX.PropertyEditing.Automation;
using PFXToolKitUI.Interactivity.Formatting;
using PFXToolKitUI.PropertyEditing;
using PFXToolKitUI.PropertyEditing.Core;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer.Automatic;

namespace FramePFX.PropertyEditing;

/// <summary>
/// A class which stores the video editor's general property editor information
/// </summary>
public class VideoEditorPropertyEditor : PropertyEditor {
    /// <summary>
    /// Gets the property editor group used to present selected clip(s) properties
    /// </summary>
    public SimplePropertyEditorGroup ClipGroup { get; }

    /// <summary>
    /// Gets the property editor group used to present selected track(s) properties
    /// </summary>
    public SimplePropertyEditorGroup TrackGroup { get; }

    /// <summary>
    /// Gets the special property editor group used to present the effects of the selected clip(s)
    /// </summary>
    public EffectListPropertyEditorGroup ClipEffectListGroup { get; }

    /// <summary>
    /// Gets the special property editor group used to present the effects of the selected track(s)
    /// </summary>
    public EffectListPropertyEditorGroup TrackEffectListGroup { get; }

    public VideoEditorPropertyEditor() {
        {
            this.ClipGroup = new SimplePropertyEditorGroup(typeof(Clip)) {
                DisplayName = "Clip", IsExpanded = true
            };

            this.ClipGroup.AddItem(new DisplayNamePropertyEditorSlot());
            this.ClipGroup.AddItem(new ParameterDoublePropertyEditorSlot(VideoClip.OpacityParameter, typeof(VideoClip), "Opacity", DragStepProfile.UnitOne) { ValueFormatter = UnitToPercentFormatter.Standard });
            this.ClipGroup.AddItem(new VideoClipMediaFrameOffsetPropertyEditorSlot());

            {
                SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(VideoClip), GroupType.SecondaryExpander) { DisplayName = "Motion/Transformation" };
                // Animatable parameters
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClip.MediaPositionParameter, typeof(VideoClip), "Position", DragStepProfile.InfPixelRange) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClip.MediaScaleParameter, typeof(VideoClip), "Scale", DragStepProfile.UnitOne));
                group.AddItem(new ParameterDoublePropertyEditorSlot(VideoClip.MediaRotationParameter, typeof(VideoClip), "Rotation", DragStepProfile.Rotation) { ValueFormatter = SuffixValueFormatter.StandardDegrees });
                
                // DataParameters with built in behaviour to manage an auto-generated value from another bool parameter
                group.AddItem(new AutomaticDataParameterVector2PropertyEditorSlot(VideoClip.MediaScaleOriginParameter, VideoClip.IsMediaScaleOriginAutomaticParameter, typeof(VideoClip), "Scale Origin") { ValueFormatter = SuffixValueFormatter.StandardPixels, StepProfile = DragStepProfile.InfPixelRange });
                group.AddItem(new AutomaticDataParameterVector2PropertyEditorSlot(VideoClip.MediaRotationOriginParameter, VideoClip.IsMediaRotationOriginAutomaticParameter, typeof(VideoClip), "Rotation Origin") { ValueFormatter = SuffixValueFormatter.StandardPixels, StepProfile = DragStepProfile.InfPixelRange });
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
                textGroup.AddItem(new ParameterFloatPropertyEditorSlot(TextVideoClip.LineSpacingParameter, typeof(TextVideoClip), "Line Spacing", DragStepProfile.Pixels) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                textGroup.AddItem(new DataParameterStringPropertyEditorSlot(TextVideoClip.TextParameter, typeof(TextVideoClip), "Text") { AnticipatedLineCount = 8 });
                textGroup.AddItem(new DataParameterColourPropertyEditorSlot(TextVideoClip.ForegroundParameter, typeof(TextVideoClip), "Foreground"));
                this.ClipGroup.AddItem(textGroup);
            }

            {
                SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(TimecodeClip), GroupType.SecondaryExpander) { DisplayName = "Timecode Info" };
                group.AddItem(new TimecodeFontFamilyPropertyEditorSlot());
                group.AddItem(new ParameterDoublePropertyEditorSlot(TimecodeClip.FontSizeParameter, typeof(TimecodeClip), "Font Size", DragStepProfile.FontSize) { ValueFormatter = SuffixValueFormatter.StandardPixels });
                group.AddItem(new DataParameterNumberPropertyEditorSlot<double>(TimecodeClip.StartTimeParameter, TimecodeClip.UseClipStartTimeParameter, typeof(TimecodeClip), "Start secs", DragStepProfile.SecondsRealtime) { ValueFormatter = SuffixValueFormatter.StandardSeconds });
                group.AddItem(new DataParameterNumberPropertyEditorSlot<double>(TimecodeClip.EndTimeParameter, TimecodeClip.UseClipEndTimeParameter, typeof(TimecodeClip), "End secs", DragStepProfile.SecondsRealtime) { ValueFormatter = SuffixValueFormatter.StandardSeconds });
                group.AddItem(new DataParameterColourPropertyEditorSlot(TimecodeClip.ForegroundParameter, typeof(TimecodeClip), "Foreground"));
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
                group.AddItem(new ParameterDoublePropertyEditorSlot(VideoTrack.MediaRotationParameter, typeof(VideoTrack), "Rotation", DragStepProfile.Rotation) { ValueFormatter = SuffixValueFormatter.StandardDegrees });
                group.AddItem(new AutomaticDataParameterVector2PropertyEditorSlot(VideoTrack.MediaScaleOriginParameter, VideoTrack.IsMediaScaleOriginAutomaticParameter, typeof(VideoTrack), "Scale Origin") { ValueFormatter = SuffixValueFormatter.StandardPixels, StepProfile = DragStepProfile.InfPixelRange });
                group.AddItem(new AutomaticDataParameterVector2PropertyEditorSlot(VideoTrack.MediaRotationOriginParameter, VideoTrack.IsMediaRotationOriginAutomaticParameter, typeof(VideoTrack), "Rotation Origin") { ValueFormatter = SuffixValueFormatter.StandardPixels, StepProfile = DragStepProfile.InfPixelRange });
                this.TrackGroup.AddItem(group);
            }

            this.TrackEffectListGroup = new EffectListPropertyEditorGroup();
            this.TrackGroup.AddItem(this.TrackEffectListGroup);
        }

        this.Root.AddItem(this.TrackGroup);
    }
}