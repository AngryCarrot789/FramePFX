using System.Windows;
using System.Windows.Controls;
using FramePFX.Commands;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.AdvancedMenuService.WPF {
    public class AdvancedCommandMenuItem : MenuItem {
        public static readonly DependencyProperty CommandIdProperty = DependencyProperty.Register("CommandId", typeof(string), typeof(AdvancedCommandMenuItem), new PropertyMetadata(null));

        public string CommandId {
            get => (string) this.GetValue(CommandIdProperty);
            set => this.SetValue(CommandIdProperty, value);
        }

        private DataContext loadedDataContext;

        public AdvancedCommandMenuItem() {
            this.Click += this.OnClick;
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.loadedDataContext = DataManager.GetDataContext(this);
            this.IsEnabled = this.CommandId is string id && CommandManager.Instance.CanExecute(id, this.loadedDataContext);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            this.loadedDataContext = null;
        }

        private void OnClick(object sender, RoutedEventArgs e) {
            if (this.CommandId is string commandId) {
                DataContext dataContext = this.loadedDataContext ?? DataManager.GetDataContext(this);
                CommandManager.Instance.Execute(commandId, dataContext);
            }
        }
    }
}