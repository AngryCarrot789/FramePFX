using System;

namespace FramePFX.Core {
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
            parameter = ImplicitConvertParameter<T>(parameter, this.ConvertParameter);
            return base.CanExecute(parameter) && (this.canExecute == null || (parameter == null || parameter is T) && this.canExecute((T) parameter));
        }

        public override void Execute(object parameter) {
            parameter = ImplicitConvertParameter<T>(parameter, this.ConvertParameter);
            if (parameter == null || parameter is T) {
                this.execute((T) parameter);
            }
            else {
                throw new InvalidCastException($"Parameter type ({parameter.GetType()}) cannot be used for the callback method (which requires type {typeof(T).Name})");
            }
        }
    }
}