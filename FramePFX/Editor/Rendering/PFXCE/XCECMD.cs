using System.Runtime.InteropServices;
using SkiaSharp;

namespace FramePFX.Editor.Rendering.PFXCE {
    public enum XCECMD {
        PushClipRect,
        CmdDrawRect
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct XCECMD_CLIP_RECT {
        [FieldOffset(0)] public float clip_left;
        [FieldOffset(4)] public float clip_top;
        [FieldOffset(8)] public float clip_right;
        [FieldOffset(12)] public float clip_bottom;
        [FieldOffset(16)] public bool op_Itersect;
        [FieldOffset(17)] public bool antiAlias;

        public XCECMD_CLIP_RECT(SKRect rect, SKClipOperation op, bool isAntiAlised) {
            this.clip_left = rect.Left;
            this.clip_top = rect.Top;
            this.clip_right = rect.Right;
            this.clip_bottom = rect.Bottom;
            this.op_Itersect = op == SKClipOperation.Intersect;
            this.antiAlias = isAntiAlised;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct XCECMD_DRAW_RECT {
        [FieldOffset(0)] public float x;
        [FieldOffset(4)] public float y;
        [FieldOffset(8)] public float w;
        [FieldOffset(12)] public float h;
        [FieldOffset(16)] public uint colour;
        [FieldOffset(20)] public bool antiAlias;

        public XCECMD_DRAW_RECT(float x, float y, float w, float h, uint colour, bool antiAlias) {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.colour = colour;
            this.antiAlias = antiAlias;
        }
    }
}