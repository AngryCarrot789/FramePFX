namespace FramePFX.Core.Views.Dialogs.UserInputs
{
    public class SingleInputViewModel : BaseConfirmableDialogViewModel
    {
        private string title;

        public string Title
        {
            get => this.title;
            set => this.RaisePropertyChanged(ref this.title, value);
        }

        private string message;

        public string Message
        {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        private string input;

        public string Input
        {
            get => this.input;
            set => this.RaisePropertyChanged(ref this.input, value);
        }

        public InputValidator ValidateInput { get; set; }

        public SingleInputViewModel()
        {
        }
    }
}