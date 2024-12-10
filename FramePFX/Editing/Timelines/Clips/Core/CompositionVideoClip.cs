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
using FramePFX.Editing.ResourceManaging.ResourceHelpers;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.Timelines.Clips.Video;
using SkiaSharp;

namespace FramePFX.Editing.Timelines.Clips.Core;

/// <summary>
/// A clip that represents the visual part of a composition timeline
/// </summary>
public class CompositionVideoClip : VideoClip, ICompositionClip
{
    public IResourcePathKey<ResourceComposition> ResourceCompositionKey { get; }

    private Task renderTask;
    private ResourceComposition renderResource;

    public CompositionVideoClip()
    {
        this.UsesCustomOpacityCalculation = true;
        this.ResourceCompositionKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceComposition>();
        this.ResourceCompositionKey.ResourceChanged += this.ResourceCompositionKeyOnResourceChanged;
    }

    private void ResourceCompositionKeyOnResourceChanged(IResourcePathKey<ResourceComposition> key, ResourceComposition olditem, ResourceComposition newitem)
    {
        this.InvalidateRender();
    }

    public override Vector2? GetRenderSize()
    {
        Project project = this.Project;
        if (project == null)
        {
            return null;
        }

        return new Vector2(project.Settings.Width, project.Settings.Height);
        // if (!this.ResourceCompositionKey.TryGetResource(out ResourceComposition resource)) {
        //     return null;
        // }
        // SKRect lastRect = resource.Timeline.RenderManager.LastRenderRect;
        // return new Vector2(lastRect.Width, lastRect.Height);
    }

    public override bool PrepareRenderFrame(PreRenderContext rc, long frame)
    {
        if (!this.ResourceCompositionKey.TryGetResource(out ResourceComposition resource))
        {
            return false;
        }

        this.renderResource = resource;
        this.renderTask = resource.Timeline.RenderManager.RenderTimelineAsync(frame, CancellationToken.None, rc.RenderQuality);
        return true;
    }

    public override void RenderFrame(RenderContext rc, ref SKRect renderArea)
    {
        try
        {
            this.renderTask.GetAwaiter().GetResult();
            RenderManager render = this.renderResource.Timeline.RenderManager;
            render.OnFrameCompleted();
            using (SKPaint paint = new SKPaint())
            {
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
        catch (AggregateException e)
        {
            if (e.InnerExceptions.FirstOrDefault(x => x is TaskCanceledException) != null)
            {
                return;
            }

            throw;
        }
        finally
        {
            this.renderResource = null;
            this.renderTask = null;
        }
    }
}