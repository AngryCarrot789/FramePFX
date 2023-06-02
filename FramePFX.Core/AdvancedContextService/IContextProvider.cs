using System.Collections.Generic;

namespace FrameControlEx.Core.AdvancedContextService {
    public interface IContextProvider {
        void GetContext(List<IContextEntry> list);
    }
}