using FrameControlEx.Core.Views.Dialogs.FilePicking;

namespace FrameControlEx.Core.Utils {
    public static class Filters {
        public static readonly string ImageTypesAndAll =
            Filter.Of().
                   Add("PNG File", "png").
                   Add("JPEG", "jpg", "jpeg").
                   Add("Bitmap", "bmp").
                   AddAllFiles().
                   ToString();

        public static readonly string FrameControlSceneDeckType = Filter.Of().Add("Scene Deck", "fcsd").AddAllFiles().ToString();

        public static readonly string VideoFormatsAndAll =
            Filter.Of().
                   Add("MP4 Container", "mp4").
                   AddAllFiles().
                   ToString();
    }
}