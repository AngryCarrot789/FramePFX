using System.Collections.Generic;

namespace FrameControlEx.Core.AdvancedContextService {
    /// <summary>
    /// An entry that simply acts as a grouping element (to group a collection of child entries)
    /// </summary>
    public class GroupContextEntry : BaseContextEntry {
        public GroupContextEntry(object dataContext, string header, string description, IEnumerable<IContextEntry> children = null) : base(dataContext, children) {
            this.Header = header;
            this.Description = description;
        }

        public GroupContextEntry(string header, string description, IEnumerable<IContextEntry> children = null) : this(null, header, description, children) {

        }

        public GroupContextEntry(object dataContext, string header, IEnumerable<IContextEntry> children = null) : this(dataContext, header, null, children) {

        }

        public GroupContextEntry(string header, IEnumerable<IContextEntry> children = null) : this(null, header, null, children) {

        }
    }
}