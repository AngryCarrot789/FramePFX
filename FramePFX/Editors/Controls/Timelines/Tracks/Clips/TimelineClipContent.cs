using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Clips {
    public class TimelineClipContent : Control {
        private static readonly Dictionary<Type, Func<TimelineClipContent>> Constructors = new Dictionary<Type, Func<TimelineClipContent>>();

        public TimelineClipControl ClipControl { get; private set; }

        public Clip Model => this.ClipControl?.Model;

        public TimelineClipContent() {
        }

        static TimelineClipContent() {
            RegisterType(typeof(ImageVideoClip), () => new ImageClipContent());
        }

        public void Connect(TimelineClipControl owner) {
            this.ClipControl = owner;
            this.OnConnected();
        }

        public void Disconnect() {
            this.OnDisconnected();
            this.ClipControl = null;
        }

        protected virtual void OnConnected() {
        }

        protected virtual void OnDisconnected() {
        }

        public static void RegisterType<T>(Type trackType, Func<T> func) where T : TimelineClipContent {
            Constructors[trackType] = func;
        }

        public static TimelineClipContent NewInstance(Type clipType) {
            if (clipType == null) {
                throw new ArgumentNullException(nameof(clipType));
            }

            for (Type type = clipType; type != null; type = type.BaseType) {
                if (Constructors.TryGetValue(clipType, out Func<TimelineClipContent> func)) {
                    return func();
                }
            }

            return null;
        }

        protected void GetTemplateChild<T>(string name, out T value) where T : DependencyObject {
            if ((value = this.GetTemplateChild(name) as T) == null)
                throw new Exception("Missing part: " + name + " of type " + typeof(T));
        }

        protected T GetTemplateChild<T>(string name) where T : DependencyObject {
            this.GetTemplateChild(name, out T value);
            return value;
        }

        protected bool TryGetTemplateChild<T>(string name, out T value) where T : DependencyObject {
            return (value = this.GetTemplateChild(name) as T) != null;
        }
    }
}