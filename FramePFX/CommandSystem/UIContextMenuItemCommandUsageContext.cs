using System;
using System.Windows;
using FramePFX.AdvancedContextService.WPF;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.CommandSystem {
    public class UIContextMenuItemCommandUsageContext : CommandUsageContext {
        public AdvancedContextMenuItem MenuItem { get; }

        public bool CanExecute { get; private set; }

        public UIContextMenuItemCommandUsageContext(AdvancedContextMenuItem menuItem) {
            this.MenuItem = menuItem ?? throw new ArgumentNullException(nameof(menuItem));
            this.CanExecute = true;
        }

        public override void OnCanExecuteInvalidated(IDataContext context) {
            this.CanExecute = this.CommandId == null || CommandManager.Instance.CanExecute(this.CommandId, context, true);
            this.MenuItem.CoerceValue(UIElement.IsEnabledProperty);
        }
    }
}