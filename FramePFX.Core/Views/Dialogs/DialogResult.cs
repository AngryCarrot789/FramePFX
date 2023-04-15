namespace FramePFX.Core.Views.Dialogs {
    public class DialogResult<T> : BaseDialogResult {
        public T Value { get; set; }

        public DialogResult() {
        }

        public DialogResult(bool isSuccess) : base(isSuccess) {
        }

        public DialogResult(T value) : base(true) {
            this.Value = value;
        }

        public DialogResult(bool isSuccess, T value) : base(isSuccess) {
            this.Value = value;
        }
    }
}