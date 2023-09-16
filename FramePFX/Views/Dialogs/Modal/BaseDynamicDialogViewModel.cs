using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Utils;

namespace FramePFX.Views.Dialogs.Modal {
    /// <summary>
    /// Base view model class for a dialog with dynamically generated buttons
    /// </summary>
    public abstract class BaseDynamicDialogViewModel : BaseViewModel {
        protected string titlebar;
        protected string defaultResult;
        protected string primaryResult;
        protected internal readonly ObservableCollectionEx<DialogButton> buttons;
        protected DialogButton lastClickedButton;

        /// <summary>
        /// Whether or not this dialog's core behaviour is locked or not. The message and caption can still be modified, but pretty
        /// much everything else cannot, unless marked as not read only
        /// <para>
        /// This is mainly just used to prevent accidentally modifying a "template" instance, because templates should
        /// be cloned (via <see cref="Clone"/>) and then furtuer modified
        /// </para>
        /// </summary>
        public bool IsReadOnly { get; protected internal set; }

        public IDialog Dialog { get; set; }

        public string Titlebar {
            get => this.titlebar;
            set => this.RaisePropertyChanged(ref this.titlebar, value);
        }

        /// <summary>
        /// The resulting action ID that gets returned when the dialog closes with a successful result but no button was explicitly clicked
        /// </summary>
        public string PrimaryResult {
            get => this.primaryResult;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.primaryResult, value);
            }
        }

        /// <summary>
        /// This dialog's default result, which is the result used if the dialog closed without a button (e.g. clicking esc or some dodgy Win32 usage)
        /// </summary>
        public string DefaultResult {
            get => this.defaultResult;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.defaultResult, value);
            }
        }

        /// <summary>
        /// The buttons for this message dialog. This list is ordered left to right, meaning that the first element will be on the very left.
        /// <para>
        /// On windows, the UI list is typically aligned to the right, meaning the last element is on the very right. So to have the typical
        /// Yes/No/Cancel buttons, you would add them to this list in that exact order; left to right
        /// </para>
        /// </summary>
        public ReadOnlyObservableCollection<DialogButton> Buttons { get; }

        public bool HasNoButtons => this.Buttons.Count < 1;
        public bool HasButtons => this.Buttons.Count > 0;

        protected BaseDynamicDialogViewModel(string primaryResult = null, string defaultResult = null) {
            this.buttons = new ObservableCollectionEx<DialogButton>();
            this.Buttons = new ReadOnlyObservableCollection<DialogButton>(this.buttons);
            this.primaryResult = primaryResult;
            this.defaultResult = defaultResult;
            this.buttons.CollectionChanged += (sender, args) => {
                // Shouldn't be read only, because buttons is private
                this.RaisePropertyChanged(nameof(this.HasNoButtons));
                this.RaisePropertyChanged(nameof(this.HasButtons));
            };
        }

        protected abstract Task<bool?> ShowDialogAsync();

        public virtual async Task<string> ShowAsync() {
            this.UpdateButtons();
            try {
                bool? result = await this.ShowDialogAsync();
                return this.GetResult(result, this.lastClickedButton);
            }
            finally {
                this.lastClickedButton = null;
            }
        }

        public virtual string GetResult(bool? result, DialogButton button) {
            string output;
            if (result == true) {
                output = button != null ? button.ActionType : this.PrimaryResult;
            }
            else {
                output = this.DefaultResult;
            }

            return output;
        }

        public virtual async Task OnButtonClicked(DialogButton button) {
            this.lastClickedButton = button;
            await this.Dialog.CloseDialogAsync(button?.ActionType != null);
        }

        public DialogButton InsertButton(int index, string msg, string actionType, bool canUseAsAutoResult = true) {
            this.EnsureNotReadOnly();
            DialogButton button = new DialogButton(this, actionType, msg, canUseAsAutoResult);
            this.buttons.Insert(index, button);
            return button;
        }

        public DialogButton ReplaceButton(int index, string msg, string actionType, bool canUseAsAutoResult = true) {
            this.EnsureNotReadOnly();
            this.buttons.RemoveAt(index);
            return this.InsertButton(index, msg, actionType, canUseAsAutoResult);
        }

        public DialogButton AddButton(string msg, string actionType, bool canUseAsAutoResult = true) {
            return this.InsertButton(this.buttons.Count, msg, actionType, canUseAsAutoResult);
        }

        public void AddButton(DialogButton button) {
            this.EnsureNotReadOnly();
            this.buttons.Add(button);
        }

        public void InsertButton(int index, DialogButton button) {
            this.EnsureNotReadOnly();
            this.buttons.Insert(index, button);
        }

        public void AddButtons(params DialogButton[] buttons) {
            this.EnsureNotReadOnly();
            foreach (DialogButton button in buttons) {
                this.buttons.Add(button);
            }
        }

        public DialogButton GetButtonAt(int index) {
            return this.buttons[index];
        }

        public DialogButton GetButtonById(string id) {
            return id == null ? null : this.buttons.First(x => x.ActionType != null && x.ActionType == id);
        }

        public DialogButton RemoveButtonAt(int index) {
            this.EnsureNotReadOnly();
            DialogButton button = this.buttons[index];
            this.buttons.RemoveAt(index);
            return button;
        }

        public DialogButton RemoveButtonById(string id) {
            int index = this.buttons.FindIndexOf(x => x.ActionType == id);
            if (index == -1) {
                return null;
            }

            DialogButton button = this.buttons[index];
            this.buttons.RemoveAt(index);
            return button;
        }

        public virtual void UpdateButtons() {
            foreach (DialogButton button in this.Buttons) {
                button.UpdateState();
            }
        }

        /// <summary>
        /// Creates a clone of this dialog. The returned instance will not be read only, allowing it to be further modified
        /// </summary>
        /// <returns></returns>
        public abstract BaseProcessDialogViewModel CloneCore();

        /// <summary>
        /// Marks this dialog as read-only, meaning most properties cannot be modified (apart from header, message, etc)
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="BaseProcessDialogViewModel.ShowAlwaysUseNextResultOption"/> is true</exception>
        public virtual void MarkReadOnly() {
            this.IsReadOnly = true;
        }

        protected void EnsureNotReadOnly() {
            if (this.IsReadOnly) {
                throw new InvalidOperationException("This message dialog instance is read-only. Create a clone to modify it");
            }
        }
    }
}