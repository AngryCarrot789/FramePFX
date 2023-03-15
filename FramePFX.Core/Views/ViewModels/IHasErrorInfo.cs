using System.Collections.Generic;

namespace FramePFX.Core.Views.ViewModels {
    public interface IHasErrorInfo {
        /// <summary>
        /// A dictionary of errors currently present. The default behaviour is that a view is prevented from
        /// closing if any errors are present, or at least, cannot close with a successful result
        /// </summary>
        Dictionary<string, object> Errors { get; }
    }
}