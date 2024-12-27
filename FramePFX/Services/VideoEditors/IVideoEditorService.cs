// 
// Copyright (c) 2024-2024 REghZy
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

using FramePFX.Editing;
using FramePFX.Editing.UI;

namespace FramePFX.Services.VideoEditors;

public delegate void VideoEditorCreationEventHandler(IVideoEditorWindow window, bool isBeforeShow);

/// <summary>
/// A service that manages video editors and notifications of video editor creation
/// </summary>
public interface IVideoEditorService {
    /// <summary>
    /// Creates a new video editor window using the given video editor model
    /// </summary>
    /// <param name="editor"></param>
    /// <returns></returns>
    IVideoEditorWindow OpenVideoEditor(VideoEditor editor);
    
    /// <summary>
    /// An event fired when any video editor is created and also fired again when it is shown
    /// </summary>
    event VideoEditorCreationEventHandler? VideoEditorCreatedOrShown;
}