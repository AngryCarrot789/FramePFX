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

using System.Text.RegularExpressions;

namespace PFXToolKitUI.Shortcuts;

/// <summary>
/// Manages multiple input states and only allows one to be active at a time. Instances of this class are managed by a <see cref="ShortcutManager"/>
/// </summary>
public class InputStateManager {
    private readonly List<InputStateEntry> inputStates;

    public ShortcutManager Manager { get; }

    /// <summary>
    /// This state manager's unique ID, relative to the manager
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// All of the input states that are managed. Any <see cref="InputStateEntry"/> instances in this list will also be in the <see cref="Group"/>'s <see cref="ShortcutGroupEntry.InputStates"/> collection
    /// </summary>
    public IReadOnlyList<InputStateEntry> InputStates => this.inputStates;

    public InputStateManager(ShortcutManager manager, string id) {
        this.Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        this.inputStates = new List<InputStateEntry>();
        this.Id = id;
    }

    // The input state that was active before another input state was activated
    private InputStateEntry lastActiveInput;

    // e.g. clicking a toggle button
    public Task OnInputStateTriggeredExternal(ShortcutInputProcessor inputProcessor, InputStateEntry stateEntry, bool activate) {
        return Task.CompletedTask;
    }

    // CBA to get it to work :'(

    /// <summary>
    /// Called by a <see cref="ShortcutInputProcessor"/> when an input state is triggered. The given group's <see cref="InputStateEntry.IsActive"/>
    /// will be the opposite of the given <see cref="isActive"/> parameter (e.g. when
    /// <see cref="isActive"/> is false, <see cref="InputStateEntry.IsActive"/> will be true)
    /// </summary>
    /// <param name="inputProcessor">The processor that caused this input state to be triggered</param>
    /// <param name="stateEntry">The input state to modify</param>
    /// <param name="activate">Whether or not to activate or deactivate the state</param>
    public void OnInputStateTriggered(ShortcutInputProcessor inputProcessor, InputStateEntry stateEntry, bool activate) {
        if (activate) {
            if (!stateEntry.IsActive) {
                foreach (InputStateEntry inputState in this.inputStates) {
                    if (inputState.IsActive) {
                        inputState.OnDeactivated(inputProcessor);
                    }
                }

                stateEntry.OnActivated(inputProcessor);
                this.lastActiveInput = stateEntry;
            }
        }
        else if (stateEntry.IsActive) {
            stateEntry.OnDeactivated(inputProcessor);
        }

        // if (true) { // !state.IsInputPressAndRelease()
        //     // this.activationTime = -1;
        //     // this.lastActiveInput = null;
        //     foreach (GroupedInputState inputState in this.inputStates) {
        //         if (inputState.IsActive) {
        //             await inputState.OnDeactivated(inputManager);
        //         }
        //     }
        //
        //     if (activate) {
        //         if (!state.IsActive) {
        //             await state.OnActivated(inputManager);
        //         }
        //     }
        //     else if (state.IsActive) {
        //         await state.OnDeactivated(inputManager);
        //     }
        // }
        // else {
        //     if (activate) {
        //         if (state.IsActive) {
        //             return;
        //         }
        //
        //         foreach (GroupedInputState inputState in this.inputStates) {
        //             if (inputState.IsActive) {
        //                 await inputState.OnDeactivated(inputManager);
        //             }
        //         }
        //
        //         await state.OnActivated(inputManager);
        //         this.lastActiveInput = this.activeInput;
        //         this.activeInput = state;
        //         this.activationTime = Time.GetSystemMillis();
        //     }
        //     else if (state.IsActive) {
        //         if (state != this.activeInput) {
        //             await state.OnDeactivated(inputManager);
        //         }
        //         else {
        //             long interval = Time.GetSystemMillis() - this.activationTime;
        //             if (interval < 500) {
        //                 this.activeInput = null;
        //             }
        //             else {
        //                 await state.OnDeactivated(inputManager);
        //                 this.activeInput = this.lastActiveInput;
        //                 this.lastActiveInput = null;
        //                 if (this.activeInput != null) {
        //                     await this.activeInput.OnActivated(inputManager);
        //                 }
        //             }
        //         }
        //     }
        // }
    }

    public bool Add(InputStateEntry stateEntry) {
        if (stateEntry.StateManager != null) {
            throw new Exception($"State ({stateEntry}) was already located in the state manager '{stateEntry.StateManager}'");
        }

        if (this.inputStates.Contains(stateEntry))
            return false;
        this.inputStates.Add(stateEntry);
        stateEntry.StateManager = this;
        return true;
    }
}