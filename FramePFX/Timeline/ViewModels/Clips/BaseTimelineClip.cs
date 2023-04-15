using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Timeline.Layer.Clips;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline.ViewModels.Clips {
    public abstract class BaseTimelineClip : BaseViewModel {
        private string name;
        public string Name {
            get => this.name;
            set => this.RaisePropertyChanged(ref this.name, value);
        }

        /// <summary>
        /// A reference to the actual UI element clip container
        /// </summary>
        public IClipHandle Handle { get; set; }

        /// <summary>
        /// The layer that this clip container is currently in. Should be null if the clip is not yet in a layer
        /// </summary>
        public TimelineLayer Layer { get; set; }

        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }

        protected BaseTimelineClip() {
            this.RenameCommand = new RelayCommand(this.RenameAction);
            this.DeleteCommand = new RelayCommand(() => {
                this.Layer.DeleteClip(this);
            });
        }

        private void RenameAction() {
            string newName = CoreIoC.UserInput.ShowSingleInputDialog("Rename clip", "Input a new clip name:", this.Name ?? "");
            if (newName != null) {
                this.Name = newName;
            }
        }
    }
}