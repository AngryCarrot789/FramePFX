using System;
using System.Collections;
using System.Collections.Generic;

namespace FramePFX.Core.Shortcuts.Inputs {
    public class InputStrokeSet : IEnumerable<IInputStroke> {
        private readonly IInputStroke[] inputs;

        public InputStrokeSet(IInputStroke[] inputs) {
            this.inputs = new IInputStroke[inputs.Length];
            for (int i = 0; i < inputs.Length; i++) {
                this.inputs[i] = inputs[i] ?? throw new ArgumentException($"Array contains a null element at index {i}", nameof(inputs));
            }
        }

        public bool AnyMatch(IInputStroke input) {
            foreach (IInputStroke stroke in this.inputs) {
                if (stroke.Equals(input)) {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator<IInputStroke> GetEnumerator() {
            foreach (IInputStroke inputStroke in this.inputs) {
                yield return inputStroke;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.inputs.GetEnumerator();
        }
    }

    public class InputStrokeSet<T> : IEnumerable<T> where T : IInputStroke {
        private readonly T[] inputs;

        public InputStrokeSet(T[] inputs) {
            this.inputs = new T[inputs.Length];
            for (int i = 0; i < inputs.Length; i++) {
                this.inputs[i] = inputs[i] is T t ? t : throw new ArgumentException($"Array contains a null or invalid element at index {i}", nameof(inputs));
            }
        }

        public bool AnyMatch(T input) {
            foreach (T stroke in this.inputs) {
                if (input.Equals(stroke)) {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator<T> GetEnumerator() {
            foreach (T inputStroke in this.inputs) {
                yield return inputStroke;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.inputs.GetEnumerator();
        }
    }
}