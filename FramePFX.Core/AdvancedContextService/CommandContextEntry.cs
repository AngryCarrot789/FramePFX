using System.Collections.Generic;
using System.Windows.Input;

namespace FramePFX.Core.AdvancedContextService {
    /// <summary>
    /// The default implementation for a context entry in which an ICommand is executed when clicked
    /// </summary>
    public class CommandContextEntry : BaseContextEntry {
        private string inputGestureText;
        private ICommand command;
        private object commandParameter;

        /// <summary>
        /// The preview input gesture text, which is typically on the right side of a menu item (used for shortcuts)
        /// </summary>
        public string InputGestureText {
            get => this.inputGestureText;
            set => this.RaisePropertyChanged(ref this.inputGestureText, value);
        }

        public ICommand Command {
            get => this.command;
            set => this.RaisePropertyChanged(ref this.command, value);
        }

        public object CommandParameter {
            get => this.commandParameter;
            set => this.RaisePropertyChanged(ref this.commandParameter, value);
        }

        public CommandContextEntry(string header, string inputGestureText, string description, ICommand command, object commandParameter, IEnumerable<IContextEntry> children = null) : base(null, header, description, children) {
            this.inputGestureText = inputGestureText;
            this.command = command;
            this.commandParameter = commandParameter;
        }

        public CommandContextEntry(string header, string inputGestureText, string description, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, inputGestureText, description, command, null, children) {
        }

        public CommandContextEntry(string header, string inputGestureText, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, inputGestureText, null, command, null, children) {
        }

        public CommandContextEntry(string header, string inputGestureText, ICommand command, object commandParam, IEnumerable<IContextEntry> children = null) : this(header, inputGestureText, null, command, commandParam, children) {
        }

        public CommandContextEntry(string header, ICommand command, IEnumerable<IContextEntry> children = null) : this(header, null, null, command, null, children) {
        }

        public CommandContextEntry(string header, ICommand command, object commandParam, IEnumerable<IContextEntry> children = null) : this(header, null, null, command, commandParam, children) {
        }
    }
}