using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips.Pages;

namespace FramePFX.Editor.Properties {
    public class PropertyPageDataTemplateSelector : DataTemplateSelector {
        // Here goes every single registered "property pageable" type

        public DataTemplate ClipPageTemplate { get; set; }
        public DataTemplate VideoClipPageTemplate { get; set; }
        public DataTemplate ShapeClipPageTemplate { get; set; }
        public DataTemplate TextClipPageTemplate { get; set; }
        public DataTemplate ImageClipPageTemplate { get; set; }
        public DataTemplate MediaClipPageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                case ClipPageViewModel _:       return this.ClipPageTemplate;
                case VideoClipPageViewModel _:  return this.VideoClipPageTemplate;
                case ShapeClipPageViewModel _:  return this.ShapeClipPageTemplate;
                case TextClipPageViewModel _:   return this.TextClipPageTemplate;
                case ImageClipPageViewModel _:  return this.ImageClipPageTemplate;
                case MediaClipPageViewModel _:  return this.MediaClipPageTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
