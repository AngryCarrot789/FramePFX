using System;
using FramePFX.FFmpeg;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.Resources {
    /// <summary>
    /// A resource that contains resources for decoding mpeg media
    /// </summary>
    public class ResourceMpegMedia : ResourceItem {
        public string FilePath { get; set; }

        public FFmpegReader reader;

        public ResourceMpegMedia() {
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (!string.IsNullOrEmpty(this.FilePath))
                data.SetString(nameof(this.FilePath), this.FilePath);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.FilePath = data.GetString(nameof(this.FilePath), null);
        }

        protected override void OnDisableCore(ErrorList list, bool user) {
            base.OnDisableCore(list, user);
            this.CloseReader();
        }

        protected override void DisposeCore(ErrorList list) {
            base.DisposeCore(list);
            this.CloseReader();
        }

        public void CloseReader() {
            FFmpegReader r = this.reader;
            this.reader = null;
            if (r != null && r.IsOpen) {
                r.Close();
            }
        }

        public void LoadMedia(string filePath) {
            if (this.reader != null) {
                this.CloseReader();
            }

            this.reader = new FFmpegReader();
            try {
                this.reader.Open(filePath, false);
            }
            catch (Exception e) {
                try {
                    this.reader.Close();
                }
                catch (Exception ex) {
                    e.AddSuppressed(ex);
                }
                finally {
                    this.reader = null;
                }

                throw;
            }
        }
    }
}