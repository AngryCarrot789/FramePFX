using System.Collections.Generic;

namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// An interface for an object that can generate context entries from its own state
    /// </summary>
    public interface IContextProvider {
        void GetContext(List<IContextEntry> list);
    }
}