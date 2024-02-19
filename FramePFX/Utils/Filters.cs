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