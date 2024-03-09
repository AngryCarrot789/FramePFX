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

using System;

namespace FramePFX.Utils
{
    public static class AudioUtils
    {
        public static double DbToVolume(double db) => Math.Pow(10d, 0.05d * db);
        public static float DbToVolume(float db) => (float) Math.Pow(10d, 0.05d * db);

        public static double VolumeToDb(double vol) => 20d * Math.Log10(vol);
        public static float VolumeToDb(float vol) => (float) (20d * Math.Log10(vol));
    }
}