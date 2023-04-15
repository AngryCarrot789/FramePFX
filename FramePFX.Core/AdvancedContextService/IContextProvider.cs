using System.Collections.Generic;
using SharpPadV2.Core.AdvancedContextService.Base;

namespace SharpPadV2.Core.AdvancedContextService {
    public interface IContextProvider {
        List<IContextEntry> GetContext(List<IContextEntry> list);
    }
}