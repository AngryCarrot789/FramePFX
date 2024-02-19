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

using System.Collections.Generic;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.AdvancedContextService {
    /// <summary>
    /// An interface that defines a function for generating context entries that are appropriate for the given context data
    /// </summary>
    public interface IContextGenerator {
        /// <summary>
        /// Generates context entries and adds them into the list parameter. Leading, repeated and trailing separators are automatically filtered out
        /// </summary>
        /// <param name="list">The list in which entries should be added to</param>
        /// <param name="context">The context data available</param>
        void Generate(List<IContextEntry> list, IContextData context);
    }
}