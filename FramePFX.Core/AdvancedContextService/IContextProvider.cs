using System.Collections.Generic;
using MCNBTViewer.Core.AdvancedContextService.Base;

namespace MCNBTViewer.Core.AdvancedContextService {
    public interface IContextProvider {
        List<IContextEntry> GetContext(List<IContextEntry> list);
    }
}