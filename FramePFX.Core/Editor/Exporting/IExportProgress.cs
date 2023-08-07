namespace FramePFX.Core.Editor.Exporting
{
    public interface IExportProgress
    {
        void OnFrameRendered(long frame);

        void OnFrameEncoded(long frame);
    }
}