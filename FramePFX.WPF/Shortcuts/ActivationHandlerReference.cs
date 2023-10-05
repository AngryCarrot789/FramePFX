using System;

namespace FramePFX.WPF.Shortcuts {
    public class ActivationHandlerReference {
        private readonly WeakReference<ShortcutActivateHandler> weakReference;
        private readonly ShortcutActivateHandler strongReference;

        public ShortcutActivateHandler Value {
            get {
                if (this.weakReference != null) {
                    return this.weakReference.TryGetTarget(out ShortcutActivateHandler target) ? target : null;
                }
                else {
                    return this.strongReference;
                }
            }
        }

        public bool IsWeak => this.weakReference != null;

        public bool IsStrong => this.weakReference == null;

        public ActivationHandlerReference(ShortcutActivateHandler handler, bool weak) {
            if (weak) {
                this.weakReference = new WeakReference<ShortcutActivateHandler>(handler);
            }
            else {
                this.strongReference = handler;
            }
        }
    }
}