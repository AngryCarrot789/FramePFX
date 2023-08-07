using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging.Actions
{
    public class ToggleResourceOnlineStateAction : ToggleAction
    {
        public override async Task<bool> OnToggled(AnActionEventArgs e, bool isToggled)
        {
            if (e.DataContext.TryGetContext(out ResourceItemViewModel resItem))
            {
                List<ResourceItemViewModel> items = resItem.Parent.SelectedItems.OfType<ResourceItemViewModel>().ToList();
                if (items.Count > 0)
                {
                    await SetOnlineState(items, isToggled);
                    return true;
                }
            }

            return true;
        }

        public override async Task<bool> ExecuteNoToggle(AnActionEventArgs e)
        {
            if (e.DataContext.TryGetContext(out ResourceItemViewModel resItem))
            {
                List<ResourceItemViewModel> items = resItem.Parent.SelectedItems.OfType<ResourceItemViewModel>().ToList();
                if (items.Count > 0)
                {
                    await SetOnlineState(items, null);
                    return true;
                }
            }

            return false;
        }

        private static async Task SetOnlineState(IEnumerable<ResourceItemViewModel> items, bool? state)
        {
            List<ResourceItemViewModel> list = new List<ResourceItemViewModel>();
            using (ExceptionStack stack = new ExceptionStack(false))
            {
                foreach (ResourceItemViewModel item in items)
                {
                    if (state == false || (state == null && item.IsOnline))
                    {
                        item.Model.Disable(stack, true);
                    }
                    else
                    {
                        list.Add(item);
                    }
                }

                if (stack.TryGetException(out Exception exception))
                {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exception setting offline", "An exception occurred while setting one or more resource to offline", exception.GetToString());
                }
            }

            if (list.Count > 0)
            {
                await ResourceCheckerViewModel.LoadResources(list, true);
            }
        }
    }
}