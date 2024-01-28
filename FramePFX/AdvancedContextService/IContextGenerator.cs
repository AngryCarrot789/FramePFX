using System.Collections.Generic;
using FramePFX.Interactivity;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// An interface that defines a function for generating context entries that are appropriate for the given context data
    /// </summary>
    public interface IContextGenerator {
        /// <summary>
        /// Generates context entries and adds them into the list parameter. Leading, repeated and trailing separators are automatically filtered out
        /// </summary>
        /// <param name="list">The list in which entries should be added to</param>
        /// <param name="context">The context data available</param>
        void Generate(List<IContextEntry> list, IDataContext context);
    }
}