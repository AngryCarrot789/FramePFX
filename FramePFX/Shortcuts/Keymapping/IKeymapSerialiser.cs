using System.IO;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Keymapping
{
    public interface IKeymapSerialiser
    {
        Keymap Deserialise(ShortcutManager manager, Stream stream);
        void Serialise(Keymap keymap, Stream stream);
    }
}