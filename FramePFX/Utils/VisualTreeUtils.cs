using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FramePFX.Utils {
    public static class VisualTreeUtils {
        /// <summary>
        /// Returns the control which has the given inherited property defined
        /// </summary>
        /// <param name="property"></param>
        /// <param name="startObject"></param>
        /// <returns></returns>
        public static DependencyObject FindInheritedPropertyDefinition(DependencyProperty property, DependencyObject startObject) {
            DependencyObject obj = startObject;
            while (obj != null && obj.ReadLocalValue(property) == DependencyProperty.UnsetValue) {
                obj = GetParent(obj);
            }

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

        public static T FindVisualParent<T>(DependencyObject obj, bool includeSelf = true) where T : DependencyObject {
            if (obj == null || (includeSelf && obj is T)) {
                return (T) obj;
            }

            do {
                obj = GetParent(obj);
            } while (obj != null && !(obj is T));
            return (T) obj;
        }

        public static T FindDescendant<T>(DependencyObject d) where T : DependencyObject {
            if (d == null)
                return null;
            if (d is T t)
                return t;

            int count = VisualTreeHelper.GetChildrenCount(d);
            for (int i = 0; i < count; i++) {
                DependencyObject child = VisualTreeHelper.GetChild(d, i);
                T result = child as T ?? FindDescendant<T>(child);
                if (result != null) {
                    return result;
                }
            }

            return null;
        }

        public static T FindVisualChild<T>(DependencyObject obj, bool includeSelf = true) where T : DependencyObject {
            if (obj == null || (includeSelf && obj is T)) {
                return (T) obj;
            }

            if (!(obj is Visual) && !(obj is Visual3D)) {
                return null;
            }

            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++) {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T) {
                    return (T) child;
                }
            }

            for (int i = 0; i < count; i++) {
                T child = FindVisualChild<T>(VisualTreeHelper.GetChild(obj, i));
                if (child != null) {
                    return child;
                }
            }

            return obj is ContentControl element && element.Content is T t ? t : null;
        }

        public static object GetDataContext(DependencyObject value) {
            if (value is FrameworkElement element) {
                return element.DataContext;
            }
            else if (value is FrameworkContentElement contentElement) {
                return contentElement.DataContext;
            }
            else {
                return null;
            }
        }
    }
}