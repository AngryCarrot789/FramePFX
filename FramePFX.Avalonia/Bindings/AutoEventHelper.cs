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

using System;
using System.Reflection;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Bindings;

/// <summary>
/// A struct that generates an event handler (in the form of an action) from almost any event
/// </summary>
public readonly struct AutoEventHelper {
    public readonly EventInfo EventInfo;
    public readonly Delegate HandlerDelegate;

    public AutoEventHelper(string eventName, Type modelType, Action callback) {
        Validate.NotNull(eventName);

        EventInfo? info = modelType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
        if (info == null)
            throw new Exception("Could not find event by name: " + modelType.Name + "." + eventName);

        Type handlerType = info.EventHandlerType ?? throw new Exception("Missing event handler type");

        this.EventInfo = info;
        this.HandlerDelegate = EventUtils.CreateDelegateToInvokeActionFromEvent(handlerType, callback);
    }

    public void AddEventHandler(object model) {
        this.EventInfo.AddEventHandler(model, this.HandlerDelegate);
    }

    public void RemoveEventHandler(object model) {
        this.EventInfo.RemoveEventHandler(model, this.HandlerDelegate);
    }
}