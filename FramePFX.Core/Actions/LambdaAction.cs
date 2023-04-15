using System;
using System.Threading.Tasks;

namespace FramePFX.Core.Actions {
    public class LambdaAction : AnAction {
        public Func<AnActionEventArgs, Task<bool>> MyAction { get; }

        public Func<AnActionEventArgs, Presentation> MyGetPresentation { get; }

        public LambdaAction(Func<string> header, Func<string> description, Func<AnActionEventArgs, Task<bool>> action, Func<AnActionEventArgs, Presentation> getPresentation) : base(header, description) {
            this.MyAction = action ?? throw new ArgumentNullException(nameof(action), "Action function cannot be null");
            this.MyGetPresentation = getPresentation;
        }

        public override Task<bool> ExecuteAsync(AnActionEventArgs e) {
            return this.MyAction(e);
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            return this.MyGetPresentation != null ? this.MyGetPresentation(e) : base.GetPresentation(e);
        }

        public static AnAction Lambda(Func<AnActionEventArgs, Task<bool>> action, string header = null, string description = null) {
            return new LambdaAction(() => header, () => description, action, null);
        }

        public static AnAction LambdaEx(Func<AnActionEventArgs, Task<bool>> action, Func<AnActionEventArgs, Presentation> getPresentation, string header = null, string description = null) {
            return new LambdaAction(() => header, () => description, action, getPresentation);
        }

        public static AnAction LambdaForContext<T>(Func<T, Task<bool>> action, string header = null, string description = null) {
            return Lambda(GetLambdaExecutor(action), header, description);
        }

        public static AnAction LambdaForContextEx<T>(Func<T, Task<bool>> action, Func<T, Presentation> presentation, string header = null, string description = null) {
            return LambdaForContextEx(action, presentation, Presentation.VisibleAndDisabled, header, description);
        }

        public static AnAction LambdaForContextEx<T>(Func<T, Task<bool>> action, Func<T, Presentation> presentation, Presentation noContextAvailable, string header = null, string description = null) {
            return LambdaEx(GetLambdaExecutor(action), GetLambdaPresentator(presentation, noContextAvailable), header, description);
        }

        public static AnAction LambdaI18N(Func<AnActionEventArgs, Task<bool>> action, Func<string> header = null, Func<string> description = null) {
            return new LambdaAction(header, description, action, null);
        }

        public static AnAction LambdaI18NEx(Func<AnActionEventArgs, Task<bool>> action, Func<AnActionEventArgs, Presentation> getPresentation, Func<string> header = null, Func<string> description = null) {
            return new LambdaAction(header, description, action, getPresentation);
        }

        public static AnAction LambdaForContextI18N<T>(Func<T, Task<bool>> action, Func<string> header = null, Func<string> description = null) {
            return LambdaI18N(GetLambdaExecutor(action), header, description);
        }

        public static AnAction LambdaForContextI18NEx<T>(Func<T, Task<bool>> action, Func<T, Presentation> presentation, Func<string> header = null, Func<string> description = null) {
            return LambdaForContextI18NEx(action, presentation, Presentation.VisibleAndDisabled, header, description);
        }

        public static AnAction LambdaForContextI18NEx<T>(Func<T, Task<bool>> action, Func<T, Presentation> presentation, Presentation noContextAvailable, Func<string> header = null, Func<string> description = null) {
            return LambdaI18NEx(GetLambdaExecutor(action), GetLambdaPresentator(presentation, noContextAvailable), header, description);
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