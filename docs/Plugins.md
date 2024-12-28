# Plugin System Overview
A recent update allows support for plugins, either core projects or dynamically loaded plugins.

### Core projects
These are not really plugins, but instead modules. They are projects referenced by the main project and are loaded before dynamic plugins.

For example, the FFmpegMedia plugin is a core plugin, and implements exporting   
and playback of media files. This core plugin is only loaded when FFmpeg is available

### Dynamic/Assembly plugins
These are plugins that are completely dynamically loaded. This system is still WIP, but they can do everything a core plugin can do.   
Assembly loading doesn't use an `AssemblyLoadContext`, so there's versioning issues to deal with

The only dynamic plugin is `CircleClipPlugin`, which is a test plugin which adds a video clip that draws a circle.

## Supported and unsupported features overview

So far, plugins are able to do a fair amount of things, such as:
- Registering custom exporters (see FFmpegMedia plugin which adds an FFmpeg exporter, to export .mp4 files)
- Custom configuration pages (still slightly WIP; no built-in way to save data to disk)
- Custom data parameters and property editor slots  (aka rows)
- Custom clips (see FFmpegMedia plugin which adds a video clip to play videos)
- Custom commands
- Adding context menu entries to clip, track and resource context menus
- Resource drop-in-timeline handling
- Handling native files dropped in resource manager

... And a few others. There's still lots that cannot be done yet. A few of the main things are:
- Cannot add buttons in the toolbars for the timeline, viewport and track surface list box
- Cannot add menus to the editor's top-level menu
- Custom panels in the UI (e.g. maybe a side panel for previewing a clip)
- Access to the keyframe UI system, thought I can't why a plugin would want to access this

# FramePFX API
This section describes the APIs of FramePFX  (incomplete)

## Property Editor
The property editor system is highly customisable framework, which also makes it somewhat complicated to use.

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
new ParameterFloatPropertyEditorSlot(   // The slot which manages animatable float parameters  
    TextVideoClip.LineSpacingParameter, // The specific parameter we want to modify
    typeof(TextVideoClip),              // The type of object the slot is applicable to (multiselection requires this) 
    "Line Spacing",                     // A readable display name string, on the left column 
    DragStepProfile.Pixels)             // The NumberDragger's increment behaviour. Pixels is fine tuned
{   
    // Converts, say, "25.3" into "25.3 px", and supports converting back too "25.3"  
    ValueFormatter = SuffixValueFormatter.StandardPixels 
});
```  

This example is for an automatable parameter though, and you might not need animatable parameters, in which case,  you can use `DataParameterFloatPropertyEditorSlot`  for a `DataParameterFloat`.

You can register a custom property editor slot's UI control via the `BasePropertyEditorSlotControl.Registry`, like so:
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
and the `Handlers` property is updated with the objects that can be modified. `ClearHandlers` will obviously clear the handlers and the slot is no longer applicable.

## Automatable and Data Parameters
FramePFX provides two parameter subsystems: data parameters, and automatable parameters.

Data parameters are represented by the base class `DataParameter`, but you shouldn't override this class directly, instead override `DataParameter<T>` if you need your own type. These parameters are used to make interfacing with a `PropertyEditor` or binding to the UI in code-behind generally easier; it saves having to use reflection everywhere by instead going through a `ValueAccessor<T>`.

Automatable parameters have a more specific usage, in that they are just keys to a dictionary of automation sequences. The file `docs/Automation.md` has more information about the automation system. Automatable parameters, like data parameters, can also be used with property editors, and the standard automation property editor slots provide insert key frame/toggle override/reset value commands, which saves doing it manually.

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

The prepare render frame method indicates to the rendering system whether or not the clip should be rendered. For example, if a resource reference required by the clip is not linked, then this method returns false. This method is invoked on the main thread

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

private class ShowCompTimlineNameCommand : AsyncCommand {  
    protected override Executability CanExecuteOverride(CommandEventArgs e) {  
        if (!DataKeys.TimelineKey.TryGetContext(e.ContextData, out Timeline? timeline)) {  
            // Normally, MenuItem will be invisible, or button will be disabled
            return Executability.Invalid;
	    }  
          
        return timeline is CompositionTimeline 
	        ? Executability.Valid 				   // Control clickable
	        : Executability.ValidButCannotExecute; // Control disabled
    }
    
    protected override async Task ExecuteAsync(CommandEventArgs e) {  
        if (!DataKeys.TimelineKey.TryGetContext(e.ContextData, out var timeline)) {  
            return;  
        }
        
        if (!(timeline is CompositionTimeline composition)) {  
            return;  
        }
        
        await IMessageDialogService.Instance.ShowMessage(
            "hello", 
            $"My resource = '{composition.Resource.DisplayName}'"
        );  
    }  
}
```

Async commands are an extension of `Command`. If you don't need async, then just use `Command` if you want to.

## End
This documentation is still being updated, there's a few things missing.