// 
// Copyright (c) 2024-2024 REghZy
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

namespace FramePFX.Utils;

public static class Filters {
    public static readonly string AllAndTextType =
        Filter.Of().
               AddAllFiles().
               AddFilter("Text File", "txt").
               ToString();

    public static readonly string ProjectType =
        Filter.Of().
               AddAllFiles().
               AddFilter("Frame PFX", "fpx").
               ToString();

    public static readonly string ImageTypesAndAll =
        Filter.Of().
               AddFilter("PNG File", "png").
               AddFilter("JPEG", "jpg", "jpeg").
               AddFilter("Bitmap", "bmp").
               AddAllFiles().
               ToString();
}