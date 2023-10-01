namespace FramePFX.Interactivity
{
    public static class DropUtils
    {
        public static EnumDropType GetDropAction(int keyStates, EnumDropType effects)
        {
            keyStates &= ~19; // remove mouse buttons
            if ((keyStates & 40) == 40 && (effects & EnumDropType.Link) == EnumDropType.Link)
            {
                // Link drag-and-drop effect.
                return EnumDropType.Link;
            }
            else if ((keyStates & 32) == 32 && (effects & EnumDropType.Link) == EnumDropType.Link)
            {
                // ALT KeyState for link.
                return EnumDropType.Link;
            }
            else if ((keyStates & 4) == 4 && (effects & EnumDropType.Move) == EnumDropType.Move)
            {
                // SHIFT KeyState for move.
                return EnumDropType.Move;
            }
            else if ((keyStates & 8) == 8 && (effects & EnumDropType.Copy) == EnumDropType.Copy)
            {
                // CTRL KeyState for copy.
                return EnumDropType.Copy;
            }
            else if ((effects & EnumDropType.Move) == EnumDropType.Move)
            {
                // By default, the drop action should be move, if allowed.
                return EnumDropType.Move;
            }
            else
            {
                return EnumDropType.None;
            }
        }
    }
}