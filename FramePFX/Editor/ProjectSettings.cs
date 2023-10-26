using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor {
    public class ProjectSettings {
        #region Video

        public Rect2i Resolution;

        /// <summary>
        /// This project's time base, which is the rate at which everything is synchronised to. This could be 30 fps, 60 fps, etc
        /// </summary>
        public Rational TimeBase;

        /// <summary>
        /// The project's render quality which affects how good the image looks but higher quality takes longer (duh)
        /// </summary>
        public EnumRenderQuality Quality;

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
            this.Quality = EnumRenderQuality.UnspecifiedQuality;
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.Resolution = data.GetStruct<Rect2i>(nameof(this.Resolution));
            this.TimeBase = (Rational) data.GetULong(nameof(this.TimeBase));
            this.ChannelFormat = data.GetString(nameof(this.ChannelFormat));
            this.SampleRate = data.GetInt(nameof(this.SampleRate));
            this.BitRate = data.GetInt(nameof(this.BitRate));
            this.Quality = (EnumRenderQuality) data.GetByte(nameof(this.Quality), (byte) EnumRenderQuality.UnspecifiedQuality);
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetStruct(nameof(this.Resolution), this.Resolution);
            data.SetULong(nameof(this.TimeBase), (ulong) this.TimeBase);
            data.SetString(nameof(this.ChannelFormat), this.ChannelFormat);
            data.SetInt(nameof(this.SampleRate), this.SampleRate);
            data.SetInt(nameof(this.BitRate), this.BitRate);
            data.SetByte(nameof(this.Quality), (byte) this.Quality);
        }
    }
}