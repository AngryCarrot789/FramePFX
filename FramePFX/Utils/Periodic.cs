namespace FramePFX.Utils {
    public static class Periodic {
        public static long Add(long lhs, long rhs, long min, long max) {
            long result = lhs + rhs;
            long range = max - min;
            while (result >= max)
                result -= range;
            while (result < min)
                result += range;
            return result;
        }
    }
}