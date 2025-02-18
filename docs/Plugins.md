# Plugin System Overview

A recent update allows support for plugins, either core projects or dynamically loaded plugins.

### Core plugins

These are more like 'modules' of the application. They are projects referenced by the main project and are loaded before dynamic plugins.

For example, the FFmpegMedia plugin is a core plugin, and implements exporting   
and playback of media files. This core plugin is only loaded when FFmpeg is available

### Dynamic/Assembly plugins

These are plugins that are completely dynamically loaded. This system is still WIP, but they can do everything a core plugin can do.   
Assembly loading doesn't use an `AssemblyLoadContext`, so there's versioning issues to deal with

The only dynamic plugin is `CircleClipPlugin`, which is a test plugin which adds a video clip that draws a circle.

## Supported and unsupported features overview

So far, plugins are able to do a fair amount of things, such as:

- Registering custom exporters (see FFmpegMedia plugin which adds an FFmpeg exporter, to export .mp4 files)
- Custom configuration pages and files (using built in persistent storage system)
- Custom data parameters and property editor slots  (aka rows)
- Custom clips (see FFmpegMedia plugin which adds a video clip to play videos)
- Custom commands
- Adding context menu entries to clip, track and resource context menus
- Resource drop-in-timeline handling
- Handling native files dropped in resource manager
- Custom timeline and viewport toolbar buttons

... And a few others. There's still lots that cannot be done yet. A few of the main things are:

- Custom panels in the UI
- Access to the keyframe UI system, thought I can't see why a plugin would want to access this

# Plugin lifetime

This is the order of API methods that are called into a plugin:

- `OnCreated`: Invoked just after the constructor is invoked and the properties (Descriptor, PluginLoader and folder) are set.
- `RegisterCommands`: Register your commands here.
- `RegisterServices`: Register your services here
- `RegisterConfigurations`: Register your persistent configs
- `GetXamlResources`: Add the path of your .axaml files (relative to your plugin project) that should be loaded and added to the application's `MergedDictionaries` list
- `OnApplicationLoaded`: Invoked once all application states are ready. No editor window will be open at this point.
- `OnApplicationExiting`: Invoked once the application is about to exit. This is called before persistent configs are saved, so here is where you can synchronize them with the application if you're not already doing it dynamically

Remarks on `OnApplicationLoaded`: This is where you can register application event handlers, your UI controls, custom clips, custom resources, exporters, context menu entries, property editor slot controls, and lots more.
This method is async so there is no rush to do things quickly here.

See `FFmpegMediaPlugin` for examples on things to register and hook into.

# FramePFX API

This section describes the main APIs of FramePFX. This is still being updated

## Property Editor

The property editor system is a highly customisable framework, which unfortunately also makes it somewhat complicated to use.

The main power of the custom property editor system is supporting multiple objects being modified at once. We call these "Handlers".

Say for example you want to edit a `double` property in multiple objects, and:

- The property has a fixed value range (e.g. 0.0 to 1.0)
- You want to post-process the displayed value string (e.g. display 0.0 to 1.0 as a percentage: 0% to 100.0%)
- You also want to support manual value entry and support converting, say, 95% back to a raw value of 0.95 for the property
- Change the value increments (aka step profile) of the NumberDragger based on if shift, ctrl or both are pressed.

You can do all of this on a single line... sort of! There's lots of built-in classes which make this doable on a single line.

The property editor consists of rows that we call "Slots". A slot typically consists of a left, spacing, and right column; left for the display name, and right for the slot content (e.g. a slider, NumberDragger or text box). The column sizes are bound to the `PropertyEditorControl.Column[0,1,2]Width` properties.

For example, this is the property editor slot used to modify the `Line Spacing` of a text clip, which is also animatable:

```csharp  
new ParameterFloatPropertyEditorSlot(   // Built-in slot which manages animatable float parameters  
    TextVideoClip.LineSpacingParameter, // The specific parameter we want to modify
    typeof(TextVideoClip),              // The type of object the slot is applicable to, typically the Owner of the parameter 
    "Line Spacing",                     // A readable display name string, on the left column 
    DragStepProfile.Pixels)             // The NumberDragger's increment behaviour. Pixels is fine tuned
{   
    // Converts, say, "25.3" into "25.3 px", and supports converting back too "25.3"  
    ValueFormatter = SuffixValueFormatter.StandardPixels 
});
```  

This example is for an automatable parameter though, and you might not need animatable parameters, in which case, you can use `DataParameterFloatPropertyEditorSlot`  for a `DataParameterFloat`.

You can register your own custom property editor slot UI controls via the registry object `BasePropertyEditorSlotControl.Registry`, like so:

```csharp  
// The model/control naming convention isn't strict, it's just good practice
Registry.RegisterType<MyCustomPropertyEditorSlot>(  
    // ModelControlRegistry<TModel, TControl> uses factory methods 
    () => new MyCustomPropertyEditorSlotControl()
);  
```

Slot controls may be recycled, so it's important to override the `OnConnected` and `OnDisconnected` methods and add/remove event handlers or bindings accordingly to and from the page model.

A lot of the system is documented in code, so for specific things like slot selection and multiple   
handlers with differing values, I recommend reading the source code to get a better understanding

To specify the handlers that a slot can modify, you call `SetHandlers(IReadOnlyList<object>)` on the slot, and, if the slot supports the number of
handlers and also the underlying types of the handler object(s), then it sets `IsCurrentlyApplicable` to true, and the slot can work as normal,
and the `Handlers` property is updated with the objects that can be modified.  
`ClearHandlers` will obviously clear the handlers and makes the slot no longer applicable.

`SetHandlers` is a recursive method, so it only needs to get called on the root object that you want to set the handlers of. `PropertyEditor` contains the 'root' slot which is what you can call `SetHandlers` on.
However, the main property editor in the UI has 2 sub-root slots, one for clips and one for tracks. Therefore, `SetHandlers` should not be invoked on the root of the `VideoEditorPropertyEditor` object.

Should you wish to add your own slot to the main UI, maybe your plugin adds a 2nd window and you want selected objects in that window to be reflected
in the main UI's property editor, you should add that slot to the `VideoEditorPropertyEditor`'s root, and managed the handlers of the slot you added.

## Automatable and Data Parameters

FramePFX provides two parameter subsystems: data parameters, and automatable parameters.

Data parameters are represented by the base class `DataParameter`, but you shouldn't override this class directly, instead override `DataParameter<T>` if you need your own type. These parameters are used to make interfacing with a `PropertyEditor` or binding to the UI in code-behind generally easier; it saves having to use reflection everywhere by instead going through a `ValueAccessor<T>`.

Automatable parameters have a more specific usage, in that they are just keys to a dictionary of automation sequences. The file `docs/Automation.md` has more information about the automation system. Automatable parameters, just like data parameters, can also be used with property editors, and the standard automation property editor slots provide insert key frame/toggle override/reset value commands, which saves doing it manually.

You can find live templated for JetBrains Rider that allow you to easily define data parameters and automatable parameters in the `LIVETEMPLATES.md` file in the solution.

## Configuration pages

Configuration pages are the standard way of modifying application or project properties.

Page models are implemented via the `ConfigurationPage` class. This base class contains the active `ConfigurationContext` which is an object created whenever the settings dialog is opened and is used to track modified pages and the active page (currently being viewed).

The methods available are:

- `ValueTask OnContextCreated(ConfigurationContext)`: This is invoked recursively for every page in a configuration manager when a settings dialog is opened.
  This is where you can load data from the application or project into the state of the page, and also register event handlers for data changes, if you need to.
- `ValueTask OnContextDestroyed(ConfigurationContext)`: This is invoked recursively for every page in a configuration manager when a settings dialog is closed.
  You should unregister event handlers in this method

- `ValueTask Apply(List<ApplyChangesFailureEntry>)`: Apply changes back into the application or project.
  The provided list is not fully implemented yet, however, it should be used instead of showing message dialogs, since it might annoy the user if there's 100s of errors that occur. So instead, all errors will be shown at once in a custom dialog using the `ApplyChangesFailureEntry` objects as the models.

- `void OnActiveContextChanged(ConfigurationContext, ConfigurationContext)`: This is invoked when the viewed configuration page changes. `newContext` is null when this page is no longer visible, and is non-null when this page is now being viewed. You may wish to implement the loading data behaviour in this method instead of `OnContextCreated` to help with performance.
  But beware, this method isn't async, since it is invoked during a UI input event (the tree node being clicked), so don't do anything too slow here

### Application configuration pages

There's a singleton configuration page for the entire application stored in `ApplicationConfigurationManager.Instance`. You can add your own configuration entries and pages in your plugin's `OnApplicationLoading` method

### Project configuration pages

Project settings are stored in a `ProjectConfigurationManager`.

Since there's a configuration manager for each instance of a project, you must listen to the `ProjectConfigurationManager.SetupProjectConfiguration` event, and add your own configuration entries and pages to the `ProjectConfigurationManager` given as an event parameter.

### Page controls

The simplest way to create a configuration page would be to derive from `PropertyEditorConfigurationPage` and use its property editor

But if you wish to implement a completely custom configuration page control (either XAML or declarative if you so please), you can register a mapping via the `ConfigurationPageRegistry.Registry`, like so:

```csharp
Registry.RegisterType<MyConfigurationPage>(
    () => new MyConfigurationPageControl()
);
```

By doing this, you allow the UI to create your control when it tries to present your page. Page controls may be recycled, so it's important to override the `OnConnected` and `OnDisconnected` methods and add/remove event handlers or bindings accordingly to and from the page model.

## Persistent configurations (aka config files)

FramePFX has a built-in system for loading and saving configurations (PSP, or persistent storage system), saving you from having to manage a file path and file IO yourself.

You create a type that derives from `PersistentConfiguration`. Then you can register persistent properties via the `PersistentProperty` class.

For example, say you want to save the location of the editor window:

```csharp
public sealed class EditorWindowConfigurationOptions : PersistentConfiguration {
    // This lets us get/set the instance of this configuration, so that we can update
    // the PosX/PosY properties when the window moves.
    public static EditorWindowConfigurationOptions Instance => Application.Instance.PersistentStorageManager.GetConfiguration<EditorWindowConfigurationOptions>();
    
    // Register the persistent properties.
    public static readonly PersistentProperty<int> PosXProperty = PersistentProperty.RegisterParsable<int, EditorWindowConfigurationOptions>(nameof(PosX), 0, o => o.posX, (o, val) => o.posX = val, false);
    public static readonly PersistentProperty<int> PosYProperty = PersistentProperty.RegisterParsable<int, EditorWindowConfigurationOptions>(nameof(PosY), 0, o => o.posY, (o, val) => o.posY = val, false);
    
    // Value backing fields
    private int posX, posY;
    
    // Get/Set helpers
    public int PosX {
        get => PosXProperty.GetValue(this);
        set => PosXProperty.SetValue(this, value);
    }
    public int PosY {
        get => PosYProperty.GetValue(this);
        set => PosYProperty.SetValue(this, value);
    }
    
    // Value change helpers. There are other ways of adding value change handlers too
    public event PersistentPropertyInstanceValueChangeEventHandler<int>? PosXChanged {
        add => PosXProperty.AddValueChangeHandler(this, value);
        remove => PosXProperty.RemoveValueChangeHandler(this, value);
    }
    
    public event PersistentPropertyInstanceValueChangeEventHandler<int>? PosYChanged {
        add => PosYProperty.AddValueChangeHandler(this, value);
        remove => PosYProperty.RemoveValueChangeHandler(this, value);
    }
    
    public EditorWindowConfigurationOptions() {
        IVideoEditorService.Instance.VideoEditorCreatedOrShown += OnVideoEditorCreatedOrShown;
    }
    
    private void OnVideoEditorCreatedOrShown(IVideoEditorWindow window, bool isbeforeshow) {
        if (!isbeforeshow) { // when false, the window is actually visible
            window.WindowPosition = new SKPointI(this.PosX, this.PosY);
        }
    }
}
```

Then to register the config, override `RegisterConfigurations` in your plugin class.

```csharp
// The manager is the application's PSM, 
// accessible directly via Application.Instance.PersistentStorageManager
public override void RegisterConfigurations(PersistentStorageManager manager) {
    // Register our config.
    //   'editor' is the area. Areas are just files. There can be multiple configs per area.
    //   'windowinfo' is the config name in the area.
    manager.Register(new EditorWindowConfigurationOptions(), "editor", "windowinfo");
}
```

If you're modifying the main FramePFX source code, you register configs in the `OnFrameworkInitializationCompleted` method

## Registering exporters

#### Defining the exporter

First you need a class that derives `BaseExporterInfo`. This base class defines the standard information and behaviours for exporters.

This class then needs to override the `CreateContext` method, which creates an export context object. This object should derive from `BaseExportContext`.  
This class can then implement the `Export` method, which is what actually does the exporting (including creating the file and writing media information)

#### Exporter properties

Exporters might want adjustable parameters to change the export process (e.g. bitrate or resolution). Currently, this can  
only be done via the `PropertyEditor` instance defined in `BaseExporterInfo`. You can define data parameters (or use custom property editor slots),  
and register them in the property editor. The front end will automatically create standard UI controls for built-in property editor slots.

#### Export setup

The `ExportSetup` class contains basic information about the export process, which is common across every exporter (e.g. the start and end frame to export, and target file/directory)

#### Registration

Finally, you register the exporter via the `ExporterRegistry` (accessible by the `Instance` static property),  
and by invoking `RegisterExporter(ExporterKey, BaseExporterInfo)`. An exporter key contains a unique key and display name for the exporter

## Custom timeline clips

Creating a custom clip is very simple. Your clip should derive from `VideoClip` or `AudioClip` (audio is currently not working, since a rework of the rendering system is needed to make it work property).

Video clips have two main methods: `bool PrepareRenderFrame(PreRenderContext rc, long frame)` and `void RenderFrame(RenderContext rc, ref SKRect renderArea)`.

The prepare render frame method indicates to the rendering system whether the clip should be rendered. For example, if a resource reference required by the clip is not linked, then this method returns false. This method is invoked on the main thread

The render frame method is invoked on a background rendering thread, and is what should render the clip. You can access the skia canvas via `rc.Canvas`. the `ref renderArea` is used to tell the rendering system the affected pixel area, as an optimisation. For example, if you draw a 10x10 square starting at 5,5, then you would do: `renderArea = rc.TranslateRect(new SKRect(5, 5, 15, 15));`. The method `rc.TranslateRect` translates the rect into the effective rectangle based on the current `TotalMatrix` of the canvas

## Custom Commands

You can register custom commands in your plugin's `RegisterCommands` method, like so:

```csharp
public override void RegisterCommands(CommandManager manager) {  
    manager.Register(
        "myplugin.commands.editor.ShowCompTimlineName", 
        new ShowCompTimlineNameCommand()
    );  
}

private class ShowCompTimlineNameCommand : Command {  
    protected override Executability CanExecuteCore(CommandEventArgs e) 
    {  
        if (!DataKeys.TimelineKey.TryGetContext(e.ContextData, out Timeline? timeline)) {  
            // Normally, MenuItem will be invisible, or button will be disabled
            return Executability.Invalid;
        }  
    
        return timeline is CompositionTimeline 
            ? Executability.Valid 				   // Control clickable
            : Executability.ValidButCannotExecute; // Control disabled
    }
    
    protected override async Task ExecuteAsync(CommandEventArgs e) 
    {  
        if (!DataKeys.TimelineKey.TryGetContext(e.ContextData, out Timeline timeline)) 
            return;
        if (!(timeline is CompositionTimeline composition)) 
            return;
    
        await IMessageDialogService.Instance.ShowMessage(
            "hello", $"My resource = '{composition.Resource.DisplayName}'"
        );
    }
}
```

## Brush and Icon API

This section describes the colour brush and icon API. This provides a way for plugins to create icons and use them in different parts of the application without having to ever interact with avalonia bitmaps or images directly.

### Brush Manager

This service provides a mechanism for creating abstract delegates around Avalonia brushes.

- `CreateConstant`: creates the equivalent of `ImmutableSolidColorBrush`
- `GetStaticThemeBrush`: creates the equivalent of `IImmutableBrush`
- `GetDynamicThemeBrush` is more complicated. It's a subscription based object where the front end subscribes to dynamic changes
  of a brush via the application's `ResourcesChanged` and `ActualThemeVariantChanged` events.
  This allows, for example, an icon to use the standard glyph colour (which is white within dark themes and black within light themes, adjustable of course)

### Icon Manager

Icons are managed via the `IconManager`. This provides a way to creating different types of icons, such as images from the disk, bitmaps, custom geometry (SVG) and so on.
When creating an icon, you provide brushes created by the `BrushManager`.  
and they take brushes created by the .

Icon can be passed to context menu entries and used in toolbar buttons

> Accessing underlying icon pixel data is not currently implemented but is certainly possible; SVG icons for example would have to be rendered first using `RenderTargetBitmap`.

# Toolbars

FramePFX provides a way to dynamically add UI components, such as buttons and toggle buttons, that
can execute custom user code or commands. This is to allow plugins to add UI components without having
to necessarily dig into the raw UI components (like Buttons, ToggleButtons, etc.)

These are the current toolbars available and their class:

- `TimelineToolBarManager`: The toolbar at the bottom of the timeline. Supports west and east anchored buttons
- `ControlSurfaceListToolBarManager`: The toolbar at the bottom of the control surface list.
- `ViewPortToolBarManager`: The toolbar just below the view port. Supports west, center and east anchoring

## End

This documentation is still being updated, there's a few things missing.