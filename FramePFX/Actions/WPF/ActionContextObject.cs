using System.Windows;

namespace FramePFX.Actions.WPF {
    public class ActionContextObject : ActionContextProviderBase {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ActionContextObject), new PropertyMetadata(null));

        public object Value {
            get => this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        public ActionContextObject() {
        }

        protected override Freezable CreateInstanceCore() => new ActionContextObject();
    }
}