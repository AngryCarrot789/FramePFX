using System.Collections.ObjectModel;
using FramePFX.Core.Editor;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Editor.Project.EditorDialogs {
    public class ProjectSettingsEditorViewModel : BaseConfirmableDialogViewModel {
        private static readonly ObservableCollection<string> ActualFrameRates;
        public static readonly ReadOnlyObservableCollection<string> FrameRates;

        private static readonly Rational[] rationals = new Rational[] {
            Timecode.Fps10,
            Timecode.Fps12,
            Timecode.Fps15,
            Timecode.Fps18,
            Timecode.Fps23_976,
            Timecode.Fps24,
            Timecode.Fps25,
            Timecode.Fps29_970,
            Timecode.Fps30,
            Timecode.Fps50,
            Timecode.Fps59_940,
            Timecode.Fps60,
            Timecode.Fps74_925,
            Timecode.Fps75,
            Timecode.Fps119_88,
            Timecode.Fps120,
            Timecode.Fps143_86,
            Timecode.Fps144,
            Timecode.Fps239_76,
            Timecode.Fps240_00
        };

        static ProjectSettingsEditorViewModel() {
            FrameRates = new ReadOnlyObservableCollection<string>(ActualFrameRates = new ObservableCollection<string>() {
                "10.00", "12.00", "15.00", "18.00", "23.976", "24.00",
                "25.00", "29.97", "30.00", "50.00", "59.94", "60.00",
                "74.925", "75.00", "119.88", "120.00",
                "143.86", "144.00", "239.76", "240.00"
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

        // private string userInputText;
        // public string UserInputText {
        //     get => this.userInputText;
        //     set {
        //         if (this.FrameRateModificationLock.IsRunning) {
        //             this.RaisePropertyChanged();
        //             return;
        //         }
        //         if (string.IsNullOrWhiteSpace(value)) {
        //             this.RaisePropertyChanged(ref this.userInputText, value);
        //         }
        //         else {
        //             if (!FrameRates.Contains(value)) {
        //                 if (double.TryParse(value, out double _)) {
        //                     ActualFrameRates.Add(value);
        //                 }
        //                 else {
        //                     string oldText = this.userInputText;
        //                     this.FrameRateModificationLock.Execute(async () => {
        //                         await IoC.MessageDialogs.ShowMessageAsync("Invalid FPS", "The given value is not a valid number: " + value);
        //                         this.userInputText = oldText;
        //                         this.RaisePropertyChanged(nameof(this.UserInputText));
        //                     });
        //                     this.RaisePropertyChanged();
        //                     return;
        //                 }
        //             }
        //             this.RaisePropertyChanged(ref this.userInputText, value);
        //         }
        //     }
        // }

        // public Rational SelectedRational {
        //     get {
        //         if (this.SelectedIndex < 0) {
        //             return rationals[0];
        //         }
        //         else if (this.SelectedIndex > (rationals.Length - 1)) {
        //             return Rational.FromDouble(double.Parse(ActualFrameRates[this.SelectedIndex]));
        //         }
        //         else {
        //             return rationals[this.SelectedIndex];
        //         }
        //     }
        // }

        public Rational SelectedRational => rationals[this.SelectedIndex];

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

        // public AsyncLock FrameRateModificationLock { get; } = new AsyncLock();

        public ProjectSettingsEditorViewModel(IDialog dialog) : base(dialog) {

        }

        public ProjectSettings ToSettings() {
            return new ProjectSettings() {
                Resolution = new Resolution(this.width, this.height),
                TimeBase = this.SelectedRational
            };
        }

        public void SetSettings(ProjectSettings settings) {
            this.Width = settings.Resolution.Width;
            this.Height = settings.Resolution.Height;

            Rational fps = settings.TimeBase;
            for (int i = 0; i < rationals.Length; i++) {
                if (fps <= rationals[i]) {
                    this.SelectedIndex = i;
                    return;
                    //goto end;
                }
            }

            this.SelectedIndex = 7;
            // end:
            // this.userInputText = ActualFrameRates[this.SelectedIndex];
            // this.RaisePropertyChanged(nameof(this.UserInputText));
        }
    }
}
