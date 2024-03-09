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

namespace FramePFX.RBC
{
    public enum RBEType : byte
    {
        // In order for old data to be compatible, the existing type values
        // should not be modified. There can only be 255 different types of element (1-255)

        // when adding new types, a case must be added to the RBEBase.TypeToIdTable dictionary, RBEBase.CreateById and RBEBase.GetTypeById

        Unknown = 0,
        Dictionary = 1,
        List = 2,
        Byte = 3,
        Short = 4,
        Int = 5,
        Long = 6,
        Float = 7,
        Double = 8,
        String = 9,
        Struct = 10,
        ByteArray = 11,
        ShortArray = 12,
        IntArray = 13,
        LongArray = 14,
        FloatArray = 15,
        DoubleArray = 16,
        StringArray = 17,
        StructArray = 18,
        Guid = 19,
    }
}