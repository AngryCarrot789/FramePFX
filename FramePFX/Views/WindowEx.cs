using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using FramePFX.Utils;

namespace FramePFX.Views {
    /// <summary>
    /// An extended window which adds support for a few of the things in the dark theme I made (e.g. Titlebar brush)
    /// </summary>
    public class WindowEx : Window {
        public static readonly DependencyProperty TitlebarBrushProperty = DependencyProperty.Register("TitlebarBrush", typeof(Brush), typeof(WindowEx));
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

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        private bool isHandlingSyncClosing;
        private bool isHandlingAsyncClose;
        private bool? closeEventResult;

        private readonly Action showAction;
        private readonly Func<bool?> showDialogAction;

        private readonly WindowInteropHelper helper;
        private readonly EventHandler SingleContentRenderHandler;
        private readonly RoutedEventHandler SingleLoadedHandler;

        public WindowEx() : base() {
            this.showAction = this.Show;
            this.showDialogAction = this.ShowDialog;
            this.helper = new WindowInteropHelper(this);
            this.SingleContentRenderHandler = (sender, args) => {
                WinDarkTheme.UpdateDarkTheme(this.helper.Handle, true);
                this.ContentRendered -= this.SingleContentRenderHandler;
            };

            this.SingleLoadedHandler = (s, e) => {
                WinDarkTheme.UpdateDarkTheme(this.helper.Handle, true);
                this.Loaded -= this.SingleLoadedHandler;
            };

            this.ContentRendered += this.SingleContentRenderHandler;
            this.Loaded += this.SingleLoadedHandler;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
        }

        static WindowEx() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowEx), new FrameworkPropertyMetadata(typeof(WindowEx)));
        }

        public static Window GetCurrentActiveWindow() {
            IntPtr window = GetActiveWindow();
            if (window != IntPtr.Zero) {
                foreach (Window win in Application.Current.Windows) {
                    if (new WindowInteropHelper(win).Handle == window) {
                        return win;
                    }
                }
            }

            return Application.Current.MainWindow;
        }

        public void CalculateOwnerAndSetCentered() {
            Window owner = GetCurrentActiveWindow();
            if (owner != this && owner.Owner != this) {
                this.Owner = owner;
            }

            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
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
                NonClientFrameEdges = NonClientFrameEdges.None,
                UseAeroCaptionButtons = false,
            };

            WindowChrome.SetWindowChrome(this, chrome);
            // WindowChrome.SetWindowChrome(this, null);
            // <Setter Property="WindowChrome.WindowChrome">
            //     <Setter.Value>
            //         <!-- In order to have a window shadow, GlassFrameThickness needs to be non-zero which is annoying -->
            //         <!-- because the glass frame causes this weird flickering white border when resizing :( -->
            //         <!-- Seems like it's the best idea to just put Top to 1 and the rest on 0, because you'll probably notice it less -->
            //         <WindowChrome CaptionHeight="26" ResizeBorderThickness="6" CornerRadius="0" GlassFrameThickness="1 0 0 0"
            //             NonClientFrameEdges="None" UseAeroCaptionButtons="False"/>
            //     </Setter.Value>
            // </Setter>
        }

        protected sealed override void OnClosing(CancelEventArgs e) {
            // check isHandlingSyncClosing for recursive close attempt, even though it shouldn't occur
            // check isHandlingAsyncClose for close attempt in async code (OnClosingInternal dispatches back to AMT)
            if (this.isHandlingSyncClosing || this.isHandlingAsyncClose) {
                return;
            }

            try {
                this.isHandlingSyncClosing = true;
                this.OnClosingInternal(e);

                // closeEventResult is only set if the async state of OnClosingInternal does not require
                // dispatching back to the main thread (no usage of Task.Delay(), no awaiting real async things, etc.).
                // However when it does, cancel the close and let the async code handle the window's closing state (isHandlingAsyncClose becomes true)
                bool? result = Helper.Exchange(ref this.closeEventResult, null);
                if (result.HasValue) {
                    e.Cancel = !result.Value; // true = close, false = do not close
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
                CloseAsync will manually close the window itself because the original OnClosing was cancelled


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
        public async Task<bool> CloseAsync() {
            if (await this.OnClosingAsync()) {
                if (this.isHandlingSyncClosing) {
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
    }
}