using FramePFX.History;

namespace FramePFX.Interactivity {
    public static class DropUtils {
        private const int None = 0;
        private const int LeftMouseButton = 1;
        private const int RightMouseButton = 2;
        private const int ShiftKey = 4;
        private const int ControlKey = 8;
        private const int MiddleMouseButton = 16;
        private const int AltKey = 32;

        private const int ControlShift = ControlKey | ShiftKey;
        private const int MouseButtons = LeftMouseButton | RightMouseButton | MiddleMouseButton;

        public static EnumDropType GetDropAction(int keyStates, EnumDropType effects) {
            keyStates &= ~MouseButtons; // remove mouse buttons
            if ((keyStates & ControlShift) == ControlShift && (effects & EnumDropType.Link) == EnumDropType.Link) {
                return EnumDropType.Link; // Hold CTRL + SHIFT to create link
            }
            else if ((keyStates & AltKey) == AltKey && (effects & EnumDropType.Link) == EnumDropType.Link) {
                return EnumDropType.Link; // Hold ALT to create link
            }
            else if ((keyStates & ShiftKey) == ShiftKey && (effects & EnumDropType.Move) == EnumDropType.Move) {
                return EnumDropType.Move; // Hold SHIFT to move.
            }
            else if ((keyStates & ControlKey) == ControlKey && (effects & EnumDropType.Copy) == EnumDropType.Copy) {
                return EnumDropType.Copy; // Hold CTRL to top
            }
            else if ((effects & EnumDropType.Move) == EnumDropType.Move) {
                return EnumDropType.Move; // Try to move by default
            }
            else if ((effects & EnumDropType.Copy) == EnumDropType.Copy) {
                return EnumDropType.Copy; // Try to copy by default
            }
            else if ((effects & EnumDropType.Link) == EnumDropType.Link) {
                return EnumDropType.Link; // Try to link by default
            }
            else {
                return EnumDropType.None; // None of the above will work so no drag drop for you :)
            }
        }

        private const int EnumNone = 0;
        private const int EnumAltKey = 1;
        private const int EnumControlKey = 2;
        private const int EnumShiftKey = 4;
        private const int EnumControlShiftKeys = EnumControlKey | EnumShiftKey;

        public static EnumDropType GetDropActionForModKeys(int mods, EnumDropType effects) {
            if ((mods & EnumControlShiftKeys) == EnumControlShiftKeys && (effects & EnumDropType.Link) == EnumDropType.Link) {
                return EnumDropType.Link; // Hold CTRL + SHIFT to create link
            }
            else if ((mods & EnumAltKey) == EnumAltKey && (effects & EnumDropType.Link) == EnumDropType.Link) {
                return EnumDropType.Link; // Hold ALT to create link
            }
            else if ((mods & EnumShiftKey) == EnumShiftKey && (effects & EnumDropType.Move) == EnumDropType.Move) {
                return EnumDropType.Move; // Hold SHIFT to move.
            }
            else if ((mods & EnumControlKey) == EnumControlKey && (effects & EnumDropType.Copy) == EnumDropType.Copy) {
                return EnumDropType.Copy; // Hold CTRL to top
            }
            else if ((effects & EnumDropType.Move) == EnumDropType.Move) {
                return EnumDropType.Move; // Try to move by default
            }
            else if ((effects & EnumDropType.Copy) == EnumDropType.Copy) {
                return EnumDropType.Copy; // Try to copy by default
            }
            else if ((effects & EnumDropType.Link) == EnumDropType.Link) {
                return EnumDropType.Link; // Try to link by default
            }
            else {
                return EnumDropType.None; // None of the above will work so no drag drop for you :)
            }
        }
    }
}