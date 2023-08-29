using System;
using System.Windows;

namespace FramePFX.WPF.Resources.I18N {
    public class LocalizationController {
        public static ResourceDictionary I18NText {
            get => Application.Current.Resources.MergedDictionaries[3];
            set => Application.Current.Resources.MergedDictionaries[3] = value;
        }

        public static void SetLang(LangType type) {
            Uri source = new Uri($"Resources/I18N/Text/{GetLangName(type)}.xaml", UriKind.Relative);
            ResourceDictionary dictionary = new ResourceDictionary() {
                Source = source
            };

            // just in case there's some weird dependency on the translator, reload that first
            ((WPFDictionaryTranslator) IoC.Translator).Dictionary = dictionary;
            I18NText = dictionary;
        }

        public static string GetLangName(LangType type) {
            switch (type) {
                case LangType.En: return "en";
                case LangType.De: return "de";
                default: throw new ArgumentOutOfRangeException(nameof(type), "Unknown lang type: " + type);
            }
        }
    }
}