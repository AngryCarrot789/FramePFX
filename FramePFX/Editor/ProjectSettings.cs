using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor {
    public delegate void ProjectSettingChangedEventHandler(ProjectSettings settings, string propertyName);

    public class ProjectSettings {
        public static IList<EnumRenderQuality> RenderQualities { get; }

        #region Video

        private Rect2i resolution;
        private Rational frameRate;
        private EnumRenderQuality quality;

        /// <summary>
        /// Gets the resolution of the preview, and default resolution of the export (export resolution cannot be changed directly yet)
        /// </summary>
        public Rect2i Resolution {
            get => this.resolution;
            set => this.OnPropertyChanged(ref this.resolution, value);
        }

        /// <summary>
        /// This project's time base, which is the rate at which everything is synchronised to. This could be 30 fps, 60 fps, etc
        /// </summary>
        public Rational FrameRate {
            get => this.frameRate;
            set => this.OnPropertyChanged(ref this.frameRate, value);
        }

        /// <summary>
        /// The project's render quality which affects how good the image looks but higher quality takes longer (duh)
        /// </summary>
        public EnumRenderQuality Quality {
            get => this.quality;
            set => this.OnPropertyChanged(ref this.quality, value);
        }

        #endregion

        #region Audio

        public string ChannelFormat {get;set;}
        public int SampleRate {get;set;}
        public int BitRate {get;set;}
        public int Channels {get;set;}

        #endregion

        #region Video Plugins ???

        #endregion

        #region Audio Plugins (e.g. VST)

        #endregion

        /// <summary>
        /// An event called when the video settings for this instance changes
        /// </summary>
        public event ProjectSettingChangedEventHandler SettingChanged;

        public ProjectSettings() {
            this.FrameRate = Timecode.Fps30;
            this.ChannelFormat = "Stereo";
            this.SampleRate = 44100;
            this.BitRate = 16;
            this.Channels = 2;
            this.Quality = EnumRenderQuality.UnspecifiedQuality;
        }

        static ProjectSettings() {
            RenderQualities = new List<EnumRenderQuality>() {
                EnumRenderQuality.UnspecifiedQuality,
                EnumRenderQuality.Low,
                EnumRenderQuality.Medium,
                EnumRenderQuality.High
            }.AsReadOnly();
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.Resolution = data.GetStruct<Rect2i>(nameof(this.Resolution));
            this.FrameRate = (Rational) data.GetULong(nameof(this.FrameRate));
            this.ChannelFormat = data.GetString(nameof(this.ChannelFormat));
            this.SampleRate = data.GetInt(nameof(this.SampleRate));
            this.BitRate = data.GetInt(nameof(this.BitRate));
            this.Quality = (EnumRenderQuality) data.GetByte(nameof(this.Quality), (byte) EnumRenderQuality.UnspecifiedQuality);
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetStruct(nameof(this.Resolution), this.Resolution);
            data.SetULong(nameof(this.FrameRate), (ulong) this.FrameRate);
            data.SetString(nameof(this.ChannelFormat), this.ChannelFormat);
            data.SetInt(nameof(this.SampleRate), this.SampleRate);
            data.SetInt(nameof(this.BitRate), this.BitRate);
            data.SetByte(nameof(this.Quality), (byte) this.Quality);
        }

        private void OnPropertyChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            field = value;
            this.SettingChanged?.Invoke(this, propertyName);
        }
    }
}