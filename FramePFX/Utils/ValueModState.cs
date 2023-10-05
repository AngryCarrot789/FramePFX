using System;

namespace FramePFX.Utils {
    public enum ValueModState {
        /// <summary>
        /// Value is now being modified
        /// </summary>
        Begin = 1,

        /// <summary>
        /// Value has finished being modified
        /// </summary>
        Finish = 2,

        /// <summary>
        /// Value modification was cancelled and should be reverted back to value before <see cref="Begin"/>
        /// </summary>
        Cancelled = 3
    }

    public static class ValueModStateExtensions {
        public static bool IsBegin(this ValueModState state) {
            return state == ValueModState.Begin;
        }

        public static bool IsFinish(this ValueModState state) {
            return state != ValueModState.Begin;
        }

        public static void Apply(this ValueModState state, ref bool isBeginState, Action isBegin, Action isFinish, Action isCancelled = null) {
            switch (state) {
                case ValueModState.Begin: {
                    if (!isBeginState) {
                        isBeginState = true;
                        isBegin?.Invoke();
                    }

                    break;
                }
                default: {
                    if (isBeginState) {
                        isBeginState = false;
                        if (state == ValueModState.Cancelled) {
                            (isCancelled ?? isFinish)?.Invoke();
                        }
                        else {
                            isFinish?.Invoke();
                        }
                    }

                    break;
                }
            }
        }
    }
}