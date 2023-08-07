using System.Collections;

namespace FramePFX.Core.Exceptions
{
    public class ExceptionDataViewModel : BaseViewModel
    {
        public ExceptionViewModel Exception { get; }

        public IDictionary Data { get; }

        public ExceptionDataViewModel(ExceptionViewModel exception, IDictionary data)
        {
            this.Exception = exception;
            this.Data = data;
        }
    }
}