using SkiaSharp;

namespace FramePFX.Editor.Rendering.PFXCE {
    public class DeferredRenderContext {
        private readonly RenderCommandList list;

        public DeferredRenderContext() {
            this.list = new RenderCommandList();
        }

        public unsafe void PushClip(SKRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antiAliased = false) {
            XCECMD_CLIP_RECT record = new XCECMD_CLIP_RECT(rect, operation, antiAliased);
            this.list.WriteCommandRecord(XCECMD.PushClipRect, (byte*) &record, sizeof(XCECMD_CLIP_RECT));
        }

        public unsafe void DrawRect(float x, float y, float width, float height, SKColor colour, bool antiAliased = true) {
            XCECMD_DRAW_RECT record = new XCECMD_DRAW_RECT(x, y, width, height, (uint) colour, antiAliased);
            this.list.WriteCommandRecord(XCECMD.CmdDrawRect, (byte*) &record, sizeof(XCECMD_DRAW_RECT));
        }
    }
}