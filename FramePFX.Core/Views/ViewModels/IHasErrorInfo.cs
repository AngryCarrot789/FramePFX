using System.Collections.Generic;

namespace FramePFX.Core.Views.ViewModels
{
    public interface IHasErrorInfo
    {
        /// <summary>
        /// A dictionary of errors currently present. The default behaviour is that a view is prevented from
        /// closing if any errors are present, or at least, cannot close with a successful result
        /// </summary>
        Dictionary<string, object> Errors { get; }

        /// <summary>
        /// A shorthand way, and possibly more efficient way, of checking if <see cref="Errors"/> will contain any elements when fetched
        /// </summary>
        bool HasAnyErrors { get; }
    }
}