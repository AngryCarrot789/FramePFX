using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Interactivity;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class ShapeSquareVideoClipViewModel : VideoClipViewModel {
        public new ShapeSquareVideoClip Model => (ShapeSquareVideoClip) base.Model;

        public float Width {
            get => this.Model.Width;
            set {
                this.ValidateNotInAutomationChange();
                if (AutomationUtils.GetNewKeyFrameTime(this, ShapeSquareVideoClip.WidthKey, out long frame)) {
                    this.AutomationData[ShapeSquareVideoClip.WidthKey].GetActiveKeyFrameOrCreateNew(frame).SetFloatValue(value);
                }
                else {
                    this.AutomationData[ShapeSquareVideoClip.WidthKey].GetOverride().SetFloatValue(value);
                }
            }
        }

        public float Height {
            get => this.Model.Height;
            set {
                this.ValidateNotInAutomationChange();
                if (AutomationUtils.GetNewKeyFrameTime(this, ShapeSquareVideoClip.HeightKey, out long frame)) {
                    this.AutomationData[ShapeSquareVideoClip.HeightKey].GetActiveKeyFrameOrCreateNew(frame).SetFloatValue(value);
                }
                else {
                    this.AutomationData[ShapeSquareVideoClip.HeightKey].GetOverride().SetFloatValue(value);
                }
            }
        }

        public ShapeSquareVideoClipViewModel(ShapeSquareVideoClip model) : base(model) {
            this.AutomationData.AssignRefreshHandler(ShapeSquareVideoClip.WidthKey, (s, e) => {
                ShapeSquareVideoClipViewModel clip = (ShapeSquareVideoClipViewModel) s.AutomationData.Owner;
                clip.RaisePropertyChanged(nameof(clip.Width));
                clip.InvalidateRenderForAutomationRefresh(in e);
            });
            this.AutomationData.AssignRefreshHandler(ShapeSquareVideoClip.HeightKey, (s, e) => {
                ShapeSquareVideoClipViewModel clip = (ShapeSquareVideoClipViewModel) s.AutomationData.Owner;
                clip.RaisePropertyChanged(nameof(clip.Height));
                clip.InvalidateRenderForAutomationRefresh(in e);
            });
        }

        public override bool CanDropResource(ResourceItemViewModel resource) {
            return resource is ResourceColourViewModel;
        }

        public override Task OnDropResource(ResourceItemViewModel resource, EnumDropType dropType) {
            this.Model.ResourceHelper.SetTargetResourceId(resource.UniqueId);
            return Task.CompletedTask;
        }
    }
}