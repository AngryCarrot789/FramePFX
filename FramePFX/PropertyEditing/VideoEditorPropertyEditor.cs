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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.Editors.PropertyEditors.Effects;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing
{
    /// <summary>
    /// A class which stores the video editor's general property editor information
    /// </summary>
    public class VideoEditorPropertyEditor : BasePropertyEditor
    {
        public static VideoEditorPropertyEditor Instance { get; } = new VideoEditorPropertyEditor();

        private volatile int isUpdateClipSelectionScheduled;
        private volatile int isUpdateTrackSelectionScheduled;

        public SimplePropertyEditorGroup ClipGroup { get; }

        public SimplePropertyEditorGroup TrackGroup { get; }

        public EffectListPropertyEditorGroup ClipEffectListGroup { get; }

        public EffectListPropertyEditorGroup TrackEffectListGroup { get; }

        private VideoEditorPropertyEditor()
        {
            {
                this.ClipGroup = new SimplePropertyEditorGroup(typeof(Clip))
                {
                    DisplayName = "Clip", IsExpanded = true
                };

                this.ClipGroup.AddItem(new DisplayNamePropertyEditorSlot());
                this.ClipGroup.AddItem(new ParameterDoublePropertyEditorSlot(VideoClip.OpacityParameter, typeof(VideoClip), "Opacity", DragStepProfile.UnitOne));
                this.ClipGroup.AddItem(new VideoClipMediaFrameOffsetPropertyEditorSlot());

                {
                    SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(VideoClip), GroupType.SecondaryExpander) {DisplayName = "Motion/Transformation"};
                    group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClip.MediaPositionParameter, typeof(VideoClip), "Pos", DragStepProfile.InfPixelRange));
                    group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClip.MediaScaleParameter, typeof(VideoClip), "Scale", DragStepProfile.UnitOne));
                    group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClip.MediaScaleOriginParameter, typeof(VideoClip), "Scale Origin", DragStepProfile.InfPixelRange));
                    group.AddItem(new ParameterDoublePropertyEditorSlot(VideoClip.MediaRotationParameter, typeof(VideoClip), "Rotation", DragStepProfile.Rotation));
                    group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClip.MediaRotationOriginParameter, typeof(VideoClip), "Rotation Origin", DragStepProfile.InfPixelRange));
                    this.ClipGroup.AddItem(group);
                }

                {
                    SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(VideoClipShape), GroupType.SecondaryExpander) {DisplayName = "Shape Info"};
                    group.AddItem(new ParameterVector2PropertyEditorSlot(VideoClipShape.SizeParameter, typeof(VideoClipShape), "Size", DragStepProfile.InfPixelRange));
                    this.ClipGroup.AddItem(group);
                }

                {
                    SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(TimecodeClip), GroupType.SecondaryExpander) {DisplayName = "Timecode Info"};
                    group.AddItem(new TimecodeFontFamilyPropertyEditorSlot());
                    group.AddItem(new ParameterDoublePropertyEditorSlot(TimecodeClip.FontSizeParameter, typeof(TimecodeClip), "Font Size", DragStepProfile.Percentage));
                    group.AddItem(new DataParameterDoublePropertyEditorSlot(TimecodeClip.StartTimeParameter, TimecodeClip.UseClipStartTimeParameter, true, typeof(TimecodeClip), "Start secs", DragStepProfile.Percentage));
                    group.AddItem(new DataParameterDoublePropertyEditorSlot(TimecodeClip.EndTimeParameter, TimecodeClip.UseClipEndTimeParameter, true, typeof(TimecodeClip), "End secs", DragStepProfile.Percentage));
                    this.ClipGroup.AddItem(group);
                }

                this.ClipEffectListGroup = new EffectListPropertyEditorGroup();
                this.ClipGroup.AddItem(this.ClipEffectListGroup);
            }

            this.Root.AddItem(this.ClipGroup);

            {
                this.TrackGroup = new SimplePropertyEditorGroup(typeof(Track))
                {
                    DisplayName = "Track", IsExpanded = false
                };

                this.TrackGroup.AddItem(new DisplayNamePropertyEditorSlot());
                this.TrackGroup.AddItem(new ParameterDoublePropertyEditorSlot(VideoTrack.OpacityParameter, typeof(VideoTrack), "Opacity", DragStepProfile.UnitOne));

                {
                    SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(VideoTrack), GroupType.SecondaryExpander) {DisplayName = "Motion/Transformation"};
                    group.AddItem(new ParameterVector2PropertyEditorSlot(VideoTrack.MediaPositionParameter, typeof(VideoTrack), "Pos", DragStepProfile.InfPixelRange));
                    group.AddItem(new ParameterVector2PropertyEditorSlot(VideoTrack.MediaScaleParameter, typeof(VideoTrack), "Scale", DragStepProfile.UnitOne));
                    group.AddItem(new ParameterVector2PropertyEditorSlot(VideoTrack.MediaScaleOriginParameter, typeof(VideoTrack), "Scale Origin", DragStepProfile.InfPixelRange));
                    group.AddItem(new ParameterDoublePropertyEditorSlot(VideoTrack.MediaRotationParameter, typeof(VideoTrack), "Rotation", DragStepProfile.Rotation));
                    group.AddItem(new ParameterVector2PropertyEditorSlot(VideoTrack.MediaRotationOriginParameter, typeof(VideoTrack), "Rotation Origin", DragStepProfile.InfPixelRange));
                    this.TrackGroup.AddItem(group);
                }

                this.TrackEffectListGroup = new EffectListPropertyEditorGroup();
                this.TrackGroup.AddItem(this.TrackEffectListGroup);
            }

            this.Root.AddItem(this.TrackGroup);
        }

        public void UpdateClipSelectionAsync(Timeline timeline)
        {
            if (timeline == null || Interlocked.CompareExchange(ref this.isUpdateClipSelectionScheduled, 1, 0) != 0)
            {
                return;
            }

            Application.Current?.Dispatcher?.InvokeAsync(() =>
            {
                try
                {
                    this.UpdateClipSelection(timeline);
                }
                finally
                {
                    this.isUpdateClipSelectionScheduled = 0;
                }
            }, DispatcherPriority.Background);
        }

        public void UpdateTrackSelectionAsync(Timeline timeline)
        {
            if (timeline == null || Interlocked.CompareExchange(ref this.isUpdateTrackSelectionScheduled, 1, 0) != 0)
            {
                return;
            }

            Application.Current?.Dispatcher?.InvokeAsync(() =>
            {
                try
                {
                    this.UpdateTrackSelection(timeline);
                }
                finally
                {
                    this.isUpdateTrackSelectionScheduled = 0;
                }
            }, DispatcherPriority.Background);
        }

        private void UpdateClipSelection(Timeline timeline)
        {
            List<Clip> selection = timeline.SelectedClips.ToList();
            if (selection.CollectionEquals(this.ClipGroup.Handlers))
            {
                return;
            }

            this.ClipGroup.SetupHierarchyState(selection);
            if (selection.Count == 1)
            {
                this.ClipEffectListGroup.SetupHierarchyState(selection[0]);
            }
            else
            {
                this.ClipEffectListGroup.ClearHierarchy();
            }
        }

        private void UpdateTrackSelection(Timeline timeline)
        {
            List<Track> selection = timeline.SelectedTracks.ToList();
            if (selection.CollectionEquals(this.TrackGroup.Handlers))
            {
                return;
            }

            this.TrackGroup.SetupHierarchyState(selection);
            if (selection.Count == 1)
            {
                this.TrackEffectListGroup.SetupHierarchyState(selection[0]);
            }
            else
            {
                this.TrackEffectListGroup.ClearHierarchy();
            }
        }

        public void OnProjectChanged()
        {
            this.ClipEffectListGroup.ClearHierarchy();
            this.TrackEffectListGroup.ClearHierarchy();
            this.ClipGroup.ClearHierarchy();
            this.TrackGroup.ClearHierarchy();
        }
    }
}