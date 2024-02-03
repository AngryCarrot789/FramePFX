using System;
using System.Windows;

namespace FramePFX.AdvancedContextService.WPF {
    /// <summary>
    /// An interface for an advanced context menu or advanced menu
    /// </summary>
    public interface IAdvancedMenu {
        bool PushCachedItem(Type entryType, FrameworkElement element);

        FrameworkElement PopCachedItem(Type entryType);

        FrameworkElement CreateChildItem(IContextEntry entry);
    }
}