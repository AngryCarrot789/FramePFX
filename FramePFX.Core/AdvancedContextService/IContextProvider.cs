using System.Collections.Generic;
using FramePFX.Core.AdvancedContextService.Base;

namespace FramePFX.Core.AdvancedContextService {
    public interface IContextProvider {
        List<IContextEntry> GetContext(List<IContextEntry> list);
    }
}