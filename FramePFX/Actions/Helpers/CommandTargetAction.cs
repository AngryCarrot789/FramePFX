using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using FramePFX.Commands;

namespace FramePFX.Actions.Helpers {
    public class CommandTargetAction<T> : ContextAction {
        public Type TargetType { get; }

        public string PropertyName { get; }

        public PropertyInfo Property { get; }

        public CommandTargetAction(string propertyName) {
            this.PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            this.TargetType = typeof(T);
            this.Property = this.TargetType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (this.Property == null) {
                throw new Exception($"No such property: {this.TargetType}.{propertyName}");
            }
        }

        public ICommand GetCommand(object instance) {
            return this.Property.GetValue(instance) as ICommand;
        }

        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out T instance)) {
                return;
            }

            ICommand cmd = this.GetCommand(instance);
            if (cmd == null || !cmd.CanExecute(null)) {
                return;
            }

            if (cmd is BaseAsyncRelayCommand asyncCmd) {
                await asyncCmd.ExecuteAsync(null);
            }
            else {
                cmd.Execute(null);
            }
        }

        public override bool CanExecute(ContextActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out T instance)) {
                return false;
            }

            ICommand cmd = this.GetCommand(instance);
            return cmd != null && cmd.CanExecute(null);
        }
    }
}