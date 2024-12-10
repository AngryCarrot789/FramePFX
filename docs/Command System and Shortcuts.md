### Command System
I created a system that is inspired from IntelliJ IDEA's action system, where you have a single command manager which contains all registered commands. 
You access commands via a string key, and then execute the command by passing in contextual data (stored in a `IContextData`). The context data gets a 
value from data keys, and the `ContextData` implementation stores entries in a map which is how UI components store their context data. 
The `DataManager` class manages the context data for all UI components (scroll down for more info)

This means that when you for example press F2 or CTRL+R while focused on a clip, there's a lot of data keys between the root window and the clip UI object, and so
in the rename command, you have access to all of them; the editor, project, timeline, track and clip. Whereas if you just have the window focused and press a shortcut, you
may only have the editor and project available; It's context-sensitive, duh

### Shortcuts
The shortcut system listens to inputs at an application level instead of receiving input from a specific window (however, a window can only really process 
shortcuts if it has a focus path associated with it, which can be set via the `UIInputManager.FocusPath` attached property). 
Long story short, the shortcut system figures out a list of shortcuts to "activate" based on the current global focus path, and activates all of them until 
one is activated successfully. 
Keymap.xml contains the shortcuts (and some unused ones from the old app version)

### Advanced Context Menu system
So far, all context menus use the `AdvancedContextMenu` class. The menu items have a model-view connection and all the model items are stored in a `ContextRegistry`
which, when linked to a control, will find the `AdvancedContextMenu` instance associated with the registry, or it creates one and then the UI components are generated, 
and it sets it as the control's ContextMenu so that standard context behaviour works. There's one menu per registry, to help with performance and memory usage.

Context registries contain a list of group objects, which are name. This gives some control order the ordering of menu items if 
a plugin system is ever implemented; plugins could access known groups and insert their commands into the appropriate ones.

Entries are the actual menu item models, which are represented as the `IContextObject` interface. 
`IContextEntry` is for actual menu items, but things like separators and captions just use IContextGroup

Currently, there's only 2 types of groups:
- Static groups, which contain a list of entry instances.
- Dynamic groups, which contains a generator callback which can create entries based on the available context, and they're injected into the context menu 
  at the location of the dynamic group entry (which is a placeholder entry so that the generator system knows where to put the items).

There's also the `ContextCapturingMenu`, which is used as a window's top level menu. This menu captures the fully-inherited IContextData of the control
that was focused just before a menu item was opened in said menu (see below for more info about context data and this fully-inherited behaviour)

### Data Manager
The data manager is used to store local context data in a control, and implement context data inheritance containing the merged context data for all of a control's visual parents and itself.

It has two primary properties: `ContextData`, and `InheritedContextData`. Despite the name, the inherited version does not use Avalonia's built-in property inheritance feature, but instead
uses my own inheritance implementation by using `ContextDataProperty` changes and the `VisualAncestorChanged` event (which I add/remove reflectively when the `ContextDataProperty` changes,
and it fires when an object's visual parent changes). By doing this, an event can be fired (`InheritedContextChangedEvent`) for every single child of an element in its visual tree when its
local context data is changed. The `ContextUsage` class depends on this feature in order to do things like re-query the executability state of a command when a control's full inherited context changes
