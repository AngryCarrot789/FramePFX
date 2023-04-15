using System.Collections.Generic;
using SharpPadV2.Core.Views.ViewModels;

namespace SharpPadV2.Core.Views.Dialogs.UserInputs {
    public class SingleInputViewModel : BaseConfirmableDialogViewModel, IErrorInfoHandler {
        private string title;
        public string Title {
            get => this.title;
            set => this.RaisePropertyChanged(ref this.title, value);
        }

        private string message;
        public string Message {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        private string input;
        public string Input {
            get => this.input;
            set => this.RaisePropertyChanged(ref this.input, value);
        }

        public InputValidator ValidateInput { get; set; }

        public SingleInputViewModel() {

        }

        public void OnErrorsUpdated(Dictionary<string, object> errors) {
            this.ConfirmCommand.IsEnabled = errors.Count < 1;
        }
    }
}
