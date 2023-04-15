using System;

namespace FramePFX.Core.Utils {
    public static class ExceptionUtils {
        public static void AddSuppressed(this Exception @this, Exception suppressed) {
            @this.Data[$"Suppressed {@this.Data.Count}"] = suppressed;
        }
    }
}