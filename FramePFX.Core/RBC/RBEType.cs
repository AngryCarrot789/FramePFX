namespace FramePFX.Core.RBC {
    public enum RBEType : byte {
        // In order for old data to be compatible, the existing type values
        // should not be modified. There can only be 255 different types of element (1-255)

        // when adding new types, a case must be added to the RBEBase.TypeToIdTable dictionary, RBEBase.CreateById and RBEBase.GetTypeById

        Unknown     = 0,
        Dictionary  = 1,
        List        = 2,
        Byte        = 3,
        Short       = 4,
        Int         = 5,
        Long        = 6,
        Float       = 7,
        Double      = 8,
        String      = 9,
        Struct      = 10,
        ByteArray   = 11,
        ShortArray  = 12,
        IntArray    = 13,
        LongArray   = 14,
        FloatArray  = 15,
        DoubleArray = 16,
        StringArray = 17,
        StructArray = 18,
        Guid        = 19,
    }
}