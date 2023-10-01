# FramePFX
A small (non-linear) video editor, written in C# using WPF/MVVM

I mainly started this as a learning tool into the world of video/audio processing (all I really knew before this was basic OpenGL drawing), but also because other editors like vegas, premiere pro, hitfilm, etc, just seem to run so slowly and some of them just lack basic features (e.g zoom in the view port with CTRL + MouseWheel)

I doubt this will ever even come close to those editors, but hopefully it will at least support some basic editing

# Preview
There are 6 themes. This is the `Soft Dark` theme. But there's also `Deep dark`, `Dark Grey`, `Grey`, `Red and Black` and a really bad `Light Theme` that makes discord's light theme look good
![](FramePFX.WPF_2023-09-22_22.08.29.png)

### Automation/animation
Always found automating parameters in the popular editors to be generally finicky. Ableton Live has a really good automation editor though, so I took a fair bit of inspiration from it:
- Each clip has it's own keyframe/envelope editor that stretches the entirety of the clip. 
- Tracks have the same, but it stretches the entire timeline. 
- Automating project settings, or anything else really, will soon be do-able on a timeline automation specific track (allowing for more than just video/audio tracks)

#### Demo of video track opacity automation
The automation/parameter selector for tracks is on the right side of the thingy on the left. For clips, select a clip and the selectors show on the right side (in its header). The "O" button toggles an override, which disables automation, and the "-" button clears the current selection. 

The "Rec" button above the timeline on the left starts recoding every parameter (similar to the feature in Cinema4D. This screen shot is old so it doesn't show the button); modify anything like scale or opacity and it inserts a key frame

![](FramePFX_2023-06-21_03.33.35.png)

### Encoding/Exporting
Click "Export" in the file menu at the top left, and you can specify some render details. Currently, only .mp4 aka MPEG-4 aka h.264 is supported. Might try to implement more soon. The output cannot be scaled at the moment. A timeline frame is rendered, then that BGRA frame is converted to YUV, then encoded, then written to file (encoding and writing aren't necessarily in the same order; encoding takes some time)

![](FramePFX_2023-06-23_03.20.48.png)

# Backend stuff

### ViewModels/Models 
I tried to wrap all models with view models so that the app can still function even if it had no view models or even UI, 
similar to how OBS can run without the front-end entirely, not that you'd want to...

View models still take a good few big responsibilities of the models though, such as firing the model events when view model properties change in order to force a re-render

### Rendering
Previously rendering was done with SkiaSharp for simplicity, but I decided to move back to OpenGL to make use of shaders and general GPU rendering, at the price of having to deal with device-independent coordinates and manually allocating/deallocating rendering data (textures, meshes, shaders, etc.) which I still need to fully implement.

Each track has its own framebuffer and texture, which the clip is rendered to (first found clip intersecting the playhead. will soon try to implement fading somehow). 
Then each track's texture is drawn into the timeline's framebuffer, and then that is finally drawn into the default framebuffer. 

The render context class contains a stack of active framebuffers, so recursive rendering (composition timelines) work

### Resource list
`ResourceListControl` and `ResourceItemControl` are an example of how to implement multi-selection, drag dropping, and also shift-selection (to select a range of items)

The resources are shareable between clips so that clips can obviously share similar details (e.g. same text or font/font size), or same image, same shape colour, etc.

To drag videos, images, etc., into the editor: drag and drop the file to the top left "resource manager", and then drag one of those items into the timeline. Will soon support directly dropping a clip into the timeline

Oh and uh... don't drag drop something like your C:\ drive or a folder which contains 100,000s of files in the hierarchy into the ResourceListControl, otherwise the app will probably freeze as it recursively loads all of those files

### Audio
I don't know how to implement audio playback yet, so that's a TODO thing. If you think you could help implement that, then feel free to give it a try

# Compiling
FFmpeg's shared x64 libraries are the only external library (at the moment...). They can be found here: 
https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl-shared.zip

You build the project (debug or release), then place all of the FFmpeg DLL files in the WPF project's bin folder, and then you should be able to run the editor, and debug it if you want. 

The front-end uses .NET Framework 4.8, and the back-end uses .NET Standard 2.0

## Cross platform
Currently there's only a WPF implementation, but I hope to switch to Avalonia at some point or MAUI. Most, if not all, of the important classes are located in the .NET Standard project, so it's relatively easy to port the app over to different platforms. However there's also SkiaSharp, FFmpeg, and soon NAudio (or some other audio processor library) dependencies too...

## BUGS
- Importing certain video files can crash (some sort of "found invalid data while decoding" error)

## Licence
Project is licenced under MIT. I'm not a lawyer but, AFAIK, FFmpeg and FFmpeg.AutoGen being licenced primarily under GNU Lesser General Public License allows MIT to be used as long as the FFmpeg source code is not modified (which in this project, it isn't)
