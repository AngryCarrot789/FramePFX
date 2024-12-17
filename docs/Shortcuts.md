# Shortcut Manager

I created this because WPF doesn't have any kind of built in 'hotkey' manager, except for input bindings but they don't support the triggering hotkeys to be modified (AFAIK)

This library basically just converts a key combination into a collection of objectified shortcuts to be "activated"

## Overview

An application typically has a global `ShortcutManager`. Each window/shell would have a `ShortcutProcessor` (which is created by the manager) and that handles the input
for that specific window; essentially it processes the inputs for that target and activates shortcuts.
The manager stores a root `ShortcutGroup`, forming a hierarchy of groups and shortcuts.

Each group and shortcut have their own identifier `Name`, and is used by the `FullPath`, where the groups are separated by a '/' character

The application would create focus groups, which is what would allow advanced context-specific shortcuts to work (e.g. clicking F2 changes what happens based on what part of the app is focused)

`ShortcutGroup`, `GroupedShortcut` and `GroupedInputState` all have a name associated with them, unique relative to their parent (basically like a file system).

## Shortcuts

A shortcut could be activated with a single keystroke (e.g. S or CTRL+X), or by a long chain
or sequential input strokes (LMB click, CTRL+SHIFT+X, B, Q, WheelUp, ALT+E to finally activate a shortcut)

## Other stuff

Any time an input for a "normal" key or mouse press is received (e.g. clicking A or the number 4), the respective input stroke object (typically a struct) is created with the modifier keys that were
pressed at that moment. This is different from IntelliJ where active modifiers appear to be cleared when a "normal" input is received, unless that's just how the IDE presents shortcuts,
I don't know; maybe the expand all shortcut really is just `CTRL+M and CTRL+X` and not `CTRL+M and X`

## Limitations

I haven't even used this input system much, I kinda just wrote it for future me to use (and probably heavily modify), so there's probably more limitations

But so far the main limitation is that shortcuts cannot be triggered specifically mouse up events. I can't remember exactly why (i put comments around the
shortcut processor code), but as a result, all mouse inputs are just classed as "clicks", as in, mouse down clicks; mouse up does nothing. However, input states can
acknowledge mouse down and up, so a state could be activated on LMB down and deactivated on LMB up, or another mouse button up for that matter
if that's what the user wants

# Invoking code

To make a shortcut run code, there a few things you can do

In WPF, you can use the `ShortcutCommandBinding` which can be added to a `ShortcutCommandCollection` associated with a dependency object

You can also use the command system (which uses command IDs). This allows registered commands to be invoked at will
