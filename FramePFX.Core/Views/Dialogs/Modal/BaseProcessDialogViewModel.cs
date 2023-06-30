using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FramePFX.Core.Views.Dialogs.Modal {
    /// <summary>
    /// A base dialog for dynamic dialogs that can save their
    /// </summary>
    public abstract class BaseProcessDialogViewModel : BaseDynamicDialogViewModel {
        protected string automaticResult;
        protected bool showAlwaysUseNextResultOption;
        protected bool isAlwaysUseThisOptionChecked;
        protected bool canShowAlwaysUseNextResultForCurrentQueueOption;
        protected bool isAlwaysUseThisOptionForCurrentQueueChecked;

        public string AutomaticResult {
            get => this.automaticResult;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.automaticResult, value);
            }
        }

        /// <summary>
        /// Whether or not to show the "always use next result option" in the GUI
        /// </summary>
        public bool ShowAlwaysUseNextResultOption { // dialog will show "Always use this option"
            get => this.showAlwaysUseNextResultOption;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.showAlwaysUseNextResultOption, value);
                if (!value && this.IsAlwaysUseThisOptionChecked) {
                    this.IsAlwaysUseThisOptionChecked = false;
                }
            }
        }

        /// <summary>
        /// Whether or not the GUI option to use the next outcome as an automatic result is checked
        /// </summary>
        [Bindable(true)]
        public bool IsAlwaysUseThisOptionChecked {
            get => this.isAlwaysUseThisOptionChecked;
            set {
                this.EnsureNotReadOnly();
                this.isAlwaysUseThisOptionChecked = value && this.ShowAlwaysUseNextResultOption;
                this.RaisePropertyChanged();
                if (!this.isAlwaysUseThisOptionChecked && this.IsAlwaysUseThisOptionForCurrentQueueChecked) {
                    this.IsAlwaysUseThisOptionForCurrentQueueChecked = false;
                }

                this.UpdateButtons();
            }
        }

        public bool CanShowAlwaysUseNextResultForCurrentQueueOption {
            get => this.canShowAlwaysUseNextResultForCurrentQueueOption;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.canShowAlwaysUseNextResultForCurrentQueueOption, value);
                if (!value && this.IsAlwaysUseThisOptionForCurrentQueueChecked) {
                    this.IsAlwaysUseThisOptionForCurrentQueueChecked = false;
                }
            }
        }

        /// <summary>
        /// Whether or not the GUI option to use the next outcome as an automatic result, but only for the current queue/usage, is checked
        /// </summary>
        [Bindable(true)]
        public bool IsAlwaysUseThisOptionForCurrentQueueChecked {
            get => this.isAlwaysUseThisOptionForCurrentQueueChecked;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.isAlwaysUseThisOptionForCurrentQueueChecked, value && this.CanShowAlwaysUseNextResultForCurrentQueueOption);
                this.UpdateButtons();
            }
        }

        protected BaseProcessDialogViewModel(string primaryResult = null, string defaultResult = null) : base(primaryResult, defaultResult) {

        }

        public override Task<string> ShowAsync() {
            if (this.AutomaticResult != null) {
                return Task.FromResult(this.AutomaticResult);
            }

            return base.ShowAsync();
        }

        public override string GetResult(bool? result, DialogButton button) {
            string output;
            if (result == true) {
                if (button == null) {
                    output = this.PrimaryResult;
                }
                else {
                    output = button.ActionType;
                    if (output != null && this.IsAlwaysUseThisOptionChecked) { // (output != null || this.AllowNullButtonActionForAutoResult)
                        this.AutomaticResult = output;
                    }
                }
            }
            else {
                output = this.DefaultResult;
            }

            return output;
        }

        public override void MarkReadOnly() {
            if (this.ShowAlwaysUseNextResultOption) {
                throw new InvalidOperationException($"Cannot set read-only when {nameof(ShowAlwaysUseNextResultOption)}");
            }

            base.MarkReadOnly();
        }
    }
}