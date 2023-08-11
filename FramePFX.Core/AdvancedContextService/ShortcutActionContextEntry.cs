using System.Collections.Generic;

namespace FramePFX.Core.AdvancedContextService {
    public class ShortcutActionContextEntry : ActionContextEntry {
        private string shortcutId;

        public string ShortcutId {
            get => this.shortcutId;
            set => this.RaisePropertyChanged(ref this.shortcutId, value);
        }

        public ShortcutActionContextEntry(object dataContext, string actionId, string header, string description, IEnumerable<IContextEntry> children = null) : base(dataContext, actionId, header, description, children) {
        }

        public ShortcutActionContextEntry(object dataContext, string actionId, string header, IEnumerable<IContextEntry> children = null) : base(dataContext, actionId, header, children) {
        }

        public ShortcutActionContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : base(dataContext, actionId, children) {
        }

        public ShortcutActionContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) : base(dataContext, children) {
        }
    }
}