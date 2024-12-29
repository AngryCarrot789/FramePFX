//
// Copyright (c) 2024-2024 REghZy
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

using FramePFX.Utils.Collections.Observable;
using FramePFX.Utils.RDA;

namespace FramePFX.Logging;

/// <summary>
/// A thread-safe logger that contains a list of log entries. Logged entries are temporarily stored 
/// </summary>
public class AppLogger {
    public static AppLogger Instance { get; } = new AppLogger();

    private readonly List<LogEntry> cachedEntries;
    private readonly ObservableList<LogEntry> entries;
    private readonly RateLimitedDispatchAction delayedFlush;

    public ReadOnlyObservableList<LogEntry> Entries { get; }

    public AppLogger() {
        this.entries = new ObservableList<LogEntry>();
        this.Entries = new ReadOnlyObservableList<LogEntry>(this.entries);

        ObservableItemProcessor.MakeSimple(this.entries, (e) => {
            string text = $"[{e.LogTime:hh:mm:ss}] {e.Content}";
            Console.WriteLine(text);
            System.Diagnostics.Debug.WriteLine(text);
        }, null);

        // We use a delayed flushing mechanism in order to reduce complete UI stall if
        // some random thread is dumping 10000s of log entries into the UI.
        this.cachedEntries = new List<LogEntry>();
        this.delayedFlush = new RateLimitedDispatchAction(this.FlushEntries, TimeSpan.FromMilliseconds(50)) { DebugName = nameof(AppLogger) };
    }

    static AppLogger() {
    }

    private static bool CanonicalizeLine(ref string line) {
        int strlen;
        if (string.IsNullOrEmpty(line))
            return false;

        if (line[(strlen = line.Length) - 1] == '\n') {
            int offset = (strlen > 1 && line[strlen - 2] == '\r' ? 2 : 1);
            line = line.Substring(0, strlen - offset);
        }

        line = line.Trim();
        return true;
    }

    public void WriteLine(string line) {
        if (!CanonicalizeLine(ref line))
            return;

        LogEntry entry = new LogEntry(DateTime.Now, Environment.StackTrace, line);
        lock (this.cachedEntries)
            this.cachedEntries.Add(entry);

        this.delayedFlush.InvokeAsync();
    }

    /// <summary>
    /// Flushes cached entries to our <see cref="entries"/> collection
    /// </summary>
    /// <returns></returns>
    public Task FlushEntries() {
        return Application.Instance.Dispatcher.InvokeAsync(async () => {
            LogEntry[] items;
            lock (this.cachedEntries) {
                items = this.cachedEntries.ToArray();
                this.cachedEntries.Clear();
            }

            int count = items.Length;

            // count = 10, entryCount = 495, excess = 505
            const int EntryLimit = 500;
            int excess = this.entries.Count + count;
            if (excess > EntryLimit) // remove (505-500)=5
                this.entries.RemoveRange(0, excess - EntryLimit);

            // Dispatch in chunks to give the UI some responsiveness
            const int ChunkSize = 10;
            this.entries.AddSpanRange(items.AsSpan().Slice(0, Math.Min(ChunkSize, count)));
            for (int i = ChunkSize; i < count; i += ChunkSize) {
                int j = Math.Min(i + ChunkSize, count);
                await Application.Instance.Dispatcher.InvokeAsync(() => {
                    this.entries.AddSpanRange(items.AsSpan().Slice(i, j - i));
                }, DispatchPriority.INTERNAL_AfterRender);
            }
        }, DispatchPriority.INTERNAL_BeforeRender).Unwrap();
    }
}