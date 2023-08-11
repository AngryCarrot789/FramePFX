using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using FramePFX.Core.Utils;

namespace FramePFX.Core.AdvancedContextService {
    /// <summary>
    /// A context entry class that supports invoking an <see cref="ICommand"/> based on a shortcut
    /// </summary>
    public class ShortcutCommandContextEntry : BaseContextEntry {
        public ObservableCollectionEx<string> ShortcutIds { get; }

        private ICommand command;

        public ICommand Command {
            get => this.command;
            set => this.RaisePropertyChanged(ref this.command, value);
        }

        private object commandParameter;

        public object CommandParameter {
            get => this.commandParameter;
            set => this.RaisePropertyChanged(ref this.commandParameter, value);
        }

        public ShortcutCommandContextEntry(IEnumerable<string> shortcutIds, ICommand command, object commandParameter, IEnumerable<IContextEntry> children = null) : base(null, children) {
            this.ShortcutIds = new ObservableCollectionEx<string>(shortcutIds ?? Enumerable.Empty<string>());
            this.command = command;
            this.commandParameter = commandParameter;
        }

        public ShortcutCommandContextEntry(IEnumerable<string> shortcutIds, ICommand command, IEnumerable<IContextEntry> children = null) : this(shortcutIds, command, null, children) {
        }

        public ShortcutCommandContextEntry(string shortcutId, ICommand command, object commandParameter, IEnumerable<IContextEntry> children = null) :
            this(shortcutId != null ? new List<string> {shortcutId} : null, command, commandParameter, children) {
        }

        public ShortcutCommandContextEntry(string shortcutId, ICommand command, IEnumerable<IContextEntry> children = null) : this(shortcutId, command, null, children) {
        }

        public ShortcutCommandContextEntry(string header, string description, string shortcutId, ICommand command, IEnumerable<IContextEntry> children = null) : this(shortcutId, command, null, children) {
            this.Header = header;
            this.Description = description;
        }
    }
}