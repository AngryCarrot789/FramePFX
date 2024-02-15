using System.Collections.Generic;
using FramePFX.Utils;

namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// The class for command-based context entries. The header, tooltip, etc, are automatically fetched
    /// </summary>
    public class CommandContextEntry : BaseContextEntry {
        public string CommandId { get; }

        public CommandContextEntry(string commandId, string header, string description, IEnumerable<IContextEntry> children = null) : base(header, description, children) {
            this.CommandId = commandId;
        }

        public CommandContextEntry(string commandId, string header, IEnumerable<IContextEntry> children = null) : this(commandId, header, null, children) {
        }

        public CommandContextEntry(string commandId, IEnumerable<IContextEntry> children = null) : this(commandId, StringUtils.SplitLast(commandId, '.'), null, children) {
        }

        public CommandContextEntry(IEnumerable<IContextEntry> children = null) : this(null, null, null, children) {
        }
    }
}