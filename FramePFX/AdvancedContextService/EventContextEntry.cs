using System;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// The class for action-based context entries. The header, tooltip, etc, are automatically fetched
    /// </summary>
    public class EventContextEntry : BaseContextEntry {
        public Action<IDataContext> Action { get; set; }

        public EventContextEntry(string header, string description = null) : base(header, description) {

        }

        public EventContextEntry(Action<IDataContext> action, string header, string description = null) : base(header, description) {
            this.Action = action;
        }
    }
}