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
using FramePFX.Editing.Toolbars;
using FramePFX.Editing.UI;
using FramePFX.Services.VideoEditors;
using PFXToolKitUI;

namespace FramePFX.Avalonia.Services.Startups;

public class VideoEditorServiceImpl : IVideoEditorService {
    public event VideoEditorCreationEventHandler? VideoEditorCreatedOrShown;

    public VideoEditorServiceImpl() {
    }

    public IVideoEditorWindow OpenVideoEditor(VideoEditor editor) {
        editor.ServiceManager.RegisterConstant(new TimelineToolBarManager());
        editor.ServiceManager.RegisterConstant(new ControlSurfaceListToolBarManager());
        editor.ServiceManager.RegisterConstant(new ViewPortToolBarManager());

        EditorWindow window = new EditorWindow(editor);

        // Something might want to actually do stuff before the visual tree is fully loaded
        this.VideoEditorCreatedOrShown?.Invoke(window, false);

        window.Show();
        Application.Instance.Dispatcher.InvokeAsync(() => {
            window.PART_ViewPort!.PART_FreeMoveViewPort!.FitContentToCenter();
            if (editor.Project != null) {
                editor.Project.ActiveTimeline.InvalidateRender();
            }
        }, DispatchPriority.Background);

        this.VideoEditorCreatedOrShown?.Invoke(window, true);
        return window;
    }
}