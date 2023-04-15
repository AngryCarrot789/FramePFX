using System.Collections.Generic;
using System.Windows.Input;
using SharpPadV2.Core.AdvancedContextService.Base;

namespace SharpPadV2.Core.AdvancedContextService.Commands {
    public class CheckableCommandContextEntry : CommandContextEntry {
        private bool isChecked;
        public bool IsChecked {
            get => this.isChecked;
            set => this.RaisePropertyChanged(ref this.isChecked, value);
        }

        public CheckableCommandContextEntry(IEnumerable<IContextEntry> children = null) : base(children) {
        }

        public CheckableCommandContextEntry(ICommand command, object commandParameter, IEnumerable<IContextEntry> children = null) : base(command, commandParameter, children) {
        }

        public CheckableCommandContextEntry(string header, ICommand command, object commandParameter, IEnumerable<IContextEntry> children = null) : base(header, command, commandParameter, children) {
        }

        public CheckableCommandContextEntry(string header, ICommand command, object commandParameter, string inputGestureText, IEnumerable<IContextEntry> children = null) : base(header, command, commandParameter, inputGestureText, children) {
        }

        public CheckableCommandContextEntry(string header, ICommand command, object commandParameter, string inputGestureText, string toolTip, IEnumerable<IContextEntry> children = null) : base(header, command, commandParameter, inputGestureText, toolTip, children) {
        }
    }
}