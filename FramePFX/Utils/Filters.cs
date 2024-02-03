namespace FramePFX.Utils {
    public static class Filters {
        public const string FramePFXExtension = "fpfx";
        public const string DotFramePFXExtension = "." + FramePFXExtension;

        public static readonly string ImageTypesAndAll =
            Filter.Of().
                   AddFilter("PNG File", "png").
                   AddFilter("JPEG", "jpg", "jpeg").
                   AddFilter("Bitmap", "bmp").
                   AddAllFiles().
                   ToString();

        public static readonly string ProjectTypeAndAllFiles = Filter.Of().AddFilter("FramePFX Project", FramePFXExtension).AddAllFiles().ToString();

        public static readonly string ProjectType = Filter.Of().AddFilter("FramePFX Project", FramePFXExtension).ToString();

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