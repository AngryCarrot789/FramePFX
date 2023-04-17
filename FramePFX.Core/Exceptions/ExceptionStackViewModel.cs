using System;
using System.Collections.ObjectModel;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Exceptions {
    public class ExceptionStackViewModel : BaseViewModel {
        public ObservableCollection<ExceptionViewModel> Exceptions { get; }

        public ExceptionStackViewModel(ExceptionStack stack) {
            this.Exceptions = new ObservableCollection<ExceptionViewModel>();
            foreach (Exception exception in stack.Exceptions) {
                this.Exceptions.Add(new ExceptionViewModel(null, exception, false));
            }
        }
    }
}