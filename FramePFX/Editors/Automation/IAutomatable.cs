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

using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.Automation {
    /// <summary>
    /// An interface implemented by an object which supports parameter automation
    /// </summary>
    public interface IAutomatable : IHaveTimeline {
        /// <summary>
        /// The automation data for this object, which stores a collection of automation
        /// sequences for storing the key frames for each type of automate-able parameters
        /// </summary>
        AutomationData AutomationData { get; }

        /// <summary>
        /// Checks if the given parameter has any key frames for this object
        /// </summary>
        /// <param name="parameter">The parameter to check</param>
        /// <returns>True or false</returns>
        bool IsAutomated(Parameter parameter);
    }
}