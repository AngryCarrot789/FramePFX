using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class ProjectSettings : IRBESerialisable {
        #region Video

        public Resolution Resolution;

        /// <summary>
        /// This project's time base, which is the rate at which everything is synchronised to. This could be 30 fps, 60 fps, etc
        /// </summary>
        public Rational TimeBase;

        #endregion

        #region Audio

        public string ChannelFormat;
        public int SampleRate;
        public int BitRate;
        public int Channels;

        #endregion

        #region Video Plugins ???

        #endregion

        #region Audio Plugins (e.g. VST)

        #endregion

        public ProjectSettings() {
            this.TimeBase = Timecode.Fps30;
            this.ChannelFormat = "Stereo";
            this.SampleRate = 44100;
            this.BitRate = 16;
            this.Channels = 2;
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.Resolution = data.GetStruct<Resolution>(nameof(this.Resolution));
            this.TimeBase = (Rational) data.GetULong(nameof(this.TimeBase));
            this.ChannelFormat = data.GetString(nameof(this.ChannelFormat));
            this.SampleRate = data.GetInt(nameof(this.SampleRate));
            this.BitRate = data.GetInt(nameof(this.BitRate));
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetStruct(nameof(this.Resolution), this.Resolution);
            data.SetULong(nameof(this.TimeBase), (ulong) this.TimeBase);
            data.SetString(nameof(this.ChannelFormat), this.ChannelFormat);
            data.SetInt(nameof(this.SampleRate), this.SampleRate);
            data.SetInt(nameof(this.BitRate), this.BitRate);
        }
    }
}