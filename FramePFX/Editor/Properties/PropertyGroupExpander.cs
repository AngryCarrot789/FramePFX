using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FramePFX.Editor.Properties {
    public class PropertyGroupExpander : Expander {
        public static readonly DependencyProperty ResetCommandProperty = DependencyProperty.Register("ResetCommand", typeof(ICommand), typeof(PropertyGroupExpander), new PropertyMetadata());

        public ICommand ResetCommand {
            get => (ICommand) this.GetValue(ResetCommandProperty);
            set => this.SetValue(ResetCommandProperty, value);
        }
    }
}
