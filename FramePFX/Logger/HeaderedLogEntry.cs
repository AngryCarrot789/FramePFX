// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;

namespace FramePFX.Logger
{
    public delegate void LogEntryAddedEventHandler(HeaderedLogEntry sender, LogEntry entry);

    public delegate void HeaderedLogEntryEventHandler(HeaderedLogEntry sender);

    public class HeaderedLogEntry : LogEntry
    {
        private readonly List<LogEntry> entries;
        private bool isExpanded;

        public IReadOnlyList<LogEntry> Entries { get; }

        public bool IsExpanded {
            get => this.isExpanded;
            set
            {
                if (this.isExpanded == value)
                    return;
                this.isExpanded = value;
                this.IsExpandedChanged?.Invoke(this);
            }
        }

        public event LogEntryAddedEventHandler EntryAdded;
        public event HeaderedLogEntryEventHandler IsExpandedChanged;

        public HeaderedLogEntry(DateTime logTime, int index, string stackTrace, string content) : base(logTime, index, stackTrace, content)
        {
            this.entries = new List<LogEntry>();
            this.Entries = this.entries;
            this.IsExpanded = true;
        }

        public void AddEntry(LogEntry entry)
        {
            this.entries.Add(entry);
            this.EntryAdded?.Invoke(this, entry);
        }
    }
}