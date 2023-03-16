namespace FramePFX.Utils {
    public static class Maths {
        public static double Map(double input, double inA, double inB, double outA, double outB) {
            return outA + ((outB - outA) / (inB - inA) * (input - inA));
        }
    }
}