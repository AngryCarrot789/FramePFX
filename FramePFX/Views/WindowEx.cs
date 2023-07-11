using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using FramePFX.Core.Actions;
using FramePFX.Utils;

namespace FramePFX.Views {
    /// <summary>
    /// An extended window which adds support for a few of the things in the dark theme I made (e.g. Titlebar brush)
    /// </summary>
    public class WindowEx : Window {
        public static readonly DependencyProperty TitlebarBrushProperty = DependencyProperty.Register("TitlebarBrush", typeof(Brush), typeof(WindowEx), new PropertyMetadata());
        public static readonly DependencyProperty CanCloseWithEscapeKeyProperty = DependencyProperty.Register("CanCloseWithEscapeKey", typeof(bool), typeof(WindowEx), new PropertyMetadata(false));

        [Category("Brush")]
        public Brush TitlebarBrush {
            get => (Brush) this.GetValue(TitlebarBrushProperty);
            set => this.SetValue(TitlebarBrushProperty, value);
        }

        public bool CanCloseWithEscapeKey {
            get => (bool) this.GetValue(CanCloseWithEscapeKeyProperty);
            set => this.SetValue(CanCloseWithEscapeKeyProperty, value);
        }

        private bool isHandlingSyncClosing;
        private bool isHandlingAsyncClose;
        private bool? closeEventResult;

        private readonly Action showAction;
        private readonly Func<bool?> showDialogAction;

        public WindowEx() : base() {
            this.showAction = this.Show;
            this.showDialogAction = this.ShowDialog;
        }

        public Task ShowAsync() {
            // Just in case this is called off the main thread
            return DispatcherUtils.InvokeAsync(this.Dispatcher, this.showAction);
        }

        public Task<bool?> ShowDialogAsync() {
            return DispatcherUtils.InvokeAsync(this.Dispatcher, this.showDialogAction);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            WindowChrome chrome = new WindowChrome() {
                CaptionHeight = 26,
                ResizeBorderThickness = new Thickness(6),
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(1, 0, 0, 0),
                NonClientFrameEdges= NonClientFrameEdges.None,
                UseAeroCaptionButtons = false,
            };

            WindowChrome.SetWindowChrome(this, chrome);

            // <Setter Property="WindowChrome.WindowChrome">
            //     <Setter.Value>
            //         <!-- In order to have a window shadow, GlassFrameThickness needs to be non-zero which is annoying -->
            //         <!-- because the glass frame causes this weird flickering white border when resizing :( -->
            //         <!-- Seems like it's the best idea to just put Top to 1 and the rest on 0, because you'll probably notice it less -->
            //         <WindowChrome CaptionHeight="26" ResizeBorderThickness="6" CornerRadius="0" GlassFrameThickness="1 0 0 0"
            //             NonClientFrameEdges="None" UseAeroCaptionButtons="False" />
            //     </Setter.Value>
            // </Setter>
        }

        protected sealed override void OnClosing(CancelEventArgs e) {
            if (this.isHandlingSyncClosing || this.isHandlingAsyncClose) {
                return;
            }

            try {
                this.isHandlingSyncClosing = true;
                this.OnClosingInternal(e);
                if (this.closeEventResult.HasValue) {
                    try { // try finally juuust in case...
                        e.Cancel = !this.closeEventResult.Value; // true = close, false = do not close
                    }
                    finally {
                        this.closeEventResult = null;
                    }
                }
                else {
                    e.Cancel = true;
                }
            }
            finally {
                this.isHandlingSyncClosing = false;
            }
        }

        /*
            async void is required here
            OnClosing is fired, that sets isHandlingSyncClosing to true and invokes this method which awaits CloseAsync()

            During the invocation of CloseAsync, If the call does not require
            real async (e.g. does not use Task.Delay() or whatever):
                CloseAsync will return in the same execution context as OnClosing, meaning isHandlingSyncClosing
                stays true, and OnClosing can access closeEventResult and set the e.Cancel accordingly

            However, if the call chain in CloseAsync uses Task.Delay() or something which returns
            a task that is incomplete by the time the async state machine comes to actually "awaiting" it,
            then the behaviour changes:
                OnClosing returns before CloseAsync is completed, setting isHandlingSyncClosing to false, meaning that
                CloseAsyncInternal will manually close the window itself because the original OnClosing was cancelled


         */
        private async void OnClosingInternal(CancelEventArgs e) {
            bool result = await this.CloseAsync();
            if (this.isHandlingSyncClosing) {
                this.closeEventResult = result;
            }
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        /// <returns>Whether the window was closed or not</returns>
        public Task<bool> CloseAsync() {
            // return await await Task.Run(async () => await DispatcherUtils.InvokeAsync(this.Dispatcher, this.CloseAsyncInternal));
            return DispatcherUtils.Invoke(this.Dispatcher, this.CloseAsyncInternal);
        }

        private async Task<bool> CloseAsyncInternal() {
            if (await this.OnClosingAsync()) {
                if (!this.isHandlingSyncClosing) {
                    try {
                        this.isHandlingAsyncClose = true;
                        await DispatcherUtils.InvokeAsync(this.Dispatcher, this.Close);
                        return true;
                    }
                    finally {
                        this.isHandlingAsyncClose = false;
                    }
                }

                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Called when the window is trying to be closed
        /// </summary>
        /// <returns>True if the window can close, otherwise false to stop it from closing</returns>
        protected virtual Task<bool> OnClosingAsync() {
            return Task.FromResult(true);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            base.OnPreviewKeyDown(e);
            if (e.Handled) {
                return;
            }

            if (e.Key == Key.Escape && this.CanCloseWithEscapeKey) {
                e.Handled = true;
                this.Close();
            }
        }

        // [ActionRegistration("actions.views.windows.CloseViewAction")]
        // private class CloseViewAction : AnAction {
        //     public CloseViewAction() : base(() => "Close window", () => "Closes the current window") {
        //     }
        //     public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
        //         if (e.DataContext.TryGetContext(out WindowEx w) && w.CanCloseWithEscapeKey) {
        //             await w.CloseAsync();
        //             return true;
        //         }
        //         return false;
        //     }
        //     public override Presentation GetPresentation(AnActionEventArgs e) {
        //         return Presentation.BoolToEnabled(e.DataContext.TryGetContext<WindowEx>(out _));
        //     }
        // }


        // Binding a checkbox to the window's Topmost property is more effective and works both ways
        [ActionRegistration("actions.views.MakeWindowTopMost")]
        private class MakeTopMostAction : ToggleAction {
            public MakeTopMostAction() {

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

            public override bool CanExecute(AnActionEventArgs e) {
                return e.DataContext.TryGetContext<WindowEx>(out _);
            }
        }
    }
}