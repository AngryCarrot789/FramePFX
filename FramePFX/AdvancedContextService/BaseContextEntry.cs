using System.Collections.Generic;

namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// Base class for context entries, supporting custom data context
    /// </summary>
    public abstract class BaseContextEntry : BaseViewModel, IContextEntry {
        private string header;
        private string description;
        private IconType iconType;

        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        public string Description {
            get => this.description;
            set => this.RaisePropertyChanged(ref this.description, value);
        }

        public IconType IconType {
            get => this.iconType;
            set => this.RaisePropertyChanged(ref this.iconType, value);
        }

        public IEnumerable<IContextEntry> Children { get; }

        protected BaseContextEntry(string header, string description, IEnumerable<IContextEntry> children = null) {
            this.Children = children;
            this.header = header;
            this.description = description;
        }

        protected BaseContextEntry(IEnumerable<IContextEntry> children = null) : this(null, null, children) {
        }
    }
}