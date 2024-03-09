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
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Automation
{
    public static class AutomationEngine
    {
        public static void UpdateValues(Timeline timeline) => UpdateValues(timeline, timeline.PlayHeadPosition);

        public static void UpdateValues(Timeline timeline, long playHead)
        {
            foreach (Track track in timeline.Tracks)
            {
                track.AutomationData.UpdateAllAutomated(playHead);
                foreach (Clip clip in track.GetClipsAtFrame(playHead))
                {
                    UpdateValues(clip, clip.ConvertTimelineToRelativeFrame(playHead, out _));
                }
            }
        }

        public static void UpdateValues(Clip clip, long relativePlayHead)
        {
            clip.AutomationData.UpdateAllAutomated(relativePlayHead);
            foreach (BaseEffect effect in clip.Effects)
            {
                effect.AutomationData.UpdateAllAutomated(relativePlayHead);
            }

            if (clip is ICompositionClip compClip && compClip.ResourceCompositionKey.TryGetResource(out ResourceComposition compResource))
            {
                UpdateValues(compResource.Timeline, relativePlayHead);
            }
        }

        public static void ConvertProjectFrameRate(Project project, double ratio)
        {
            ConvertResourceManagerFrameRateRecursive(project.ResourceManager.RootContainer, ratio);
            ConvertTimelineFrameRate(project.MainTimeline, ratio);
        }

        public static void ConvertResourceManagerFrameRateRecursive(BaseResource resource, double ratio)
        {
            if (resource is ResourceFolder)
            {
                foreach (BaseResource item in ((ResourceFolder) resource).Items)
                {
                    ConvertResourceManagerFrameRateRecursive(item, ratio);
                }
            }
            else if (resource is ResourceComposition)
            {
                ConvertTimelineFrameRate(((ResourceComposition) resource).Timeline, ratio);
            }
        }

        public static void ConvertTimelineFrameRate(Timeline timeline, double ratio)
        {
            // ConvertTimeRatios(timeline.AutomationData, ratio);
            foreach (Track track in timeline.Tracks)
            {
                ConvertTimeRatios(track.AutomationData, ratio);
                foreach (Clip clip in track.Clips)
                {
                    ConvertTimeRatios(clip.AutomationData, ratio);
                    FrameSpan span = clip.FrameSpan;
                    clip.FrameSpan = new FrameSpan((long) Math.Round(ratio * span.Begin), (long) Math.Round(ratio * span.Duration));
                    foreach (BaseEffect effect in clip.Effects)
                    {
                        ConvertTimeRatios(effect.AutomationData, ratio);
                    }
                }
            }
        }

        public static void ConvertTimeRatios(AutomationData data, double ratio)
        {
            foreach (AutomationSequence sequence in data.Sequences)
            {
                for (int i = sequence.KeyFrames.Count - 1; i >= 0; i--)
                {
                    KeyFrame keyFrame = sequence.KeyFrames[i];
                    keyFrame.Frame = (long) Math.Round(ratio * keyFrame.Frame);
                }
            }
        }
    }
}