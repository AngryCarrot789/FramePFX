using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class ProjectSettings : IRBESerialisable {
        #region Video

        public Resolution Resolution { get; set; }

        public Rational FrameRate { get; set; }

        #endregion

        #region Audio

        public string ChannelFormat { get; set; }

        public int SampleRate { get; set; }

        public int BitRate { get; set; }

        public int Channels { get; set; }

        #endregion

        #region Video Plugins ???

        #endregion

        #region Audio Plugins (e.g. VST)

        #endregion

        public ProjectSettings() {
            this.FrameRate = Rational.Fps30;

            this.ChannelFormat = "Stereo";
            this.SampleRate = 44100;
            this.BitRate = 16;
            this.Channels = 2;
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.Resolution = data.GetStruct<Resolution>(nameof(this.Resolution));
            this.FrameRate = data.GetStruct<Rational>(nameof(this.FrameRate));
            this.ChannelFormat = data.GetString(nameof(this.ChannelFormat));
            this.SampleRate = data.GetInt(nameof(this.SampleRate));
            this.BitRate = data.GetInt(nameof(this.BitRate));
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetStruct(nameof(this.Resolution), this.Resolution);
            data.SetStruct(nameof(this.FrameRate), this.FrameRate);
            data.SetString(nameof(this.ChannelFormat), this.ChannelFormat);
            data.SetInt(nameof(this.SampleRate), this.SampleRate);
            data.SetInt(nameof(this.BitRate), this.BitRate);
        }
    }
}