using System;
using System.Collections.Generic;
using FramePFX.AdvancedContextService;
using FramePFX.Commands;
using FramePFX.Editor.ViewModels.Timelines.Effects;
using FramePFX.PropertyEditing;

namespace FramePFX.Editor.PropertyEditors.Effects {
    /// <summary>
    /// A fixed property group for an effect
    /// </summary>
    public class EffectPropertyGroupViewModel : FixedPropertyGroupViewModel {
        private BaseEffectViewModel handler;

        private RelayCommand removeEffectCommand;
        public RelayCommand RemoveEffectCommand {
            get {
                if (this.removeEffectCommand == null)
                    this.removeEffectCommand = new RelayCommand(
                        () => this.handler.OwnerClip?.RemoveEffect(this.handler),
                        () => this.ParentMode == DynamicMode.SingleHandlerPerSubGroup && this.handler?.OwnerClip != null);
                return this.removeEffectCommand;
            }
        }

        private DynamicMode ParentMode => this.Parent is DynamicPropertyGroupViewModel parent ? parent.CurrentMode : DynamicMode.Inactive;

        public EffectPropertyGroupViewModel(Type applicableType) : base(applicableType) {
        }

        public override void SetupHierarchyState(IReadOnlyList<object> input) {
            base.SetupHierarchyState(input);
            if (this.ParentMode == DynamicMode.SingleHandlerPerSubGroup) {
                this.handler = (BaseEffectViewModel) input[0];
            }
        }

        public override void ClearHierarchyState() {
            this.handler = null;
            base.ClearHierarchyState();
        }

        public override void GetContext(List<IContextEntry> list) {
            base.GetContext(list);
            if (!(this.Parent is DynamicPropertyGroupViewModel parent) || parent.CurrentMode != DynamicMode.SingleHandlerPerSubGroup) {
                return;
            }

            if (list.Count > 0) {
                list.Add(SeparatorEntry.Instance);
            }

            list.Add(new CommandContextEntry("Remove Effect", this.RemoveEffectCommand));
        }
    }
}