using System.Windows;

namespace FramePFX.TestBinding {
    public static class SexMachines {
        public static readonly DependencyProperty SexMachineCollectionProperty =
            DependencyProperty.RegisterAttached(
                "SexMachineCollection",
                typeof(SexMachineCollection),
                typeof(SexMachines),
                new FrameworkPropertyMetadata(null));

        public static SexMachineCollection GetSexMachineCollection(DependencyObject element) => (SexMachineCollection) element.GetValue(SexMachineCollectionProperty);

        public static void SetSexMachineCollection(DependencyObject element, SexMachineCollection collection) => element.SetValue(SexMachineCollectionProperty, collection);
    }
}