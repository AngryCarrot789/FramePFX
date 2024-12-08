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

namespace FramePFX.Editing.Timelines.Clips.Video;

/// <summary>
/// The base class for a video source. This is responsible for creating a video source context for a
/// specific video clip which may or may not assist with actually providing video data.
/// </summary>
public abstract class VideoSource
{
    /// <summary>
    /// The main method for creating a video source context using a given clip. The given clip's video source context is not affected by this method
    /// </summary>
    /// <param name="clip"></param>
    /// <returns></returns>
    public abstract VideoSourceContext CreateContext(VideoClip clip);
}