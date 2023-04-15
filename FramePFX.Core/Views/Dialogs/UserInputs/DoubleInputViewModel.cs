using System.Collections.Generic;
using FramePFX.Core.Views.ViewModels;

namespace FramePFX.Core.Views.Dialogs.UserInputs {
    public class DoubleInputViewModel : BaseConfirmableDialogViewModel, IErrorInfoHandler {
        private string title;
        private string msgA;
        private string msgB;
        private string inputA;
        private string inputB;

        public string Title {
            get => this.title;
            set => this.RaisePropertyChanged(ref this.title, value);
        }

        public string MessageA {
            get => this.msgA;
            set => this.RaisePropertyChanged(ref this.msgA, value);
        }

        public string MessageB {
            get => this.msgB;
            set => this.RaisePropertyChanged(ref this.msgB, value);
        }

        public string InputA {
            get => this.inputA;
            set => this.RaisePropertyChanged(ref this.inputA, value);
        }

        public string InputB {
            get => this.inputB;
            set => this.RaisePropertyChanged(ref this.inputB, value);
        }

        public InputValidator ValidateInputA { get; set; }
        public InputValidator ValidateInputB { get; set; }

        public DoubleInputViewModel() {

        }

        public void OnErrorsUpdated(Dictionary<string, object> errors) {
            this.ConfirmCommand.IsEnabled = errors.Count < 1;
        }
    }
}
