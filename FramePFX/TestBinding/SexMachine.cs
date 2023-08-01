using System;
using System.ComponentModel;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace FramePFX.TestBinding {
    public class SexMachine : Freezable {
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(
                "DisplayName",
                typeof(string),
                typeof(SexMachine),
                new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {

        }

        public string DisplayName {
            get { return (string) this.GetValue(DisplayNameProperty); }
            set { this.SetValue(DisplayNameProperty, value); }
        }

        public SexMachine() {

        }

        protected override Freezable CreateInstanceCore() => new SexMachine();

        protected override void CloneCore(Freezable sourceFreezable) {
            base.CloneCore(sourceFreezable);
        }

        protected override void CloneCurrentValueCore(Freezable sourceFreezable) {
            base.CloneCurrentValueCore(sourceFreezable);
        }

        protected override void GetAsFrozenCore(Freezable sourceFreezable) {
            base.GetAsFrozenCore(sourceFreezable);
        }

        protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable) {
            base.GetCurrentValueAsFrozenCore(sourceFreezable);
        }
    }
}