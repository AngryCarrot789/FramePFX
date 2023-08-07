using System;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceChecker
{
    public abstract class InvalidResourceViewModel : BaseViewModel
    {
        public ResourceItemViewModel Resource { get; }

        public ResourceCheckerViewModel Checker { get; set; }

        public AsyncRelayCommand SetOfflineCommand { get; }

        protected InvalidResourceViewModel(ResourceItemViewModel resource)
        {
            this.Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            this.SetOfflineCommand = new AsyncRelayCommand(this.SetOfflineAndRemove, () => this.Checker != null);
            resource.Model.IsOnline = false;
        }

        public async Task RemoveFromCheckerAction()
        {
            await this.Checker.RemoveItemAction(this);
        }

        /// <summary>
        /// Sets this resource's state as offline
        /// </summary>
        public virtual async Task SetResourceOfflineAsync()
        {
            using (ExceptionStack stack = new ExceptionStack(false))
            {
                this.Resource.Model.Disable(stack, true);
                if (stack.TryGetException(out Exception exception))
                {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exception setting offline", "An exception occurred while setting resource to offline", exception.GetToString());
                }
            }
        }

        public async Task SetOfflineAndRemove()
        {
            await this.SetResourceOfflineAsync();
            await this.RemoveFromCheckerAction();
        }
    }
}