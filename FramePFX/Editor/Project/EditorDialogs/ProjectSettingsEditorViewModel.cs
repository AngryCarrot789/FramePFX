using System.Collections.ObjectModel;
using FramePFX.Core.Editor;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Editor.Project.EditorDialogs {
    public class ProjectSettingsEditorViewModel : BaseConfirmableDialogViewModel {
        public static readonly ReadOnlyObservableCollection<string> FrameRates;

        private static readonly Rational[] rationals = new Rational[] {
            new Rational(10, 1),
            new Rational(12, 1),
            new Rational(15, 1),
            new Rational(24000, 1001), // 23.976
            new Rational(24, 1),
            new Rational(25, 1),
            new Rational(30000, 1001), // 29.97
            new Rational(30, 1),
            new Rational(50, 1),
            new Rational(60000, 1001), // 59,94
            new Rational(60, 1)
        };

        static ProjectSettingsEditorViewModel() {
            FrameRates = new ReadOnlyObservableCollection<string>(new ObservableCollection<string>() {
                "10.000 FPS",
                "12.000 FPS",
                "15.000 FPS",
                "23.976 FPS",
                "24.000 FPS",
                "25.000 FPS",
                "29.970 FPS",
                "30.000 FPS",
                "50.000 FPS",
                "59.940 FPS",
                "60.000 FPS",
            });
        }

        private int selectedIndex;
        public int SelectedIndex {
            get => this.selectedIndex;
            set {
                this.RaisePropertyChanged(ref this.selectedIndex, value);
                this.RaisePropertyChanged(nameof(this.SelectedRational));
            }
        }

        public Rational SelectedRational => rationals[Maths.Clamp(this.SelectedIndex, 0, rationals.Length - 1)];

        private int width;
        public int Width {
            get => this.width;
            set => this.RaisePropertyChanged(ref this.width, value);
        }

        private int height;
        public int Height {
            get => this.height;
            set => this.RaisePropertyChanged(ref this.height, value);
        }

        public ProjectSettingsEditorViewModel(IDialog dialog) : base(dialog) {
        }

        public ProjectSettingsModel ToSettings() {
            return new ProjectSettingsModel() {
                Resolution = new Resolution(this.width, this.height),
                FrameRate = this.SelectedRational
            };
        }

        public void SetSettings(ProjectSettingsModel settings) {
            this.Width = settings.Resolution.Width;
            this.Height = settings.Resolution.Height;

            double fps = settings.FrameRate.FPS;
            if (fps <= 10.000)
                this.SelectedIndex = 0;
            else if (fps <= 12.000)
                this.SelectedIndex = 1;
            else if (fps <= 15.000)
                this.SelectedIndex = 2;
            else if (fps <= 23.976)
                this.SelectedIndex = 3;
            else if (fps <= 24.000)
                this.SelectedIndex = 4;
            else if (fps <= 25.000)
                this.SelectedIndex = 5;
            else if (fps <= 29.970)
                this.SelectedIndex = 6;
            else if (fps <= 30.000)
                this.SelectedIndex = 7;
            else if (fps <= 50.000)
                this.SelectedIndex = 8;
            else if (fps <= 59.940)
                this.SelectedIndex = 9;
            else if (fps <= 60.000)
                this.SelectedIndex = 10;
            else
                this.SelectedIndex = 7;
        }
    }
}
