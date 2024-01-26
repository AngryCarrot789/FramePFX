## The general idea
I decided to not use much MVVM for this version of FramePFX (because how 
else do I bind to a property in a Clip? I need a ClipViewMode, and therefore 
a TrackViewModel and so on, and it just results in having hierarchy-based 
view models that match their model counterparts 1:1 pretty much)

So this version instead uses a mix between MVC (model-view-controller) and 
well MV (view accessing and modifying models and vice versa) except no models 
contain references to any models

## Control model Connection and Disconnection
A lot of controls like ResourceListItemControl, TimelineClipControl, contain
methods along the lines of `OnAdding`, `OnAdded`, `OnRemoving`, `OnRemoved`, and 
for 'content' of those controls, they might just have `OnConnected` and `OnDisconnected`
(as in, to and from the owner control). This gives a simpler way to add/remove event handlers, 
query data from the models, etc.

The order is something along the line of this: 
- *model adds a new item, e.g. new Clip*
- View creates a new control (or retrieves a cached item; recycling, for performance)
- `OnAdding` is called on the control. If the control is recyclable, the parent control (e.g. Track Control) 
  and the model may be passed in. If non-recyclable, then I might just pass objects int he constructor, though 
  I don't plan on doing this anymore since recycling is always more performant due to pre-applied styles
- Control is added to the parent's internal items list
- The control's measure may be invalidated and then UpdateLayout too. This is to force the template to be applied. But
  sometimes this might not work, so ApplyTemplate might be called before/afterwards too
- `OnAdded` is called on the control. The template  (if there is one) should be applied
