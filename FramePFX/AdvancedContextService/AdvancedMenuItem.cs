using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FramePFX.Core;
using FramePFX.Resources;

namespace FramePFX.AdvancedContextService {
    public class AdvancedMenuItem : MenuItem {
        private object currentItem;

        public static readonly DependencyProperty IconTypeProperty =
            DependencyProperty.Register(
                "IconType",
                typeof(IconType),
                typeof(AdvancedMenuItem),
                new PropertyMetadata(IconType.None, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is AdvancedMenuItem item) {
                if (IconTypeToImageSourceConverter.IconTypeToImageSource((IconType) e.NewValue) is ImageSource x) {
                    Image image = new Image {
                        Source = x, Stretch = Stretch.Uniform,
                        SnapsToDevicePixels = true,
                        UseLayoutRounding = true
                    };

                    RenderOptions.SetEdgeMode(image, EdgeMode.Aliased);
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.LowQuality);
                    item.UseLayoutRounding = true;
                    item.Icon = image;
                }
            }
        }

        public IconType IconType {
            get => (IconType) this.GetValue(IconTypeProperty);
            set => this.SetValue(IconTypeProperty, value);
        }

        static AdvancedMenuItem() {

        }

        public AdvancedMenuItem() {

        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            if (item is MenuItem || item is Separator)
                return true;
            this.currentItem = item;
            return false;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            object item = this.currentItem;
            this.currentItem = null;
            if (this.UsesItemContainerTemplate) {
                DataTemplate dataTemplate = this.ItemContainerTemplateSelector.SelectTemplate(item, this);
                if (dataTemplate != null) {
                    object obj = dataTemplate.LoadContent();
                    if (obj is MenuItem || obj is Separator) {
                        return (DependencyObject) obj;
                    }

                    throw new InvalidOperationException("Invalid data template object: " + obj);
                }
            }

            return AdvancedContextMenu.CreateChildMenuItem(item);
        }
    }
}