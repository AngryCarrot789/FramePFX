using System.Collections.Generic;
using System.Linq;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Commands;
using FramePFX.Editor.History;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.History;
using FramePFX.PropertyEditing.Editors;

namespace FramePFX.Editor.PropertyEditors.Clips.Shapes {
    public class ShapeSquareDataEditorViewModel : HistoryAwarePropertyEditorViewModel {
        private readonly RefreshAutomationValueEventHandler RefreshWidthHandler;
        private readonly RefreshAutomationValueEventHandler RefreshHeightHandler;

        private float width;

        public float Width {
            get => this.width;
            set {
                float oldVal = this.width;
                this.width = value;
                bool useAddition = this.Handlers.Count > 1 && this.WidthEditStateChangedCommand.IsEditing;
                float change = value - oldVal;
                Transaction<float>[] array = ((HistorySquareWidth) this.WidthEditStateChangedCommand.HistoryAction)?.Values;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    ShapeSquareVideoClipViewModel clip = (ShapeSquareVideoClipViewModel) this.Handlers[i];
                    float val = useAddition ? (clip.Width + change) : value;
                    clip.Width = val;
                    if (array != null) {
                        array[i].Current = val;
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        private float height;

        public float Height {
            get => this.height;
            set {
                float oldVal = this.height;
                this.height = value;
                bool useAddition = this.Handlers.Count > 1 && this.HeightEditStateChangedCommand.IsEditing;
                float change = value - oldVal;
                Transaction<float>[] array = ((HistorySquareHeight) this.HeightEditStateChangedCommand.HistoryAction)?.Values;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    ShapeSquareVideoClipViewModel clip = (ShapeSquareVideoClipViewModel) this.Handlers[i];
                    float val = useAddition ? (clip.Height + change) : value;
                    clip.Height = val;
                    if (array != null) {
                        array[i].Current = val;
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        public RelayCommand ResetWidthCommand { get; }
        public RelayCommand ResetHeightCommand { get; }

        public EditStateCommand WidthEditStateChangedCommand { get; }
        public EditStateCommand HeightEditStateChangedCommand { get; }

        public ShapeSquareVideoClipViewModel SingleSelection => (ShapeSquareVideoClipViewModel) this.Handlers[0];
        public IEnumerable<ShapeSquareVideoClipViewModel> Clips => this.Handlers.Cast<ShapeSquareVideoClipViewModel>();


        public ShapeSquareDataEditorViewModel() : base(typeof(ShapeSquareVideoClip)) {
            this.RefreshWidthHandler = this.RefreshWidth;
            this.RefreshHeightHandler = this.RefreshHeight;
            this.ResetWidthCommand = new RelayCommand(() => this.Width = ShapeSquareVideoClip.WidthKey.Descriptor.DefaultValue);
            this.ResetHeightCommand = new RelayCommand(() => this.Height = ShapeSquareVideoClip.HeightKey.Descriptor.DefaultValue);
            this.WidthEditStateChangedCommand = new EditStateCommand(() => new HistorySquareWidth(this), "Modify width");
        }

        private void RefreshWidth(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.width, this.SingleSelection.Width, nameof(this.Width));
        }

        private void RefreshHeight(AutomationSequenceViewModel sender, RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(ref this.height, this.SingleSelection.Height, nameof(this.Height));
        }

        public void RequeryWidthFromHandlers() {
            this.width = GetEqualValue(this.Handlers, (x) => ((ShapeSquareVideoClipViewModel) x).Width, out float d) ? d : default;
            this.RaisePropertyChanged(nameof(this.Width));
        }

        public void RequeryHeightFromHandlers() {
            this.width = GetEqualValue(this.Handlers, (x) => ((ShapeSquareVideoClipViewModel) x).Height, out float d) ? d : default;
            this.RaisePropertyChanged(nameof(this.Height));
        }

        protected class HistorySquareWidth : HistoryBasicSingleProperty<ShapeSquareVideoClipViewModel, float> {
            public HistorySquareWidth(ShapeSquareDataEditorViewModel editor) : base(editor.Clips, (x) => x.Width, (a, b) => a.Width = b, editor.RequeryWidthFromHandlers) {
            }
        }

        protected class HistorySquareHeight : HistoryBasicSingleProperty<ShapeSquareVideoClipViewModel, float> {
            public HistorySquareHeight(ShapeSquareDataEditorViewModel editor) : base(editor.Clips, (x) => x.Height, (a, b) => a.Height = b, editor.RequeryHeightFromHandlers) {
            }
        }
    }
}