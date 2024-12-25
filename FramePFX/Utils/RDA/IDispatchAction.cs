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

namespace FramePFX.Utils.RDA;

/// <summary>
/// An interface that represents a parameter-less object that signals work to be dispatched somewhere at some point.
/// Implementations so far are <see cref="RapidDispatchAction"/> and <see cref="RapidDispatchActionEx"/>
/// </summary>
public interface IDispatchAction {
    /// <summary>
    /// Tries to schedule this RDA for execution with its dispatcher
    /// </summary>
    void InvokeAsync();
}

/// <summary>
/// A parameterised version of <see cref="IDispatchAction"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDispatchAction<in T> {
    /// <summary>
    /// Tries to schedule this RDA for execution with its dispatcher
    /// </summary>
    /// <param name="param">The new parameter to use during execution</param>
    void InvokeAsync(T param);
}