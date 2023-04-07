using System.Threading.Tasks;

namespace FramePFX.Core.Actions {
    public abstract class Action {
        public string Header { get; }

        public string Description { get; }

        public string InputGestureText { get; }

        protected Action(string header, string description) : this(header, description, null) {

        }

        protected Action(string header, string description, string inputGestureText) {
            this.Header = header;
            this.Description = description;
            this.InputGestureText = inputGestureText;
        }

        public abstract Task<bool> Execute(ActionEvent e);
    }
}