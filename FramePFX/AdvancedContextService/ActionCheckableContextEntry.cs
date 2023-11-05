using System.Collections.Generic;
using FramePFX.Actions;
using FramePFX.Utils;

namespace FramePFX.AdvancedContextService {
    public class ActionCheckableContextEntry : ActionContextEntry {
        public ActionCheckableContextEntry(object dataContext, string actionId, string header, string description, IEnumerable<IContextEntry> children = null) : base(actionId, header, description, children) {
        }

        public ActionCheckableContextEntry(object dataContext, string actionId, string header, IEnumerable<IContextEntry> children = null) : base(actionId, header, children) {
        }

        public ActionCheckableContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : base(actionId, children) {
        }

        public ActionCheckableContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) : base(children) {
        }
    }
}