using System.Collections.Generic;

namespace FramePFX.AdvancedContextService {
    public class ShortcutActionContextEntry : ActionContextEntry {
        public string ShortcutId { get; set; }

        public ShortcutActionContextEntry(object dataContext, string actionId, string header, string description, IEnumerable<IContextEntry> children = null) : base(actionId, header, description, children) {
        }

        public ShortcutActionContextEntry(object dataContext, string actionId, string header, IEnumerable<IContextEntry> children = null) : base(actionId, header, children) {
        }

        public ShortcutActionContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : base(actionId, children) {
        }

        public ShortcutActionContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) : base(children) {
        }
    }
}