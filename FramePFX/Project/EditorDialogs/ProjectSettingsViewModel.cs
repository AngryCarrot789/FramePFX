using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FramePFX.Core;

namespace FramePFX.Project.EditorDialogs {
    public class ProjectSettingsViewModel : BaseViewModel {
        private int width;
        private int height;
        private double frameRate;

        public int Width {
            get => this.width;
            set => this.RaisePropertyChanged(ref this.width, value);
        }

        public int Height {
            get => this.height;
            set => this.RaisePropertyChanged(ref this.height, value);
        }

        public double FrameRate {
            get => this.frameRate;
            set => this.RaisePropertyChanged(ref this.frameRate, value);
        }
    }
}
