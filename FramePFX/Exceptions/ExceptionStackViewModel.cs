using System;
using System.Collections.ObjectModel;
using FramePFX.Utils;

namespace FramePFX.Exceptions
{
    public class ExceptionStackViewModel : BaseViewModel
    {
        public ObservableCollection<ExceptionViewModel> Exceptions { get; }

        public ExceptionStackViewModel(ErrorList list)
        {
            this.Exceptions = new ObservableCollection<ExceptionViewModel>();
            foreach (Exception exception in list)
            {
                this.Exceptions.Add(new ExceptionViewModel(null, exception, false));
            }
        }
    }
}