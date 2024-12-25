# Automation Engine

The automation engine is used to automate pieces of data, e.g. media position, scale, opacity, etc.

Always found automating parameters in the standard editors to be generally finicky. Ableton Live has a really good automation editor though, so I took a fair bit of inspiration from it:

- Each clip has its own keyframe/envelope editor that stretches the entirety of the clip. Effects also use this
- Tracks have the same, but it stretches the entire timeline.
- Automating project settings, or anything else really, will soon be do-able on a timeline automation specific track (allowing for more than just video/audio tracks)

The clip's automation sequence editor's target parameter can be changed by selecting a row (I call them "slots") in the property editor. The currently selected slot is what the clip shows (if clip automation is visible, click C to toggle)

### Key frames

I'm sure you know what key frame are; they just store a value at some time. I decided to store the time as a frame (relative to the project FPS)
because it's much simpler, however, when you change the project FPS it will cause automation to run faster or slower depending on the frame rate difference,
which is why I added a conversion popup when changing the FPS

### Automation Keys

In order for data to be automated, there needs to be an `AutomationKey`. Automation keys are just a helper container to store the
key's full ID and data type, as well as storing all registered keys. Keys have a `Domain` and `Id`, which are joined by `::` to
form `FullId`, which gets serialised and deserialises when saving and loading project, in order to identify a key from a
string (similar to WPF's dependency property system, except instead of an owner type it's a domain).

I'm not sure why I went with domain and not the owner type, but I don't see any reason to switch back and I don't plan on using reflection to update the values

I also sometimes call these automation parameters, similar to how VST has parameters

### Automation Sequence

Sequences are basically just a list of key frames with a "default keyframe" too. The sequence is what calculates a final value by interpolating
between key frames at some specific input frame, via methods such as `GetDoubleValue`, `GetVector2Value`, etc.

The default key frame is used as backing storage for the default automation value of the automatable object, or put simply, it's the value when not using key frames.
This used to be called `OverrideKeyFrame` but I renamed it to `DefaultKeyFrame` because it mainly serves the above purpose, and not just storing the override value

The override mode is a way to temporarily disable automation. When there are no key frames present, the sequence is implicitly
in override mode (hence why I previously named that key frame `OverrideKeyFrame`)

### Automation data

This is where automation keys and automation sequences are used. This class is used to map an `AutomationKey` to an `AutomationSequence`, and
also stores the automatable object that owns all of the sequences. Automation sequences are defined via the `AutomationData`'s `AssignKey`, which
is where a new sequence is created and added to the internal dictionary. This is also where you provide your `UpdateAutomationValueEventHandler`

### UpdateAutomationValueEventHandler

This event is used to tell an automatable object to query its backing value from its automation data. Automatable objects can have
a field or property (or whatever they want) in order to store the value from the most recent automation update. #

This technically means, when in override mode, this event is just copying the value from the `DefaultKeyFrame` to the object's field or
whatever (the event handler would call something like `GetDoubleValue` which, in override mode, just accesses the default key frame's value)

However when there are key frames and the override mode is not enabled, the interpolated value is calculated during a call to something like `GetDoubleValue`

That newly updated value can then be used during rendering to do things. Rendering shouldn't reference the automation engine at all, and
instead, should rely on that field, property, etc., being updated from this event