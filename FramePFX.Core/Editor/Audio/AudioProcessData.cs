namespace FramePFX.Core.Editor.Audio {
    public struct AudioProcessData {
        public EnumSampleSize sampleSize;
        public int numSamples;
        public int numInputs;
        public int numOutputs;
        public unsafe AudioBusBuffers* inputs;
        public unsafe AudioBusBuffers* outputs;
    }
}