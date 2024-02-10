using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.Controls {
    /// <summary>
    /// A control that contains a collection of property editor objects, such as slots, groups and separators
    /// </summary>
    public class PropertyEditorGroupControl : Control {
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(PropertyEditorGroupControl), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty GroupTypeProperty = DependencyProperty.Register("GroupType", typeof(GroupType), typeof(PropertyEditorGroupControl), new PropertyMetadata(GroupType.PrimaryExpander));
        public static readonly DependencyProperty PropertyEditorProperty = DependencyProperty.Register("PropertyEditor", typeof(PropertyEditorControl), typeof(PropertyEditorGroupControl), new PropertyMetadata(null));

        public bool IsExpanded {
            get => (bool) this.GetValue(IsExpandedProperty);
            set => this.SetValue(IsExpandedProperty, value.Box());
        }

        public GroupType GroupType {
            get => (GroupType) this.GetValue(GroupTypeProperty);
            set => this.SetValue(GroupTypeProperty, value);
        }

        public PropertyEditorControl PropertyEditor {
            get => (PropertyEditorControl) this.GetValue(PropertyEditorProperty);
            set => this.SetValue(PropertyEditorProperty, value);
        }

        public PropertyEditorItemsPanel Panel { get; private set; }

        public BasePropertyEditorGroup Model { get; private set; }

        public Expander TheExpander { get; private set; }

        private readonly AutoPropertyUpdateBinder<BasePropertyEditorGroup> displayNameBinder = new AutoPropertyUpdateBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.DisplayNameChanged), UpdateControlDisplayName, null);
        private readonly AutoPropertyUpdateBinder<BasePropertyEditorGroup> isVisibleBinder = new AutoPropertyUpdateBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.IsCurrentlyApplicableChanged), obj => ((PropertyEditorGroupControl) obj.Control).Visibility = (obj.Model.IsRoot || obj.Model.IsVisible) ? Visibility.Visible : Visibility.Collapsed, null);
        private readonly AutoPropertyUpdateBinder<BasePropertyEditorGroup> isExpandedBinder = new AutoPropertyUpdateBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.IsExpandedChanged), obj => ((PropertyEditorGroupControl) obj.Control).IsExpanded = obj.Model.IsExpanded, obj => obj.Model.IsExpanded = ((PropertyEditorGroupControl) obj.Control).IsExpanded);

        public PropertyEditorGroupControl() {

        }

        static PropertyEditorGroupControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorGroupControl), new FrameworkPropertyMetadata(typeof(PropertyEditorGroupControl)));
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);

            if (!e.Handled && e.OriginalSource is PropertyEditorItemsPanel) {
                e.Handled = true;
                this.PropertyEditor?.PropertyEditor?.ClearSelection();
                this.Focus();
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.GetTemplateChild("PART_Panel", out PropertyEditorItemsPanel panel);
            this.Panel = panel;
            this.Panel.OwnerGroup = this;

            if (this.GetTemplateChild("PART_Expander") is Expander expander) {
                this.TheExpander = expander;
            }
        }

        public void ConnectModel(PropertyEditorControl propertyEditor, BasePropertyEditorGroup group) {
            if (propertyEditor == null)
                throw new ArgumentNullException(nameof(propertyEditor));
            this.PropertyEditor = propertyEditor;
            this.Model = group;
            group.ItemAdded += this.ModelOnItemAdded;
            group.ItemRemoved += this.ModelOnItemRemoved;
            group.ItemMoved += this.ModelOnItemMoved;
            group.IsCurrentlyApplicableChanged += this.GroupOnIsCurrentlyApplicableChanged;
            this.GroupType = group.GroupType;
            this.displayNameBinder.Attach(this, group);
            this.isVisibleBinder.Attach(this, group);
            this.isExpandedBinder.Attach(this, group);

            int i = 0;
            foreach (BasePropertyEditorObject obj in group.PropertyObjects) {
                this.Panel.InsertItem(obj, i++);
            }
        }

        private void GroupOnIsCurrentlyApplicableChanged(BasePropertyEditorItem sender) {

        }

        public void DisconnectModel() {
            for (int i = this.Panel.Count - 1; i >= 0; i--) {
                this.Panel.RemoveItem(i);
            }

            this.displayNameBinder.Detatch();
            this.isVisibleBinder.Detatch();
            this.isExpandedBinder.Detatch();
            this.Model.ItemAdded -= this.ModelOnItemAdded;
            this.Model.ItemRemoved -= this.ModelOnItemRemoved;
            this.Model.ItemMoved -= this.ModelOnItemMoved;
            this.Model.IsCurrentlyApplicableChanged -= this.GroupOnIsCurrentlyApplicableChanged;
            this.Model = null;
        }

        private void ModelOnItemAdded(BasePropertyEditorGroup group, BasePropertyEditorObject item, int index) {
            this.Panel.InsertItem(item, index);
            this.Panel.UpdateLayout();
        }

        private void ModelOnItemRemoved(BasePropertyEditorGroup group, BasePropertyEditorObject item, int index) {
            this.Panel.RemoveItem(index);
        }

        private void ModelOnItemMoved(BasePropertyEditorGroup group, BasePropertyEditorObject item, int oldindex, int newindex) {
            this.Panel.MoveItem(oldindex, newindex);
        }

        private void GetTemplateChild<T>(string name, out T value) where T : DependencyObject {
            if ((value = this.GetTemplateChild(name) as T) == null)
                throw new Exception("Missing part: " + name);
        }

        private static void UpdateControlDisplayName(IBinder<BasePropertyEditorGroup> obj) {
            PropertyEditorGroupControl ctrl = (PropertyEditorGroupControl) obj.Control;
            if (ctrl.TheExpander != null)
                ctrl.TheExpander.Header = obj.Model.DisplayName;
        }
    }
}