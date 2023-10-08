using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FramePFX.WPF.Utils {
    public static class VisualTreeUtils {
        /// <summary>
        /// Returns the control which has the given inherited property defined
        /// </summary>
        /// <param name="property"></param>
        /// <param name="startObject"></param>
        /// <returns></returns>
        public static DependencyObject FindNearestInheritedPropertyDefinition(DependencyProperty property, DependencyObject startObject) {
            DependencyObject obj = startObject;
            while (obj != null && obj.ReadLocalValue(property) == DependencyProperty.UnsetValue) {
                obj = GetParent(obj);
            }

            return obj;
        }

        public static DependencyObject FindNearestInheritedPropertyDefinition(DependencyProperty property, DependencyObject startObject, out object value) {
            object val = null;
            DependencyObject obj = startObject;
            while (obj != null && (val = obj.ReadLocalValue(property)) == DependencyProperty.UnsetValue) {
                obj = GetParent(obj);
            }

            value = val;
            return obj;
        }

        public static DependencyObject GetParent(DependencyObject source) {
            if (source is Visual || source is Visual3D) {
                return VisualTreeHelper.GetParent(source);
            }
            else if (source is FrameworkContentElement fce) {
                return fce.Parent;
            }
            else {
                return null;
            }
        }

        public static T GetParent<T>(DependencyObject obj, bool includeSelf = true) where T : class {
            if (obj == null)
                return null;
            if (includeSelf && obj is T)
                return (T) (object) obj;

            do {
                obj = GetParent(obj);
                if (obj == null)
                    return null;
                if (obj is T)
                    return (T) (object) obj;
            } while (true);
        }

        public static T FindVisualChild<T>(DependencyObject obj, bool includeSelf = true) where T : class {
            if (obj == null)
                return null;
            if (includeSelf && obj is T t)
                return t;

            return FindVisualChildInternal<T>(obj);
        }

        private static T FindVisualChildInternal<T>(DependencyObject obj) where T : class {
            int count, i;
            if (obj is ContentControl) {
                DependencyObject child = ((ContentControl) obj).Content as DependencyObject;
                if (child is T t) {
                    return t;
                }
                else {
                    return child != null ? FindVisualChildInternal<T>(child) : null;
                }
            }
            else if ((obj is Visual || obj is Visual3D) && (count = VisualTreeHelper.GetChildrenCount(obj)) > 0) {
                for (i = 0; i < count;) {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i++);
                    if (child is T t) {
                        return t;
                    }
                }

                for (i = 0; i < count;) {
                    T child = FindVisualChildInternal<T>(VisualTreeHelper.GetChild(obj, i++));
                    if (child != null) {
                        return child;
                    }
                }
            }

            return null;
        }

        public static object GetDataContext(DependencyObject value) {
            if (value is FrameworkElement) {
                return ((FrameworkElement) value).DataContext;
            }
            else if (value is FrameworkContentElement) {
                return ((FrameworkContentElement) value).DataContext;
            }
            else {
                return null;
            }
        }

        public static bool GetDataContext(DependencyObject value, out object context, bool includeNullContext = false) {
            if (value is FrameworkElement) {
                return (context = ((FrameworkElement) value).DataContext) != null || includeNullContext;
            }
            else if (value is FrameworkContentElement) {
                return (context = ((FrameworkContentElement) value).DataContext) != null || includeNullContext;
            }
            else {
                context = null;
                return false;
            }
        }

        public static ItemsControl GetItemsControlFromObject(DependencyObject obj) {
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(obj);
            if (ic != null) {
                return ic;
            }

            DependencyObject templated;
            if (obj is FrameworkElement && (templated = ((FrameworkElement) obj).TemplatedParent) != null) {
                ic = templated as ItemsControl ?? ItemsControl.ItemsControlFromItemContainer(templated);
            }

            return ic;
        }

        public static AdornerLayer GetRootAdornerLayer(Visual visual) {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(visual);
            for (AdornerLayer parent = layer; parent != null; parent = GetParent(parent) is Visual v ? AdornerLayer.GetAdornerLayer(v) : null) {
                layer = parent;
            }

            return layer;
        }
    }
}