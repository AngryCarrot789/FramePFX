// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.IO;
using System.Text;

namespace FramePFX.Utils {
    public static class FileUtils {
        public static string ChangeFileName(string path, string fileName) {
            string parent = Path.GetDirectoryName(path);
            return string.IsNullOrEmpty(parent) ? fileName : Path.Combine(parent, fileName);
        }

        public static string ChangeActualFileName(string path, string fileName) {
            string parent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parent)) {
                return fileName;
            }

            string extension = Path.GetExtension(path);
            return Path.Combine(parent, fileName + extension);
        }

        public static Encoding ParseEncoding(byte[] data, out int offset, out string encoding) {
            if (data[0] == 0x2b && data[1] == 0x2f && data[2] == 0x76) {
                offset = 3;
                encoding = "UTF7";
                return Encoding.UTF7;
            }
            else if (data[0] == 0xef && data[1] == 0xbb && data[2] == 0xbf) {
                offset = 3;
                encoding = "UTF8";
                return Encoding.UTF8;
            }
            else if (data[0] == 0xff && data[1] == 0xfe) {
                offset = 2;
                encoding = "UTF-16LE";
                return Encoding.Unicode;
            }
            else if (data[0] == 0xfe && data[1] == 0xff) {
                offset = 2;
                encoding = "UTF-16BE";
                return Encoding.BigEndianUnicode;
            }
            else if (data[0] == 0xff && data[1] == 0xfe && data[2] == 0 && data[3] == 0) {
                offset = 4;
                encoding = "UTF-32LE";
                return Encoding.UTF32;
            }
            else if (data[0] == 0 && data[1] == 0 && data[2] == 0xfe && data[3] == 0xff) {
                offset = 4;
                encoding = "UTF-32BE";
                return new UTF32Encoding(true, true);
            }
            else {
                offset = 0;
                encoding = null;
                return null;
            }
        }
    }
}