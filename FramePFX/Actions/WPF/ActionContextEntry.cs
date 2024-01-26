using System.Windows;

namespace FramePFX.Actions.WPF {
    public class ActionContextEntry : ActionContextProviderBase {
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(string), typeof(ActionContextEntry), new PropertyMetadata(null));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ActionContextEntry), new PropertyMetadata(null));

        public string Key {
            get => (string) this.GetValue(KeyProperty);
            set => this.SetValue(KeyProperty, value);
        }

        public object Value {
            get => this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        public ActionContextEntry() {
        }

        protected override Freezable CreateInstanceCore() => new ActionContextEntry();
    }
}