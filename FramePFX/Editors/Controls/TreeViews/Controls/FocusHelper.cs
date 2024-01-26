using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace FramePFX.Editors.Controls.TreeViews.Controls {
    /// <summary>
    /// Helper methods to focus.
    /// </summary>
    public static class FocusHelper {
        #region Public methods

        public static void Focus(EditTextBox element) {
            //System.Diagnostics.Debug.WriteLine("Focus textbox with helper:" + element.Text);
            FocusCore(element);
            element.BringIntoView();
        }

        public static void Focus(MultiSelectTreeViewItem element, bool bringIntoView = false) {
            //System.Diagnostics.Debug.WriteLine("FocusHelper focusing " + (bringIntoView ? "[into view] " : "") + element);
            FocusCore(element);
            if (bringIntoView) {
                FrameworkElement itemContent = (FrameworkElement) element.Template.FindName("headerBorder", element);
                if (itemContent != null) // May not be rendered yet...
                {
                    itemContent.BringIntoView();
                }
            }
        }

        public static void Focus(MultiSelectTreeView element) {
            //System.Diagnostics.Debug.WriteLine("Focus Tree with helper");
            FocusCore(element);
            element.BringIntoView();
        }

        private static void FocusCore(FrameworkElement element) {
            //System.Diagnostics.Debug.WriteLine("Focusing element " + element.ToString());
            //System.Diagnostics.Debug.WriteLine(Environment.StackTrace);
            if (!element.Focus()) {
                //System.Diagnostics.Debug.WriteLine("- Element could not be focused, invoking in dispatcher thread");
                element.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => element.Focus()));
            }

#if DEBUG
            // no good idea, seems to block sometimes
            int i = 0;
            while (i < 5) {
                if (element.IsFocused) {
                    return;
                }

                Thread.Sleep(20);
                i++;
            }
#endif
        }

        #endregion Public methods
    }
}