namespace FramePFX.Core.Editor.Exporting {
    public interface IExportProgress {
        void OnFrameCompleted(long frame);
    }
}