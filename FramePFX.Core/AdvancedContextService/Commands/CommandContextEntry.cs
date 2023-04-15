using System.Collections.Generic;
using System.Windows.Input;
using FramePFX.Core.AdvancedContextService.Base;

namespace FramePFX.Core.AdvancedContextService.Commands {
    /// <summary>
    /// The default implementation for a context entry (aka menu item), which also supports modifying the header,
    /// input gesture text, command and command parameter to reflect the UI menu item
    /// </summary>
    public class CommandContextEntry : ContextEntry, IContextEntry {
        private ICommand command;
        private object commandParameter;

        public ICommand Command {
            get => this.command;
            set => this.RaisePropertyChanged(ref this.command, value);
        }

        public object CommandParameter {
            get => this.commandParameter;
            set => this.RaisePropertyChanged(ref this.commandParameter, value);
        }

        public CommandContextEntry(IEnumerable<IContextEntry> children = null) : base(children) {

        }

        public CommandContextEntry(ICommand command, object commandParameter, IEnumerable<IContextEntry> children = null) : base(children) {
            this.command = command;
            this.commandParameter = commandParameter;
        }

        public CommandContextEntry(string header, ICommand command, IEnumerable<IContextEntry> children = null) : base(null, header, children) {
            this.command = command;
        }

        public CommandContextEntry(string header, ICommand command, object commandParameter, IEnumerable<IContextEntry> children = null) : base(null, header, children) {
            this.command = command;
            this.commandParameter = commandParameter;
        }

        public CommandContextEntry(string header, ICommand command, object commandParameter, string inputGestureText, IEnumerable<IContextEntry> children = null) : base(null, header, inputGestureText, children) {
            this.command = command;
            this.commandParameter = commandParameter;
        }

        public CommandContextEntry(string header, ICommand command, object commandParameter, string inputGestureText, string toolTip, IEnumerable<IContextEntry> children = null) : base(null, header, inputGestureText, toolTip, children) {
            this.command = command;
            this.commandParameter = commandParameter;
        }
    }
}