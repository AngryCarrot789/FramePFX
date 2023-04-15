using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SharpPadV2.Shortcuts {
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
    }
}