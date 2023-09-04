using System.Windows;
using System.Windows.Controls;

namespace FramePFX.WPF.PropertyEditing {
    public class PropertyEditorItem : ContentPresenter {
        // The commented out code was going to be used to support a hard-coded left indentation for deeper items...
        // but due to how the visual tree is constructed, it's hard to only indent the editor content but not the group header

        // public static readonly DependencyProperty ColumnWidth0Property = DependencyProperty.Register("ColumnWidth0", typeof(GridLength), typeof(PropertyEditorItem), new PropertyMetadata(PropertyEditor.ColumnWidth0Property.DefaultMetadata.DefaultValue, null, CoerceValueCallback));
        // public static readonly DependencyProperty ColumnWidth1Property = DependencyProperty.Register("ColumnWidth1", typeof(GridLength), typeof(PropertyEditorItem), new PropertyMetadata(PropertyEditor.ColumnWidth1Property.DefaultMetadata.DefaultValue, null));
        // public static readonly DependencyProperty ColumnWidth2Property = DependencyProperty.Register("ColumnWidth2", typeof(GridLength), typeof(PropertyEditorItem), new PropertyMetadata(PropertyEditor.ColumnWidth2Property.DefaultMetadata.DefaultValue, null));
        // 
        // public GridLength ColumnWidth0 { get => (GridLength) this.GetValue(ColumnWidth0Property); set => this.SetValue(ColumnWidth0Property, value); }
        // public GridLength ColumnWidth1 { get => (GridLength) this.GetValue(ColumnWidth1Property); set => this.SetValue(ColumnWidth1Property, value); }
        // public GridLength ColumnWidth2 { get => (GridLength) this.GetValue(ColumnWidth2Property); set => this.SetValue(ColumnWidth2Property, value); }

        public PropertyEditorItem() {
            // this.DataContextChanged += (sender, args) => {
            //     this.CoerceValue(ColumnWidth0Property);
            //     this.CoerceValue(ColumnWidth1Property);
            //     this.CoerceValue(ColumnWidth2Property);
            // };
        }

        private static object CoerceValueCallback(DependencyObject d, object basevalue) {
            // GridLength length = (GridLength) basevalue;
            // if (length.GridUnitType != GridUnitType.Pixel) {
            //     return basevalue;
            // }
            // 
            // PropertyEditorItem container = (PropertyEditorItem) d;
            // if (container.DataContext is BasePropertyEditorViewModel editor && editor.HierarchyDepth > 0) {
            //     return new GridLength(Math.Max(length.Value - (editor.HierarchyDepth * 4), 0), GridUnitType.Pixel);
            // }
            return basevalue;
        }
    }
}