using FramePFX.Core.Views.Dialogs.FilePicking;

namespace FramePFX.Core.Utils {
    public static class Filters {
        public static readonly string ImageTypesAndAll =
            Filter.Of().
                   AddFilter("PNG File", "png").
                   AddFilter("JPEG", "jpg", "jpeg").
                   AddFilter("Bitmap", "bmp").
                   AddAllFiles().
                   ToString();

        public static readonly string FrameControlSceneDeckType = Filter.Of().AddFilter("Scene Deck", "fcsd").AddAllFiles().ToString();

        public static readonly string VideoFormatsAndAll =
            Filter.Of().
                   AddFilter("MP4 Container", "mp4").
                   AddAllFiles().
                   ToString();
    }
}