using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PSWMGRv2.Utils
{
    public static class VisualTreeUtils
    {
        /// <summary>
        /// Returns the control which has the given inherited property defined
        /// </summary>
        /// <param name="property"></param>
        /// <param name="startObject"></param>
        /// <returns></returns>
        public static DependencyObject FindNearestInheritedPropertyDefinition(DependencyProperty property, DependencyObject startObject)
        {
            DependencyObject obj = startObject;
            while (obj != null && obj.ReadLocalValue(property) == DependencyProperty.UnsetValue)
            {
                obj = GetParent(obj);
            }

            return obj;
        }

        public static DependencyObject GetParent(DependencyObject source)
        {
            if (source is Visual || source is Visual3D)
            {
                return VisualTreeHelper.GetParent(source);
            }
            else if (source is FrameworkContentElement fce)
            {
                return fce.Parent;
            }
            else
            {
                return null;
            }
        }

        public static T FindParent<T>(DependencyObject obj, bool includeSelf = true) where T : DependencyObject
        {
            if (obj == null || includeSelf && obj is T)
            {
                return (T) obj;
            }

            do
            {
                obj = GetParent(obj);
            } while (obj != null && !(obj is T));

            return (T) obj;
        }

        public static T FindVisualChild<T>(DependencyObject obj, bool includeSelf = true) where T : DependencyObject
        {
            if (obj == null || includeSelf && obj is T)
            {
                return (T) obj;
            }

            return FindVisualChildInternal<T>(obj);
        }

        private static T FindVisualChildInternal<T>(DependencyObject obj) where T : DependencyObject
        {
            int count, i;
            if (obj is ContentControl)
            {
                DependencyObject child = ((ContentControl) obj).Content as DependencyObject;
                if (child is T)
                {
                    return (T) child;
                }
                else
                {
                    return child != null ? FindVisualChildInternal<T>(child) : null;
                }
            }
            else if ((obj is Visual || obj is Visual3D) && (count = VisualTreeHelper.GetChildrenCount(obj)) > 0)
            {
                for (i = 0; i < count;)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i++);
                    if (child is T)
                    {
                        return (T) child;
                    }
                }

                for (i = 0; i < count;)
                {
                    T child = FindVisualChildInternal<T>(VisualTreeHelper.GetChild(obj, i++));
                    if (child != null)
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        public static object GetDataContext(DependencyObject value)
        {
            if (value is FrameworkElement)
            {
                return ((FrameworkElement) value).DataContext;
            }
            else if (value is FrameworkContentElement)
            {
                return ((FrameworkContentElement) value).DataContext;
            }
            else
            {
                return null;
            }
        }
    }
}