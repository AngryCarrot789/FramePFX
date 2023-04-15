using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FramePFX.Core.Actions;

namespace FramePFX.Views {
    /// <summary>
    /// An extended window which adds support for a few of the things in the dark theme I made (e.g. Titlebar brush)
    /// </summary>
    public class WindowEx : Window {
        public static readonly DependencyProperty TitlebarBrushProperty = DependencyProperty.Register("TitlebarBrush", typeof(Brush), typeof(WindowEx), new PropertyMetadata());
        public static readonly DependencyProperty CanCloseWithActionProperty = DependencyProperty.Register("CanCloseWithAction", typeof(bool), typeof(WindowEx), new PropertyMetadata(true));

        [Category("Brush")]
        public Brush TitlebarBrush {
            get => (Brush) this.GetValue(TitlebarBrushProperty);
            set => this.SetValue(TitlebarBrushProperty, value);
        }

        public bool CanCloseWithAction {
            get => (bool) this.GetValue(CanCloseWithActionProperty);
            set => this.SetValue(CanCloseWithActionProperty, value);
        }

        private bool isInRegularClosingHandler;
        private bool isHandlingAsyncClose;

        public WindowEx() {

        }

        protected sealed override void OnClosing(CancelEventArgs e) {
            if (this.isInRegularClosingHandler || this.isHandlingAsyncClose) {
                return;
            }

            try {
                this.isInRegularClosingHandler = true;
                this.OnClosingInternal(e);
            }
            finally {
                this.isInRegularClosingHandler = false;
            }
        }

        private async void OnClosingInternal(CancelEventArgs e) {
            e.Cancel = true;
            if (await this.CloseAsync()) {
                e.Cancel = false;
            }
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        /// <returns>Whether the window was closed or not</returns>
        public async Task<bool> CloseAsync() {
            if (this.Dispatcher.CheckAccess()) {
                return await this.CloseAsyncInternal();
            }
            else {
                return await await this.Dispatcher.InvokeAsync(this.CloseAsyncInternal);
            }
        }

        private async Task<bool> CloseAsyncInternal() {
            if (await this.OnClosingAsync()) {
                if (this.isInRegularClosingHandler) {
                    return true;
                }

                try {
                    this.isHandlingAsyncClose = true;
                    this.Close();
                    return true;
                }
                finally {
                    this.isHandlingAsyncClose = false;
                }
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Called when the window is trying to be closed
        /// </summary>
        /// <returns>True if the window can close, otherwise false to stop it from closing</returns>
        public virtual Task<bool> OnClosingAsync() {
            return Task.FromResult(true);
        }

        [ActionRegistration("actions.views.CloseViewAction")]
        private class CloseViewAction : AnAction {
            public CloseViewAction() : base(() => "Close window", () => "Closes the current window") {

            }

            public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
                if (e.DataContext.TryGetContext(out WindowEx w) && w.CanCloseWithAction) {
                    await w.CloseAsync();
                    return true;
                }

                return false;
            }

            public override Presentation GetPresentation(AnActionEventArgs e) {
                return Presentation.BoolToEnabled(e.DataContext.TryGetContext<WindowEx>(out _));
            }
        }

        [ActionRegistration("actions.views.MakeWindowTopMost")]
        private class MakeTopMostAction : ToggleAction {
            public MakeTopMostAction() : base(() => "Make window top-most", () => "Makes the window top most, so that non-top-most windows cannot be on top of it") {

            }

            public override Task<bool> OnToggled(AnActionEventArgs e, bool isToggled) {
                if (e.DataContext.TryGetContext(out WindowEx window)) {
                    window.Topmost = isToggled;
                    return Task.FromResult(true);
                }
                else {
                    return Task.FromResult(false);
                }
            }

            public override Task<bool> ExecuteNoToggle(AnActionEventArgs e) {
                if (e.DataContext.TryGetContext(out WindowEx window)) {
                    window.Topmost = !window.Topmost;
                    return Task.FromResult(true);
                }
                else {
                    return Task.FromResult(false);
                }
            }

            public override Presentation GetPresentation(AnActionEventArgs e) {
                return Presentation.BoolToEnabled(e.DataContext.TryGetContext<WindowEx>(out _));
            }
        }
    }
}