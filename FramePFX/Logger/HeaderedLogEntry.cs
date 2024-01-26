using System;
using System.Collections.Generic;

namespace FramePFX.Logger {
    public delegate void LogEntryAddedEventHandler(HeaderedLogEntry sender, LogEntry entry);
    public delegate void HeaderedLogEntryEventHandler(HeaderedLogEntry sender);

    public class HeaderedLogEntry : LogEntry {
        private readonly List<LogEntry> entries;
        private bool isExpanded;

        public IReadOnlyList<LogEntry> Entries { get; }

        public bool IsExpanded {
            get => this.isExpanded;
            set {
                if (this.isExpanded == value)
                    return;
                this.isExpanded = value;
                this.IsExpandedChanged?.Invoke(this);
            }
        }

        public event LogEntryAddedEventHandler EntryAdded;
        public event HeaderedLogEntryEventHandler IsExpandedChanged;

        public HeaderedLogEntry(DateTime logTime, int index, string stackTrace, string content) : base(logTime, index, stackTrace, content) {
            this.entries = new List<LogEntry>();
            this.Entries = this.entries;
            this.IsExpanded = true;
        }

        public void AddEntry(LogEntry entry) {
            this.entries.Add(entry);
            this.EntryAdded?.Invoke(this, entry);
        }
    }
}