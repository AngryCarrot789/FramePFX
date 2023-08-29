using System;
using FFmpeg.AutoGen;

namespace FramePFX.Plugins {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IgnoredPluginAttribute : Attribute {

    }
}