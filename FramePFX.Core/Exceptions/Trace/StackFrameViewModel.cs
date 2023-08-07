using System.Diagnostics;
using System.Reflection;

namespace FramePFX.Core.Exceptions.Trace
{
    public class StackFrameViewModel : BaseViewModel
    {
        public StackTraceViewModel StackTrace { get; }

        public StackFrame TheFrame { get; }

        public int FileColumnNumber { get; }

        public int FileLineNumber { get; }

        public string FileName { get; }

        public int ILOffset { get; }

        public MethodBase Method { get; }

        public int NativeOffset { get; }

        public StackFrameViewModel(StackTraceViewModel stackTrace, StackFrame frame)
        {
            this.StackTrace = stackTrace;
            this.TheFrame = frame;
            this.FileColumnNumber = frame.GetFileColumnNumber();
            this.FileLineNumber = frame.GetFileLineNumber();
            this.FileName = frame.GetFileName();
            this.ILOffset = frame.GetILOffset();
            this.Method = frame.GetMethod();
            this.NativeOffset = frame.GetNativeOffset();
        }
    }
}