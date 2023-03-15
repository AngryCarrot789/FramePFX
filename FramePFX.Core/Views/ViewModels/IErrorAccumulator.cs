using System.Collections.Generic;

namespace FramePFX.Core.Views.ViewModels {
    public interface IErrorAccumulator {
        void Accumulate(Dictionary<string, object> errors);
    }
}