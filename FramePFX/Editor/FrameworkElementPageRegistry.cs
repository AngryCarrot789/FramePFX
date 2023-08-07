using System;
using System.Collections.Generic;
using System.Windows;

namespace FramePFX.Editor
{
    public class FrameworkElementPageRegistry<TBase>
    {
        private readonly Dictionary<Type, Func<TBase, FrameworkElement>> Map;

        public FrameworkElementPageRegistry()
        {
            this.Map = new Dictionary<Type, Func<TBase, FrameworkElement>>();
        }

        protected void Register<T>(Func<T, FrameworkElement> func) where T : TBase
        {
            this.Map[typeof(T)] = (t) => func((T) t);
        }

        public bool GenerateControl(Type type, TBase value, out FrameworkElement control)
        {
            return (control = this.Map.TryGetValue(type, out Func<TBase, FrameworkElement> func) ? func(value) : null) != null;
        }
    }
}