using System;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.Controls {
    /// <summary>
    /// A control that contains a collection of property editor objects, such as slots, groups and separators
    /// </summary>
    public class PropertyEditorGroupControl : Control {
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(PropertyEditorGroupControl), new PropertyMetadata(BoolBox.False));

        public bool IsExpanded {
            get => (bool) this.GetValue(IsExpandedProperty);
            set => this.SetValue(IsExpandedProperty, value.Box());
        }

        public PropertyEditorControlPanel Panel { get; private set; }

        public BasePropertyEditorGroup Model { get; private set; }

        public Expander TheExpander { get; private set; }

        private readonly AutoPropertyUpdateBinder<BasePropertyEditorGroup> displayNameBinder = new AutoPropertyUpdateBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.DisplayNameChanged), UpdateControlDisplayName, null);
        private readonly AutoPropertyUpdateBinder<BasePropertyEditorGroup> isVisibleBinder = new AutoPropertyUpdateBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.IsCurrentlyApplicableChanged), obj => ((PropertyEditorGroupControl) obj.Control).Visibility = (obj.Model.IsRoot || obj.Model.IsCurrentlyApplicable) ? Visibility.Visible : Visibility.Collapsed, null);
        private readonly AutoPropertyUpdateBinder<BasePropertyEditorGroup> isExpandedBinder = new AutoPropertyUpdateBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.IsExpandedChanged), obj => ((PropertyEditorGroupControl) obj.Control).IsExpanded = obj.Model.IsExpanded, obj => obj.Model.IsExpanded = ((PropertyEditorGroupControl) obj.Control).IsExpanded);

        public PropertyEditorGroupControl() {

        }

        static PropertyEditorGroupControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorGroupControl), new FrameworkPropertyMetadata(typeof(PropertyEditorGroupControl)));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.GetTemplateChild("PART_Panel", out PropertyEditorControlPanel panel);
            this.Panel = panel;
            this.Panel.OwnerGroup = this;

            if (this.GetTemplateChild("PART_Expander") is Expander expander) {
                this.TheExpander = expander;
            }
        }

        public void ConnectModel(BasePropertyEditorGroup group) {
            this.Model = group;
            this.Model.ItemAdded += this.ModelOnItemAdded;
            this.Model.ItemRemoved += this.ModelOnItemRemoved;
            this.displayNameBinder.Attach(this, group);
            this.isVisibleBinder.Attach(this, group);
            this.isExpandedBinder.Attach(this, group);

            int i = 0;
            foreach (BasePropertyEditorObject obj in group.PropertyObjects) {
                this.Panel.InsertItem(obj, i++);
            }
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
            this.Model = null;
        }

        private void ModelOnItemAdded(BasePropertyEditorGroup @group, BasePropertyEditorObject item, int index) {
            this.Panel.InsertItem(item, index);
        }

        private void ModelOnItemRemoved(BasePropertyEditorGroup @group, BasePropertyEditorObject item, int index) {
            this.Panel.RemoveItem(index);
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