using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.Controls {
    public class PropertyEditorSlotControl : ContentControl {
        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(PropertyEditorSlotControl),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    (d, e) => ((PropertyEditorSlotControl) d).OnSelectionChanged((bool) e.OldValue, (bool) e.NewValue),
                    (o, value) => ((PropertyEditorSlotControl) o).IsSelectable ? value : BoolBox.False));

        public static readonly DependencyProperty IsSelectableProperty =
            DependencyProperty.Register(
                "IsSelectable",
                typeof(bool),
                typeof(PropertyEditorSlotControl),
                new PropertyMetadata(BoolBox.False, (o, e) => ((PropertyEditorSlotControl) o).CoerceValue(IsSelectedProperty)));

        /// <summary>
        /// Whether or not this slot is selected. Setting this property automatically affects
        /// our <see cref="PropertyEditing.PropertyEditor"/>'s selected items
        /// </summary>
        [Category("Appearance")]
        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value.Box());
        }

        [Category("Appearance")]
        public bool IsSelectable {
            get => (bool) this.GetValue(IsSelectableProperty);
            set => this.SetValue(IsSelectableProperty, value.Box());
        }

        public PropertyEditorSlot Model { get; private set; }

        private readonly GetSetAutoPropertyBinder<PropertyEditorSlot> isSelectedBinder = new GetSetAutoPropertyBinder<PropertyEditorSlot>(IsSelectedProperty, nameof(PropertyEditorSlot.IsSelectedChanged), b => b.Model.IsSelected.Box(), (b, v) => b.Model.IsSelected = (bool) v);

        public PropertyEditorSlotControl() {
        }

        static PropertyEditorSlotControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorSlotControl), new FrameworkPropertyMetadata(typeof(PropertyEditorSlotControl)));
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseDown(e);
            if (!e.Handled && this.IsSelectable) {
                this.IsSelected = true;
            }
        }

        public void ConnectModel(PropertyEditorSlot item) {
            BasePropEditControlContent content = BasePropEditControlContent.NewContentInstance(item.GetType());
            this.Model = item;
            this.Content = content;
            this.InvalidateMeasure();
            content.InvalidateMeasure();
            content.ApplyTemplate();
            content.Connect(this);

            this.IsSelectable = item.IsSelectable;
            this.isSelectedBinder.Attach(this, item);
        }

        public void DisconnectModel() {
            ((BasePropEditControlContent) this.Content).Disconnect();
            this.isSelectedBinder.Detatch();
            this.Model = null;
        }

        private void OnSelectionChanged(bool oldValue, bool newValue) {

        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            this.isSelectedBinder.OnPropertyChanged(e);
        }
    }
}