using System;
using System.Globalization;
using System.Windows.Input;
using FramePFX.Core.Utils;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Core {
    /// <summary>
    /// A base relay command class, that implements ICommand, and also has a simple
    /// implementation for dealing with the <see cref="CanExecuteChanged"/> event handler
    /// </summary>
    public abstract class BaseRelayCommand : ICommand {
        private bool isEnabled;
        public bool IsEnabled {
            get => this.isEnabled;
            set {
                this.isEnabled = value;
                this.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Raised when <see cref="RaiseCanExecuteChanged"/> is called
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of <see cref="BaseRelayCommand"/>
        /// </summary>
        /// <param name="canExecute">The execution status logic</param>
        protected BaseRelayCommand() {
            this.isEnabled = true;
        }

        protected static object ImplicitConvertParameter<T>(object parameter, bool useTypeConversion) {
            Type type = typeof(T);
            if (parameter == null) {
                return default(T);
            }
            else if (type == typeof(string) && parameter is string) {
                return parameter;
            }
            else if (type == typeof(bool) && parameter is string str) {
                if (str.Equals("true", StringComparison.CurrentCultureIgnoreCase)) {
                    return BoolBox.True;
                }
                else if (str.Equals("false", StringComparison.CurrentCultureIgnoreCase)) {
                    return BoolBox.False;
                }
                else {
                    return parameter;
                }
            }
            else if (useTypeConversion && parameter is IConvertible convertible) {
                try {
                    return convertible.ToType(type, CultureInfo.CurrentCulture);
                }
                catch {
                    return parameter;
                }
            }
            else {
                return parameter;
            }
        }

        public abstract void Execute(object parameter);

        /// <summary>
        /// Determines whether this <see cref="BaseRelayCommand"/> can execute in its current state
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. This may be null, if the command does not require a parameter
        /// </param>
        /// <returns>
        /// True if the command can be executed, otherwise false if it cannot be executed
        /// </returns>
        public virtual bool CanExecute(object parameter) {
            return this.IsEnabled;
        }

        /// <summary>
        /// Method used to raise the <see cref="CanExecuteChanged"/> event to indicate that the
        /// return value of the <see cref="CanExecute"/> method likely changed
        /// <para>
        /// This can be called from a view model, which, for example, may cause a
        /// button to become greyed out (disabled) if <see cref="CanExecute"/> returns false
        /// </para>
        /// </summary>
        public virtual void RaiseCanExecuteChanged() {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}