using System;
using System.Collections.Generic;
using FramePFX.AdvancedContextService;
using FramePFX.Commands;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Effects;
using FramePFX.PropertyEditing;

namespace FramePFX.Editor.PropertyEditors.Effects {
    public class EffectListPropertyGroupViewModel : DynamicPropertyGroupViewModel {
        public RelayCommand AddMotionEffectCommand { get; }

        private readonly Func<bool> CanExecuteSingleSelection;
        private ClipViewModel handler;

        public EffectListPropertyGroupViewModel() : base(typeof(BaseEffectViewModel)) {
            this.IsExpanded = true;
            this.DisplayName = "Effects";
            this.IsSelected = true;
            this.IsHeaderBold = true;
            this.IsVisibleWhenNotApplicable = true;

            this.CanExecuteSingleSelection = () => this.CurrentMode == DynamicMode.SingleHandlerPerSubGroup || this.handler != null;
            this.AddMotionEffectCommand = new RelayCommand(() => {
                if (!this.CanExecuteSingleSelection())
                    return;
                this.handler.AddEffect(EffectFactory.Instance.CreateViewModelFromModel(new MotionEffect()));
            }, this.CanExecuteSingleSelection);
        }

        public override void SetupHierarchyState(IReadOnlyList<object> input) {
            base.SetupHierarchyState(input);
            if (this.CurrentMode == DynamicMode.SingleHandlerPerSubGroup) {
                this.handler = ((BaseEffectViewModel) input[0]).OwnerClip;
            }
            else if (this.Parent.Handlers?.Count == 1) {
                this.handler = (ClipViewModel) this.Parent.Handlers[0];
                this.IsCurrentlyApplicable = true;
            }

            this.AddMotionEffectCommand.RaiseCanExecuteChanged();
        }

        public override void ClearHierarchyState() {
            base.ClearHierarchyState();
            this.handler = null;
            this.AddMotionEffectCommand.RaiseCanExecuteChanged();
        }

        public override void GetContext(List<IContextEntry> list) {
            base.GetContext(list);
            if (list.Count > 0) {
                list.Add(SeparatorEntry.Instance);
            }

            list.Add(new CommandContextEntry("Add Motion Effect", this.AddMotionEffectCommand));
        }
    }
}