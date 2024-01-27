using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.PropertyEditors.AControls.Clips;
using FramePFX.Editors.PropertyEditors.AControls.Effects;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.Editors.PropertyEditors.Effects.Motion;

namespace FramePFX.PropertyEditing.Controls {
    public abstract class BasePropEditControlContent : Control {
        private static readonly Dictionary<Type, Func<BasePropEditControlContent>> Constructors;

        public PropertyEditorSlotControl Slot { get; private set; }

        protected BasePropEditControlContent() {
        }

        static BasePropEditControlContent() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasePropEditControlContent), new FrameworkPropertyMetadata(typeof(BasePropEditControlContent)));
            Constructors = new Dictionary<Type, Func<BasePropEditControlContent>>();
            RegisterType(typeof(VideoClipOpacityPropertyEditorSlot), () => new VideoClipOpacityPropertyEditorControl());
            RegisterType(typeof(ClipDisplayNamePropertyEditorSlot), () => new ClipDisplayNamePropertyEditorControl());
            RegisterType(typeof(MotionEffectMediaPosPropertyEditorSlot), () => new MotionEffectMediaPosEditorControl());
        }

        public static void RegisterType<T>(Type trackType, Func<T> func) where T : BasePropEditControlContent {
            Constructors[trackType] = func;
        }

        public static BasePropEditControlContent NewContentInstance(Type slotType) {
            if (slotType == null) {
                throw new ArgumentNullException(nameof(slotType));
            }

            // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
            bool hasLogged = false;
            for (Type type = slotType; type != null; type = type.BaseType) {
                if (Constructors.TryGetValue(slotType, out Func<BasePropEditControlContent> func)) {
                    return func();
                }

                if (!hasLogged) {
                    hasLogged = true;
                    Debugger.Break();
                    Debug.WriteLine("Could not find control for track type on first try. Scanning base types");
                }
            }

            throw new Exception("No such content control for track type: " + slotType.Name);
        }

        public void Connect(PropertyEditorSlotControl slot) {
            this.Slot = slot;
            this.OnConnected();
        }

        public void Disconnect() {
            this.OnDisconnected();
            this.Slot = null;
        }

        protected abstract void OnConnected();

        protected abstract void OnDisconnected();

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