# FramePFX
A small video editor

I started making this because other editors like vegas, premiere pro, etc, just seem to run so slowly 
and some of them just lack basic features (e.g zoom in the view port with CTRL + MouseWheel)

I doubt this will ever even come close to those editors... but hopefully this will at least support "editing" videos, even if it's just cutting up clips

# Preview

![](FramePFX_2023-06-19_20.06.57.png)

## ViewModels/Models
I tried to wrap all models with view models so that the app could still function moderately well even if it had no view models or even UI, simiular to how OBS can run without the front-end entirely... not that you'd want to but anyway. 

View models still take a lot of big responsibilities of the models though (e.g. firing the model events when view model properties change in order to force a re-render), and it also opens up the possibility for viewmodel and model desynchronisation (especially when it comes to synchronising VM observable collections and model lists) which hopefully won't happen

## Automation/animation
Always found automating parameters in the popular editors to be generally finicky. Ableton Live has a really good automation editor though, so I plan to mimic that. My plan is:
- each clip has it's own keyframe/envolope editor that stretches the entirety of the clip. 
- Layers would have the same, but it stretches the entire timeline. 
- Automating project settings, or anything else really, could be done on an automation-specific layer (allowing for more than just video/audio layers)

### Here's a demo of video layer opacity automation (very WIP; can only move these 3 points currently)
![](FramePFX_2023-06-21_03.33.35.png)

The automation/parameter selector it on the top right of the clip (in its header). The O button toggles an override, which disables automation, and the other one towards the left clears the current selection

## Rendering
Rendering the main view port (and soon the clip/text resource previews) is done with SkiaSharp. Originally was done with OpenGL (using OpenTK) but SkiaSharp is much simpler to use (easy image loading, not sure about video yet though, easy texture drawing, etc)

## Resource list
`ResourceListControl` and `ResourceItemControl` are an example of how to implement multi-selection, drag dropping, and also shift-selection (to select a range of items)

Oh and uh... don't drag drop something like your C:\ drive or a folder which contains 100,000s of files in the hierarchy into the ResourceListControl, otherwise the app will probably freeze as it recursively loads all of those files

# Rendering/Encoding/Exporting
Click "Export" in the file menu at the top left, and you can specify some render details. Currently, only .mp4 aka MPEG-4 aka h.264 is supported. Might try to implement more soon. The output cannot be scaled at the moment

![](FramePFX_2023-06-23_03.20.48.png)

## Downloading/Running
To run this, you just need to download this repo and also the ffmpeg's shared x64 libraries (https://github.com/BtbN/FFmpeg-Builds/releases/tag/latest, specifically https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl-shared.zip). You build the project (debug, release, or whatever), then place all of the FFmpeg files in the bin folder (apart from the ffmpeg exes) and it should run fine, then you can debug it if you want. It's using .NET Framework 4.7.1 or 4.7.2 (can't quite remember) and .NET Standard 2.0

And to drag videos into the editor, you drag and drop a video file to the top left "resource manager", and then drag one of those items into the timeline. Will soon support directly dropping a clip into the timeline

## Cross platform
Currently there's only a WPF implementation, but I hope to switch to avalonia at some point or MAUI. Most of the classes are implented in the .NET Standard project, so it's relatively easy to port the app over to different platforms, but then there's also SkiaSharp, FFmpeg, etc, dependencies too...

## BUGS ono
- Fixed the bug where dragging a clip across layers crashes the app. However, importing certain video files can still crash (some sort of "found invalid data while decoding" error)

