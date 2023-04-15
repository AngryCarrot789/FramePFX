using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.Core.Actions;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.Utils;

namespace FramePFX.AdvancedContextService {
    public class AdvancedActionMenuItem : AdvancedMenuItem {
        public static readonly DependencyProperty ActionIdProperty =
            DependencyProperty.Register(
                "ActionId",
                typeof(string),
                typeof(AdvancedActionMenuItem),
                new FrameworkPropertyMetadata(null, (d, e) => ((AdvancedActionMenuItem) d).UpdateCanExecute()));
        public static readonly DependencyProperty InvokeActionAfterBaseClickProperty =
            DependencyProperty.Register(
                "InvokeActionAfterBaseClick",
                typeof(bool),
                typeof(AdvancedActionMenuItem),
                new PropertyMetadata(BoolBox.True));

        public string ActionId {
            get => (string) this.GetValue(ActionIdProperty);
            set => this.SetValue(ActionIdProperty, value);
        }

        public bool InvokeActionAfterBaseClick {
            get => (bool) this.GetValue(InvokeActionAfterBaseClickProperty);
            set => this.SetValue(InvokeActionAfterBaseClickProperty, value.Box());
        }

        private volatile int isExecuting; // 1 == true, 0 == false

        public bool IsExecuting => this.isExecuting == 1;

        private bool canExecute;
        protected bool CanExecute {
            get => this.canExecute;
            set {
                this.canExecute = value;

                // Causes IsEnableCore to be fetched, which returns false if we are executing something or
                // we have no valid action, causing this menu item to be "disabled"
                this.CoerceValue(IsEnabledProperty);
            }
        }

        public AdvancedActionMenuItem() {

        }

        protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

        protected DefaultDataContext GetDataContext(bool includeToggleState = true) {
            DefaultDataContext context = new DefaultDataContext();
            object dc = this.DataContext;
            if (dc != null) {
                if (dc is IDataContext ctx) {
                    context.Merge(ctx);
                }
                else {
                    context.AddContext(dc);
                }
            }

            context.AddContext(this);
            IInputElement focused = Keyboard.FocusedElement;
            if (!ReferenceEquals(focused, this)) {
                context.AddContext(focused);
            }

            if (Window.GetWindow(this) is Window win) {
                context.AddContext(win);
            }

            if (includeToggleState && this.IsCheckable) {
                context.Set(ToggleAction.IsToggledKey, this.IsChecked.Box());
            }

            return context;
        }

        protected bool GetCanExecute() {
            if (this.isExecuting == 1) {
                return false;
            }

            string id = this.ActionId;
            if (string.IsNullOrEmpty(id)) {
                return false;
            }

            DefaultDataContext context = this.GetDataContext();
            return this.GetCanExecute(ActionManager.Instance.GetPresentation(id, context));
        }

        protected virtual bool GetCanExecute(Presentation presentation) {
            return presentation.IsEnabled;
        }

        public void UpdateCanExecute() {
            this.CanExecute = this.GetCanExecute();
        }

        protected override void OnClick() {
            // Originally used a binding to bind this menu item's command to an ActionContextEntry's
            // internal command, but you lose the ability to access Keyboard.FocusedElement, so it's
            // better to just handle the click manually
            // context should not be an instance of CommandContextEntry... but just in case
            // if (this.DataContext is CommandContextEntry || this.DataContext is ActionContextEntry) {
            //     base.OnClick(); // clicking is handled in the entry
            //     return;
            // }

            if (Interlocked.CompareExchange(ref this.isExecuting, 1, 0) == 1) {
                this.CanExecute = false;
                return;
            }

            string id = this.ActionId;
            if (string.IsNullOrEmpty(id)) {
                base.OnClick();
                this.isExecuting = 0;
                this.CanExecute = false;
                return;
            }

            this.CanExecute = false;
            if (this.InvokeActionAfterBaseClick) { // true by default, and ToggleActions would break if this was false
                base.OnClick();
                this.DispatchAction(id);
            }
            else {
                this.DispatchAction(id);
                base.OnClick();
            }
        }

        private void DispatchAction(string id) {
            DefaultDataContext context = this.GetDataContext();
            if (this.IsCheckable) {
                context.Set(ToggleAction.IsToggledKey, this.IsChecked.Box());
            }

            this.Dispatcher.InvokeAsync(async () => {
                try {
                    await ActionManager.Instance.Execute(id, context);
                }
                finally {
                    this.isExecuting = 0;
                    this.UpdateCanExecute();
                }
            }, DispatcherPriority.Render);
        }
    }
}