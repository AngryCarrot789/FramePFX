using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using OpenTK.Graphics.OpenGL4;

namespace FramePFX.Shortcuts {
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

        public static T FindVisualChild<T>(DependencyObject obj, bool includeSelf) where T : DependencyObject {
            if (obj == null || (includeSelf && obj is T)) {
                return (T) obj;
            }

            return FindVisualChild<T>(obj);
        }

        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject {
            if (obj == null || obj is T) {
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

            return null;
        }
    }
}