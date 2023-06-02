using System;

namespace FrameControlEx.Core {
    /// <summary>
    /// A simple relay command, which does not take any parameters
    /// </summary>
    /// <typeparam name="T">The type of parameter</typeparam>
    public class RelayCommand : BaseRelayCommand {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null) {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) {
            return base.CanExecute(parameter) && (this.canExecute == null || this.canExecute());
        }

        public override void Execute(object parameter) {
            this.execute();
        }
    }

    /// <summary>
    /// A simple relay command, which may take a parameter
    /// </summary>
    /// <typeparam name="T">The type of parameter</typeparam>
    public class RelayCommand<T> : BaseRelayCommand {
        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;

        public bool ConvertParameter { get; set; }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null, bool convertParameter = true) {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
            this.ConvertParameter = convertParameter;
        }

        public override bool CanExecute(object parameter) {
            if (this.ConvertParameter) {
                parameter = GetConvertedParameter<T>(parameter);
            }

            if (base.CanExecute(parameter)) {
                return this.canExecute == null || parameter == null && this.canExecute(default) || parameter is T t && this.canExecute(t);
            }

            return false;
        }

        public override void Execute(object parameter) {
            if (this.ConvertParameter) {
                parameter = GetConvertedParameter<T>(parameter);
            }

            if (parameter == null) {
                this.execute(default);
            }
            else if (parameter is T value) {
                this.execute(value);
            }
            else {
                throw new InvalidCastException($"Parameter type ({parameter.GetType()}) cannot be used for the callback method (which requires type {typeof(T).Name})");
            }
        }
    }
}