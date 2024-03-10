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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using FramePFX.Editors.Controls.TreeViews.Controls;

namespace FramePFX.Editors.Controls.TreeViews.Automation.Peers
{
    /// <summary>
    /// Powers UI-Automation for <see cref="MultiSelectTreeViewItem"/> types
    /// </summary>
    public class MultiSelectTreeViewItemAutomationPeer :
        ItemsControlAutomationPeer,
        IExpandCollapseProvider,
        ISelectionItemProvider,
        IScrollItemProvider,
        IValueProvider,
        IInvokeProvider
    {
        #region Public properties

        public ExpandCollapseState ExpandCollapseState
        {
            get
            {
                MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem) this.Owner;
                if (!treeViewItem.HasItems)
                {
                    return ExpandCollapseState.LeafNode;
                }

                if (!treeViewItem.IsExpanded)
                {
                    return ExpandCollapseState.Collapsed;
                }

                return ExpandCollapseState.Expanded;
            }
        }

        #endregion Public properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSelectTreeViewItemAutomationPeer"/>
        /// class.
        /// </summary>
        /// <param name="owner">
        /// Das <see cref="T:FramePFX.WPF.Controls.TreeViews.Controls.MultiSelectTreeViewItem"/>, das diesem
        /// <see cref="T:FramePFX.Editors.Controls.TreeViews.Automation.Peers.MultiSelectTreeViewItemAutomationPeer"/>
        /// zugeordnet ist.
        /// </param>
        public MultiSelectTreeViewItemAutomationPeer(MultiSelectTreeViewItem owner) : base(owner)
        {
        }

        #endregion Constructor

        #region IInvokeProvider members

        public void Invoke()
        {
            ((MultiSelectTreeViewItem) this.Owner).InvokeMouseDown();
        }

        #endregion IInvokeProvider members

        protected override Rect GetBoundingRectangleCore()
        {
            MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem) this.Owner;
            ContentPresenter contentPresenter = GetContentPresenter(treeViewItem);
            if (contentPresenter != null)
            {
                Vector offset = VisualTreeHelper.GetOffset(contentPresenter);
                Point p = new Point(offset.X, offset.Y);
                p = contentPresenter.PointToScreen(p);
                return new Rect(p.X, p.Y, contentPresenter.ActualWidth, contentPresenter.ActualHeight);
            }

            return base.GetBoundingRectangleCore();
        }

        protected override Point GetClickablePointCore()
        {
            MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem) this.Owner;
            ContentPresenter contentPresenter = GetContentPresenter(treeViewItem);
            if (contentPresenter != null)
            {
                Vector offset = VisualTreeHelper.GetOffset(contentPresenter);
                Point p = new Point(offset.X, offset.Y);
                p = contentPresenter.PointToScreen(p);
                return p;
            }

            return base.GetClickablePointCore();
        }

        private static ContentPresenter GetContentPresenter(MultiSelectTreeViewItem treeViewItem)
        {
            ContentPresenter contentPresenter = treeViewItem.Template.FindName("PART_Header", treeViewItem) as ContentPresenter;
            return contentPresenter;
        }

        /// <summary>
        /// Overridden because original wpf tree does show the expander button and the contents of the
        /// header as children, too. That was requested by the users.
        /// </summary>
        /// <returns>Returns a list of children.</returns>
        protected override List<AutomationPeer> GetChildrenCore()
        {
            //System.Diagnostics.Trace.WriteLine("MultiSelectTreeViewItemAutomationPeer.GetChildrenCore()");
            MultiSelectTreeViewItem owner = (MultiSelectTreeViewItem) this.Owner;

            List<AutomationPeer> children = new List<AutomationPeer>();
            ToggleButton button = owner.Template.FindName("Expander", owner) as ToggleButton;
            AddAutomationPeer(children, button);
            //System.Diagnostics.Trace.WriteLine("- Adding ToggleButton, " + (button == null ? "IS" : "is NOT") + " null, now " + children.Count + " items");

            ContentPresenter contentPresenter = GetContentPresenter(owner);

            if (contentPresenter != null)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(contentPresenter);
                for (int i = 0; i < childrenCount; i++)
                {
                    UIElement child = VisualTreeHelper.GetChild(contentPresenter, i) as UIElement;
                    AddAutomationPeer(children, child);
                    //System.Diagnostics.Trace.WriteLine("- Adding child UIElement, " + (child == null ? "IS" : "is NOT") + " null, now " + children.Count + " items");
                }
            }

            ItemCollection items = owner.Items;
            for (int i = 0; i < items.Count; i++)
            {
                MultiSelectTreeViewItem treeViewItem = owner.ItemContainerGenerator.ContainerFromIndex(i) as MultiSelectTreeViewItem;
                AddAutomationPeer(children, treeViewItem);
                //System.Diagnostics.Trace.WriteLine("- Adding MultiSelectTreeViewItem, " + (treeViewItem == null ? "IS" : "is NOT") + " null, now " + children.Count + " items");
            }

            if (children.Count > 0)
            {
                //System.Diagnostics.Trace.WriteLine("MultiSelectTreeViewItemAutomationPeer.GetChildrenCore(): returning " + children.Count + " children");
                //for (int i = 0; i < children.Count; i++)
                //{
                //    System.Diagnostics.Trace.WriteLine("- Item " + i + " " + (children[i] == null ? "IS" : "is NOT") + " null");
                //}
                return children;
            }

            //System.Diagnostics.Trace.WriteLine("MultiSelectTreeViewItemAutomationPeer.GetChildrenCore(): returning null");
            return null;
        }

        private static void AddAutomationPeer(List<AutomationPeer> children, UIElement child)
        {
            if (child != null)
            {
                AutomationPeer peer = FromElement(child);
                if (peer == null)
                {
                    peer = CreatePeerForElement(child);
                }

                if (peer != null)
                {
                    // In the array that GetChildrenCore returns, which is used by AutomationPeer.EnsureChildren,
                    // no null entries are allowed or a NullReferenceException will be thrown from the guts of WPF.
                    // This has reproducibly been observed null on certain systems so the null check was added.
                    // This may mean that some child controls are missing for automation, but at least the
                    // application doesn't crash in normal usage.
                    children.Add(peer);
                }
            }
        }

        #region Explicit interface properties

        bool ISelectionItemProvider.IsSelected
        {
            get
            {
                return ((MultiSelectTreeViewItem) this.Owner).IsSelected;
            }
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get
            {
                ItemsControl parentItemsControl = ((MultiSelectTreeViewItem) this.Owner).ParentTreeView;
                if (parentItemsControl != null)
                {
                    AutomationPeer automationPeer = FromElement(parentItemsControl);
                    if (automationPeer != null)
                    {
                        return this.ProviderFromPeer(automationPeer);
                    }
                }

                return null;
            }
        }

        #endregion Explicit interface properties

        #region Public methods

        public void Collapse()
        {
            if (!this.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem) this.Owner;
            if (!treeViewItem.HasItems)
            {
                throw new InvalidOperationException("Cannot collapse because item has no children.");
            }

            treeViewItem.IsExpanded = false;
        }

        public void Expand()
        {
            if (!this.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem) this.Owner;
            if (!treeViewItem.HasItems)
            {
                throw new InvalidOperationException("Cannot expand because item has no children.");
            }

            treeViewItem.IsExpanded = true;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse)
            {
                return this;
            }

            if (patternInterface == PatternInterface.SelectionItem)
            {
                return this;
            }

            if (patternInterface == PatternInterface.ScrollItem)
            {
                return this;
            }

            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        #endregion Public methods

        #region Explicit interface methods

        void IScrollItemProvider.ScrollIntoView()
        {
            ((MultiSelectTreeViewItem) this.Owner).BringIntoView();
        }

        void ISelectionItemProvider.AddToSelection()
        {
            throw new NotImplementedException();
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            throw new NotImplementedException();
        }

        void ISelectionItemProvider.Select()
        {
            ((MultiSelectTreeViewItem) this.Owner).ParentTreeView.Selection.SelectCore((MultiSelectTreeViewItem) this.Owner);
        }

        #endregion Explicit interface methods

        #region Methods

        protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
        {
            return new MultiSelectTreeViewItemDataAutomationPeer(item, this);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.TreeItem;
        }

        protected override string GetClassNameCore()
        {
            return "MultiSelectTreeViewItem";
        }

        #endregion Methods

        #region IValueProvider members

        public bool IsReadOnly
        {
            get { return false; }
        }

        string requestedValue;

        public void SetValue(string value)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(value))
                    return;

                string[] ids = value.Split(new[] {';'});

                object obj;
                if (ids.Length > 0 && ids[0] == "Context")
                {
                    MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem) this.Owner;
                    obj = treeViewItem.DataContext;
                }
                else
                {
                    obj = this.Owner;
                }

                if (ids.Length < 2)
                {
                    this.requestedValue = obj.ToString();
                }
                else
                {
                    Type type = obj.GetType();
                    PropertyInfo pi = type.GetProperty(ids[1]);
                    this.requestedValue = pi.GetValue(obj, null).ToString();
                }
            }
            catch (Exception ex)
            {
                this.requestedValue = ex.ToString();
            }
        }

        public string Value
        {
            get
            {
                if (this.requestedValue == null)
                {
                    MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem) this.Owner;
                    return treeViewItem.DataContext.ToString();
                }

                return this.requestedValue;
            }
        }

        #endregion IValueProvider members
    }
}