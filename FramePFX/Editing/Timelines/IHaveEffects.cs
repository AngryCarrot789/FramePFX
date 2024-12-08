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

using FramePFX.Editing.Timelines.Effects;

namespace FramePFX.Editing.Timelines;

public delegate void EffectOwnerEventHandler(IHaveEffects effects, BaseEffect effect, int index);

public delegate void EffectMovedEventHandler(IHaveEffects effects, BaseEffect effect, int oldIndex, int newIndex);

/// <summary>
/// An interface for an object that has effects associated with it. This is either a clip or track
/// </summary>
public interface IHaveEffects : IHaveTimeline
{
    /// <summary>
    /// Returns a list of effects. This collection is read-only so it cannot and also must not be modified directly
    /// </summary>
    IReadOnlyList<BaseEffect> Effects { get; }

    /// <summary>
    /// An event fired when an effect is added to this object
    /// </summary>
    event EffectOwnerEventHandler EffectAdded;

    /// <summary>
    /// An event fired when an effect is removed from this object
    /// </summary>
    event EffectOwnerEventHandler EffectRemoved;

    /// <summary>
    /// An event fired when an effect is moved from one index to another (within this owner)
    /// </summary>
    event EffectMovedEventHandler EffectMoved;

    /// <summary>
    /// Returns true if the given type of effect could be added, otherwise false
    /// </summary>
    /// <param name="effectType">The type of effect being added</param>
    /// <returns>True or false if the effect is applicable and could be added</returns>
    bool IsEffectTypeAccepted(Type effectType);

    /// <summary>
    /// Adds the given effect to this object
    /// </summary>
    /// <param name="effect">The effect to add</param>
    void AddEffect(BaseEffect effect);

    /// <summary>
    /// Inserts the effect at the given index
    /// </summary>
    /// <param name="index">The index of the effect</param>
    /// <param name="effect">The effect to add</param>
    void InsertEffect(int index, BaseEffect effect);

    /// <summary>
    /// Removes the effect from this object, if it exists
    /// </summary>
    /// <param name="effect">The effect to remove</param>
    /// <returns>True if the effect existed, otherwise false</returns>
    bool RemoveEffect(BaseEffect effect);

    /// <summary>
    /// Removes an effect at the given index
    /// </summary>
    /// <param name="index">The index of the effect to remove</param>
    void RemoveEffectAt(int index);

    /// <summary>
    /// Moves an effect at the old index to the new index
    /// </summary>
    /// <param name="oldIndex">The index of an effect to move</param>
    /// <param name="newIndex">The new index for that effect</param>
    void MoveEffect(int oldIndex, int newIndex);
}