using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Logger;

namespace FramePFX.Views {
    /// <summary>
    /// Interaction logic for AppSplashScreen.xaml
    /// </summary>
    public partial class AppSplashScreen : Window, IApplicationStartupProgress {
        public string CurrentActivity {
            get => this.CurrentActivityTextBlock.Text;
            set => this.CurrentActivityTextBlock.Text = value;
        }

        public AppSplashScreen() {
            this.InitializeComponent();
        }

        public async Task SetAction(string header, string description) {
            AppLogger.Instance.WriteLine(header);
            this.CurrentActivity = header;
            await this.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        }
    }
}