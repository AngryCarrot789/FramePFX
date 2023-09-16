using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Editor.ViewModels.Timelines.Effects;

namespace FramePFX.PropertyEditing.Editor.Editor.Effects {
    public abstract class BaseEffectDataEditorViewModel : HistoryAwarePropertyEditorViewModel {
        public IEnumerable<BaseEffectViewModel> Effects => this.Handlers.Cast<BaseEffectViewModel>();

        public override ApplicabilityMode ApplicabilityMode => ApplicabilityMode.Any;

        protected BaseEffectDataEditorViewModel(Type applicableType) : base(applicableType) {
        }
    }
}