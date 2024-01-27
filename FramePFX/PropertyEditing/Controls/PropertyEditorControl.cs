using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FramePFX.PropertyEditing.Controls {
    public class PropertyEditorControl : Control {
        private static readonly GridLength Star = new GridLength(1, GridUnitType.Star);
        public static readonly DependencyProperty PropertyEditorProperty = DependencyProperty.Register("PropertyEditor", typeof(BasePropertyEditor), typeof(PropertyEditorControl), new PropertyMetadata(null, (d, e) => ((PropertyEditorControl) d).OnPropertyEditorChanged((BasePropertyEditor) e.OldValue, (BasePropertyEditor) e.NewValue)));
        public static readonly DependencyProperty ColumnWidth0Property = DependencyProperty.Register("ColumnWidth0", typeof(GridLength), typeof(PropertyEditorControl), new PropertyMetadata(new GridLength(100d)));
        public static readonly DependencyProperty ColumnWidth1Property = DependencyProperty.Register("ColumnWidth1", typeof(GridLength), typeof(PropertyEditorControl), new PropertyMetadata(new GridLength(5)));
        public static readonly DependencyProperty ColumnWidth2Property = DependencyProperty.Register("ColumnWidth2", typeof(GridLength), typeof(PropertyEditorControl), new PropertyMetadata(Star));

        public BasePropertyEditor PropertyEditor {
            get => (BasePropertyEditor) this.GetValue(PropertyEditorProperty);
            set => this.SetValue(PropertyEditorProperty, value);
        }

        public GridLength ColumnWidth0 { get => (GridLength) this.GetValue(ColumnWidth0Property); set => this.SetValue(ColumnWidth0Property, value); }

        public GridLength ColumnWidth1 { get => (GridLength) this.GetValue(ColumnWidth1Property); set => this.SetValue(ColumnWidth1Property, value); }

        public GridLength ColumnWidth2 { get => (GridLength) this.GetValue(ColumnWidth2Property); set => this.SetValue(ColumnWidth2Property, value); }

        public PropertyEditorGroupControl RootGroupControl { get; private set; }

        public PropertyEditorSlotControl TouchedSlot { get; set; }

        public PropertyEditorControl() {
        }

        static PropertyEditorControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorControl), new FrameworkPropertyMetadata(typeof(PropertyEditorControl)));
        }

        // protected override void OnMouseDown(MouseButtonEventArgs e) {
        //     base.OnMouseDown(e);
        //     if (e.LeftButton != MouseButtonState.Pressed || e.Handled) {
        //         return;
        //     }
        // 
        //     if (this.TouchedSlot != null) {
        //         return;
        //     }
        // 
        //     this.PropertyEditor?.ClearSelection();
        // }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.RootGroupControl = (PropertyEditorGroupControl) this.GetTemplateChild("PART_RootGroupControl") ?? throw new Exception("Missing PART_RootGroupControl");
        }

        private void OnPropertyEditorChanged(BasePropertyEditor oldEditor, BasePropertyEditor newEditor) {
            if (oldEditor != null) {
                this.RootGroupControl.DisconnectModel();
            }

            this.InvalidateMeasure();
            this.UpdateLayout();
            if (newEditor != null) {
                this.RootGroupControl.ConnectModel(this, newEditor.Root);
            }
        }
    }
}