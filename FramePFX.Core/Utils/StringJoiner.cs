using System.Text;

namespace FramePFX.Core.Utils {
    public class StringJoiner {
        private readonly StringBuilder sb;
        private readonly string delimiter;
        private bool hasFirst;

        public StringJoiner(StringBuilder sb, string delimiter) {
            this.sb = sb;
            this.delimiter = delimiter;
        }

        public void Append(string value) {
            if (this.hasFirst) {
                this.sb.Append(this.delimiter);
            }
            else {
                this.hasFirst = true;
            }

            this.sb.Append(value);
        }

        public override string ToString() {
            return this.sb.ToString();
        }
    }
}