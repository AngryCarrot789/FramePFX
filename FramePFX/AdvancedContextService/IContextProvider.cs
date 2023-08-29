using System.Collections.Generic;

namespace FramePFX.AdvancedContextService {
    public interface IContextProvider {
        void GetContext(List<IContextEntry> list);
    }
}