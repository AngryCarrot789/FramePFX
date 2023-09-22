using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using FramePFX.WPF.Controls.TreeViews.Controls;

namespace FramePFX.WPF.Controls.TreeViews.Automation.Peers {
    public class MultiSelectTreeViewAutomationPeer : ItemsControlAutomationPeer, ISelectionProvider {
        #region Constructor

        public MultiSelectTreeViewAutomationPeer(MultiSelectTreeView owner)
            : base(owner) {
        }

        #endregion Constructor

        #region Public methods

        public override object GetPattern(PatternInterface patternInterface) {
            if (patternInterface == PatternInterface.Selection) {
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

        IRawElementProviderSimple[] ISelectionProvider.GetSelection() {
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

        public bool CanSelectMultiple {
            get {
                return false;
            }
        }

        public bool IsSelectionRequired {
            get {
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
        protected override ItemAutomationPeer CreateItemAutomationPeer(object item) {
            return new MultiSelectTreeViewItemDataAutomationPeer(item, this);
        }

        protected override AutomationControlType GetAutomationControlTypeCore() {
            return AutomationControlType.Tree;
        }

        protected override string GetClassNameCore() {
            return "MultiSelectTreeView";
        }

        #endregion Methods
    }
}