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

using System.Diagnostics.CodeAnalysis;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Automation.Params;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Effects;
using FramePFX.Editing.Timelines.Tracks;

namespace FramePFX.Editing.Automation;

public static class AutomationUtils
{
    public static void GetDefaultKeyFrameOrAddNew(AutomationSequence sequence, out KeyFrame keyFrame, bool enableOverrideIfOutOfRange = true)
    {
        if (sequence.IsEmpty || sequence.IsOverrideEnabled)
        {
            keyFrame = sequence.DefaultKeyFrame;
        }
        else if (sequence.AutomationData.Owner.GetRelativePlayHead(out long playHead))
        {
            // Either get the last key frame at the playhead or create a new one at that location
            keyFrame = sequence.GetOrCreateKeyFrameAtFrame(playHead, out _);
        }
        else
        {
            // when the object is has a strict frame range, e.g. clip, effect, and it is not in range,
            // enable override and set the default key frame
            keyFrame = sequence.DefaultKeyFrame;
            if (enableOverrideIfOutOfRange)
                sequence.IsOverrideEnabled = true;
        }
    }

    public static void SetDefaultKeyFrameOrAddNew(IAutomatable automatable, Parameter parameter, object value, bool createFirstIfEmpty = false) => SetDefaultKeyFrameOrAddNew(automatable, parameter, value, (k, d, o) => k.SetValueFromObject(o), createFirstIfEmpty);

    public static void SetDefaultKeyFrameOrAddNew<T>(IAutomatable automatable, Parameter parameter, T value, Action<KeyFrame, ParameterDescriptor, T> setter, bool createFirstIfEmpty = false)
    {
        AutomationSequence sequence = automatable.AutomationData[parameter];
        if ((!createFirstIfEmpty && sequence.IsEmpty) || sequence.IsOverrideEnabled)
        {
            setter(sequence.DefaultKeyFrame, parameter.Descriptor, value);
        }
        else if (sequence.AutomationData.Owner.GetRelativePlayHead(out long playHead))
        {
            // Either get the last key frame at the playhead or create a new one at that location
            setter(sequence.GetOrCreateKeyFrameAtFrame(playHead, out _), parameter.Descriptor, value);
        }
        else
        {
            // when the object is has a strict frame range, e.g. clip, effect, and it is not in range,
            // enable override and set the default key frame
            setter(sequence.DefaultKeyFrame, parameter.Descriptor, value);
            sequence.IsOverrideEnabled = true;
        }
    }

    public static bool TryAddKeyFrameAtLocation(AutomationSequence sequence, out KeyFrame keyFrame)
    {
        if (sequence.AutomationData.Owner.GetRelativePlayHead(out long playHead))
        {
            keyFrame = sequence.GetOrCreateKeyFrameAtFrame(playHead, out _, true);
            return true;
        }
        else
        {
            keyFrame = null;
            return false;
        }
    }

    public static bool GetClipForAutomatable(IAutomatable? automatable, [NotNullWhen(true)] out Clip? clip)
    {
        if ((clip = automatable as Clip) != null)
        {
            return true;
        }
        else if (automatable is BaseEffect effect)
        {
            return (clip = effect.OwnerClip) != null;
        }
        else
        {
            clip = null;
            return false;
        }
    }
    
    public static bool GetTrackForAutomatable(IAutomatable? automatable, [NotNullWhen(true)] out Track? track)
    {
        if ((track = automatable as Track) != null)
        {
            return true;
        }
        else if (automatable is BaseEffect effect)
        {
            return (track = effect.OwnerTrack) != null;
        }
        else
        {
            track = null;
            return false;
        }
    }
}