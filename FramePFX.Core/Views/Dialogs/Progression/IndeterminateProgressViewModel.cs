using System.Threading.Tasks;
using FramePFX.Core.Views.Windows;

namespace FramePFX.Core.Views.Dialogs.Progression
{
    public class IndeterminateProgressViewModel : BaseViewModel
    {
        public IWindow Window { get; set; }

        public AsyncRelayCommand CancelCommand { get; }

        public AsyncRelayCommand ForceCompleteCommand { get; }

        public bool CanForceCompletion { get; }

        public bool WasForcedCompletion { get; set; }

        private string titlebar;
        private string message;

        public string Titlebar
        {
            get => this.titlebar;
            set => this.RaisePropertyChanged(ref this.titlebar, value);
        }

        public string Message
        {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        public bool IsCancelled => !this.Window.IsOpen;

        public IndeterminateProgressViewModel(bool canForceCompletion)
        {
            this.CanForceCompletion = canForceCompletion;
            this.CancelCommand = new AsyncRelayCommand(this.CancelAction);
            this.ForceCompleteCommand = new AsyncRelayCommand(this.ForceCompleteAction, () => canForceCompletion);
        }

        public Task CancelAction()
        {
            return this.Window.CloseWindowAsync();
        }

        public async Task ForceCompleteAction()
        {
            if (this.CanForceCompletion)
            {
                this.WasForcedCompletion = true;
                await this.Window.CloseWindowAsync();
            }
            else
            {
                await IoC.MessageDialogs.ShowMessageAsync("Cannot force", "Cannot force progress completion");
            }
        }
    }
}