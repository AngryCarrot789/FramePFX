using System;
using System.Runtime.CompilerServices;
using FramePFX.Editor.Registries;

namespace FramePFX.Editor.ResourceManaging.ViewModels
{
    public static class ResourceExtensions
    {
        public static BaseResourceViewModel CreateViewModel(this BaseResource obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return ResourceTypeFactory.Instance.CreateViewModelFromModel(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TViewModel CreateViewModel<TViewModel>(this BaseResource obj) where TViewModel : BaseResourceViewModel
        {
            return (TViewModel) CreateViewModel(obj);
        }
    }
}