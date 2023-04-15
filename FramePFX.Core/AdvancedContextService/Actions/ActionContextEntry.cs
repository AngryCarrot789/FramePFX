using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using SharpPadV2.Core.Actions;
using SharpPadV2.Core.Actions.Contexts;
using SharpPadV2.Core.AdvancedContextService.Base;

namespace SharpPadV2.Core.AdvancedContextService.Actions {
    /// <summary>
    /// The default implementation for a context entry (aka menu item), which also supports modifying the header,
    /// input gesture text, command and command parameter to reflect the UI menu item
    /// <para>
    /// Setting <see cref="ContextEntry.InputGestureText"/> will not do anything, as the UI will automatically search for the action ID shortcut
    /// </para>
    /// </summary>
    public class ActionContextEntry : ContextEntry, IContextEntry {
        private string actionId;
        public string ActionId {
            get => this.actionId;
            set => this.RaisePropertyChanged(ref this.actionId, value);
        }

        public ICommand InvokeCommand { get; private set; }

        public ActionContextEntry(IEnumerable<IContextEntry> children = null) : base(children) {
            this.Init();
        }

        public ActionContextEntry(string actionId, IEnumerable<IContextEntry> children = null) : base(children) {
            this.actionId = actionId;
            this.Init();
            this.SetHeaderAndTooltipFromAction(this.actionId);
        }

        public ActionContextEntry(object dataContext, string actionId, IEnumerable<IContextEntry> children = null) : base(dataContext, children) {
            this.actionId = actionId;
            this.Init();
            this.SetHeaderAndTooltipFromAction(this.actionId);
        }

        public ActionContextEntry(object dataContext, string header, string actionId, IEnumerable<IContextEntry> children = null) : base(dataContext, header, children) {
            this.actionId = actionId;
            this.Init();
        }

        public ActionContextEntry(object dataContext, string header, string actionId, string toolTip, IEnumerable<IContextEntry> children = null) : base(dataContext, header, null, toolTip, children) {
            this.actionId = actionId;
            this.Init();
        }

        private void Init() {
            this.InvokeCommand = new AsyncRelayCommand(this.InvokeAction, () => IoC.ActionManager.GetPresentation(this.ActionId, this).IsEnabled);
        }

        protected virtual Task InvokeAction() {
            return IoC.ActionManager.Execute(this.actionId, this);
        }

        public void SetHeaderAndTooltipFromAction(string id) {
            AnAction action = IoC.ActionManager.GetAction(id);
            if (action != null) {
                string header = action.Header();
                if (!string.IsNullOrEmpty(header)) {
                    this.Header = header;
                }

                string description = action.Description();
                if (!string.IsNullOrEmpty(description)) {
                    this.ToolTip = description;
                }
            }

            if (string.IsNullOrEmpty(this.Header)) {
                this.Header = id;
            }
        }
    }
}