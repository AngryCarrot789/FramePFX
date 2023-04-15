using System.Collections.Generic;

namespace FramePFX.Core.AdvancedContextService.Base {
    public class ContextEntry : BaseInteractableEntry {
        private string header;
        private string inputGestureText;
        private string toolTip;

        /// <summary>
        /// The menu item's header, aka text
        /// </summary>
        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        /// <summary>
        /// The preview input gesture text, which is typically on the right side of a menu item (used for shortcuts)
        /// </summary>
        public string InputGestureText {
            get => this.inputGestureText;
            set => this.RaisePropertyChanged(ref this.inputGestureText, value);
        }

        /// <summary>
        /// A mouse over tooltip for this entry
        /// </summary>
        public string ToolTip {
            get => this.toolTip;
            set => this.RaisePropertyChanged(ref this.toolTip, value);
        }

        public IEnumerable<IContextEntry> Children { get; }

        public ContextEntry(IEnumerable<IContextEntry> children = null) : base(null) {
            this.Children = children;
        }

        public ContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) : base(dataContext) {
            this.Children = children;
        }

        public ContextEntry(object dataContext, string header, IEnumerable<IContextEntry> children = null) : base(dataContext) {
            this.header = header;
            this.Children = children;
        }

        public ContextEntry(object dataContext, string header, string inputGestureText, IEnumerable<IContextEntry> children = null) : base(dataContext) {
            this.header = header;
            this.inputGestureText = inputGestureText;
            this.Children = children;
        }

        public ContextEntry(object dataContext, string header, string inputGestureText, string toolTip, IEnumerable<IContextEntry> children = null) : base(dataContext) {
            this.header = header;
            this.inputGestureText = inputGestureText;
            this.toolTip = toolTip;
            this.Children = children;
        }
    }
}