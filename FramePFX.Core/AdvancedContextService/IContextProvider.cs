using System.Collections.Generic;

namespace FramePFX.Core.AdvancedContextService {
    public interface IContextProvider {
        void GetContext(List<IContextEntry> list);
    }
}