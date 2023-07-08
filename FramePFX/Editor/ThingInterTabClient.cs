using System;
using System.Windows;
using System.Windows.Threading;
using Dragablz;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Editor {
    public class ThingInterTabClient : IInterTabClient {
        public Style WindowExStyle { get; set; }

        public DataTemplate WinContentTemplate { get; set; }

        public virtual INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source) {
            Window window = new WindowEx() {
                Style = this.WindowExStyle
            };

            window.Dispatcher.Invoke(() => { }, DispatcherPriority.DataBind);
            window.Height = 400;
            window.Width = 400;

            DependencyObject content = this.WinContentTemplate.LoadContent() ?? throw new ArgumentException("Failed to create control");
            TabablzControl dst = VisualTreeUtils.FindVisualChild<TabablzControl>(content, true) ?? throw new ArgumentException("Failed to find tab control");
            dst.DataContext = source.DataContext;
            window.Content = dst;
            return new NewTabHost<Window>(window, dst);
        }

        public virtual TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window) {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}