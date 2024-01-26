using System.Collections.Generic;
using FramePFX.Utils;

namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// The class for action-based context entries. The header, tooltip, etc, are automatically fetched
    /// </summary>
    public class ActionContextEntry : BaseContextEntry {
        public string ActionId { get; }

        public ActionContextEntry(string actionId, string header, string description, IEnumerable<IContextEntry> children = null) : base(header, description, children) {
            this.ActionId = actionId;
        }

        public ActionContextEntry(string actionId, string header, IEnumerable<IContextEntry> children = null) : this(actionId, header, null, children) {
        }

        public ActionContextEntry(string actionId, IEnumerable<IContextEntry> children = null) : this(actionId, StringUtils.SplitLast(actionId, '.'), null, children) {
        }

        public ActionContextEntry(IEnumerable<IContextEntry> children = null) : this(null, null, null, children) {
        }
    }
}