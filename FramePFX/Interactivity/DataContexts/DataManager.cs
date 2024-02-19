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

using System.Collections.Generic;
using System.Windows;
using FramePFX.Utils;

namespace FramePFX.Interactivity.DataContexts {
    /// <summary>
    /// A class that is used to extract contextual information from WPF components
    /// </summary>
    public static class DataManager {
        /// <summary>
        /// The context data property, used to store contextual information relative to a specific dependency object
        /// </summary>
        public static readonly DependencyProperty ContextDataProperty =
            DependencyProperty.RegisterAttached(
                "ContextData",
                typeof(IDataContext),
                typeof(DataManager),
                new PropertyMetadata(null));

        /// <summary>
        /// Sets or replaces the context data for the specific dependency object
        /// </summary>
        public static void SetContextData(DependencyObject element, IDataContext value) {
            element.SetValue(ContextDataProperty, value);
        }

        /// <summary>
        /// Gets the context data for the specific dependency object
        /// </summary>
        public static IDataContext GetContextData(DependencyObject element) {
            return (IDataContext) element.GetValue(ContextDataProperty);
        }

        /// <summary>
        /// Clears the <see cref="ContextDataProperty"/> value for the specific dependency object
        /// </summary>
        public static void ClearContextData(DependencyObject element) {
            element.ClearValue(ContextDataProperty);
        }

        // private static readonly PropertyInfo TreeLevelPropertyInfo = typeof(Visual).GetProperty("TreeLevel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) ?? throw new Exception("Could not find TreeLevel property");

        /// <summary>
        /// Does bottom-to-top scan of the element's visual tree, and then accumulates and merged all of the data keys
        /// associated with each object from top to bottom, ensuring the bottom of the visual tree has the most power
        /// over the final data context key values
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DataContext EvaluateContextData(DependencyObject obj) {
            DataContext ctx = new DataContext();

            // I thought about using TreeLevel, then thought reflection was too slow, so then I profiled the code...
            // This entire method (for a clip, 26 visual elements to the root) takes about 20 microseconds
            // Using the TreeLevel trick adds about 10 microseconds on top of it

            // int initialSize = 0;
            // if (obj is UIElement element && element.IsArrangeValid)
            //     initialSize = (int) (uint) TreeLevelPropertyInfo.GetValue(element);
            // if (initialSize < 1)
            //     initialSize = 32;

            // Accumulate visual tree bottom-to-top. Visual tree will contain the reverse tree
            List<DependencyObject> visualTree = new List<DependencyObject>(32);
            for (DependencyObject dp = obj; dp != null; dp = VisualTreeUtils.GetParent(dp)) {
                visualTree.Add(dp);
            }

            // Scan top-down in order to allow deeper objects' entries to override higher up entries
            for (int i = visualTree.Count - 1; i >= 0; i--) {
                DependencyObject dp = visualTree[i];
                object localEntry = dp.ReadLocalValue(ContextDataProperty);
                if (localEntry != DependencyProperty.UnsetValue && localEntry is IDataContext dpCtx) {
                    ctx.Merge(dpCtx);
                }
            }

            return ctx;
        }
    }
}