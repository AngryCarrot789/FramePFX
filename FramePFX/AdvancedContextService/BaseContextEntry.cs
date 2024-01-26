using System.Collections.Generic;

namespace FramePFX.AdvancedContextService {
    public delegate void BaseContextEntryEventHandler(BaseContextEntry entry);

    /// <summary>
    /// Base class for context entries, supporting custom data context
    /// </summary>
    public abstract class BaseContextEntry : IContextEntry {
        private string header;
        private string description;

        public string Header {
            get => this.header;
            set {
                if (this.header == value)
                    return;
                this.header = value;
                this.DescriptionChanged?.Invoke(this);
            }
        }

        public string Description {
            get => this.description;
            set {
                if (this.description == value)
                    return;
                this.description = value;
                this.DescriptionChanged?.Invoke(this);
            }
        }

        public IEnumerable<IContextEntry> Children { get; }

        public event BaseContextEntryEventHandler DescriptionChanged;
        public event BaseContextEntryEventHandler HeaderChanged;

        protected BaseContextEntry(string header, string description, IEnumerable<IContextEntry> children = null) {
            this.Children = children;
            this.header = header;
            this.description = description;
        }

        protected BaseContextEntry(IEnumerable<IContextEntry> children = null) : this(null, null, children) {
        }
    }
}