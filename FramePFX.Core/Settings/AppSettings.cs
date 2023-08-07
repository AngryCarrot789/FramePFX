using FramePFX.Core.RBC;

namespace FramePFX.Core.Settings
{
    public class AppSettings : IRBESerialisable
    {
        // fields so that the view models can use these easier
        public bool UseVerticalTrackNumberDraggerBehaviour;
        public bool StopOnTogglePlay;

        /// <summary>
        /// Returns a new instance of the default app settings
        /// </summary>
        /// <returns></returns>
        public static AppSettings Defaults()
        {
            return new AppSettings() {
                UseVerticalTrackNumberDraggerBehaviour = false,
                StopOnTogglePlay = true
            };
        }

        public AppSettings()
        {
        }

        public AppSettings Clone()
        {
            AppSettings settings = new AppSettings();
            RBEDictionary dictionary = new RBEDictionary();
            this.WriteToRBE(dictionary);
            settings.ReadFromRBE(dictionary);
            return settings;
        }

        public void WriteToRBE(RBEDictionary data)
        {
            data.SetBool(nameof(this.UseVerticalTrackNumberDraggerBehaviour), this.UseVerticalTrackNumberDraggerBehaviour);
            data.SetBool(nameof(this.StopOnTogglePlay), this.StopOnTogglePlay);
        }

        public void ReadFromRBE(RBEDictionary data)
        {
            this.UseVerticalTrackNumberDraggerBehaviour = data.GetBool(nameof(this.UseVerticalTrackNumberDraggerBehaviour));
            this.StopOnTogglePlay = data.GetBool(nameof(this.StopOnTogglePlay));
        }
    }
}