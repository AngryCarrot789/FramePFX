using System;
using System.Threading.Tasks;

namespace FramePFX.Actions {
    public class LambdaAction : ExecutableAction {
        public Func<ActionEventArgs, Task<bool>> ExecuteFunction { get; }

        public Func<ActionEventArgs, bool> CanExecuteFunction { get; }

        public LambdaAction(Func<ActionEventArgs, Task<bool>> action, Func<ActionEventArgs, bool> getPresentation) : base() {
            this.ExecuteFunction = action ?? throw new ArgumentNullException(nameof(action), "Action function cannot be null");
            this.CanExecuteFunction = getPresentation;
        }

        public override Task<bool> ExecuteAsync(ActionEventArgs e) {
            return this.ExecuteFunction(e);
        }

        public override bool CanExecute(ActionEventArgs e) {
            return this.CanExecuteFunction != null ? this.CanExecuteFunction(e) : base.CanExecute(e);
        }

        public static ExecutableAction Lambda(Func<ActionEventArgs, Task<bool>> action) {
            return new LambdaAction(action, null);
        }

        public static ExecutableAction LambdaEx(Func<ActionEventArgs, Task<bool>> action, Func<ActionEventArgs, bool> getPresentation) {
            return new LambdaAction(action, getPresentation);
        }

        public static ExecutableAction LambdaForContext<T>(Func<T, Task<bool>> action) {
            return Lambda(GetLambdaExecutor(action));
        }

        public static ExecutableAction LambdaForContextEx<T>(Func<T, Task<bool>> action, Func<T, bool> presentation, bool noContextAvailable = false) {
            return LambdaEx(GetLambdaExecutor(action), GetLambdaPresentator(presentation, noContextAvailable));
        }

        public static ExecutableAction LambdaI18N(Func<ActionEventArgs, Task<bool>> action) {
            return new LambdaAction(action, null);
        }

        public static ExecutableAction LambdaI18NEx(Func<ActionEventArgs, Task<bool>> action, Func<ActionEventArgs, bool> getPresentation) {
            return new LambdaAction(action, getPresentation);
        }

        public static ExecutableAction LambdaForContextI18N<T>(Func<T, Task<bool>> action) {
            return LambdaI18N(GetLambdaExecutor(action));
        }

        public static ExecutableAction LambdaForContextI18NEx<T>(Func<T, Task<bool>> action, Func<T, bool> presentation, bool noContextAvailable = false) {
            return LambdaI18NEx(GetLambdaExecutor(action), GetLambdaPresentator(presentation, noContextAvailable));
        }

        private static Func<ActionEventArgs, Task<bool>> GetLambdaExecutor<T>(Func<T, Task<bool>> action) {
            return async x => {
                if (x.DataContext.TryGetContext(out T context)) {
                    return await action(context);
                }
                else {
                    return false;
                }
            };
        }

        private static Func<ActionEventArgs, bool> GetLambdaPresentator<T>(Func<T, bool> action, bool noContextAvailable) {
            return x => x.DataContext.TryGetContext(out T editor) ? action(editor) : noContextAvailable;
        }
    }
}