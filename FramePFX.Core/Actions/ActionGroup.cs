using System.Collections.Generic;
using System.Threading.Tasks;

namespace FramePFX.Core.Actions {
    public abstract class ActionGroup : Action {
        protected ActionGroup(string header, string description) : base(header, description) {

        }

        protected ActionGroup(string header, string description, string inputGestureText) : base(header, description, inputGestureText) {

        }

        public abstract List<Action> GetChildren();

        public override Task<bool> Execute(ActionEvent e) {
            return Task.FromResult(true);
        }
    }
}