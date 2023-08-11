using System.Collections.Generic;
using FramePFX.Core.Actions.Contexts;

namespace FramePFX.Core.AdvancedContextService {
    /// <summary>
    /// Base class for context entries, supporting custom data context
    /// </summary>
    public abstract class BaseContextEntry : BaseViewModel, IContextEntry {
        private readonly DataContext context;

        public IDataContext Context => this.context;

        private string header;

        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        private string description;

        public string Description {
            get => this.description;
            set => this.RaisePropertyChanged(ref this.description, value);
        }

        private IconType iconType;

        public IconType IconType {
            get => this.iconType;
            set => this.RaisePropertyChanged(ref this.iconType, value);
        }

        public IEnumerable<IContextEntry> Children { get; }

        protected BaseContextEntry(object dataContext, string header, string description, IEnumerable<IContextEntry> children = null) {
            this.context = new DataContext();
            if (dataContext != null)
                this.context.AddContext(dataContext);
            this.Children = children;
            this.header = header;
            this.description = description;
        }

        protected BaseContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) : this(dataContext, null, null, children) {
        }

        protected BaseContextEntry(IEnumerable<IContextEntry> children = null) : this(null, null, null, children) {
        }

        protected void SetContextKey(string key, object value) {
            this.context.Set(key, value);
        }
    }
}