# FramePFX
A small video editor

I started making this because other editors like vegas, prmiere pro, etc, just seem to run so slowly 
and some of them just lack basic features (e.g zoom in the view port with CTRL + MouseWheel)

I doubt this will ever even come close to those editors... but hopefully this will at least support "editing" videos, even if it's just cutting up clips

# Preview

![](FramePFX_2023-04-16_18.56.57.png)

## ViewModels/Models
I tried to wrap all models with view models so that the app could still function moderately well even if it had no view models. However, view models still take a lot of big responsibilities of the models (e.g. firing the model events when view model properties change in order to force a re-render), and it also opens up the possibility for viewmodel-model desynchronisation which hopefully won't happen

## Rendering
Rendering the main view port (and soon the clip/text resource previews) is done with SkiaSharp. Originally was done with OpenGL (using OpenTK) but SkiaSharp is much simpler to use (easy image loading, not sure about video yet though, easy texture drawing, etc)

## Resource list
`ResourceListControl` and `ResourceItemControl` are an example of how to implement multi-selection, drag dropping, and also shift-selection (to select a range of items)

Oh and uh... don't drag drop something like your C:\ drive or a folder which contains 100,000s of files in the hierarchy into the ResourceListControl, otherwise the app will probably freeze as it recursively loads all of those files

# Rendering/Encoding
TODO... but it won't modify the UI at all, in order to help render times. It will probably just setup a view port based on the render output resolution,
draw the clips each frame, and then copy the pixels from OpenGL to an encoder. Maybe FFMPEG?

## Downloading/Running
To run this, you just need to download this repo and also the ffmpeg's shared x64 libraries (https://github.com/BtbN/FFmpeg-Builds/releases/tag/latest, specifically  https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl-shared.zip). You place all of the files in the bin folder (apart from the ffmpeg exes) into the bin folder of this project (FramePFX/Bin/Debug or Release depending on how you compiled it)

And to drag videos into the editor, you drag and drop a video file to the top left "resource manager", and then drag one of those items into the timeline. Will soon support directly dropping a clip into the timeline

