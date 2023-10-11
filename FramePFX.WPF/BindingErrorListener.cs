using System.Diagnostics;
using System.Text;
using FramePFX.Logger;

namespace FramePFX.WPF {
    // https://stackoverflow.com/questions/4225867/how-can-i-turn-binding-errors-into-runtime-exceptions
    public class BindingErrorListener : TraceListener {
        private readonly StringBuilder sb;

        public BindingErrorListener() {
            this.sb = new StringBuilder();
        }

        public static void Listen() {
            PresentationTraceSources.DataBindingSource.Listeners.Add(new BindingErrorListener());
        }


        public override void Write(string message) {
            if (string.IsNullOrEmpty(message)) {
                return;
            }

            int index = message.IndexOf('\n');
            if (index == -1) {
                this.sb.Append(message);
            }
            else {
                this.sb.Append(message, 0, index);
                AppLogger.WriteLine($"[{nameof(BindingErrorListener)}] {this.sb}");
                this.sb.Clear();
                int j = index + 1;
                if (j < message.Length) {
                    this.sb.Append(message, j, message.Length - (j));
                }
            }
        }

        public override void WriteLine(string message) {
            message = this.sb + message;
            this.sb.Clear();
            AppLogger.WriteLine($"[{nameof(BindingErrorListener)}] {message}");
        }
    }
}