using System;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceChecker {
    /// <summary>
    /// The base class for invalid resource resolver objects
    /// </summary>
    public abstract class InvalidResourceViewModel : BaseViewModel {
        public ResourceItemViewModel Resource { get; }

        public ResourceCheckerViewModel Checker { get; set; }

        public AsyncRelayCommand SetOfflineCommand { get; }

        protected InvalidResourceViewModel(ResourceItemViewModel resource) {
            this.Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            this.SetOfflineCommand = new AsyncRelayCommand(this.SetOfflineAndRemove, () => this.Checker != null);
        }

        /// <summary>
        /// Removes ourself from the <see cref="Checker"/>. This can be used when the issue is resolved or ignored and the resource is now online
        /// </summary>
        public async Task RemoveSelf() {
            await this.Checker.RemoveItemAction(this);
        }

        /// <summary>
        /// Sets this resource's state as offline
        /// </summary>
        public virtual async Task SetResourceOfflineAsync() {
            if (!this.Resource.IsOnline) {
                return;
            }

            using (ErrorList stack = new ErrorList(false)) {
                this.Resource.Model.Disable(stack, true);
                if (stack.TryGetException(out Exception exception)) {
                    await IoC.DialogService.ShowMessageExAsync("Exception setting offline", "An exception occurred while setting resource to offline", exception.GetToString());
                }
            }
        }

        /// <summary>
        /// Sets our resource to offline and removes ourself from the <see cref="Checker"/>, effectively ignoring the problem
        /// </summary>
        public async Task SetOfflineAndRemove() {
            await this.SetResourceOfflineAsync();
            await this.RemoveSelf();
        }
    }
}