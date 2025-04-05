// 
// Copyright (c) 2024-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Diagnostics;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Shortcuts.Keymapping;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Shortcuts.Usage;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Shortcuts.Avalonia;

public sealed class AvaloniaShortcutManager : ShortcutManager {
    public const int BUTTON_WHEEL_UP = 143; // Away from the user
    public const int BUTTON_WHEEL_DOWN = 142; // Towards the user
    public const string DEFAULT_USAGE_ID = "DEF";

    public static AvaloniaShortcutManager AvaloniaInstance => (AvaloniaShortcutManager) Instance ?? throw new Exception("No WPF shortcut manager available");

    static AvaloniaShortcutManager() {
        KeyStroke.KeyCodeToStringProvider = (x) => ((Key) x).ToString();
        KeyStroke.ModifierToStringProvider = (x, s) => {
            StringJoiner joiner = new StringJoiner(s ? " + " : "+");
            KeyModifiers keys = (KeyModifiers) x;
            if ((keys & KeyModifiers.Control) != 0)
                joiner.Append("Ctrl");
            if ((keys & KeyModifiers.Alt) != 0)
                joiner.Append("Alt");
            if ((keys & KeyModifiers.Shift) != 0)
                joiner.Append("Shift");
            if ((keys & KeyModifiers.Meta) != 0)
                joiner.Append("Win");
            return joiner.ToString();
        };

        MouseStroke.MouseButtonToStringProvider = (x) => {
            switch (x) {
                case (int) MouseButton.Left:     return "LMB";
                case (int) MouseButton.Middle:   return "MMB";
                case (int) MouseButton.Right:    return "RMB";
                case (int) MouseButton.XButton1: return "X1";
                case (int) MouseButton.XButton2: return "X2";
                case BUTTON_WHEEL_UP:            return "WHEEL_UP";
                case BUTTON_WHEEL_DOWN:          return "WHEEL_DOWN";
                default:                         return $"Unknown Button ({x})";
            }
        };
    }

    public AvaloniaShortcutManager() { }

    public override ShortcutInputProcessor NewProcessor() => new AvaloniaShortcutInputProcessor(this);

    public override void ReloadFromStream(Stream stream) {
        this.InvalidateShortcutCache();
        Keymap map = KeyMapSerialiser.Instance.Deserialise(this, stream);
        this.Root = map.Root; // invalidates cache automatically
        try {
            this.EnsureCacheBuilt(); // do keymap check; crash on errors (e.g. duplicate shortcut path)
        }
        catch (Exception e) {
            this.InvalidateShortcutCache();
            this.Root = ShortcutGroupEntry.CreateRoot(this);
            throw new Exception("Failed to process keymap and built caches", e);
        }
    }

    protected override void OnSecondShortcutUsagesProgressed(ShortcutInputProcessor inputProcessor) {
        base.OnSecondShortcutUsagesProgressed(inputProcessor);
        StringJoiner joiner = new StringJoiner(", ");
        foreach (KeyValuePair<IShortcutUsage, ShortcutEntry> pair in inputProcessor.ActiveUsages) {
            joiner.Append(pair.Key.CurrentStroke.ToString());
        }

        BroadcastShortcutActivity("Waiting for next input: " + joiner);
    }

    protected override void OnShortcutUsagesCreated(ShortcutInputProcessor inputProcessor) {
        base.OnShortcutUsagesCreated(inputProcessor);
        StringJoiner joiner = new StringJoiner(", ");
        foreach (KeyValuePair<IShortcutUsage, ShortcutEntry> pair in inputProcessor.ActiveUsages) {
            joiner.Append(pair.Key.CurrentStroke.ToString());
        }

        BroadcastShortcutActivity("Waiting for next input: " + joiner);
    }

    protected override void OnCancelUsageForNoSuchNextMouseStroke(ShortcutInputProcessor inputProcessor, IShortcutUsage usage, ShortcutEntry shortcutEntry, MouseStroke stroke) {
        base.OnCancelUsageForNoSuchNextMouseStroke(inputProcessor, usage, shortcutEntry, stroke);
        BroadcastShortcutActivity("No such shortcut for next mouse stroke: " + stroke);
    }

    protected override void OnCancelUsageForNoSuchNextKeyStroke(ShortcutInputProcessor inputProcessor, IShortcutUsage usage, ShortcutEntry shortcutEntry, KeyStroke stroke) {
        base.OnCancelUsageForNoSuchNextKeyStroke(inputProcessor, usage, shortcutEntry, stroke);
        BroadcastShortcutActivity("No such shortcut for next key stroke: " + stroke);
    }

    protected override void OnNoSuchShortcutForMouseStroke(ShortcutInputProcessor inputProcessor, string group, MouseStroke stroke) {
        base.OnNoSuchShortcutForMouseStroke(inputProcessor, group, stroke);
        if (Debugger.IsAttached) {
            BroadcastShortcutActivity("No such shortcut for mouse stroke: " + stroke + " in group: " + group);
        }
    }

    protected override void OnNoSuchShortcutForKeyStroke(ShortcutInputProcessor inputProcessor, string group, KeyStroke stroke) {
        base.OnNoSuchShortcutForKeyStroke(inputProcessor, group, stroke);
        if (stroke.IsKeyDown && Debugger.IsAttached) {
            BroadcastShortcutActivity("No such shortcut for key stroke: " + stroke + " in group: " + group);
        }
    }

    protected override bool OnShortcutActivatedOverride(ShortcutInputProcessor inputProcessor, ShortcutEntry shortcutEntry) {
        string str;

        if (Debugger.IsAttached) {
            str = $"shortcut command: {shortcutEntry} -> {(string.IsNullOrWhiteSpace(shortcutEntry.CommandId) ? "<none>" : shortcutEntry.CommandId)}";
        }
        else {
            str = $"shortcut command: {(string.IsNullOrWhiteSpace(shortcutEntry.CommandId) ? "<none>" : shortcutEntry.CommandId)}";
        }

        BroadcastShortcutActivity($"Activating {str}...");
        bool result = ApplicationPFX.Instance.Dispatcher.Invoke(() => base.OnShortcutActivatedOverride(inputProcessor, shortcutEntry), DispatchPriority.Render);
        BroadcastShortcutActivity($"Activated {str}!");
        return result;
    }

    private static void BroadcastShortcutActivity(string msg) {
    }
}