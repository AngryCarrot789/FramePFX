## MVP, MVC, MVVM?

Unlike previous versions of FramePFX, this version consists almost entirely of MVP based code, 
except the UI elements act like the presenters, and models are just on their own. MVVM is rarely used.

When a model needs to access UI-specifics (e.g. a command wants to expand a node in the resource tree),
it does that through interfaces. The general pattern is `I<Model>Element` or `I<Model>UIElement`, e.g. `IResourceTreeElement`.

# Projects
FramePFX contains 3 main projects: `FramePFX`, `FramePFX.BaseFrontEnd` and `FramePFX.Avalonia`:
- `FramePFX`: This is the 'core' project, and contains the entire FramePFX API
- `FramePFX.BaseFrontEnd`: This project references avalonia libraries, and contains most of the base UI 
  components (such as binding utilities, model->control bi-dictionaries, and lots more).
  The reason for the base front end project is so that plugins may access and interact directly with avalonia,
  while also supporting the 'core plugins' feature
- `FramePFX.Avalonia`: The UI entry point project. This implements a lot of the core elements of the UI, such as
  the timeline, track, clip controls, resources, and dialogs (e.g. export dialogs).

  As time goes on, this project should ideally get smaller to a point, since we want to move as much over to the base front end to maximize
  what plugins can do. Core plugins cannot reference the avalonia project due to cyclic dependency problems

## Control model Connection and Disconnection

A lot of controls, such as `ResourceExplorerListItemContent` or `TimelineClipControl`, contain
methods along the lines of `OnAdding`, `OnAdded`, `OnRemoving`, `OnRemoved`, and
for 'content' of those controls, they might just have `OnConnected` and `OnDisconnected`
(as in, connect to and from the model). This gives a simpler way to add/remove event handlers,
query data from the models, etc.

The order is something along the line of this:

- *model with a list (e.g. Track) adds a new item, (e.g. new Clip)*
- View creates a new control (or retrieves a cached item; recycling, for performance)
- `OnAdding` is called on the control, passing in the model and sometimes the owner control.
- Clip control is added to the parent's internal items list
- The control's styling and template may be explicitly applied (because avalonia does not apply them until they are measured)
- `OnAdded` is called on the control.
- *Clip is removed from the track*
- `OnRemoving` is invoked on the clip control, it might unregistered events handlers.
- Clip is removed from the owner track
- `OnRemoved` is invoked. The clip control clears references to the model and owner track control

