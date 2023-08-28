using System;
using AvalonDock.Themes;

namespace FramePFX.Themes.AvalonDock {
    /// <inheritdoc/>
    public class GeneralDockTheme : Theme {
        /// <inheritdoc/>
        public override Uri GetResourceUri() {
            return new Uri("/FramePFX;component/Themes/AvalonDock/GeneralDockTheme.xaml", UriKind.Relative);
        }
    }
}