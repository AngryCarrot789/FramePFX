using System;
using System.Windows;
using System.Windows.Controls;

namespace FramePFX.PropertyEditing.Controls {
    public class PropertyEditorControl : Control {
        private static readonly GridLength Star = new GridLength(1, GridUnitType.Star);
        public static readonly DependencyProperty RootGroupProperty = DependencyProperty.Register("RootGroup", typeof(FixedPropertyEditorGroup), typeof(PropertyEditorControl), new PropertyMetadata(null, OnRootGroupChanged));
        public static readonly DependencyProperty ColumnWidth0Property = DependencyProperty.Register("ColumnWidth0", typeof(GridLength), typeof(PropertyEditorControl), new PropertyMetadata(new GridLength(100d)));
        public static readonly DependencyProperty ColumnWidth1Property = DependencyProperty.Register("ColumnWidth1", typeof(GridLength), typeof(PropertyEditorControl), new PropertyMetadata(new GridLength(5)));
        public static readonly DependencyProperty ColumnWidth2Property = DependencyProperty.Register("ColumnWidth2", typeof(GridLength), typeof(PropertyEditorControl), new PropertyMetadata(Star));

        public FixedPropertyEditorGroup RootGroup {
            get => (FixedPropertyEditorGroup) this.GetValue(RootGroupProperty);
            set => this.SetValue(RootGroupProperty, value);
        }

        public PropertyEditorGroupControl RootGroupControl { get; private set; }

        public GridLength ColumnWidth0 { get => (GridLength) this.GetValue(ColumnWidth0Property); set => this.SetValue(ColumnWidth0Property, value); }
        public GridLength ColumnWidth1 { get => (GridLength) this.GetValue(ColumnWidth1Property); set => this.SetValue(ColumnWidth1Property, value); }
        public GridLength ColumnWidth2 { get => (GridLength) this.GetValue(ColumnWidth2Property); set => this.SetValue(ColumnWidth2Property, value); }

        public PropertyEditorControl() {
        }

        static PropertyEditorControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorControl), new FrameworkPropertyMetadata(typeof(PropertyEditorControl)));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.RootGroupControl = (PropertyEditorGroupControl) this.GetTemplateChild("PART_RootGroupControl") ?? throw new Exception("Missing PART_RootGroupControl");
        }

        private static void OnRootGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            PropertyEditorControl control = (PropertyEditorControl) d;
            if (e.OldValue is FixedPropertyEditorGroup) {
                control.RootGroupControl.DisconnectModel();
            }

            if (e.NewValue is FixedPropertyEditorGroup newGroup) {
                control.RootGroupControl.ConnectModel(newGroup);
            }
        }
    }
}