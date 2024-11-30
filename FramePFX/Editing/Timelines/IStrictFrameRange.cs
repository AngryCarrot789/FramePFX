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

namespace FramePFX.Editing.Timelines;

/// <summary>
/// An interface for objects (typically clips and effects) that have a strict frame span
/// range which requires translating timeline play head frames into relative frames
/// </summary>
public interface IStrictFrameRange {
    /// <summary>
    /// Converts a relative frame to an absolute frame, relative to the timeline
    /// </summary>
    /// <param name="relative">Input relative frame</param>
    /// <returns>Output absolute frame</returns>
    long ConvertRelativeToTimelineFrame(long relative);

    /// <summary>
    /// Converts an absolute timeline frame into a relative frame
    /// </summary>
    /// <param name="timeline">Input timeline frame</param>
    /// <param name="inRange">
    /// True if the timeline frame is within our strict frame range,
    /// otherwise false, meaning it is out of range (and technically invalid)
    /// </param>
    /// <returns>Output relative frame</returns>
    long ConvertTimelineToRelativeFrame(long timeline, out bool inRange);

    /// <summary>
    /// Returns true if the timeline frame is within our strict frame range,
    /// otherwise false, meaning it is out of range (and technically invalid)
    /// </summary>
    /// <param name="timeline">Input timeline frame</param>
    /// <returns>See above</returns>
    bool IsTimelineFrameInRange(long timeline);

    bool IsRelativeFrameInRange(long relative);
}