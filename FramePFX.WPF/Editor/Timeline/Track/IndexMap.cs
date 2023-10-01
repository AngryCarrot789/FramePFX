using System.Collections.Generic;

namespace FramePFX.WPF.Editor.Timeline.Track
{
    public class IndexMap<T>
    {
        public Dictionary<T, int> ValueToRealIndex { get; }
        public Dictionary<T, int> ValueToOrderedIndex { get; }
        public Dictionary<int, T> RealIndexToValue { get; }
        public List<T> OrderedValues { get; }

        public IndexMap(Dictionary<T, int> valueToRealIndex, Dictionary<int, T> realIndexToValue, Dictionary<T, int> valueToOrderedIndex, List<T> orderedValues)
        {
            this.ValueToRealIndex = valueToRealIndex;
            this.RealIndexToValue = realIndexToValue;
            this.ValueToOrderedIndex = valueToOrderedIndex;
            this.OrderedValues = orderedValues;
        }

        public int RealIndexOf(T value)
        {
            return this.ValueToRealIndex.TryGetValue(value, out int index) ? index : -1;
        }

        public int OrderedIndexOf(T value)
        {
            return this.ValueToOrderedIndex.TryGetValue(value, out int index) ? index : -1;
        }

        public int OrderedIndexToRealIndex(int ordered)
        {
            return ordered < 0 || ordered >= this.OrderedValues.Count ? -1 : this.RealIndexOf(this.OrderedValues[ordered]);
        }

        public bool OrderedIndexToValue(int ordered, out T value)
        {
            if (ordered < 0 || ordered >= this.OrderedValues.Count)
            {
                value = default;
                return false;
            }

            value = this.OrderedValues[ordered];
            return true;
        }
    }
}