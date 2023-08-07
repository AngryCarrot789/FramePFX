using System.Windows;

namespace FramePFX.Views
{
    /// <summary>
    /// Interaction logic for AppSplashScreen.xaml
    /// </summary>
    public partial class AppSplashScreen : Window
    {
        public string CurrentActivity
        {
            get => this.CurrentActivityTextBlock.Text;
            set => this.CurrentActivityTextBlock.Text = value;
        }

        public AppSplashScreen()
        {
            this.InitializeComponent();
        }
    }
}