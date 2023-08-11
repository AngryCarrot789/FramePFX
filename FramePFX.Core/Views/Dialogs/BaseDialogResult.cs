namespace FramePFX.Core.Views.Dialogs {
    public abstract class BaseDialogResult {
        /// <summary>
        /// True if the user confirmed the UI action, otherwise false to indicate the user cancelled the action
        /// </summary>
        public bool IsSuccess { get; set; }

        protected BaseDialogResult() : this(true) {
        }

        protected BaseDialogResult(bool isSuccess) {
            this.IsSuccess = isSuccess;
        }
    }
}