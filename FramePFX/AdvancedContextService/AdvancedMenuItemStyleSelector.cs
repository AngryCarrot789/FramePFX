using System.Windows;
using System.Windows.Controls;
using MCNBTViewer.Core.AdvancedContextService;

namespace MCNBTViewer.AdvancedContextService {
    public class AdvancedMenuItemStyleSelector : StyleSelector {
        public Style NonCheckableCommandMenuItemStyle { get; set; }

        public Style CheckableCommandMenuItemStyle { get; set; }

        public Style NonCheckableActionMenuItemStyle { get; set; }
        
        public Style CheckableActionMenuItemStyle { get; set; }

        public Style SeparatorStyle { get; set; }

        public AdvancedMenuItemStyleSelector() {

        }

        public override Style SelectStyle(object item, DependencyObject container) {
            if (container is MenuItem) {
                switch (item) {
                    case CheckableActionContextEntry _:  return this.CheckableActionMenuItemStyle ?? this.NonCheckableActionMenuItemStyle;
                    case CheckableCommandContextEntry _: return this.CheckableCommandMenuItemStyle ?? this.NonCheckableCommandMenuItemStyle;
                    case CommandContextEntry _:          return this.NonCheckableCommandMenuItemStyle;
                    case ActionContextEntry _:           return this.NonCheckableActionMenuItemStyle;
                    default:                             return this.NonCheckableCommandMenuItemStyle;
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