## Rendering

***This system needs a rework to get audio playback working***

## Video

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

This is a simple but still performant rendering technique over say rendering all clips sequentially on the main thread (which is what I used to do).
This may change in the future if I come up with a better system, but for now, it works pretty well


## Audio
Audio playback is done via portaudio. Currently, it is sort of implemented but not active due to crackling issues
