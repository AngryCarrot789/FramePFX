using System;
using System.Collections.ObjectModel;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Exceptions {
    public class ExceptionStackViewModel : BaseViewModel {
        public ObservableCollection<ExceptionViewModel> Exceptions { get; }

        public ExceptionStackViewModel(ErrorList stack) {
            this.Exceptions = new ObservableCollection<ExceptionViewModel>();
            foreach (Exception exception in stack) {
                this.Exceptions.Add(new ExceptionViewModel(null, exception, false));
            }
        }
    }
}