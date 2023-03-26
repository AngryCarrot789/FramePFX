using System.Windows;

namespace FramePFX.Utils {
    public class ThreadSafeDP {
        private volatile object value;
        private DependencyProperty Property { get; }

        public object Value => this.value;

        public ThreadSafeDP(DependencyProperty property) {
            // this.Property = DependencyProperty.Register()
        }
    }
}