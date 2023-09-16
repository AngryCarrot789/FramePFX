namespace FramePFX.Localisation {
    public readonly struct LocalisedString {
        /// <summary>
        /// This localised string's key
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// Parameters passed to the translation service in order to add extra details to the output message
        /// </summary>
        public readonly object[] Parameters;

        public string GetValue() {
            return this.Key;
        }
    }
}