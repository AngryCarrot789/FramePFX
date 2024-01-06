using System;
using System.Windows.Markup;

namespace FramePFX.WPF.DataTemplates {
    public class RegisteredTemplateType : MarkupExtension {
        [ConstructorArgument("manager")]
        public DataTemplateManager Manager { get; set; }

        public RegisteredTemplateType(DataTemplateManager manager) {
            this.Manager = manager;
        }

        public RegisteredTemplateType() {
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}