namespace FramePFX.Core.AdvancedContextService.Base {
    public class BaseInteractableEntry : BaseViewModel, IContextEntry {
        private object dataContext;
        public object DataContext {
            get => this.dataContext;
            set => this.RaisePropertyChanged(ref this.dataContext, value);
        }

        protected BaseInteractableEntry(object dataContext = null) {
            this.dataContext = dataContext;
        }
    }
}