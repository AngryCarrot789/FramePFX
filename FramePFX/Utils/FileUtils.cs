using System.Text;

namespace FramePFX.Utils
{
    public static class FileUtils
    {
        public static Encoding ParseEncoding(byte[] data, out int offset, out string encoding)
        {
            if (data[0] == 0x2b && data[1] == 0x2f && data[2] == 0x76)
            {
                offset = 3;
                encoding = "UTF7";
                return Encoding.UTF7;
            }
            else if (data[0] == 0xef && data[1] == 0xbb && data[2] == 0xbf)
            {
                offset = 3;
                encoding = "UTF8";
                return Encoding.UTF8;
            }
            else if (data[0] == 0xff && data[1] == 0xfe)
            {
                offset = 2;
                encoding = "UTF-16LE";
                return Encoding.Unicode;
            }
            else if (data[0] == 0xfe && data[1] == 0xff)
            {
                offset = 2;
                encoding = "UTF-16BE";
                return Encoding.BigEndianUnicode;
            }
            else if (data[0] == 0xff && data[1] == 0xfe && data[2] == 0 && data[3] == 0)
            {
                offset = 4;
                encoding = "UTF-32LE";
                return Encoding.UTF32;
            }
            else if (data[0] == 0 && data[1] == 0 && data[2] == 0xfe && data[3] == 0xff)
            {
                offset = 4;
                encoding = "UTF-32BE";
                return new UTF32Encoding(true, true);
            }
            else
            {
                offset = 0;
                encoding = null;
                return null;
            }
        }
    }
}