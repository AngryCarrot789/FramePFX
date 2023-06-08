using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.Editor.ResourceChecker {
    public class ResourceCheckerViewModel : BaseViewModel {
        private readonly ObservableCollectionEx<InvalidResourceViewModel> resources;
        public ReadOnlyObservableCollection<InvalidResourceViewModel> Resources { get; }

        private InvalidResourceViewModel currentItem;
        public InvalidResourceViewModel CurrentItem {
            get => this.currentItem;
            set => this.RaisePropertyChanged(ref this.currentItem, value);
        }

        private int currentIndex;
        public int CurrentIndex {
            get => this.currentIndex;
            set => this.RaisePropertyChanged(ref this.currentIndex, value);
        }

        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand OfflineCurrentCommand { get; }
        public AsyncRelayCommand OfflineAllCommand { get; }

        public IDialog Dialog { get; set; }

        public ResourceCheckerViewModel() {
            this.resources = new ObservableCollectionEx<InvalidResourceViewModel>();
            this.Resources = new ReadOnlyObservableCollection<InvalidResourceViewModel>(this.resources);
            this.CancelCommand = new AsyncRelayCommand(this.CancelAction);
            this.OfflineCurrentCommand = new AsyncRelayCommand(this.OfflineCurrentAction);
            this.OfflineAllCommand = new AsyncRelayCommand(this.OfflineAllAction);
        }

        private async Task CancelAction() {
            await this.Dialog.CloseDialogAsync(false);
        }

        private async Task OfflineCurrentAction() {
            int index = this.currentIndex;
            if (index >= 0 && index < this.resources.Count) {
                this.resources[index].Checker = null;
                this.resources.RemoveAt(index);
            }

            if (this.resources.Count < 1) {
                await this.Dialog.CloseDialogAsync(true);
            }
        }

        private async Task OfflineAllAction() {
            foreach (InvalidResourceViewModel item in this.resources)
                item.Checker = null;
            this.resources.Clear();
            await this.Dialog.CloseDialogAsync(true);
        }

        public async Task<bool> RemoveItemAction(InvalidResourceViewModel item) {
            int index = this.resources.IndexOf(item);
            if (index < 0) {
                return false;
            }

            item.Checker = null;
            this.resources.RemoveAt(index);
            if (this.resources.Count < 1) {
                await this.Dialog.CloseDialogAsync(true);
            }

            return true;
        }

        public void Add(InvalidResourceViewModel item) {
            item.Checker = this;
            this.resources.Add(item);
        }
    }
}
