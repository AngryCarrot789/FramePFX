using System.Diagnostics;

namespace FramePFX.WPF
{
    // https://stackoverflow.com/questions/4225867/how-can-i-turn-binding-errors-into-runtime-exceptions
    public class BindingErrorListener : TraceListener
    {
        public static void Listen()
        {
            PresentationTraceSources.DataBindingSource.Listeners.Add(new BindingErrorListener());
        }

        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
            // Debugger.Break();
        }
    }
}