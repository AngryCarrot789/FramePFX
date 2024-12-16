# FramePFX
A small (non-linear) video editor, written in C# using Avalonia for the UI

I mainly started this as a learning tool into the world of video/audio processing (all I really knew before this was basic OpenGL drawing), but also because other editors like vegas, premiere pro, hitfilm, etc, just seem to run so slowly and some of them just lack basic features (e.g zoom in the view port with CTRL + MouseWheel)

I doubt this will ever even come close to those editors, but hopefully it will at least support some basic editing

If you have any feedback/criticism for the app, that would be appreciate! Also feel free to contribute, if you would like to. 
You can see the TODO list near the bottom, and also how to download and compile the appliation

# Preview

This is the latest version using Avalonia:
![](FramePFX-DesktopUI_2024-12-06_17.33.20.png)

Here is a preview of the export process. Export button is in File>Export, you specify a path and then click Export.
To cancel the render you just click Cancel on the dialog behind the export progress window

The grey panel below "Exporter: FFmpeg" is encoder-specific details
![](FramePFX-DesktopUI_2024-12-07_00.13.06.png)

# Building
FramePFX assumes everything is 64 bit --- x86/32-bit/AnyCPU won't work properly!

All of the native projects are automatically downloaded and compiled when you 
first build the C# projects, however,  FFmpeg needs to be downloaded separately. 
Here is the specific version that works currently (windows only): 
https://github.com/BtbN/FFmpeg-Builds/releases/download/autobuild-2024-12-11-13-02/ffmpeg-N-118048-g1e76bd2f39-win64-gpl-shared.zip

### Instructions

- Create a folder called `ffmpeg` in the solution folder. 
- From the downloaded archive copy everything (4 dirs and the LICENCE.txt) into this new ffmpeg folder

There should be 8 DLL files in `\FramePFX\ffmpeg\bin`, and one of them should be avcodec-61.dll. If it's not 61 you have the wrong version of FFmpeg.
You can delete the EXE files if you want, since they aren't used

- Open FramePFX.sln. You will get an error about projects not being loaded; Ignore it. Now build the solution by going to the `Build` menu and clicking `Build Solution` 

Hopefully then you should be able to run and modify any of the 3 FramePFX projects without issue. This project uses Avalonia 11.2.2 and .NET 8 (C# 12)

### Windows only commands

The projects in the solution use windows commands like mkdir and xcopy, which may not work on other platforms.
I only have a Windows machine, so I can't really offer any alternatives. However, feel free to create a pull request
on a more cross-platform solution!

The 8 DLLs just have to be in the same directly as the FramePFX-DesktopUI.exe executable

### Possible build problems
Sometimes, the SkiaSharp nuget library doesn't copy the skia library files to the bin folder when you clone this repo and built. There are 2 fixes I found:
- Copy `\packages\SkiaSharp.2.88.7\runtimes\win-x64\native\libSkiaSharp.dll` into the editor's bin folder.
- Or, delete the `packages` folder in the solution dir, then right-click the solution in visual studio and click "Clean Solution", then click Restore Nuget Packages, then rebuild all.
  If none of these work, try uninstalling SkiaSharp in the nuget manager and then reinstalling. If that still does not work, then I really don't know what's going on...

# TODO
### Avalonia Remake:
- Implement project settings dialog
- Implement UI for Effects list that can be dropped into a clip/track
### Audio
I removed audio playback from the Avalonia remake because it required PortAudio, requiring a native project, both of which were a hassle to auto-compile. Check out the last WPF release for the issues on it though; it wasn't great
### Automation Engine
- Add support for smooth interpolation (e.g. a curve between 2 key frames). I tried doing this, but had a hard time figuring out the math to do the interpolation, and also doing the hit testing for the UI
### Clips
- AVMediaVideoClip is extremely slow for large resolution videos (e.g. 4K takes around 40ms to decode and render onscreen), and only a few video codecs even seem to work. Lots of common file formats give an error like "found invalid 
  data while decoding". I don't know FFmpeg much but I hope to somehow fix this at some point
- Implement fading between 2 clips
### History system
- There's no undo functionality yet. I might try and implement this once I implement a few of the other features like audio and maybe hardware accelerated final-frame assembly in the renderer
### Bugs to fix
- Importing certain video files can cause the render to fail (some sort of "found invalid data while decoding" error)

## Contributing
Feel free to contribute whatever you want if you think it'll make the editor better!
The code base isn't perfect so feel free to help try and standardize things

# Licence
All source files in FramePFX are under the GNU General Public License version 3.0 or later (GPL v3.0+).
FramePFX uses libraries that have other licences, such as MIT/LGPL licences.

If any source file is missing a copyright notice, it is assumed to be licenced under the same
licence as FramePFX

Currently, the used LGPL parts are:
- FFmpeg.AutoGen, which is licenced under the GNU Lesser General Public License version 3.0 (LGPL v3.0)
- FFmpeg
