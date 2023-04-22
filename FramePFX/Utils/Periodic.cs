namespace FramePFX.Utils {
    // https://www.youtube.com/watch?v=gsOtwF2sOLc :D
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