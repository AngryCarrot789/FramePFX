using FramePFX.Utils;

namespace FramePFX.Editor.Exporting.Exporters.FFMPEG {
    public static class VideoUtils {
        // "Resolve" function info can be found at:
        // https://ffmpeg.org/ffmpeg-utils.html#Time-duration

        public static bool ResolveRate(string input, out Rational rational) {
            switch (input.ToLower()) {
                case "ntsc-film":
                    rational = new Rational(24000, 1001);
                    break;
                case "film":
                    rational = new Rational(24, 1);
                    break;
                case "pal":
                case "qpal":
                case "spal":
                    rational = new Rational(25, 1);
                    break;
                case "ntsc":
                case "qntsc":
                case "sntsc":
                    rational = new Rational(30000, 1001);
                    break;
                default:
                    rational = default;
                    return false;
            }

            return true;
        }

        public static bool ResolveResolution(string input, out Rect2i res) {
            switch (input.ToLower()) {
                case "ntsc":
                    res = new Rect2i(720, 480);
                    break;
                case "pal":
                    res = new Rect2i(720, 576);
                    break;
                case "qntsc":
                    res = new Rect2i(352, 240);
                    break;
                case "qpal":
                    res = new Rect2i(352, 288);
                    break;
                case "sntsc":
                    res = new Rect2i(640, 480);
                    break;
                case "spal":
                    res = new Rect2i(768, 576);
                    break;
                case "film":
                    res = new Rect2i(352, 240);
                    break;
                case "ntsc-film":
                    res = new Rect2i(352, 240);
                    break;
                case "sqcif":
                    res = new Rect2i(128, 96);
                    break;
                case "qcif":
                    res = new Rect2i(176, 144);
                    break;
                case "cif":
                    res = new Rect2i(352, 288);
                    break;
                case "4cif":
                    res = new Rect2i(704, 576);
                    break;
                case "16cif":
                    res = new Rect2i(1408, 1152);
                    break;
                case "qqvga":
                    res = new Rect2i(160, 120);
                    break;
                case "qvga":
                    res = new Rect2i(320, 240);
                    break;
                case "vga":
                    res = new Rect2i(640, 480);
                    break;
                case "svga":
                    res = new Rect2i(800, 600);
                    break;
                case "xga":
                    res = new Rect2i(1024, 768);
                    break;
                case "uxga":
                    res = new Rect2i(1600, 1200);
                    break;
                case "qxga":
                    res = new Rect2i(2048, 1536);
                    break;
                case "sxga":
                    res = new Rect2i(1280, 1024);
                    break;
                case "qsxga":
                    res = new Rect2i(2560, 2048);
                    break;
                case "hsxga":
                    res = new Rect2i(5120, 4096);
                    break;
                case "wvga":
                    res = new Rect2i(852, 480);
                    break;
                case "wxga":
                    res = new Rect2i(1366, 768);
                    break;
                case "wsxga":
                    res = new Rect2i(1600, 1024);
                    break;
                case "wuxga":
                    res = new Rect2i(1920, 1200);
                    break;
                case "woxga":
                    res = new Rect2i(2560, 1600);
                    break;
                case "wqsxga":
                    res = new Rect2i(3200, 2048);
                    break;
                case "wquxga":
                    res = new Rect2i(3840, 2400);
                    break;
                case "whsxga":
                    res = new Rect2i(6400, 4096);
                    break;
                case "whuxga":
                    res = new Rect2i(7680, 4800);
                    break;
                case "cga":
                    res = new Rect2i(320, 200);
                    break;
                case "ega":
                    res = new Rect2i(640, 350);
                    break;
                case "hd480":
                    res = new Rect2i(852, 480);
                    break;
                case "hd720":
                    res = new Rect2i(1280, 720);
                    break;
                case "hd1080":
                    res = new Rect2i(1920, 1080);
                    break;
                case "2k":
                    res = new Rect2i(2048, 1080);
                    break;
                case "2kflat":
                    res = new Rect2i(1998, 1080);
                    break;
                case "2kscope":
                    res = new Rect2i(2048, 858);
                    break;
                case "4k":
                    res = new Rect2i(4096, 2160);
                    break;
                case "4kflat":
                    res = new Rect2i(3996, 2160);
                    break;
                case "4kscope":
                    res = new Rect2i(4096, 1716);
                    break;
                case "nhd":
                    res = new Rect2i(640, 360);
                    break;
                case "hqvga":
                    res = new Rect2i(240, 160);
                    break;
                case "wqvga":
                    res = new Rect2i(400, 240);
                    break;
                case "fwqvga":
                    res = new Rect2i(432, 240);
                    break;
                case "hvga":
                    res = new Rect2i(480, 320);
                    break;
                case "qhd":
                    res = new Rect2i(960, 540);
                    break;
                case "2kdci":
                    res = new Rect2i(2048, 1080);
                    break;
                case "4kdci":
                    res = new Rect2i(4096, 2160);
                    break;
                case "uhd2160":
                    res = new Rect2i(3840, 2160);
                    break;
                case "uhd4320":
                    res = new Rect2i(7680, 4320);
                    break;
                default:
                    res = default;
                    return false;
            }

            return true;
        }
    }
}