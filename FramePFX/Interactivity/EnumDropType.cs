using System;

namespace FramePFX.Interactivity {
    [Flags]
    public enum EnumDropType {
        /// <summary>
        /// No drop (default state)
        /// </summary>
        None = 0,

        /// <summary>
        /// A copy drop; the object should be copied from the source to the target
        /// </summary>
        Copy = 1,

        /// <summary>
        /// A move drop; the object should be removed from the source and added to the target
        /// </summary>
        Move = 2,

        /// <summary>
        /// A link drop; a reference to the source object should be added to the target
        /// </summary>
        Link = 4,

        // not entirely sure what scroll is for, maybe to notify a list to scroll up/down?
        Scroll = -2147483648, // 0x80000000
        All = Scroll | Move | Copy, // 0x80000003
    }
}