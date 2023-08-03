namespace FramePFX.Core {
    public interface ITranslator {
        bool TryGetString(out string output, string key);
        bool TryGetString(out string output, string key, params object[] formatParams);

        string GetString(string key);
        string GetString(string key, params object[] formatParams);
    }
}