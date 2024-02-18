using System;
using System.Windows;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.CommandSystem {
    public class UICommandUsageContext : CommandUsageContext {
        public UIElement Element { get; }

        public UICommandUsageContext(UIElement element) {
            this.Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public override void OnCanExecuteInvalidated(IDataContext context) {
            this.Element.IsEnabled = this.CommandId == null || CommandManager.Instance.CanExecute(this.CommandId, context, true);
        }
    }
}