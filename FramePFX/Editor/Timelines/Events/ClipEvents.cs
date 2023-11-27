using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.Events {
    public delegate void ClipRenderInvalidatedEventHandler(Clip clip);
    public delegate void ClipSpanChangedEventHandler(Clip clip, FrameSpan oldSpan, FrameSpan newSpan);
}