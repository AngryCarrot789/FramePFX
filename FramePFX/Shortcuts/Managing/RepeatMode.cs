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

namespace FramePFX.Shortcuts.Managing {
    public enum RepeatMode {
        /// <summary>
        /// Repeated input strokes are ignored
        /// </summary>
        Ignored,

        /// <summary>
        /// Input strokes that were not repeated are accepted (as in, when a user holds down a key,
        /// this requires that only that first key stroke should be processed, and any
        /// proceeding key strokes that are not release key strokes should be ignored)
        /// </summary>
        NonRepeat,

        /// <summary>
        /// Input strokes that are only processable when they are repeated. No real reason to handle this but it's a possibility
        /// </summary>
        RepeatOnly
    }
}