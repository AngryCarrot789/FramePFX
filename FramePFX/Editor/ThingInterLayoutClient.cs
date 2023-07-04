using System;
using System.Windows;
using System.Windows.Data;
using Dragablz;

namespace FramePFX.Editor {
    public class ThingInterLayoutClient : IInterLayoutClient {
        public INewTabHost<UIElement> GetNewHost(object partition, TabablzControl source) {
            TabablzControl tabablzControl = new TabablzControl {DataContext = source.DataContext};
            if (source.InterTabController == null)
                throw new InvalidOperationException("Source tab does not have an InterTabCOntroller set.  Ensure this is set on initial, and subsequently generated tab controls.");

            InterTabController newInterTabController = new InterTabController {
                Partition = source.InterTabController.Partition,
                InterTabClient = source.InterTabController.InterTabClient
            };

            tabablzControl.SetCurrentValue(TabablzControl.InterTabControllerProperty, newInterTabController);
            return new NewTabHost<UIElement>(tabablzControl, tabablzControl);
        }
    }
}