/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012 Yves Goergen, Goroll
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR
 * A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace FramePFX.Editors.Controls.TreeViews.Controls
{
    /// <summary>
    /// Helper methods to focus.
    /// </summary>
    public static class FocusHelper
    {
#region Public methods

        public static void Focus(EditTextBox element)
        {
            //System.Diagnostics.Debug.WriteLine("Focus textbox with helper:" + element.Text);
            FocusCore(element);
            element.BringIntoView();
        }

        public static void Focus(MultiSelectTreeViewItem element, bool bringIntoView = false)
        {
            //System.Diagnostics.Debug.WriteLine("FocusHelper focusing " + (bringIntoView ? "[into view] " : "") + element);
            FocusCore(element);
            if (bringIntoView)
            {
                FrameworkElement itemContent = (FrameworkElement) element.Template.FindName("headerBorder", element);
                if (itemContent != null) // May not be rendered yet...
                {
                    itemContent.BringIntoView();
                }
            }
        }

        public static void Focus(MultiSelectTreeView element)
        {
            //System.Diagnostics.Debug.WriteLine("Focus Tree with helper");
            FocusCore(element);
            element.BringIntoView();
        }

        private static void FocusCore(FrameworkElement element)
        {
            //System.Diagnostics.Debug.WriteLine("Focusing element " + element.ToString());
            //System.Diagnostics.Debug.WriteLine(Environment.StackTrace);
            if (!element.Focus())
            {
                //System.Diagnostics.Debug.WriteLine("- Element could not be focused, invoking in dispatcher thread");
                element.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => element.Focus()));
            }

#if DEBUG
            // no good idea, seems to block sometimes
            int i = 0;
            while (i < 5)
            {
                if (element.IsFocused)
                {
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