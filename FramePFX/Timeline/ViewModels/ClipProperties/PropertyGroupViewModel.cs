using System;
using FramePFX.Core;

namespace FramePFX.Timeline.ViewModels.ClipProperties {
    public class PropertyGroupViewModel : BaseViewModel {
        public string Header { get; }

        public PropertyGroupViewModel Parent { get; set; }

        public PropertyGroupViewModel(string header) {
            this.Header = string.IsNullOrWhiteSpace(header) ? throw new ArgumentNullException(nameof(header), "Value cannot be null/empty or whitespaces") : header;
        }

        public virtual void OnModified() {

        }
    }
}