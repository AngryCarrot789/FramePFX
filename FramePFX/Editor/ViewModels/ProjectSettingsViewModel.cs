using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels {
    public class ProjectSettingsViewModel : BaseViewModel {
        public ProjectSettings Model { get; }

        public Rational FrameRate {
            get => this.Model.FrameRate;
            set => this.Model.FrameRate = value;
        }

        public Rect2i Resolution {
            get => this.Model.Resolution;
            set => this.Model.Resolution = value;
        }

        public EnumRenderQuality RenderQuality {
            get => this.Model.Quality;
            set => this.Model.Quality = value;
        }

        public ObservableCollection<string> ChannelFormats { get; }

        public int Width => this.Model.Resolution.Width;
        public int Height => this.Model.Resolution.Height;

        public ProjectSettingsViewModel(ProjectSettings model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.ChannelFormats = new ObservableCollection<string>() {
                "Stereo"
            };

            model.SettingChanged += (settings, property) => {
                switch (property) {
                    case nameof(model.Resolution):
                        this.RaisePropertyChanged(nameof(this.Resolution));
                        this.RaisePropertyChanged(nameof(this.Width));
                        this.RaisePropertyChanged(nameof(this.Height));
                        break;
                    case nameof(model.FrameRate): this.RaisePropertyChanged(nameof(this.FrameRate)); break;
                    case nameof(model.Quality): this.RaisePropertyChanged(nameof(this.RenderQuality)); break;
                    default: return;
                }
            };
        }
    }
}