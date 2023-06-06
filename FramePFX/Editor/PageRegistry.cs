using System;
using System.Collections.Generic;
using System.Windows;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Editor {
    public class PageRegistry {
        private readonly Dictionary<Type, Func<ClipViewModel, FrameworkElement>> Map;

        public PageRegistry() {
            this.Map = new Dictionary<Type, Func<ClipViewModel, FrameworkElement>>();
        }

        protected void Register<T>(Func<T, FrameworkElement> func) where T : ClipViewModel {
            this.Map[typeof(T)] = (t) => func((T) t);
        }

        public bool GenerateControl(Type type, ClipViewModel clip, out FrameworkElement control) {
            return (control = this.Map.TryGetValue(type, out Func<ClipViewModel, FrameworkElement> func) ? func(clip) : null) != null;
        }
    }
}