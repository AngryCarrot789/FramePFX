using System;
using System.Threading.Tasks;

namespace FramePFX.Actions {
    public class LambdaAction : ContextAction {
        public Func<ContextActionEventArgs, Task<bool>> ExecuteFunction { get; }

        public Func<ContextActionEventArgs, bool> CanExecuteFunction { get; }

        public LambdaAction(Func<ContextActionEventArgs, Task<bool>> action, Func<ContextActionEventArgs, bool> getPresentation) : base() {
            this.ExecuteFunction = action ?? throw new ArgumentNullException(nameof(action), "Action function cannot be null");
            this.CanExecuteFunction = getPresentation;
        }

        public override Task ExecuteAsync(ContextActionEventArgs e) {
            return this.ExecuteFunction(e);
        }

        public override bool CanExecute(ContextActionEventArgs e) {
            return this.CanExecuteFunction != null ? this.CanExecuteFunction(e) : base.CanExecute(e);
        }

        public static ContextAction Lambda(Func<ContextActionEventArgs, Task<bool>> action) {
            return new LambdaAction(action, null);
        }

        public static ContextAction LambdaEx(Func<ContextActionEventArgs, Task<bool>> action, Func<ContextActionEventArgs, bool> getPresentation) {
            return new LambdaAction(action, getPresentation);
        }

        public static ContextAction LambdaForContext<T>(Func<T, Task<bool>> action) {
            return Lambda(GetLambdaExecutor(action));
        }

        public static ContextAction LambdaForContextEx<T>(Func<T, Task<bool>> action, Func<T, bool> presentation, bool noContextAvailable = false) {
            return LambdaEx(GetLambdaExecutor(action), GetLambdaPresentator(presentation, noContextAvailable));
        }

        public static ContextAction LambdaI18N(Func<ContextActionEventArgs, Task<bool>> action) {
            return new LambdaAction(action, null);
        }

        public static ContextAction LambdaI18NEx(Func<ContextActionEventArgs, Task<bool>> action, Func<ContextActionEventArgs, bool> getPresentation) {
            return new LambdaAction(action, getPresentation);
        }

        public static ContextAction LambdaForContextI18N<T>(Func<T, Task<bool>> action) {
            return LambdaI18N(GetLambdaExecutor(action));
        }

        public static ContextAction LambdaForContextI18NEx<T>(Func<T, Task<bool>> action, Func<T, bool> presentation, bool noContextAvailable = false) {
            return LambdaI18NEx(GetLambdaExecutor(action), GetLambdaPresentator(presentation, noContextAvailable));
        }

        private static Func<ContextActionEventArgs, Task<bool>> GetLambdaExecutor<T>(Func<T, Task<bool>> action) {
            return async x => {
                if (x.DataContext.TryGetContext(out T context)) {
                    return await action(context);
                }
                else {
                    return false;
                }
            };
        }

        private static Func<ContextActionEventArgs, bool> GetLambdaPresentator<T>(Func<T, bool> action, bool noContextAvailable) {
            return x => x.DataContext.TryGetContext(out T editor) ? action(editor) : noContextAvailable;
        }
    }
}