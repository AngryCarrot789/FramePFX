using System;
using System.Collections.Generic;
using System.Windows.Documents;
using FrameControlEx.Core.Utils;
using TextRange = FrameControlEx.Core.Utils.TextRange;

namespace FrameControlEx.Highlighting {
    public static class InlineHelper {
        public static IEnumerable<Run> CreateHighlight(string text, IEnumerable<TextRange> ranges, Func<string, Run> normalRunProvider, Func<string, Run> highlightedRunProvider) {
            int lastIndex = 0;
            foreach (TextRange range in ranges) {
                if ((range.Index - lastIndex) > 0) {
                    yield return normalRunProvider(text.JSubstring(lastIndex, range.Index));
                }

                yield return highlightedRunProvider(range.GetString(text));
                lastIndex = range.EndIndex;
            }

            if (lastIndex < text.Length) {
                yield return normalRunProvider(text.Substring(lastIndex));
            }
        }
    }
}