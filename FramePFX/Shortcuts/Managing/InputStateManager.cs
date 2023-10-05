using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FramePFX.Shortcuts.Managing {
    /// <summary>
    /// Manages multiple input states and only allows one to be active at a time. Instances of this class are managed by a <see cref="ShortcutManager"/>
    /// </summary>
    public class InputStateManager {
        private readonly List<GroupedInputState> inputStates;

        public ShortcutManager Manager { get; }

        /// <summary>
        /// This state manager's unique ID, relative to the manager
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// All of the input states that are managed. Any <see cref="GroupedInputState"/> instances in this list will also be in the <see cref="Group"/>'s <see cref="ShortcutGroup.InputStates"/> collection
        /// </summary>
        public IReadOnlyList<GroupedInputState> InputStates => this.inputStates;

        public InputStateManager(ShortcutManager manager, string id) {
            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.inputStates = new List<GroupedInputState>();
            this.Id = id;
        }

        // public async Task OnInputStateTriggered(ShortcutProcessor processor, GroupedInputState state, bool isActive) {
        //     if (state == null)
        //         throw new ArgumentNullException(nameof(state));
        //     if (isActive) {
        //         // State is already activated, but the user tried to activate it again; can be ignored
        //         if (this.activeState != null) {
        //             if (this.activeState == state) {
        //                 if (state.IsActive) {
        //                     return;
        //                 }
        //                 else {
        //                     throw new Exception("Expected current state to be active. It was deactivated without the manager's notice");
        //                 }
        //             }
        //             else {
        //                 GroupedInputState lastActive = this.activeState;
        //                 this.lastState = lastActive;
        //                 this.activeState = state;
        //                 await lastActive.OnDeactivated();
        //             }
        //         }
        //         else {
        //             this.EnsureHasNoLastActivation();
        //             this.activeState = state;
        //         }
        //
        //         await state.OnActivated();
        //         this.LastActivationTime = Time.GetSystemMillis();
        //     }
        //     else {
        //         if (this.activeState == null) {
        //             this.EnsureHasNoLastActivation();
        //             if (state.IsActive)
        //                 throw new Exception("Expected active state (being deactivated) to equal the current state");
        //         }
        //         else {
        //             if (this.activeState != state) { // another state forcefully deactivated
        //                 this.EnsureHasNoLastActivation();
        //                 if (state.IsActive) // this means that a state was activated without the state manager being aware
        //                     throw new Exception("Expected state (being deactivated) to equal the current state");
        //             }
        //             else if (state.IsActive) {
        //                 if (this.lastState == null) {
        //                     // no previous activation available; just disable the current one and set the last one as the state
        //                     this.activeState = null;
        //                     this.lastState = state;
        //                     await state.OnDeactivated();
        //                 }
        //                 else if (this.LastActivationTime == -1) {
        //                     throw new Exception("Last execution time should have been valid");
        //                 }
        //                 else {
        //                     long time = Time.GetSystemMillis();
        //                     long interval = time - this.LastActivationTime;
        //                     await state.OnDeactivated();
        //                     if (interval > 400) {
        //                         // switch back to last state
        //                         this.activeState = this.lastState;
        //                         this.lastState = null;
        //                         await state.OnActivated();
        //                     }
        //
        //                     this.LastActivationTime = -1;
        //                 }
        //             }
        //             else {
        //                 // this shouldn't really happen; activeState should be null and line 67 should be handled
        //                 this.activeState = null;
        //                 this.lastState = null;
        //                 this.LastActivationTime = 0;
        //             }
        //         }
        //     }
        // }

        // The input state that was active before another input state was activated
        private GroupedInputState activeInput;
        private GroupedInputState lastActiveInput;
        private long activationTime;
        private bool isActivationStrokePressed;

        // e.g. clicking a toggle button
        public Task OnInputStateTriggeredExternal(ShortcutInputManager inputManager, GroupedInputState state, bool activate) {
            return Task.CompletedTask;
        }

        // CBA to get it to work :'(

        /// <summary>
        /// Called by a <see cref="ShortcutInputManager"/> when an input state is triggered. The given group's <see cref="GroupedInputState.IsActive"/>
        /// will be the opposite of the given <see cref="isActive"/> parameter (e.g. when
        /// <see cref="isActive"/> is false, <see cref="GroupedInputState.IsActive"/> will be true)
        /// </summary>
        /// <param name="inputManager">The processor that caused this input state to be triggered</param>
        /// <param name="state">The input state to modify</param>
        /// <param name="activate">Whether or not to activate or deactivate the state</param>
        public async Task OnInputStateTriggered(ShortcutInputManager inputManager, GroupedInputState state, bool activate) {
            if (activate) {
                if (!state.IsActive) {
                    foreach (GroupedInputState inputState in this.inputStates) {
                        if (inputState.IsActive) {
                            await inputState.OnDeactivated(inputManager);
                        }
                    }

                    await state.OnActivated(inputManager);
                    this.lastActiveInput = state;
                }
            }
            else if (state.IsActive) {
                await state.OnDeactivated(inputManager);
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

        public bool Add(GroupedInputState state) {
            if (state.StateManager != null) {
                throw new Exception($"State ({state}) was already located in the state manager '{state.StateManager}'");
            }

            if (this.inputStates.Contains(state))
                return false;
            this.inputStates.Add(state);
            state.StateManager = this;
            return true;
        }
    }
}