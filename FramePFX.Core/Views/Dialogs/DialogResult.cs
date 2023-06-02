namespace FrameControlEx.Core.Views.Dialogs {
    /// <summary>
    /// Just return null or use nullable instead; so much more convenient than using this class... unless null can be a successful result
    /// </summary>
    /// <typeparam name="T"></typeparam>
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