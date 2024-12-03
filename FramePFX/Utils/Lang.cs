namespace FramePFX.Utils;

public static class Lang {
    /// <summary>
    /// Returns "S" if count is not equal to 1, otherwise returns an empty string if count == 1
    /// </summary>
    public static string S(int count) => count == 1 ? "" : "s";

    public static string IsAre(int count) => count == 1 ? "is" : "are";

    public static string ThisThese(int count) => count == 1 ? "this" : "these";
}