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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia.Input;
using FramePFX.Avalonia.Shortcuts.Inputs;
using FramePFX.Avalonia.Shortcuts.Keymapping;
using FramePFX.Avalonia.Shortcuts.Managing;
using FramePFX.Avalonia.Shortcuts.Usage;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Shortcuts.Avalonia;

public class AvaloniaShortcutManager : ShortcutManager {
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
                case 0: return "LMB";
                case 1: return "MMB";
                case 2: return "RMB";
                case 3: return "X1";
                case 4: return "X2";
                case BUTTON_WHEEL_UP: return "WHEEL_UP";
                case BUTTON_WHEEL_DOWN: return "WHEEL_DOWN";
                default: return $"Unknown Button ({x})";
            }
        };
    }

    public AvaloniaShortcutManager() { }

    public override ShortcutInputProcessor NewProcessor() => new AvaloniaShortcutInputProcessor(this);

    public void DeserialiseRoot(Stream stream) {
        this.InvalidateShortcutCache();
        Keymap map = WPFKeyMapSerialiser.Instance.Deserialise(this, stream);
        this.Root = map.Root; // invalidates cache automatically
        try {
            this.EnsureCacheBuilt(); // do keymap check; crash on errors (e.g. duplicate shortcut path)
        }
        catch (Exception e) {
            this.InvalidateShortcutCache();
            this.Root = ShortcutGroup.CreateRoot(this);
            throw new Exception("Failed to process keymap and built caches", e);
        }
    }

    protected internal override void OnSecondShortcutUsagesProgressed(ShortcutInputProcessor inputProcessor) {
        base.OnSecondShortcutUsagesProgressed(inputProcessor);
        StringJoiner joiner = new StringJoiner(", ");
        foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in inputProcessor.ActiveUsages) {
            joiner.Append(pair.Key.CurrentStroke.ToString());
        }

        BroadcastShortcutActivity("Waiting for next input: " + joiner);
    }

    protected internal override void OnShortcutUsagesCreated(ShortcutInputProcessor inputProcessor) {
        base.OnShortcutUsagesCreated(inputProcessor);
        StringJoiner joiner = new StringJoiner(", ");
        foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in inputProcessor.ActiveUsages) {
            joiner.Append(pair.Key.CurrentStroke.ToString());
        }

        BroadcastShortcutActivity("Waiting for next input: " + joiner);
    }

    protected internal override void OnCancelUsageForNoSuchNextMouseStroke(ShortcutInputProcessor inputProcessor, IShortcutUsage usage, GroupedShortcut shortcut, MouseStroke stroke) {
        base.OnCancelUsageForNoSuchNextMouseStroke(inputProcessor, usage, shortcut, stroke);
        BroadcastShortcutActivity("No such shortcut for next mouse stroke: " + stroke);
    }

    protected internal override void OnCancelUsageForNoSuchNextKeyStroke(ShortcutInputProcessor inputProcessor, IShortcutUsage usage, GroupedShortcut shortcut, KeyStroke stroke) {
        base.OnCancelUsageForNoSuchNextKeyStroke(inputProcessor, usage, shortcut, stroke);
        BroadcastShortcutActivity("No such shortcut for next key stroke: " + stroke);
    }

    protected internal override void OnNoSuchShortcutForMouseStroke(ShortcutInputProcessor inputProcessor, string group, MouseStroke stroke) {
        base.OnNoSuchShortcutForMouseStroke(inputProcessor, group, stroke);
        if (Debugger.IsAttached) {
            BroadcastShortcutActivity("No such shortcut for mouse stroke: " + stroke + " in group: " + group);
        }
    }

    protected internal override void OnNoSuchShortcutForKeyStroke(ShortcutInputProcessor inputProcessor, string group, KeyStroke stroke) {
        base.OnNoSuchShortcutForKeyStroke(inputProcessor, group, stroke);
        if (stroke.IsKeyDown && Debugger.IsAttached) {
            BroadcastShortcutActivity("No such shortcut for key stroke: " + stroke + " in group: " + group);
        }
    }

    protected override bool OnShortcutActivatedOverride(ShortcutInputProcessor inputProcessor, GroupedShortcut shortcut) {
        string tostr;

        if (Debugger.IsAttached) {
            tostr = $"shortcut command: {shortcut} -> {(string.IsNullOrWhiteSpace(shortcut.CommandId) ? "<none>" : shortcut.CommandId)}";
        }
        else {
            tostr = $"shortcut command: {(string.IsNullOrWhiteSpace(shortcut.CommandId) ? "<none>" : shortcut.CommandId)}";
        }

        BroadcastShortcutActivity($"Activating {tostr}...");
        bool result = RZApplication.Instance.Dispatcher.Invoke(() => base.OnShortcutActivatedOverride(inputProcessor, shortcut), DispatchPriority.Render);
        BroadcastShortcutActivity($"Activated {tostr}!");
        return result;
    }

    private static void BroadcastShortcutActivity(string msg) {
    }
}