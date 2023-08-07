using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Editor.Resources
{
    public class ResourceGroupControl : BaseResourceItemControl
    {
        public static readonly DependencyProperty IsDroppableTargetOverProperty = DependencyProperty.Register("IsDroppableTargetOver", typeof(bool), typeof(ResourceGroupControl), new PropertyMetadata(BoolBox.False));

        public bool IsDroppableTargetOver
        {
            get => (bool) this.GetValue(IsDroppableTargetOverProperty);
            set => this.SetValue(IsDroppableTargetOverProperty, value.Box());
        }

        public INavigatableResource Resource => this.DataContext as INavigatableResource;

        private bool isProcessingAsyncDrop;

        public ResourceGroupControl()
        {
            this.AllowDrop = true;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(nameof(BaseResourceObjectViewModel)))
            {
                object obj = e.Data.GetData(nameof(BaseResourceObjectViewModel));
                if (obj is BaseResourceObjectViewModel resource && this.DataContext is IAcceptResourceDrop drop && drop.CanDropResource(resource))
                {
                    this.IsDroppableTargetOver = true;
                    e.Effects = DragDropEffects.Move;
                    e.Handled = true;
                    goto end;
                }
            }

            this.ClearValue(IsDroppableTargetOverProperty);
            e.Effects = DragDropEffects.None;

            end:
            e.Handled = true;
            base.OnDragOver(e);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
            this.Dispatcher.Invoke(() =>
            {
                this.ClearValue(IsDroppableTargetOverProperty);
            }, DispatcherPriority.Loaded);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (this.isProcessingAsyncDrop)
            {
                return;
            }

            e.Handled = true;
            this.isProcessingAsyncDrop = true;
            if (this.DataContext is IAcceptResourceDrop drop && e.Data.GetData(nameof(BaseResourceObjectViewModel)) is BaseResourceObjectViewModel resource)
            {
                if (drop.CanDropResource(resource))
                {
                    this.HandleOnDropResource(drop, resource);
                }
            }
        }

        private async void HandleOnDropResource(IAcceptResourceDrop acceptResourceDrop, BaseResourceObjectViewModel resource)
        {
            await acceptResourceDrop.OnDropResource(resource);
            this.ClearValue(IsDroppableTargetOverProperty);
            this.isProcessingAsyncDrop = false;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            this.Resource?.OnNavigate();
        }
    }
}