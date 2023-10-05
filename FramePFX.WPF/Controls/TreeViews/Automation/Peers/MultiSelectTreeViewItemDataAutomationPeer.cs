using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace FramePFX.WPF.Controls.TreeViews.Automation.Peers {
    public class MultiSelectTreeViewItemDataAutomationPeer :
        ItemAutomationPeer,
        ISelectionItemProvider,
        IScrollItemProvider,
        IExpandCollapseProvider,
        IValueProvider {
        #region Properties

        private MultiSelectTreeViewItemAutomationPeer ItemPeer {
            get {
                AutomationPeer automationPeer = null;
                UIElement wrapper = this.GetWrapper();
                if (wrapper != null) {
                    automationPeer = UIElementAutomationPeer.CreatePeerForElement(wrapper);
                    if (automationPeer == null) {
                        if (wrapper is FrameworkElement) {
                            automationPeer = new FrameworkElementAutomationPeer((FrameworkElement) wrapper);
                        }
                        else {
                            automationPeer = new UIElementAutomationPeer(wrapper);
                        }
                    }
                }

                MultiSelectTreeViewItemAutomationPeer treeViewItemAutomationPeer = automationPeer as MultiSelectTreeViewItemAutomationPeer;

                if (treeViewItemAutomationPeer == null) {
                    throw new InvalidOperationException("Could not find parent automation peer.");
                }

                return treeViewItemAutomationPeer;
            }
        }

        #endregion Properties

        #region Constructor

        public MultiSelectTreeViewItemDataAutomationPeer(object item, ItemsControlAutomationPeer itemsControlAutomationPeer) : base(item, itemsControlAutomationPeer) {
        }

        #endregion Constructor

        #region Public methods

        public override object GetPattern(PatternInterface patternInterface) {
            if (patternInterface == PatternInterface.ExpandCollapse) {
                return this;
            }

            if (patternInterface == PatternInterface.SelectionItem) {
                return this;
            }

            if (patternInterface == PatternInterface.ScrollItem) {
                return this;
            }

            if (patternInterface == PatternInterface.Value) {
                return this;
            }

            if (patternInterface == PatternInterface.ItemContainer
                || patternInterface == PatternInterface.SynchronizedInput) {
                return this.ItemPeer;
            }

            return base.GetPattern(patternInterface);
        }

        #endregion Public methods

        #region Explicit interface properties

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState {
            get {
                return this.ItemPeer.ExpandCollapseState;
            }
        }

        bool ISelectionItemProvider.IsSelected {
            get {
                return ((ISelectionItemProvider) this.ItemPeer).IsSelected;
            }
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer {
            get {
                // TreeViewItemAutomationPeer treeViewItemAutomationPeer = GetWrapperPeer() as TreeViewItemAutomationPeer;
                // if (treeViewItemAutomationPeer != null)
                // {
                // ISelectionItemProvider selectionItemProvider = treeViewItemAutomationPeer;
                // return selectionItemProvider.SelectionContainer;
                // }

                // this.ThrowElementNotAvailableException();
                return null;
            }
        }

        #endregion Explicit interface properties

        #region Explicit interface methods

        void IExpandCollapseProvider.Collapse() {
            this.ItemPeer.Collapse();
        }

        void IExpandCollapseProvider.Expand() {
            this.ItemPeer.Expand();
        }

        void IScrollItemProvider.ScrollIntoView() {
            ((IScrollItemProvider) this.ItemPeer).ScrollIntoView();
        }

        void ISelectionItemProvider.AddToSelection() {
            ((ISelectionItemProvider) this.ItemPeer).AddToSelection();
        }

        void ISelectionItemProvider.RemoveFromSelection() {
            ((ISelectionItemProvider) this.ItemPeer).RemoveFromSelection();
        }

        void ISelectionItemProvider.Select() {
            ((ISelectionItemProvider) this.ItemPeer).Select();
        }

        #endregion Explicit interface methods

        #region Methods

        protected override AutomationControlType GetAutomationControlTypeCore() {
            return AutomationControlType.TreeItem;
        }

        protected override string GetClassNameCore() {
            return "TreeViewItem";
        }

        private UIElement GetWrapper() {
            UIElement result = null;
            ItemsControlAutomationPeer itemsControlAutomationPeer = this.ItemsControlAutomationPeer;
            if (itemsControlAutomationPeer != null) {
                ItemsControl itemsControl = (ItemsControl) itemsControlAutomationPeer.Owner;
                if (itemsControl != null) {
                    result = itemsControl.ItemContainerGenerator.ContainerFromItem(this.Item) as UIElement;
                }
            }

            return result;
        }

        #endregion Methods

        #region IValueProvider members

        bool IValueProvider.IsReadOnly {
            get { return ((IValueProvider) this.ItemPeer).IsReadOnly; }
        }

        void IValueProvider.SetValue(string value) {
            ((IValueProvider) this.ItemPeer).SetValue(value);
        }

        string IValueProvider.Value {
            get { return ((IValueProvider) this.ItemPeer).Value; }
        }

        #endregion IValueProvider members
    }
}