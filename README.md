# FramePFX
A small (non-linear) video editor, written in C# using WPF

I mainly started this as a learning tool into the world of video/audio processing (all I really knew before this was basic OpenGL drawing), but also because other editors like vegas, premiere pro, hitfilm, etc, just seem to run so slowly and some of them just lack basic features (e.g zoom in the view port with CTRL + MouseWheel)

I doubt this will ever even come close to those editors, but hopefully it will at least support some basic editing

If you have any feedback/criticism for the app, that would be appreciate! Also feel free to contribute, if you would like to. You can see the TODO list near the bottom

# Preview
There are 6 themes. This is the `Soft Dark` theme. But there's also `Deep dark`, `Dark Grey`, `Grey`, `Red and Black` and a `Light Theme` (if you dare use it)

This is the latest version:
![](FramePFX_2024-02-03_01.07.07.png)

This is a slightly older version showing some automation/animation/keyframe usage:
![](FramePFX_2024-01-31_02.41.43.png)

Here is a preview of the export process. Export button is in File>Export, you specify a path and then click Export. Notice how slow the render is (bottom right corner) :'(. The render uses Skia's highest quality 
filter during export (low for the preview view port), but I will add an option to change that. To cancel the render you just click Cancel on the dialog behind the export progress window

The grey panel below "Exporter: FFmpeg" is encoder-specific details. I haven't added anything yet, but soon I will put things like a bitrate slider in there.
![](FramePFX_2024-02-03_01.08.52.png)

### Automation/animation
Always found automating parameters in the standard editors to be generally finicky. Ableton Live has a really good automation editor though, so I took a fair bit of inspiration from it:
- Each clip has it's own keyframe/envelope editor that stretches the entirety of the clip. Effects also use this
- Tracks have the same, but it stretches the entire timeline. 
- Automating project settings, or anything else really, will soon be do-able on a timeline automation specific track (allowing for more than just video/audio tracks)

The clip's automation sequence editor's target parameter can be changed by selecting a row (I call them "slots") in the property editor. The currently selected slot is what the clip shows (if clip automation is visible, click C to toggle)

# General code/docs overview

## Models and the UI

I previously made this using MVVM everywhere, but I had to basically create ViewModels for every type of Model and map hierarchies of objects (Timeline->Track->Clip, TimelineViewModel->TrackViewModel->ClipViewModel),
which was frustrating and made adding new features really difficult, so I decided to rewrite the entire program to use a mostly non-view-model design, kind of like MVC except views are the controllers (at the moment...), 
where the models are just the state and the views/controls add and remove event handlers to obverse the state of the models.

This turns out to be a lot more performant (which is actually the main reason I switched), somewhat just as easy to add new features, and the  signals between ViewModel/View are not entirely ICommands anymore. 
But it does mean moving this to something like Avalonia (which I haven't thought of doing yet) would be a lot more difficult.

The editor can run entirely without a front end though, as none of the models contain any UI specific code (except for basic things like a IsSelected property in clips, tracks, resources, etc.). It does need the WPF dispatcher though

## Rendering
Rendering uses SkiaSharp (for simplicity) and multiple render threads for speed. The `RenderManager` class handles the render initialisation.

The render phase is like this:
- Something tells the RenderManager that the render state is invalidated and to schedule a render on the app dispatcher
- All timeline tracks processed (bottom to top, as most editors do) to figure out if the track can be rendered (is it visible 
  and a non-zero opacity and are there clips on the playhead that are visible)
- `PrepareRenderFrame` is called on the video tracks, which then calls `PrepareRenderFrame` on the clip being rendered. That method is used to
  generate rendering proxy data, such as the opacity of the clip at the current point in time
- A task is started, and that task calls the tracks' `RenderFrame` method, which calls the clips' `RenderFrame` method, which uses that proxy data (generated in the preparation phase)
  to actually draw pixels into the track's `SKSurface`
- Once all tracks have been rendered, the final frame is assembled from each track's `SKSurface` (on the rendering thread as well)
- The final frame is now completed, `FrameRendered` is fired in the render manager, and the view port hooks onto that event and draws the rendered frame

## Resource list
The resources are shareable between clips, so that clips can obviously share similar details (e.g. same text or font/font size), or same image, same shape colour, etc.

To drag videos, images, etc., into the editor: drag and drop the file to the top left "resource manager panel", and then drag one of those items into the timeline

## Command system, shortcut system and context menus
### Command System
I created a system that is inspired from IntelliJ IDEA's action system, where you have a single command manager which contains all of the commands. You access commands
via a string key (simplest type of key to use), and then execute the command by passing in contextual data (stored in a `IContextData`). The context data gets a value from
data keys, and the `ContextData` implementation stores entries in a map which is how UI components store their context data. 
The `DataManager` class manages the context data for all UI components (scroll down for more info)

This means that when you for example press F2 or CTRL+R while focused on a clip, there's a lot of data keys between the root window and the clip UI object, and so
in the rename command, you have access to all of them; the editor, project, timeline, track and clip. Whereas if you just have the window focused and press a shortcut, you 
may only have the editor and project available; It's context sensitive, duh

### Shortcuts
The shortcut system listens to inputs at an application level instead of receiving input from a specific window (however, a window can only really process shortcuts if it
has a focus path associated with it, which can be set via the `UIInputManager.FocusPath` attached property). To save the details, the shortcut system figures out a list of shortcuts
to "activate" based on the current global focus path, and activates all of them until one is activated successfully. 
Keymap.xml contains the shortcuts (and some unused ones from the old app version)

### Context menus
Context menus use the `AdvancedContextMenu` class. Context menu items are generated on demand each time the context menu is open, which isn't the most performant option
but it's pretty quick for now (much quicker now than before when I used binding and ItemsSource). I try to only use the `CommandContextEntry` (which maps to a `AdvancedCommandMenuItem`) 
menu item which invokes a command but I sometimes use a `EventContextEntry` because it's much easier to use, but less portable as shortcuts can't activate/invoke them

### Data Manager
The data manager is used to store local context data in a control, and implement context data inheritance containing the merged context data for all of a control's visual parents and itself. 

It has two primary properties: `ContextData`, and `InheritedContextData`. Despite the name, the inherited version does not use WPF's built in property inheritance feature, but instead
uses my own inheritance implementation by using `ContextDataProperty` changes and the `VisualAncestorChanged` event (which I add/remove reflectively when the `ContextDataProperty` changes,
and it fires when an object's visual parent changes). By doing this, an event can be fired (`InheritedContextInvalidatedEvent`) for every single child of an element in its visual tree when its 
local context data is changed. The `ContextUsage` class depends on this feature in order to do things like re-query the executability state of a command when a control's full inherited context changes

# TODO
### Audio
While audio play back exists, it doesn't work properly at all and is extremely glitchy. Trying to figure out how to improve it
### Automation Engine
- Add support for smooth interpolation (e.g. a curve between 2 key frames). I tried doing this, but had a hard time figuring out the math to do the interpolation, and also doing the hit testing for the UI
### Clips
- AVMediaVideoClip is extremely slow for large resolution videos (e.g. 4K takes around 40ms to decode and render onscreen), and only a few video codecs even seem to work. Lots of common file formats give an error like "found invalid 
  data while decoding". I don't know FFmpeg much but I hope to somehow fix this at some point
- Implement fading between 2 clips
### Rendering
- I plan to use hardware acceleration for at least the final frame assembly because, at the moment, that is the most computationally expensive operation in the render phase right next to video decoding.
  I've added many optimisations to improve performance (like render area feedback from clips so that the renderer only copies a known "effective" area of pixels instead of the whole frame), but it's still quite slow, especially when using composition clips
### History system
- There's no undo functionality yet. I might try and implement this once I implement a few of the other features like audio and maybe hardware accelerated final-frame assembly in the renderer
### Bugs to fix
- Importing certain video files can cause the render to fail (some sort of "found invalid data while decoding" error)

# Downloading and Compiling/Building

Compiling the editor yourself requires some manual labour at the moment. It uses 2 external libraries:
- FFmpeg (for video decoding and exporting/encoding). The pre-compiled binaries and libraries can be downloaded at: 
  https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl-shared.zip 
- PortAudio for audio playback. This can be downloaded and compiled at https://files.portaudio.com/download.html. 

### FramePFX assumes everything is 64 bit: x86/32-bit/AnyCPU most likely won't work!

Create a folder called `libraries` in the solution folder and then two sub-folders called `ffmpeg` and `portaudio`. 
Copy the contents of both of the archive files you download into the respective folders. You should be able to navigate 
to `\FramePFX\libraries\ffmpeg\lib`, and `\FramePFX\libraries\portaudio\` will contain `CMakeList.txt`

I'd recommend following PortAudio's official build instructions, but they basically consist of: open CMake GUI, set sources folder and build folder, 
click configure then generate and then open the VS solution and click build solution

Now that you build PortAudio, your build folder should contain Debug or Release depending on what you used. Now, go back into the `portaudio` folder, 
create a folder called `lib` and copy, from build/Debug (or Release) folder, `portaudio_x64.lib` and `portaudio_x64.pdb`. 
The lib folder is required for the NativeEngine project.

Then back in that build/Debug (or Release) folder, copy `portaudio_x64.dll` and `portaudio_x64.exp` into the video editor's 
respective Debug or Release folder (inside the FramePFX bin folder).

Now, to make FFmpeg work, if debugging only from VS/Rider or whatever IDE you use, it works fine as long as the ffmpeg folder is in the libraries folder.
If you associated the `fpfx` file extension with FramePFX in the build folders, then you most likely will need to place all of the DLL files in ffmpeg's `bin`
folder (noty the .exe files) in the same folder as `FramePFX.exe`. PortAudio's DLL must always be placed in the same folder as the app exe 

And then hopefully if I didn't miss anything out, you should be able to compile the NativeEngine project and then the FramePFX project, and the editor should run.

The project uses .NET Framework 4.8, so you will need that installed too to compile the FramePFX project

### Possible build problems
Sometimes, the SkiaSharp nuget library doesn't copy the skia library files to the bin folder when you clone this repo and built. There are 2 fixes I found:
- Copy `\packages\SkiaSharp.2.88.7\runtimes\win-x64\native\libSkiaSharp.dll` into the editor's bin folder.
- Or, delete the `packages` folder in the solution dir, then right click the solution in visual studio and click "Clean Solution", then click Restore Nuget Packages, then rebuild all.
If none of these work, try uninstalling SkiaSharp in the nuget manager and then reinstalling. If that still does not work, then I really don't know what's going on... 

## Contributing
Feel free to contribute whatever you want if you think it'll make the editor better!

# Licence
All source files in FramePFX are under the GNU General Public License version 3.0 or later (GPL v3.0+).
FramePFX uses libraries that have other licences, such as MIT/LGPL licences.

If any source file is missing a copyright notice, it is assumed to be licenced under the same
licence as FramePFX

Currently, the used LGPL parts are:
- FFmpeg.AutoGen, which is licenced under the GNU Lesser General Public License version 3.0 (LGPL v3.0)
- FFmpeg
