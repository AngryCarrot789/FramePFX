using System.Threading.Tasks;
using FramePFX.Views.Dialogs.Modal;

namespace FramePFX.Views.Dialogs.Message {
    /// <summary>
    /// A helper view model for managing message dialogs that can have multiple buttons
    /// </summary>
    public class MessageDialog : BaseProcessDialogViewModel {
        protected string header;
        protected string message;

        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        public string Message {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        /// <summary>
        /// Creates a new instance of <see cref="MessageDialog"/>
        /// </summary>
        /// <param name="primaryResult">
        /// The resulting action ID that gets returned when the dialog closes with a successful
        /// result but no button was explicitly clicked (due to custom window closing functionality setting the dialog result to true).
        /// Realistically, this should be the recommended result (e.g. "confirm")
        /// </param>
        /// <param name="defaultResult">
        /// This dialog's default result, which is the result used if the dialog closed without a button (e.g. clicking esc or some dodgy Win32 usage)
        /// </param>
        public MessageDialog(string primaryResult = null, string defaultResult = null) : base(primaryResult, defaultResult) {
        }

        public Task<string> ShowAsync(string titlebar, string header, string message) {
            if (this.AutomaticResult != null)
                return Task.FromResult(this.AutomaticResult);

            if (titlebar != null)
                this.Titlebar = titlebar;
            if (header != null)
                this.Header = header;
            if (message != null)
                this.Message = message;
            return this.ShowAsync();
        }

        public Task<string> ShowAsync(string titlebar, string message) {
            return this.ShowAsync(titlebar, null, message);
        }

        public Task<string> ShowAsync(string message) {
            return this.ShowAsync(null, message);
        }

        /// <summary>
        /// Creates a clone of this dialog. The returned instance will not be read only, allowing it to be further modified
        /// </summary>
        /// <returns></returns>
        public MessageDialog Clone() {
            MessageDialog dialog = new MessageDialog() {
                titlebar = this.titlebar,
                header = this.header,
                message = this.message,
                automaticResult = this.automaticResult,
                ShowAlwaysUseNextResultOption = this.ShowAlwaysUseNextResultOption,
                IsAlwaysUseThisOptionChecked = this.IsAlwaysUseThisOptionChecked,
                primaryResult = this.primaryResult,
                defaultResult = this.defaultResult
            };

            foreach (DialogButton button in this.buttons)
                dialog.buttons.Add(button.Clone(dialog));
            return dialog;
        }


        protected override Task<bool?> ShowDialogAsync() {
            return Services.DialogService.ShowDialogAsync(this);
        }

        public override BaseProcessDialogViewModel CloneCore() {
            return this.Clone();
        }

        /// <summary>
        /// Creates a disposable usage/state of this message dialog which, if <see cref="IsAlwaysUseNextResultForCurrentQueueChecked"/> is true,
        /// allows the buttons and auto-result to be restored once the usage instance is disposed
        /// <para>
        /// This only needs to be used if you intend on modifying the state of the current <see cref="MessageDialog"/> during some
        /// sort of "queue/collection based" work, and want to restore those changes once you're finished
        /// <para>
        /// An example is loading files; you realistically need to use this in order to restore the <see cref="AutomaticResult"/> to the previous
        /// value if <see cref="IsAlwaysUseNextResultForCurrentQueueChecked"/> is true)
        /// </para>
        /// </para>
        /// </summary>
        /// <returns></returns>
        public MessageDialogUsage Use() {
            return new MessageDialogUsage(this);
        }
    }
}