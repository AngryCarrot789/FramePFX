using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging.ResourceHelpers;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.RBC;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips.Core {
    /// <summary>
    /// A clip that represents the visual part of a composition timeline
    /// </summary>
    public class CompositionVideoClip : VideoClip, ICompositionClip {
        public IResourcePathKey<ResourceComposition> ResourceCompositionKey { get; }

        private Task renderTask;
        private ResourceComposition renderResource;

        public CompositionVideoClip() {
            this.ResourceCompositionKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceComposition>();
            this.ResourceCompositionKey.ResourceChanged += this.ResourceCompositionKeyOnResourceChanged;
        }

        private void ResourceCompositionKeyOnResourceChanged(IResourcePathKey<ResourceComposition> key, ResourceComposition olditem, ResourceComposition newitem) {
            this.InvalidateRender();
        }

        public override Vector2? GetRenderSize() {
            Project project = this.Project;
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
            if (!this.ResourceCompositionKey.TryGetResource(out ResourceComposition resource)) {
                return false;
            }

            this.renderResource = resource;
            this.renderTask = resource.Timeline.RenderManager.RenderTimelineAsync(frame, CancellationToken.None, rc.RenderQuality);
            return true;
        }

        public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
            try {
                this.renderTask.Wait();

                RenderManager render = this.renderResource.Timeline.RenderManager;
                render.Draw(rc.Surface);

                renderArea = rc.TranslateRect(render.LastRenderRect);
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
}