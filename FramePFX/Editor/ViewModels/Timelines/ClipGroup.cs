using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FramePFX.Editor.ViewModels.Timelines {
    /// <summary>
    /// A helper for grouping together clips
    /// </summary>
    public class ClipGroup : BaseViewModel {
        /// <summary>
        /// The primary clip that initially created this group
        /// </summary>
        private ClipViewModel host;

        public ClipViewModel Host {
            get => this.host;
            set => this.RaisePropertyChanged(ref this.host, value);
        }

        /// <summary>
        /// The clips that are connected to this group, including the host clip
        /// </summary>
        public ObservableCollection<ClipViewModel> Clips { get; }

        public ClipGroup(IEnumerable<ClipViewModel> set) {
            this.Clips = new ObservableCollection<ClipViewModel>(set);
        }
    }
}