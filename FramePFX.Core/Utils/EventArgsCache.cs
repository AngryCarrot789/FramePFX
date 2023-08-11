using System.Collections.Specialized;
using System.ComponentModel;

namespace FramePFX.Core.Utils {
    internal static class EventArgsCache {
        // fun fact, mscorlib stands for multi-language standard common object runtime library
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
        internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
    }
}