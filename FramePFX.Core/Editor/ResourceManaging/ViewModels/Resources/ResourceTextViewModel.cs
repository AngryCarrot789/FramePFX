using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core.ResourceManaging.Resources;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.ResourceManaging.ViewModels.Resources {
    public class ResourceTextViewModel : ResourceItemViewModel {
        public new ResourceText Model => (ResourceText) base.Model;

        public ResourceTextViewModel(ResourceManagerViewModel manager, ResourceText model) : base(manager, model) {
        }
    }
}