//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FramePFX.Utils.Visuals
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

        public static DependencyObject FindNearestInheritedPropertyDefinition(DependencyProperty property, DependencyObject startObject, out object value)
        {
            object val = null;
            DependencyObject obj = startObject;
            while (obj != null && (val = obj.ReadLocalValue(property)) == DependencyProperty.UnsetValue)
            {
                obj = GetParent(obj);
            }

            value = val;
            return obj;
        }

        /// <summary>
        /// Gets the parent of the given source object
        /// </summary>
        /// <param name="source">The object to get the parent of</param>
        /// <param name="visualOnly">
        /// True to only allow the visual parent, otherwise false to allow visual,
        /// logical and templated parents (in that order based on availability)
        /// </param>
        /// <returns>The parent, or null, if there was no parent available</returns>
        public static DependencyObject GetParent(DependencyObject source, bool visualOnly = false)
        {
            if (source is Visual || source is Visual3D)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(source);
                if (parent == null && !visualOnly && source is FrameworkElement srcElem)
                    parent = srcElem.Parent ?? srcElem.TemplatedParent;
                return parent;
            }
            else if (source is FrameworkContentElement srcContentElem)
            {
                return srcContentElem.Parent ?? srcContentElem.TemplatedParent;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the first parent object of the given type
        /// </summary>
        /// <param name="obj">A child object</param>
        /// <param name="includeSelf">True to check if obj is of the given generic type, and if so, return that. Otherwise, scan parents</param>
        /// <typeparam name="T">Type of parent</typeparam>
        /// <returns>The parent, or null, if none of the given type were found</returns>
        public static T GetParent<T>(DependencyObject obj, bool includeSelf = true, bool visualOnly = false) where T : class
        {
            if (obj == null)
                return null;
            if (includeSelf && obj is T)
                return (T) (object) obj;

            do
            {
                obj = GetParent(obj, visualOnly);
                if (obj == null)
                    return null;
                if (obj is T)
                    return (T) (object) obj;
            } while (true);
        }

        public static T GetLastParent<T>(DependencyObject obj, bool visualOnly = false) where T : class
        {
            T lastParent = null;
            for (T parent = GetParent<T>(obj, false, visualOnly); parent != null; parent = GetParent<T>((DependencyObject) (object) parent, false, visualOnly))
                lastParent = parent;
            return lastParent;
        }

        public static T FindVisualChild<T>(DependencyObject obj, bool includeSelf = true) where T : class
        {
            if (obj == null)
                return null;
            if (includeSelf && obj is T t)
                return t;

            return FindVisualChildInternal<T>(obj);
        }

        private static T FindVisualChildInternal<T>(DependencyObject obj) where T : class
        {
            int count, i;
            if (obj is ContentControl)
            {
                DependencyObject child = ((ContentControl) obj).Content as DependencyObject;
                if (child is T t)
                {
                    return t;
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
                    if (child is T t)
                    {
                        return t;
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

        public static bool GetDataContext(DependencyObject value, out object context, bool includeNullContext = false)
        {
            if (value is FrameworkElement)
            {
                return (context = ((FrameworkElement) value).DataContext) != null || includeNullContext;
            }
            else if (value is FrameworkContentElement)
            {
                return (context = ((FrameworkContentElement) value).DataContext) != null || includeNullContext;
            }
            else
            {
                context = null;
                return false;
            }
        }

        public static ItemsControl GetItemsControlFromObject(DependencyObject obj)
        {
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(obj);
            if (ic != null)
            {
                return ic;
            }

            DependencyObject templated;
            if (obj is FrameworkElement && (templated = ((FrameworkElement) obj).TemplatedParent) != null)
            {
                ic = templated as ItemsControl ?? ItemsControl.ItemsControlFromItemContainer(templated);
            }

            return ic;
        }

        public static AdornerLayer GetRootAdornerLayer(Visual visual)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(visual);
            for (AdornerLayer parent = layer; parent != null; parent = GetParent(parent) is Visual v ? AdornerLayer.GetAdornerLayer(v) : null)
            {
                layer = parent;
            }

            return layer;
        }
    }
}