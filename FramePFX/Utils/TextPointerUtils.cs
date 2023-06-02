using System.Windows.Documents;

namespace FrameControlEx.Utils {
    public static class TextPointerUtils {
        public static TextPointer GetLineBegin(TextPointer caret) => caret.GetLineStartPosition(0) ?? caret.DocumentStart;

        public static TextPointer GetLineEnd(TextPointer caret) {
            TextPointer nextLine = caret.GetLineStartPosition(1);
            if (nextLine == null) {
                return caret.DocumentEnd;
            }

            TextPointer lineEnd = nextLine.GetNextContextPosition(LogicalDirection.Backward) ?? caret.DocumentEnd;
            if (lineEnd.CompareTo(nextLine) >= 0) {
                return lineEnd;
            }

            return nextLine;
        }
    }
}