using System;
using System.Collections.ObjectModel;

namespace FramePFX.Logger
{
    public class HeaderedLogEntry : LogEntry
    {
        public ObservableCollection<LogEntry> Entries { get; }

        public bool IsExpanded { get; set; }

        public HeaderedLogEntry(DateTime logTime, int index, string stackTrace, string content) : base(logTime, index, stackTrace, content)
        {
            this.Entries = new ObservableCollection<LogEntry>();
            this.IsExpanded = true;
        }
    }
}