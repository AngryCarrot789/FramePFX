using System.Windows;
using System.Windows.Controls;
using FramePFX.Interactivity;
using FramePFX.Shortcuts.WPF;
using FramePFX.Utils;

namespace FramePFX.Actions.WPF {
    public class ActionContextProviderCollection : FreezableCollection<ActionContextProviderBase> {
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.RegisterAttached(
                "Collection",
                typeof(ActionContextProviderCollection),
                typeof(ActionContextProviderCollection),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty UseDataContextAsProviderProperty =
            DependencyProperty.RegisterAttached(
                "UseDataContextAsProvider",
                typeof(bool),
                typeof(ActionContextProviderCollection),
                new PropertyMetadata(BoolBox.False));

        public ActionContextProviderCollection() {

        }

        public static void SetUseDataContextAsProvider(DependencyObject element, bool value) => element.SetValue(UseDataContextAsProviderProperty, value.Box());

        public static bool GetUseDataContextAsProvider(DependencyObject element) => (bool) element.GetValue(UseDataContextAsProviderProperty);

        /// <summary>
        /// Sets the attached <see cref="ActionContextProviderCollection"/> for an element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetCollection(DependencyObject element, ActionContextProviderCollection value) => element.SetValue(CollectionProperty, value);

        /// <summary>
        /// Gets the attached <see cref="ActionContextProviderCollection"/> for an element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static ActionContextProviderCollection GetCollection(DependencyObject element) => (ActionContextProviderCollection) element.GetValue(CollectionProperty);

        public static DataContext CreateContextFromTarget(DependencyObject target, bool addTargetAndItemsControl = true, bool skipTarget = false) {
            DataContext ctx = new DataContext();
            CreateContextFromTarget(ctx, target, addTargetAndItemsControl, skipTarget);
            return ctx;
        }

        public static void CreateContextFromTarget(DataContext ctx, DependencyObject target, bool addTargetAndItemsControl = true, bool skipTarget = false) {
            object value;
            ItemsControl p = null;
            DependencyObject obj = skipTarget ? VisualTreeUtils.GetParent(target) : target;
            if (addTargetAndItemsControl && obj != null) {
                if ((value = VisualTreeUtils.GetDataContext(obj)) != null && !ctx.Contains(value)) {
                    ctx.AddContext(value);
                }

                p = ItemsControl.ItemsControlFromItemContainer(obj);
            }

            while (obj != null) {
                if (obj.ReadLocalValue(CollectionProperty) is ActionContextProviderCollection collection && collection.Count > 0) {
                    foreach (ActionContextProviderBase provider in collection) {
                        if (provider is ActionContextObject) {
                            if ((value = ((ActionContextObject) provider).Value) != null && !ctx.Contains(value)) {
                                ctx.AddContext(value);
                            }
                        }

                        if (provider is ActionContextEntry entry) {
                            if (entry.Key is string key && !ctx.ContainsKey(key) && (value = entry.Value) != null) {
                                ctx.Set(key, value);
                            }
                        }
                    }
                }

                if (p == obj || obj is Window || true.Equals(obj.GetValue(UseDataContextAsProviderProperty)) || obj.ReadLocalValue(UIInputManager.FocusPathProperty) is string) {
                    if ((value = VisualTreeUtils.GetDataContext(obj)) != null && !ctx.Contains(value)) {
                        ctx.AddContext(value);
                    }
                }

                obj = VisualTreeUtils.GetParent(obj);
            }
        }
    }
}