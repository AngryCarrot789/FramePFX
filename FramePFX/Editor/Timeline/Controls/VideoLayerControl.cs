﻿using System;
using System.Diagnostics;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;

namespace FramePFX.Editor.Timeline.Controls {
    public class VideoLayerControl : TimelineLayerControl {

        //           Width
        // ---------------------------
        // UnitZoom * MaxFrameDuration

        // /// <summary>
        // /// Gets or sets the maximum duration (in frames) of this timeline layer based on it's visual/actual pixel width
        // /// <para>
        // /// Setting this will modify the <see cref="UnitZoom"/> property as ActualWidth / MaxFrameDuration
        // /// </para>
        // /// </summary>
        // public double MaxFrameDuration {
        //     get => this.ActualWidth / this.UnitZoom;
        //     set => this.UnitZoom = this.ActualWidth / value;
        // }

        public VideoLayerControl() {

        }

        protected override TimelineClipControl GetContainerForItem(object item) {
            if (item == null || item is VideoClipViewModel)
                return new VideoClipControl();
            #if DEBUG
            Debugger.Break();
            #endif
            throw new Exception($"Tried to generate {nameof(VideoClipControl)} for {item.GetType()}");
        }
    }
}