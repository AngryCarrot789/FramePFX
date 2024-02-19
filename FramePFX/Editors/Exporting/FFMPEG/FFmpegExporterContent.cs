//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using FramePFX.Editors.Controls.Dragger;
using FramePFX.Editors.Exporting.Controls;
using FramePFX.Utils;

namespace FramePFX.Editors.Exporting.FFMPEG {
    public class FFmpegExporterContent : ExporterContent {
        private NumberDragger dragger;

        public new FFmpegExporter Exporter => (FFmpegExporter) base.Exporter;

        public FFmpegExporterContent() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.dragger = this.GetTemplateChild<NumberDragger>("PART_BitRateDragger");
            this.dragger.ValueChanged += (sender, args) => {
                this.Exporter.BitRate = Maths.Clamp((long) args.NewValue, 1, 1000000000);
            };
        }

        public override void OnConnected() {

        }

        public override void OnDisconnected() {

        }
    }
}