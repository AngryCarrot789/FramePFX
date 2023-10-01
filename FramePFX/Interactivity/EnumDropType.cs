using System;

namespace FramePFX.Interactivity
{
    [Flags]
    public enum EnumDropType
    {
        /// <summary>
        /// No drop happened (default state)
        /// </summary>
        None = 0,

        /// <summary>
        /// A copy drop occurred; the object should be copied from the source to the target
        /// </summary>
        Copy = 1,

        /// <summary>
        /// A move drop occurred; the object should be removed from the source and added to the target
        /// </summary>
        Move = 2,

        /// <summary>
        /// A link drop occurred; a reference to the object should be added to the target
        /// </summary>
        Link = 4,

        // not entirely sure what scroll is for, maybe to notify a list to scroll up/down?
        Scroll = -2147483648, // 0x80000000
        All = Scroll | Move | Copy, // 0x80000003
    }
}