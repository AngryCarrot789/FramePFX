using System;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Core.Editor.ResourceChecker {
    public abstract class InvalidResourceViewModel : BaseViewModel {
        public ResourceItemViewModel Resource { get; }

        public ResourceCheckerViewModel Checker { get; set; }

        public AsyncRelayCommand SetOfflineCommand { get; }

        protected InvalidResourceViewModel(ResourceItemViewModel resource) {
            this.Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            this.SetOfflineCommand = new AsyncRelayCommand(this.RemoveFromCheckerAction, () => this.Checker != null);
        }

        public async Task RemoveFromCheckerAction() {
            await this.Checker.RemoveItemAction(this);
        }
    }
}
