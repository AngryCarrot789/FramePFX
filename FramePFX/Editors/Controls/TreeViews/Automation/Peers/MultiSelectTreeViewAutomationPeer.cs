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

using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using FramePFX.Editors.Controls.TreeViews.Controls;

namespace FramePFX.Editors.Controls.TreeViews.Automation.Peers
{
    /// <summary>
    /// Powers UI-Automation for <see cref="MultiSelectTreeView"/> types
    /// </summary>
    public class MultiSelectTreeViewAutomationPeer : ItemsControlAutomationPeer, ISelectionProvider
    {
        #region Constructor

        public MultiSelectTreeViewAutomationPeer(MultiSelectTreeView owner) : base(owner)
        {
        }

        #endregion Constructor

        #region Public methods

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Selection)
            {
                return this;
            }

            // if (patternInterface == PatternInterface.Scroll)
            // {
            // ItemsControl itemsControl = (ItemsControl)Owner;
            // if (itemsControl.ScrollHost != null)
            // {
            // AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(itemsControl.ScrollHost);
            // if (automationPeer != null && automationPeer is IScrollProvider)
            // {
            // automationPeer.EventsSource = this;
            // return (IScrollProvider)automationPeer;
            // }
            // }
            // }
            return base.GetPattern(patternInterface);
        }

        #endregion Public methods

        #region Explicit interface methods

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            IRawElementProviderSimple[] array = null;

            // MultiSelectTreeViewItem selectedContainer = ((MultiSelectTreeView) base.Owner).SelectedContainer;
            // if (selectedContainer != null)
            // {
            // AutomationPeer automationPeer = UIElementAutomationPeer.FromElement(selectedContainer);
            // if (automationPeer.EventsSource != null)
            // {
            // automationPeer = automationPeer.EventsSource;
            // }

            // if (automationPeer != null)
            // {
            // array = new[] { this.ProviderFromPeer(automationPeer) };
            // }
            // }

            // if (array == null)
            // {
            // array = new IRawElementProviderSimple[0];
            // }
            return array;
        }

        #endregion Explicit interface methods

        #region Public properties

        public bool CanSelectMultiple
        {
            get
            {
                return false;
            }
        }

        public bool IsSelectionRequired
        {
            get
            {
                return false;
            }
        }

        #endregion Public properties

        #region Methods

        /// <summary>
        /// When overridden in a derived class, creates a new instance of the
        /// <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer"/> for a data item in
        /// the <see cref="P:System.Windows.Controls.ItemsControl.Items"/> collection of this
        /// <see cref="T:System.Windows.Controls.ItemsControl"/>.
        /// </summary>
        /// <param name="item">
        /// The data item that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer"/>.
        /// </param>
        /// <returns>
        /// The new <see cref="T:System.Windows.Automation.Peers.ItemAutomationPeer"/> created.
        /// </returns>
        protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
        {
            return new MultiSelectTreeViewItemDataAutomationPeer(item, this);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Tree;
        }

        protected override string GetClassNameCore()
        {
            return "MultiSelectTreeView";
        }

        #endregion Methods
    }
}