using System.Windows;
using System.Windows.Controls;
using FramePFX.AdvancedContextService;
using FramePFX.PropertyEditing;
using FramePFX.WPF.AdvancedContextService;

namespace FramePFX.WPF.PropertyEditing {
    /// <summary>
    /// A selector for selecting styles based on <see cref="IContextEntry"/> instances, or just defaulting to the standard <see cref="AdvancedMenuItem"/> style
    /// </summary>
    public class PropertyEditorItemStyleSelector : StyleSelector {
        public Style PropertyItemsControlStyle { get; set; }
        public Style PropertyItemStyle { get; set; }
        public Style GroupSeparatorStyle { get; set; }
        public Style EditorSeparatorStyle { get; set; }

        public PropertyEditorItemStyleSelector() {
        }

        public override Style SelectStyle(object item, DependencyObject container) {
            switch (container) {
                case PropertyEditorItemsControl _: return this.PropertyItemsControlStyle;
                case PropertyEditorItem _: return this.PropertyItemStyle;
                case Separator _: return item is PropertyObjectSeparator separator ? separator.IsEditorSeparator ? this.EditorSeparatorStyle : this.GroupSeparatorStyle : null;
                default: return base.SelectStyle(item, container);
            }
        }
    }
}