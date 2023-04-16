namespace FramePFX.Core.RBC {
    public enum RBEType : byte {
        // In order for old data to be compatible, the existing type values
        // should not be modified. There can only be 255 different types of element (1-255)
        None        = 0,
        Dictionary  = 1,
        List        = 2,
        Byte        = 3,
        Short       = 4,
        Int         = 5,
        Long        = 6,
        Float       = 7,
        Double      = 8,
        Struct      = 9,
        ByteArray   = 10,
        ShortArray  = 11,
        IntArray    = 12,
        LongArray   = 13,
        FloatArray  = 14,
        DoubleArray = 15,
        StructArray = 16,
    }
}