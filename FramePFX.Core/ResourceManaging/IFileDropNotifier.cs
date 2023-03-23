namespace FramePFX.Core.ResourceManaging {
    public interface IFileDropNotifier {
        void OnFilesDropped(string[] files);
    }
}