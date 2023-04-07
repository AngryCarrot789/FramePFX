using System.Collections.Generic;
using FramePFX.Core.AdvancedContextService.Base;

namespace FramePFX.Core.AdvancedContextService {
    /// <summary>
    /// The default implementation for a context entry (aka menu item), which also supports modifying the header,
    /// input gesture text, command and command parameter to reflect the UI menu item
    /// </summary>
    public class ActionContextEntry : ContextEntry, IContextEntry {
        private string actionId;
        public string ActionId {
            get => this.actionId;
            set => this.RaisePropertyChanged(ref this.actionId, value);
        }

        public ActionContextEntry(IEnumerable<IContextEntry> children = null) : base(children) {
        }

        public ActionContextEntry(string actionId, IEnumerable<IContextEntry> children = null) : base(children) {
            this.actionId = actionId;
        }

        public ActionContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : base(dataContext, children) {
            this.actionId = actionId;
        }

        public ActionContextEntry(object dataContext, string header, string actionId, IEnumerable<IContextEntry> children = null) : base(dataContext, header, children) {
            this.actionId = actionId;
        }

        public ActionContextEntry(object dataContext, string header, string actionId, string inputGestureText, IEnumerable<IContextEntry> children = null) : base(dataContext, header, inputGestureText, children) {
            this.actionId = actionId;
        }

        public ActionContextEntry(object dataContext, string header, string actionId, string inputGestureText, string toolTip, IEnumerable<IContextEntry> children = null) : base(dataContext, header, inputGestureText, toolTip, children) {
            this.actionId = actionId;
        }
    }
}