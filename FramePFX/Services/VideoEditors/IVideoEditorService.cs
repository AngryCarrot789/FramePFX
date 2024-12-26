using FramePFX.Editing;
using FramePFX.Editing.UI;

namespace FramePFX.Services.VideoEditors;

public interface IVideoEditorService {
    /// <summary>
    /// Creates a new video editor window using the given video editor model
    /// </summary>
    /// <param name="editor"></param>
    /// <returns></returns>
    IVideoEditorWindow OpenVideoEditor(VideoEditor editor);
}