using System.Collections.Generic;
using System.Windows.Input;

namespace FrameControlEx.Core.AdvancedContextService {
    public class CommandCheckableContextEntry : CommandContextEntry {
        private bool isChecked;
        public bool IsChecked {
            get => this.isChecked;
            set => this.RaisePropertyChanged(ref this.isChecked, value);
        }

        public CommandCheckableContextEntry(string header, string inputGestureText, string description, ICommand command, object commandParameter, IEnumerable<IContextEntry> children = null) : base(header, inputGestureText, description, command, commandParameter, children) {

        }

        public CommandCheckableContextEntry(string header, string inputGestureText, string description, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, inputGestureText, description, command, null, children) {

        }

        public CommandCheckableContextEntry(string header, string inputGestureText, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, inputGestureText, null, command, null, children) {

        }

        public CommandCheckableContextEntry(string header, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, null, null, command, null, children) {

        }
    }
}