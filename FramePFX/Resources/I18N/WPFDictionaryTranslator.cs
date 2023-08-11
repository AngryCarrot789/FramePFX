using System.Windows;
using FramePFX.Core;

namespace FramePFX.Resources.I18N {
    [ServiceImplementation(typeof(ITranslator))]
    public class WPFDictionaryTranslator : ITranslator {
        public ResourceDictionary Dictionary { get; set; }

        public WPFDictionaryTranslator() {
        }

        public bool TryGetString(out string output, string key) {
            return (output = this.Dictionary[key] as string) != null;
        }

        public bool TryGetString(out string output, string key, params object[] formatParams) {
            if (this.TryGetString(out output, key)) {
                output = string.Format(output, formatParams);
                return true;
            }

            return false;
        }

        public string GetString(string key) {
            return this.TryGetString(out string value, key) ? value : key;
        }

        public string GetString(string key, params object[] formatParams) {
            return this.TryGetString(out string value, key, formatParams) ? value : $"{key} [{string.Join(", ", formatParams)}]";
        }
    }
}