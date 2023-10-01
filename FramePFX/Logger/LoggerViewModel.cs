using System.Collections.ObjectModel;

namespace FramePFX.Logger
{
    public class LoggerViewModel : BaseViewModel
    {
        public ObservableCollection<LogEntry> Entries { get; }

        public LoggerViewModel()
        {
            this.Entries = new ObservableCollection<LogEntry>();
        }

        /// <summary>
        /// Adds an entry to our entry list. This should only be called on the main thread
        /// </summary>
        /// <param name="entry"></param>
        public void AddRoot(LogEntry entry)
        {
            this.Entries.Add(entry);
        }
    }
}