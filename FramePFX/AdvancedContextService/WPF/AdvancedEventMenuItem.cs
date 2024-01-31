using System;
using System.Windows.Threading;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.AdvancedContextService.WPF {
    public class AdvancedEventMenuItem : AdvancedMenuItem {
        public new EventContextEntry Entry => (EventContextEntry) base.Entry;

        public AdvancedEventMenuItem() {

        }

        protected override void OnClick() {
            EventContextEntry entry = this.Entry;
            DataContext context = this.Menu.ContextOnMenuOpen;
            if (entry != null && context != null) {
                this.Dispatcher.BeginInvoke((Action) (() => entry.Action?.Invoke(context)), DispatcherPriority.Render);
            }

            base.OnClick();
        }
    }
}