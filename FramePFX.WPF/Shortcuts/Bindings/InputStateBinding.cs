using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using FramePFX.Utils;

namespace FramePFX.WPF.Shortcuts.Bindings {
    public class InputStateBinding : Freezable {
        public static readonly DependencyProperty CommandProperty = InputBinding.CommandProperty.AddOwner(typeof(InputStateBinding));
        public static readonly DependencyProperty InputStatePathProperty = DependencyProperty.Register(nameof(InputStatePath), typeof(string), typeof(InputStateBinding));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                "IsActive",
                typeof(bool),
                typeof(InputStateBinding),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    // OneWayToSource or TwoWay is required for binding to work on this property. But
                    // since OneWayToSource isn't available by default, TwoWay will work
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback, CoerceValueCallback));

        private static object CoerceValueCallback(DependencyObject d, object basevalue) {
            return basevalue;
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        }

        /// <summary>
        /// The command to execute when the input state is either activated or deactivated, passing the activation state as a parameter
        /// </summary>
        [Localizability(LocalizationCategory.NeverLocalize)]
        [TypeConverter(typeof(CommandConverter))]
        public ICommand Command {
            get => (ICommand) this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        /// <summary>
        /// The full path of the shortcut that must be activated in order for this binding's command to be executed
        /// </summary>
        public string InputStatePath {
            get => (string) this.GetValue(InputStatePathProperty);
            set => this.SetValue(InputStatePathProperty, value);
        }

        /// <summary>
        /// Whether or not the input state binding (whose full path matches <see cref="InputStatePath"/>) is active or not
        /// </summary>
        public bool IsActive {
            get => (bool) this.GetValue(IsActiveProperty);
            set => this.SetValue(IsActiveProperty, value.Box());
        }

        public InputStateBinding() {
        }

        protected override Freezable CreateInstanceCore() => new InputStateBinding();
    }
}