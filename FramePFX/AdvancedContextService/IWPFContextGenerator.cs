using System.Collections.Generic;
using System.Windows;
using FramePFX.Core.AdvancedContextService;

namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// An interface for generating context entries based on a target object and its context
    /// </summary>
    public interface IWPFContextGenerator : IContextGenerator {
        /// <summary>
        /// Generates context entries and adds them into the list parameter. Leading, repeated and trailing separators are automatically filtered out.
        /// <para>
        /// This function is preferred over <see cref="IContextGenerator.Generate"/> because it is more specific and
        /// predictable in terms of the context entries generated
        /// </para>
        /// </summary>
        /// <param name="list">The list in which entries should be added to</param>
        /// <param name="sender">The control whose context menu is being opened (typically the one whose generator property is set in XAML)</param>
        /// <param name="target">The actual target element which was clicked (determined via hit testing). This is typically a child of the sender parameter</param>
        /// <param name="context">The data context of the target element (for convenience)</param>
        void Generate(List<IContextEntry> list, DependencyObject sender, DependencyObject target, object context);
    }
}