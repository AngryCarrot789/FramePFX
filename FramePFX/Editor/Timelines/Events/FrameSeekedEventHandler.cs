using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Timelines.Events {
    public delegate void FrameSeekedEventHandler(ClipViewModel sender, long oldFrame, long newFrame);
}