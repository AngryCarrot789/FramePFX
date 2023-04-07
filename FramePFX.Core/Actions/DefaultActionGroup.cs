using System.Collections.Generic;

namespace MCNBTViewer.Core.Actions {
    public class DefaultActionGroup : ActionGroup {
        private readonly List<Action> actions;

        public DefaultActionGroup(string header, string description, List<Action> actions) : base(header, description) {
            this.actions = actions;
        }

        public DefaultActionGroup(string header, string description, string inputGestureText, List<Action> actions) : base(header, description, inputGestureText) {
            this.actions = actions;
        }

        public override List<Action> GetChildren() {
            return this.actions;
        }
    }
}