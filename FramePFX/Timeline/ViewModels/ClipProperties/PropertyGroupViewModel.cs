using System.Collections.ObjectModel;

namespace FramePFX.Timeline.ViewModels.ClipProperties {
    public class ClipPropertyGroupViewModel : BaseClipPropertyViewModel {
        private readonly ObservableCollection<BaseClipPropertyViewModel> items;
        public ReadOnlyObservableCollection<BaseClipPropertyViewModel> Items { get; }

        public ClipPropertyGroupViewModel() {
            this.items = new ObservableCollection<BaseClipPropertyViewModel>();
            this.Items = new ReadOnlyObservableCollection<BaseClipPropertyViewModel>(this.items);
        }
    }
}