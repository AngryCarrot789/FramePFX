using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.AdvancedContextService;

namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// A selector for selecting styles based on <see cref="IContextEntry"/> instances, or just defaulting to the standard <see cref="AdvancedMenuItem"/> style
    /// </summary>
    public class AdvancedMenuItemStyleSelector : StyleSelector {
        public Style CheckableActionMenuItemStyle { get; set; }
        public Style NonCheckableActionMenuItemStyle { get; set; }
        public Style CheckableCommandMenuItemStyle { get; set; }
        public Style NonCheckableCommandMenuItemStyle { get; set; }
        public Style ShortcutCommandMenuItemStyle { get; set; }
        public Style GroupingMenuItemStyle { get; set; }
        public Style DefaultAdvancedMenuItemStyle { get; set; }

        public Style SeparatorStyle { get; set; }

        public AdvancedMenuItemStyleSelector() {
        }

        public override Style SelectStyle(object item, DependencyObject container) {
            if (container is AdvancedMenuItem) {
                switch (item) {
                    case ActionCheckableContextEntry _: return this.CheckableActionMenuItemStyle ?? this.NonCheckableActionMenuItemStyle;
                    case ActionContextEntry _: return this.NonCheckableActionMenuItemStyle;
                    case CommandCheckableContextEntry _: return this.CheckableCommandMenuItemStyle ?? this.NonCheckableCommandMenuItemStyle;
                    case CommandContextEntry _: return this.NonCheckableCommandMenuItemStyle;
                    case ShortcutCommandContextEntry _: return this.ShortcutCommandMenuItemStyle;
                    case GroupContextEntry _: return this.GroupingMenuItemStyle;
                    default: return this.DefaultAdvancedMenuItemStyle;
                }
            }
            else if (container is Separator) {
                return this.SeparatorStyle;
            }
            else {
                return base.SelectStyle(item, container);
            }
        }
    }
}