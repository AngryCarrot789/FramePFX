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

        public static readonly string ProjectTypeAndAllFiles =
            Filter.Of().AddFilter("FramePFX Project", "fpfx").AddAllFiles().ToString();

        public static readonly string VideoFormatsAndAll =
            Filter.Of().
                   AddFilter("MP4", "mp4").
                   AddFilter("MOV", "mov").
                   AddFilter("MKV", "mkv").
                   AddFilter("FLV", "flv").
                   AddAllFiles().
                   ToString();
    }
}