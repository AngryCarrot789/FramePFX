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

namespace FramePFX.Utils.Collections.ObservableEx;

public delegate void ObservableListExChangedEventHandler<T>(IObservableListEx<T> list, ObservableListChangedEventArgs<T> e);

/// <summary>
/// A list implementation that invokes a series of events when the collection changes
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObservableListEx<T> : IList<T>
{
    event ObservableListExChangedEventHandler<T>? CollectionChanged;
}