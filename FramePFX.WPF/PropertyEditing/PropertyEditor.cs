using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml.Linq;
using FramePFX.PropertyEditing;
using FramePFX.WPF.Utils;

namespace FramePFX.WPF.PropertyEditing {
    /// <summary>
    /// The root level of a property editor tree.
    /// </summary>
    [TemplatePart(Name = "PART_ItemsControl", Type = typeof(PropertyEditorItemsControl))]
    public class PropertyEditor : Control {
        private static readonly GridLength Star = new GridLength(1, GridUnitType.Star);

        #region Dependency Properties

        private static readonly DependencyPropertyKey SelectedItemsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SelectedItems), typeof(IList<IPropertyEditorObject>), typeof(PropertyEditor), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsPropertyKey.DependencyProperty;
        public static readonly DependencyProperty EditorRegistryProperty = DependencyProperty.Register("EditorRegistry", typeof(PropertyEditorRegistry), typeof(PropertyEditor), new PropertyMetadata(null, OnEditorRegistryPropertyChanged));
        public static readonly DependencyProperty ApplicableItemsProperty = DependencyProperty.Register("ApplicableItems", typeof(IEnumerable<IPropertyEditorObject>), typeof(PropertyEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty ColumnWidth0Property = DependencyProperty.Register("ColumnWidth0", typeof(GridLength), typeof(PropertyEditor), new PropertyMetadata(new GridLength(100d)));
        public static readonly DependencyProperty ColumnWidth1Property = DependencyProperty.Register("ColumnWidth1", typeof(GridLength), typeof(PropertyEditor), new PropertyMetadata(new GridLength(5)));
        public static readonly DependencyProperty ColumnWidth2Property = DependencyProperty.Register("ColumnWidth2", typeof(GridLength), typeof(PropertyEditor), new PropertyMetadata(Star));

        #endregion

        public static readonly RoutedEvent SelectedItemsChangedEvent = EventManager.RegisterRoutedEvent("SelectedItemsChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<IList<IPropertyEditorObject>>), typeof(PropertyEditor));
        private List<ISelectablePropertyControl> selectedContainers = new List<ISelectablePropertyControl>();
        private List<IPropertyEditorObject> selectedObjects = new List<IPropertyEditorObject>();

        public PropertyEditorRegistry EditorRegistry {
            get => (PropertyEditorRegistry) this.GetValue(EditorRegistryProperty);
            set => this.SetValue(EditorRegistryProperty, value);
        }

        // OUTPUT

        /// <summary>
        /// Gets or sets a collection of root-level items that this editor should preset. This is bound by our <see cref="ChildItemsControl"/>
        /// </summary>
        public IEnumerable<IPropertyEditorObject> ApplicableItems {
            get => (IEnumerable<IPropertyEditorObject>) this.GetValue(ApplicableItemsProperty);
            set => this.SetValue(ApplicableItemsProperty, value);
        }

        public GridLength ColumnWidth0 { get => (GridLength) this.GetValue(ColumnWidth0Property); set => this.SetValue(ColumnWidth0Property, value); }
        public GridLength ColumnWidth1 { get => (GridLength) this.GetValue(ColumnWidth1Property); set => this.SetValue(ColumnWidth1Property, value); }
        public GridLength ColumnWidth2 { get => (GridLength) this.GetValue(ColumnWidth2Property); set => this.SetValue(ColumnWidth2Property, value); }

        [Bindable(true)]
        [Category("Appearance")]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<IPropertyEditorObject> SelectedItems => (IList<IPropertyEditorObject>) this.GetValue(SelectedItemsProperty);

        [Category("Behavior")]
        public event RoutedPropertyChangedEventHandler<IList<IPropertyEditorObject>> SelectedItemsChanged {
            add => this.AddHandler(SelectedItemsChangedEvent, value);
            remove => this.RemoveHandler(SelectedItemsChangedEvent, value);
        }

        public PropertyEditorItemsControl ChildItemsControl { get; private set; }

        public bool IsSelectionChangeActive { get; set; }

        public PropertyEditor() {
        }

        static PropertyEditor() {

        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.OriginalSource is DependencyObject obj) {
                PropertyEditorItemsControl parent = VisualTreeUtils.FindParent<PropertyEditorItemsControl>(obj);
                if (parent == null || parent.myPropertyEditor == this) {
                    this.ClearSelection();
                }
            }
        }

        private static void OnEditorRegistryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue is PropertyEditorRegistry editor) {
                ((PropertyEditor) d).ApplicableItems = editor.Root.PropertyObjects;
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            if (this.ChildItemsControl != null) {
                this.ChildItemsControl.myPropertyEditor = null;
            }

            this.ChildItemsControl = this.GetTemplateChild("PART_ItemsControl") as PropertyEditorItemsControl ?? throw new Exception("Missing the editor's items control");
            this.ChildItemsControl.myPropertyEditor = this;
        }

        internal void SetContainerSelection(IPropertyEditorObject data, ISelectablePropertyControl container, bool selected, bool setPrimarySelection) {
            if (this.IsSelectionChangeActive)
                return;

            if (data == null)
                throw new Exception("Data must not be null");
            if (container == null)
                throw new Exception("Container must not be null");

            IList<IPropertyEditorObject> oldValue = this.selectedObjects.ToList();
            IList<IPropertyEditorObject> newValue = null;
            bool flag = false;

            this.IsSelectionChangeActive = true;
            try {
                int index;
                if (selected) {
                    if ((index = this.selectedContainers.IndexOf(container)) == -1 || setPrimarySelection) {
                        if (setPrimarySelection) {
                            foreach (ISelectablePropertyControl t in this.selectedContainers) {
                                t.IsSelected = false;
                            }

                            this.selectedContainers = new List<ISelectablePropertyControl>() { container };
                            this.selectedObjects = new List<IPropertyEditorObject>() { data };
                        }
                        else {
                            this.selectedContainers.Add(container);
                            this.selectedObjects.Add(data);
                        }

                        flag = true;
                    }
                    else {
                        this.selectedContainers.RemoveAt(index);
                        this.selectedObjects.RemoveAt(index);
                        selected = false;
                        flag = true;
                    }
                }
                else if ((index = this.selectedContainers.IndexOf(container)) != -1) {
                    this.selectedContainers.RemoveAt(index);
                    this.selectedObjects.RemoveAt(index);
                    flag = true;
                }

                if (container.IsSelected != selected)
                    container.IsSelected = selected;

                if (flag) {
                    newValue = this.selectedObjects.ToList();
                    this.SetValue(SelectedItemsPropertyKey, newValue);
                }
            }
            finally {
                this.IsSelectionChangeActive = false;
            }

            if (flag) {
                this.RaiseEvent(new RoutedPropertyChangedEventArgs<IList<IPropertyEditorObject>>(oldValue, newValue, SelectedItemsChangedEvent));
            }
        }

        public void ClearSelection() {
            if (this.selectedContainers.Count < 1) {
                return;
            }

            IList<IPropertyEditorObject> oldValue = this.selectedObjects.ToList();
            IList<IPropertyEditorObject> newValue = null;
            try {
                this.IsSelectionChangeActive = true;
                foreach (ISelectablePropertyControl t in this.selectedContainers) {
                    t.IsSelected = false;
                }

                this.selectedContainers = new List<ISelectablePropertyControl>();
                this.selectedObjects = new List<IPropertyEditorObject>();
                newValue = this.selectedObjects.ToList();
                this.SetValue(SelectedItemsPropertyKey, newValue);
            }
            finally {
                this.IsSelectionChangeActive = false;
            }

            this.RaiseEvent(new RoutedPropertyChangedEventArgs<IList<IPropertyEditorObject>>(oldValue, newValue, SelectedItemsChangedEvent));
        }
    }
}