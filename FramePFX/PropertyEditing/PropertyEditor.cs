using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.PropertyEditing;

namespace FramePFX.PropertyEditing {
    public class PropertyEditor : Control {
        public static readonly DependencyProperty EditorRegistryProperty =
            DependencyProperty.Register(
                "EditorRegistry",
                typeof(PropertyEditorRegistry),
                typeof(PropertyEditor),
                new PropertyMetadata(null, (d, e) => {
                    if (e.NewValue is PropertyEditorRegistry editor) {
                        ((PropertyEditor) d).ApplicableItems = editor.Root.PropertyObjects;
                    }
                }));

        public static readonly DependencyProperty InputItemsProperty =
            DependencyProperty.Register(
                "InputItems",
                typeof(IEnumerable),
                typeof(PropertyEditor),
                new PropertyMetadata(null, (d, e) => ((PropertyEditor) d).OnDataSourceChanged((IEnumerable) e.OldValue, (IEnumerable) e.NewValue)));

        public static readonly DependencyProperty ApplicableItemsProperty =
            DependencyProperty.Register(
                "ApplicableItems",
                typeof(IEnumerable),
                typeof(PropertyEditor),
                new PropertyMetadata(null));

        private static readonly GridLength Star = new GridLength(1, GridUnitType.Star);
        public static readonly DependencyProperty ColumnWidth0Property = DependencyProperty.Register("ColumnWidth0", typeof(GridLength), typeof(PropertyEditor), new PropertyMetadata(new GridLength(100d)));
        public static readonly DependencyProperty ColumnWidth1Property = DependencyProperty.Register("ColumnWidth1", typeof(GridLength), typeof(PropertyEditor), new PropertyMetadata(new GridLength(5)));
        public static readonly DependencyProperty ColumnWidth2Property = DependencyProperty.Register("ColumnWidth2", typeof(GridLength), typeof(PropertyEditor), new PropertyMetadata(Star));

        public PropertyEditorRegistry EditorRegistry {
            get => (PropertyEditorRegistry) this.GetValue(EditorRegistryProperty);
            set => this.SetValue(EditorRegistryProperty, value);
        }

        // INPUT
        public IEnumerable InputItems {
            get => (IEnumerable) this.GetValue(InputItemsProperty);
            set => this.SetValue(InputItemsProperty, value);
        }

        // OUTPUT

        public IEnumerable ApplicableItems {
            get => (IEnumerable) this.GetValue(ApplicableItemsProperty);
            set => this.SetValue(ApplicableItemsProperty, value);
        }

        public GridLength ColumnWidth0 { get => (GridLength) this.GetValue(ColumnWidth0Property); set => this.SetValue(ColumnWidth0Property, value); }
        public GridLength ColumnWidth1 { get => (GridLength) this.GetValue(ColumnWidth1Property); set => this.SetValue(ColumnWidth1Property, value); }
        public GridLength ColumnWidth2 { get => (GridLength) this.GetValue(ColumnWidth2Property); set => this.SetValue(ColumnWidth2Property, value); }

        private readonly bool isInDesigner;

        public PropertyEditor() {
            this.isInDesigner = DesignerProperties.GetIsInDesignMode(this);
        }

        private void OnDataSourceChanged(IEnumerable oldItems, IEnumerable newItems) {
            if (oldItems is INotifyCollectionChanged)
                ((INotifyCollectionChanged) oldItems).CollectionChanged -= this.OnDataSourceCollectionChanged;
            if (newItems is INotifyCollectionChanged)
                ((INotifyCollectionChanged) newItems).CollectionChanged += this.OnDataSourceCollectionChanged;
            this.SetupObjects();
        }

        private void OnDataSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            this.SetupObjects();
        }

        private void SetupObjects() {
            if (this.EditorRegistry is PropertyEditorRegistry registry) {
                IEnumerable items = this.InputItems;
                List<object> list = items != null ? items.Cast<object>().ToList() : new List<object>();
                registry.SetupObjects(list);
            }
        }
    }
}