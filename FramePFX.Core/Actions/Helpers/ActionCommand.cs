using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FramePFX.Core.Actions.Helpers {
    public class ActionCommand<T> : AnAction {
        public Type TargetType { get; }

        public string PropertyName { get; }

        public PropertyInfo Property { get; }

        public ActionCommand(string propertyName) : base() {
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

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out T instance)) {
                return false;
            }

            ICommand cmd = this.GetCommand(instance);
            if (cmd == null || !cmd.CanExecute(null)) {
                return false;
            }

            if (cmd is BaseAsyncRelayCommand asyncCmd) {
                await asyncCmd.ExecuteAsync(null);
            }
            else {
                cmd.Execute(null);
            }

            return true;
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out T instance)) {
                return Presentation.Invisible;
            }

            ICommand cmd = this.GetCommand(instance);
            if (cmd == null || !cmd.CanExecute(null)) {
                return Presentation.VisibleAndDisabled;
            }

            return Presentation.VisibleAndEnabled;
        }
    }
}