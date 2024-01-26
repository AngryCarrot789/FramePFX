using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FramePFX.Utils {
    public static class CursorUtils {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int x;
            public int y;
            public int w;
            public int h;

            public RECT(int x, int y, int w, int h) {
                this.x = x;
                this.y = y;
                this.w = w;
                this.h = h;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int x;
            public int y;

            public POINT(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        [DllImport("user32.dll")]
        private static extern unsafe bool ClipCursor([In] RECT* rect);

        // rect will contain either the current clip or the screen clip
        [DllImport("user32.dll")]
        private static extern unsafe bool GetClipCursor([Out] RECT* rect);

        [DllImport("user32.dll")]
        private static extern unsafe bool SetCursorPos([In] POINT* p);

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        private static extern bool e_SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern unsafe bool GetCursorPos([Out] POINT* p);

        /// <summary>
        /// Whether or not this application has a logical clip on the cursor (set to true
        /// when <see cref="SetClip"/> is invoked, and set to false when <see cref="ClearClip"/> is invoked)
        /// </summary>
        public static bool IsCursorClipped { get; private set; }

        public static unsafe void SetClip(int x, int y, int width, int height) {
            RECT rect = new RECT(x, y, width, height);
            if (!ClipCursor(&rect))
                throw new Win32Exception();
            IsCursorClipped = true;
        }

        public static unsafe void ClearClip() {
            if (!ClipCursor(null))
                throw new Win32Exception();
            IsCursorClipped = false;
        }

        public static unsafe RECT GetClip() {
            RECT r;
            if (!GetClipCursor(&r))
                throw new Win32Exception();
            return r;
        }

        public static unsafe POINT GetCursorPos() {
            POINT p;
            if (!GetCursorPos(&p))
                throw new Win32Exception();
            return p;
        }

        public static void SetCursorPos(int x, int y) {
            if (!e_SetCursorPos(x, y))
                throw new Win32Exception();
        }
    }
}