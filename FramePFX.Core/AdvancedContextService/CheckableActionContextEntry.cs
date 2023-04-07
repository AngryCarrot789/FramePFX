using System.Collections.Generic;
using FramePFX.Core.AdvancedContextService.Base;

namespace FramePFX.Core.AdvancedContextService {
    public class CheckableActionContextEntry : ActionContextEntry {
        private bool isChecked;
        public bool IsChecked {
            get => this.isChecked;
            set => this.RaisePropertyChanged(ref this.isChecked, value);
        }

        public CheckableActionContextEntry(IEnumerable<IContextEntry> children = null) : base(children) {

        }

        public CheckableActionContextEntry(string actionId, IEnumerable<IContextEntry> children = null) : base(actionId, children) {

        }

        public CheckableActionContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : base(dataContext, actionId, children) {

        }

        public CheckableActionContextEntry(object dataContext, string header, string actionId, IEnumerable<IContextEntry> children = null) : base(dataContext, header, actionId, children) {

        }

        public CheckableActionContextEntry(object dataContext, string header, string actionId, string inputGestureText, IEnumerable<IContextEntry> children = null) : base(dataContext, header, actionId, inputGestureText, children) {

        }

        public CheckableActionContextEntry(object dataContext, string header, string actionId, string inputGestureText, string toolTip, IEnumerable<IContextEntry> children = null) : base(dataContext, header, actionId, inputGestureText, toolTip, children) {

        }
    }
}