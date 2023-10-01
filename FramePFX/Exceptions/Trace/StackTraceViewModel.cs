using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FramePFX.Exceptions.Trace
{
    public class StackTraceViewModel : BaseViewModel
    {
        private readonly ObservableCollection<StackFrameViewModel> frames;

        public ReadOnlyObservableCollection<StackFrameViewModel> Frames { get; }

        public ExceptionViewModel Exception { get; }

        public StackTrace TheTrace { get; }

        public StackTraceViewModel(ExceptionViewModel exception)
        {
            this.Exception = exception;
            this.TheTrace = new StackTrace(exception.TheException, 0, true);
            this.frames = new ObservableCollection<StackFrameViewModel>();
            this.Frames = new ReadOnlyObservableCollection<StackFrameViewModel>(this.frames);
            this.frames.Add(null);
        }

        public void Load()
        {
            this.frames.Clear();
            StackFrame[] array = this.TheTrace.GetFrames();
            if (array == null)
            {
                return;
            }

            foreach (StackFrame frame in array)
            {
                this.frames.Add(new StackFrameViewModel(this, frame));
            }
        }
    }
}