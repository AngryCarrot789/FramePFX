*These features are still being worked on

# Dynamic UI

FramePFX provides a way to dynamically add UI components, such as buttons and toggle buttons, that 
can execute custom user code or commands. This is to allow plugins to add UI components without having
to necessarily dig into the raw UI components (like Buttons, ToggleButtons, etc.)

## Toolbars
These are the current toolbars available and their class:
- `TimelineToolBarManager`: The toolbar at the bottom of the timeline. Supports west and east anchored buttons
- `ControlSurfaceListToolBarManager`: The toolbar at the bottom of the control surface list.
- 'ViewPortToolBarManager': The toolbar below the view port. Not currently implemented; more UI abstractions like checkboxes are required
