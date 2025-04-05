## MVP, MVC, MVVM?

Unlike previous versions of FramePFX, this version consists almost entirely of MVP based code,
except the UI elements act like the presenters, and models are just on their own. MVVM is rarely used.
We may slowly transition back into MVVM once a system can be figured out (e.g. how do we bind clip location in the UI? `ClipViewModel`? What about `TrackViewModel` and therefore `TimelineViewModel` and `ProjectViewModel`? It just adds complexity due to us shadowing the models with view models)

When a model needs to access UI-specifics (e.g. a command wants to expand a node in the resource tree),
it does that through interfaces. The general pattern is `I<Model>Element` or `I<Model>UIElement`, e.g. `IResourceTreeElement`.
This allows backend code to change the UI directly.

# Projects

FramePFX contains 5 main projects:
- `PFXToolKitUI`: The base core project for the PFX toolkit. This may become a git submodule at some point, since this project contains lots of useful utilities and systems for a basic application (e.g. persistent configurations, command system and more)
- `PFXToolKitUI.Avalonia`: The base UI project for the PFX toolkit. This and the above project contain nothing related to FramePFX and are basically just an extension of Avalonia with my own utilities.
- `FramePFX`: This is the 'core' project or the backend of FramePFX, and contains the entire FramePFX API
- `FramePFX.BaseFrontEnd`: This project references avalonia libraries, and contains some of the FramePFX UI
  components (such as binding utilities, model->control bi-dictionaries, and lots more).
  The reason for the base front end project is so that plugins may access and interact directly with avalonia,
  while also supporting the 'core plugins' feature
- `FramePFX.Avalonia`: The UI entry point project. This implements a lot of the core elements of the UI, such as
  the timeline, track, clip controls, resources, and dialogs (e.g. export dialogs). 

  As time goes on, this project should ideally get smaller to a point, since we want to move as much over to the base front end to maximize
  what plugins can do. Core plugins cannot reference this project due to cyclic dependency problems.

## Control model Connection and Disconnection

A lot of controls, such as `ResourceExplorerListItemContent` or `TimelineClipControl`, contain
methods along the lines of `OnAdding`, `OnAdded`, `OnRemoving`, `OnRemoved`, and
for 'content' of those controls, they might just have `OnConnected` and `OnDisconnected`
(as in, connect to and from the model). This gives a simpler way to add/remove event handlers,
query data from the models, etc.

The order is something along the line of this:

- A model object containing a list (e.g. Track) adds a new item, (e.g. new Clip)
- View receives some sort of ItemAdded event, creates a new control (or retrieves a recycled item)
- `OnAdding` is called on the control, passing in the model and sometimes the owner control.
- The new control is added to the parent's internal items list
- The control's styling and template may be explicitly applied (because avalonia does not apply them until they are measured, so doing this lets us access template controls in OnAdded)
- `OnAdded` is called on the control. It might 'connect' (via binding utilities CLR event handlers) data between the model and UI. The data parameter system makes this process easier with built-in helpers.

And then later on maybe,

- *Clip is removed from the track*
- View receives some sort of ItemRemoved event
- `OnRemoving` is invoked on the clip control, it might unregister events handlers.
- Clip control is removed from the owner track control
- `OnRemoved` is invoked. The clip control clears references to the model and owner track control.
  The control may then be appended to a list of recycled controls, since creating new control instances can be expensive

