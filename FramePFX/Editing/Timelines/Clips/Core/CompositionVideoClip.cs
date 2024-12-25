//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Numerics;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.ResourceManaging.NewResourceHelper;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editing.Timelines.Clips.Core;

/// <summary>
/// A clip that represents the visual part of a composition timeline
/// </summary>
public class CompositionVideoClip : VideoClip {
    public static readonly ResourceSlot<ResourceComposition> ResourceCompositionKey = ResourceSlot.Register<ResourceComposition>(typeof(CompositionVideoClip), "CompositionResKey");

    private Task renderTask;
    private ResourceComposition renderResource;

    public override bool IsSensitiveToPlaybackSpeed => true;

    public CompositionVideoClip() {
        this.UsesCustomOpacityCalculation = true;
    }

    static CompositionVideoClip() {
        ResourceCompositionKey.ResourceChanged += (slot, owner, oldResource, newResource) => ((CompositionVideoClip) owner).InvalidateRender();
    }

    protected override void OnProjectChanged(Project? oldProject, Project? newProject) {
        base.OnProjectChanged(oldProject, newProject);
        this.OnRenderSizeChanged();
    }

    public override Vector2? GetRenderSize() {
        Project? project = this.Project;
        if (project == null) {
            return null;
        }

        return new Vector2(project.Settings.Width, project.Settings.Height);
        // if (!this.ResourceCompositionKey.TryGetResource(out ResourceComposition resource)) {
        //     return null;
        // }
        // SKRect lastRect = resource.Timeline.RenderManager.LastRenderRect;
        // return new Vector2(lastRect.Width, lastRect.Height);
    }

    public override bool PrepareRenderFrame(PreRenderContext rc, long frame) {
        if (!ResourceCompositionKey.TryGetResource(this, out ResourceComposition? resource)) {
            return false;
        }

        long maxDuration = resource.Timeline.MaxDuration;
        if (maxDuration < 1) {
            return false;
        }

        // Check just in case someone naughtily derives this class and sets to false ;)
        if (this.IsSensitiveToPlaybackSpeed)
            frame = this.GetRelativeFrameForPlaybackSpeed(frame);

        frame = Periodic.MethodNameHere(frame, 0, resource.Timeline.MaxDuration);
        this.renderResource = resource;
        this.renderTask = resource.Timeline.RenderManager.RenderTimelineAsync(frame, CancellationToken.None, rc.RenderQuality);
        return true;
    }

    public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
        try {
            this.renderTask.GetAwaiter().GetResult();
            RenderManager render = this.renderResource.Timeline.RenderManager;
            render.OnFrameCompleted();
            using (SKPaint paint = new SKPaint()) {
                paint.FilterQuality = rc.FilterQuality;
                paint.Color = RenderUtils.BlendAlpha(SKColors.White, this.RenderOpacity);
                render.Draw(rc.Surface, paint);
            }

            renderArea = rc.TranslateRect(render.LastRenderRect);
        }
        catch (TaskCanceledException) {
        }
        catch (OperationCanceledException) {
        }
        catch (AggregateException e) {
            if (e.InnerExceptions.FirstOrDefault(x => x is TaskCanceledException) != null) {
                return;
            }

            throw;
        }
        finally {
            this.renderResource = null;
            this.renderTask = null;
        }
    }
}