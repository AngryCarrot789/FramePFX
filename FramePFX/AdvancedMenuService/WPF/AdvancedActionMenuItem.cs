using System.Windows;
using System.Windows.Controls;
using FramePFX.Actions;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Shortcuts.WPF;

namespace FramePFX.AdvancedMenuService.WPF {
    public class AdvancedActionMenuItem : MenuItem {
        public static readonly DependencyProperty ActionIdProperty = DependencyProperty.Register("ActionId", typeof(string), typeof(AdvancedActionMenuItem), new PropertyMetadata(null));

        public string ActionId {
            get => (string) this.GetValue(ActionIdProperty);
            set => this.SetValue(ActionIdProperty, value);
        }

        private DataContext loadedDataContext;

        public AdvancedActionMenuItem() {
            this.Click += this.OnClick;
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.loadedDataContext = UIInputManager.GetDataContext(this);
            this.IsEnabled = this.ActionId is string id && ActionManager.Instance.CanExecute(id, this.loadedDataContext);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            this.loadedDataContext = null;
        }

        private void OnClick(object sender, RoutedEventArgs e) {
            if (this.ActionId is string actionId) {
                DataContext dataContext = this.loadedDataContext ?? UIInputManager.GetDataContext(this);
                ActionManager.Instance.Execute(actionId, dataContext);
            }
        }
    }
}