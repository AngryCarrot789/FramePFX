using System.Collections.Generic;
using FramePFX.Core.Utils;

namespace FramePFX.Core.AdvancedContextService
{
    /// <summary>
    /// The class for action-based context entries. The header, tooltip, etc, are automatically fetched
    /// </summary>
    public class ActionContextEntry : BaseContextEntry
    {
        private string actionId;

        public string ActionId
        {
            get => this.actionId;
            set => this.RaisePropertyChanged(ref this.actionId, value);
        }

        public ActionContextEntry(object dataContext, string actionId, string header, string description, IEnumerable<IContextEntry> children = null) : base(dataContext, header, description, children)
        {
            this.actionId = actionId;
        }

        public ActionContextEntry(object dataContext, string actionId, string header, IEnumerable<IContextEntry> children = null) : this(dataContext, actionId, header, null, children)
        {
        }

        public ActionContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : this(dataContext, actionId, StringUtils.SplitLast(actionId, '.'), null, children)
        {
        }

        public ActionContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) : this(dataContext, null, null, null, children)
        {
        }

        public ActionContextEntry Set(string key, object value)
        {
            base.SetContextKey(key, value);
            return this;
        }
    }
}