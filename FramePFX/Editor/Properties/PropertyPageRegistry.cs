using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Editor.Properties.Pages;

namespace FramePFX.Editor.Properties {
    public static class PropertyPageRegistry {
        private static readonly Dictionary<Type, Func<ClipViewModel, UserControl>> Map;

        static PropertyPageRegistry() {
            Map = new Dictionary<Type, Func<ClipViewModel, UserControl>>();
            Register<ClipViewModel>((x) => new PropertyPageBaseClip());
            Register<VideoClipViewModel>((x) => new PropertyPageVideoClip());
            Register<SquareClipViewModel>((x) => new PropertyPageShapeClip());
        }

        private static void Register<T>(Func<T, UserControl> func) where T : ClipViewModel {
            Map[typeof(T)] = (t) => func((T) t);
        }

        public static bool GenerateControl(Type type, ClipViewModel clip, out FrameworkElement control) {
            return (control = Map.TryGetValue(type, out Func<ClipViewModel, UserControl> func) ? func(clip) : null) != null;
        }
    }
}