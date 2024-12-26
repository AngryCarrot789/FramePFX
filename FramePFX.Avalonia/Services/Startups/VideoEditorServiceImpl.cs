using FramePFX.Editing;
using FramePFX.Editing.UI;
using FramePFX.Services.VideoEditors;

namespace FramePFX.Avalonia.Services.Startups;

public class VideoEditorServiceImpl : IVideoEditorService {
    public IVideoEditorWindow OpenVideoEditor(VideoEditor editor) {
        EditorWindow window = new EditorWindow();
        window.Show();
        
        window.VideoEditor = editor;
        Application.Instance.Dispatcher.InvokeAsync(() => {
            window.PART_ViewPort!.PART_FreeMoveViewPort!.FitContentToCenter();
            if (editor.Project != null) {
                editor.Project.ActiveTimeline.InvalidateRender();
            }
            
        }, DispatchPriority.Background);
        
        return window;
    }
}