using System.Windows;
using System.Windows.Controls;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Shortcuts.WPF.Converters;

namespace FramePFX.AdvancedMenuService.WPF {
    public class AdvancedCommandMenuItem : MenuItem {
        public static readonly DependencyProperty CommandIdProperty = DependencyProperty.Register("CommandId", typeof(string), typeof(AdvancedCommandMenuItem), new PropertyMetadata(null));

        public string CommandId {
            get => (string) this.GetValue(CommandIdProperty);
            set => this.SetValue(CommandIdProperty, value);
        }

        private bool canExecute;

        protected bool CanExecute {
            get => this.canExecute;
            set {
                this.canExecute = value;

                // Causes IsEnableCore to be fetched, which returns false if we are executing something or
                // we have no valid command, causing this menu item to be "disabled"
                this.CoerceValue(IsEnabledProperty);
            }
        }

        protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

        private IDataContext loadedDataContext;

        public AdvancedCommandMenuItem() {
            this.Click += this.OnClick;
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.loadedDataContext = ContextCapturingMenu.GetCapturedContextData(this) ?? DataManager.EvaluateContextData(this);
            string id = this.CommandId;
            if (string.IsNullOrWhiteSpace(id))
                id = null;

            this.CanExecute = id != null && CommandManager.Instance.CanExecute(id, this.loadedDataContext);
            if (this.CanExecute) {
                if (CommandIdToGestureConverter.CommandIdToGesture(id, null, out string value)) {
                    this.SetCurrentValue(InputGestureTextProperty, value);
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            this.loadedDataContext = null;
        }

        private void OnClick(object sender, RoutedEventArgs e) {
            if (this.loadedDataContext != null && this.CommandId is string commandId) {
                CommandManager.Instance.Execute(commandId, this.loadedDataContext);
            }
        }
    }
}