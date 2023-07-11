using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Pages;
using FramePFX.Core.Editor.ViewModels.Timelines.Clips.Pages;

namespace FramePFX.Editor.Properties {
    public class PropertyPageDataTemplateSelector : DataTemplateSelector {
        // Here goes every single registered "property pageable" type

        #region Clips

        public DataTemplate ClipPageTemplate { get; set; }
        public DataTemplate VideoClipPageTemplate { get; set; }
        public DataTemplate ShapeClipPageTemplate { get; set; }
        public DataTemplate TextClipPageTemplate { get; set; }
        public DataTemplate ImageClipPageTemplate { get; set; }
        public DataTemplate MediaClipPageTemplate { get; set; }

        #endregion

        #region Resources

        public DataTemplate BaseResourcePageTemplate { get; set; }
        public DataTemplate ResourceItemPageTemplate { get; set; }
        public DataTemplate ResourceColourPageTemplate { get; set; }

        #endregion

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                #region Clips
                case ClipPageViewModel _: return this.ClipPageTemplate;
                case VideoClipPageViewModel _: return this.VideoClipPageTemplate;
                case ShapeClipPageViewModel _: return this.ShapeClipPageTemplate;
                case TextClipPageViewModel _: return this.TextClipPageTemplate;
                case ImageClipPageViewModel _: return this.ImageClipPageTemplate;
                case MediaClipPageViewModel _: return this.MediaClipPageTemplate;
                #endregion

                #region Properties
                case BaseResourcePageViewModel _: return this.BaseResourcePageTemplate;
                case ResourceItemPageViewModel _: return this.ResourceItemPageTemplate;
                case ColourResourcePageViewModel _: return this.ResourceColourPageTemplate;
                #endregion
            }

            return base.SelectTemplate(item, container);
        }
    }
}
