using System.Windows;
using System.Windows.Controls;

namespace Dragablz {
    /// <summary>
    /// Selects style to apply to a <see cref="DragablzItem"/> according to the tab item content itself.
    /// </summary>
    public class TabablzItemStyleSelector : StyleSelector {
        private readonly Style _defaultHeaderItemStyle;
        private readonly Style _customHeaderItemStyle;

        public TabablzItemStyleSelector(Style defaultHeaderItemStyle, Style customHeaderItemStyle) {
            this._defaultHeaderItemStyle = defaultHeaderItemStyle;
            this._customHeaderItemStyle = customHeaderItemStyle;
        }

        public override Style SelectStyle(object item, DependencyObject container) {
            if (item is TabItem)
                return this._defaultHeaderItemStyle;

            return this._customHeaderItemStyle;
        }
    }
}