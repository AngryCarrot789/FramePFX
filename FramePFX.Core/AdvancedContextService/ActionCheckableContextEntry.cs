using System.Collections.Generic;
using FramePFX.Core.Actions;
using FramePFX.Core.Utils;

namespace FramePFX.Core.AdvancedContextService
{
    public class ActionCheckableContextEntry : ActionContextEntry
    {
        private bool isChecked;

        public bool IsChecked
        {
            get => this.isChecked;
            set
            {
                this.RaisePropertyChanged(ref this.isChecked, value);
                this.SetContextKey(ToggleAction.IsToggledKey, value.Box());
            }
        }

        public ActionCheckableContextEntry(object dataContext, string actionId, string header, string description, IEnumerable<IContextEntry> children = null) : base(dataContext, actionId, header, description, children)
        {
        }

        public ActionCheckableContextEntry(object dataContext, string actionId, string header, IEnumerable<IContextEntry> children = null) : base(dataContext, actionId, header, children)
        {
        }

        public ActionCheckableContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : base(dataContext, actionId, children)
        {
        }

        public ActionCheckableContextEntry(object dataContext, IEnumerable<IContextEntry> children = null) : base(dataContext, children)
        {
        }
    }
}