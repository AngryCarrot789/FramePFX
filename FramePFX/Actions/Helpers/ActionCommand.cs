using System;
using System.Threading.Tasks;
using FramePFX.Actions.Contexts;
using FramePFX.Commands;

namespace FramePFX.Actions.Helpers {
    public abstract class BaseActionCommand : BaseAsyncRelayCommand {
        public string ActionId { get; }

        public IDataContext Context { get; }

        protected BaseActionCommand(string actionId, IDataContext context) {
            if (string.IsNullOrWhiteSpace(actionId))
                throw new ArgumentException("Action is must not be null, empty or whitespaces", nameof(actionId));
            this.ActionId = actionId;
            this.Context = context;
        }
    }

    public class ActionCommand : BaseActionCommand {
        public ActionCommand(string actionId, IDataContext context = null) : base(actionId, context) {
        }

        protected override bool CanExecuteCore(object parameter) {
            DataContext ctx = new DataContext();
            if (parameter is IDataContext)
                ctx.Merge((IDataContext) parameter);
            if (this.Context != null)
                ctx.Merge(this.Context);
            return ActionManager.Instance.CanExecute(this.ActionId, ctx);
        }

        protected override Task ExecuteCoreAsync(object parameter) {
            DataContext ctx = new DataContext();
            if (parameter is IDataContext)
                ctx.Merge((IDataContext) parameter);
            if (this.Context != null)
                ctx.Merge(this.Context);
            return ActionManager.Instance.Execute(this.ActionId, ctx);
        }
    }
}