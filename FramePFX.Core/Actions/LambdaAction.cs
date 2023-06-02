using System;
using System.Threading.Tasks;

namespace FrameControlEx.Core.Actions {
    public class LambdaAction : AnAction {
        public Func<AnActionEventArgs, Task<bool>> MyAction { get; }

        public Func<AnActionEventArgs, Presentation> MyGetPresentation { get; }

        public LambdaAction(Func<AnActionEventArgs, Task<bool>> action, Func<AnActionEventArgs, Presentation> getPresentation) : base() {
            this.MyAction = action ?? throw new ArgumentNullException(nameof(action), "Action function cannot be null");
            this.MyGetPresentation = getPresentation;
        }

        public override Task<bool> ExecuteAsync(AnActionEventArgs e) {
            return this.MyAction(e);
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            return this.MyGetPresentation != null ? this.MyGetPresentation(e) : base.GetPresentation(e);
        }

        public static AnAction Lambda(Func<AnActionEventArgs, Task<bool>> action) {
            return new LambdaAction(action, null);
        }

        public static AnAction LambdaEx(Func<AnActionEventArgs, Task<bool>> action, Func<AnActionEventArgs, Presentation> getPresentation) {
            return new LambdaAction(action, getPresentation);
        }

        public static AnAction LambdaForContext<T>(Func<T, Task<bool>> action) {
            return Lambda(GetLambdaExecutor(action));
        }

        public static AnAction LambdaForContextEx<T>(Func<T, Task<bool>> action, Func<T, Presentation> presentation) {
            return LambdaForContextEx(action, presentation, Presentation.VisibleAndDisabled);
        }

        public static AnAction LambdaForContextEx<T>(Func<T, Task<bool>> action, Func<T, Presentation> presentation, Presentation noContextAvailable) {
            return LambdaEx(GetLambdaExecutor(action), GetLambdaPresentator(presentation, noContextAvailable));
        }

        public static AnAction LambdaI18N(Func<AnActionEventArgs, Task<bool>> action) {
            return new LambdaAction(action, null);
        }

        public static AnAction LambdaI18NEx(Func<AnActionEventArgs, Task<bool>> action, Func<AnActionEventArgs, Presentation> getPresentation) {
            return new LambdaAction(action, getPresentation);
        }

        public static AnAction LambdaForContextI18N<T>(Func<T, Task<bool>> action) {
            return LambdaI18N(GetLambdaExecutor(action));
        }

        public static AnAction LambdaForContextI18NEx<T>(Func<T, Task<bool>> action, Func<T, Presentation> presentation) {
            return LambdaForContextI18NEx(action, presentation, Presentation.VisibleAndDisabled);
        }

        public static AnAction LambdaForContextI18NEx<T>(Func<T, Task<bool>> action, Func<T, Presentation> presentation, Presentation noContextAvailable) {
            return LambdaI18NEx(GetLambdaExecutor(action), GetLambdaPresentator(presentation, noContextAvailable));
        }

        private static Func<AnActionEventArgs, Task<bool>> GetLambdaExecutor<T>(Func<T, Task<bool>> action) {
            return async x => {
                if (x.DataContext.TryGetContext(out T context)) {
                    return await action(context);
                }
                else {
                    return false;
                }
            };
        }

        private static Func<AnActionEventArgs, Presentation> GetLambdaPresentator<T>(Func<T, Presentation> action, Presentation noContextAvailable) {
            return x => x.DataContext.TryGetContext(out T editor) ? action(editor) : noContextAvailable;
        }
    }
}