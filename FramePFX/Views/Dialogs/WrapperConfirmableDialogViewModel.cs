namespace FramePFX.Views.Dialogs
{
    public class WrapperConfirmableDialogViewModel<T> : BaseConfirmableDialogViewModel
    {
        private T model;

        public T Model
        {
            get => this.model;
            set => this.RaisePropertyChanged(ref this.model, value);
        }

        public WrapperConfirmableDialogViewModel()
        {
        }

        public WrapperConfirmableDialogViewModel(T model)
        {
            this.model = model;
        }
    }
}