namespace FramePFX.Interactivity {
    public static class DropUtils {
        public static EnumDropType GetDropAction(int keyStates, EnumDropType effects) {
            keyStates &= ~19; // remove mouse buttons
            if ((keyStates & 40) == 40 && (effects & EnumDropType.Link) == EnumDropType.Link) {
                return EnumDropType.Link; // Link drag-and-drop effect.
            }
            else if ((keyStates & 32) == 32 && (effects & EnumDropType.Link) == EnumDropType.Link) {
                return EnumDropType.Link; // ALT KeyState for link.
            }
            else if ((keyStates & 4) == 4 && (effects & EnumDropType.Move) == EnumDropType.Move) {
                return EnumDropType.Move; // SHIFT KeyState for move.
            }
            else if ((keyStates & 8) == 8 && (effects & EnumDropType.Copy) == EnumDropType.Copy) {
                return EnumDropType.Copy; // CTRL KeyState for copy.
            }
            else if ((effects & EnumDropType.Move) == EnumDropType.Move) {
                return EnumDropType.Move;
            }
            else if ((effects & EnumDropType.Copy) == EnumDropType.Copy) {
                return EnumDropType.Copy;
            }
            else if ((effects & EnumDropType.Link) == EnumDropType.Link) {
                return EnumDropType.Link;
            }
            else {
                return EnumDropType.None;
            }
        }
    }
}