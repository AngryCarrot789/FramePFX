using System.Windows;
using System.Windows.Threading;
using MCNBTViewer.Core.Actions;
using MCNBTViewer.Core.AdvancedContextService;

namespace MCNBTViewer.AdvancedContextService {
    public class AdvancedActionMenuItem : AdvancedMenuItem {
        public static readonly DependencyProperty ActionIdProperty = DependencyProperty.Register("ActionId", typeof(string), typeof(AdvancedActionMenuItem), new PropertyMetadata(null));
        public static readonly DependencyProperty InvokeActionAfterCommandProperty = DependencyProperty.Register("InvokeActionAfterCommand", typeof(bool), typeof(AdvancedActionMenuItem), new PropertyMetadata(default(bool)));

        public string ActionId {
            get => (string) this.GetValue(ActionIdProperty);
            set => this.SetValue(ActionIdProperty, value);
        }

        public bool InvokeActionAfterCommand {
            get => (bool) this.GetValue(InvokeActionAfterCommandProperty);
            set => this.SetValue(InvokeActionAfterCommandProperty, value);
        }

        private volatile bool isExecuting;

        public AdvancedActionMenuItem() {

        }

        protected override void OnClick() {
            if (this.isExecuting) {
                return;
            }

            string id = this.ActionId;
            if (string.IsNullOrEmpty(id)) {
                base.OnClick();
                return;
            }

            if (this.InvokeActionAfterCommand) {
                base.OnClick();
                this.DispatchAction(id);
            }
            else {
                this.DispatchAction(id);
                base.OnClick();
            }
        }

        protected virtual void DispatchAction(string id) {
            if (this.isExecuting) {
                return;
            }

            this.isExecuting = true;
            this.Dispatcher.InvokeAsync(async () => {
                try {
                    await ActionManager.Instance.Execute(id, ((ActionContextEntry) this.DataContext).DataContext);
                }
                finally {
                    this.isExecuting = false;
                }
            }, DispatcherPriority.Render);
        }
    }
}