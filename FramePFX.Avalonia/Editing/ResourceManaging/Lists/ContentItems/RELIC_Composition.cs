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

using System;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using FramePFX.BaseFrontEnd.AvControls;
using FramePFX.BaseFrontEnd.ResourceManaging;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Utils.RDA;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Lists.ContentItems;

public class RELIC_Composition : ResourceExplorerListItemContent {
    private SKPreviewViewPortEx? PART_ViewPort;

    public new ResourceComposition? Resource => (ResourceComposition?) base.Resource;

    private readonly RateLimitedDispatchAction updatePreviewExecutor;

    public RELIC_Composition() {
        this.updatePreviewExecutor = new RateLimitedDispatchAction(this.OnUpdatePreview, TimeSpan.FromSeconds(0.2));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ViewPort = e.NameScope.GetTemplateChild<SKPreviewViewPortEx>(nameof(this.PART_ViewPort));
    }

    private async Task OnUpdatePreview() {
        if (this.PART_ViewPort == null || this.ListItem == null)
            return;

        await Application.Instance.Dispatcher.Invoke(async () => {
            if (this.PART_ViewPort == null || this.ListItem == null) {
                return;
            }

            ResourceComposition? resource = this.Resource;
            RenderManager? rm = resource?.Timeline.RenderManager;
            if (rm?.surface != null) {
                await (rm.LastRenderTask ?? Task.CompletedTask);
                if (rm.LastRenderRect.Width > 0 && rm.LastRenderRect.Height > 0) {
                    if (this.PART_ViewPort.BeginRenderWithSurface(rm.ImageInfo)) {
                        this.PART_ViewPort.EndRenderWithSurface(rm.surface, rm.LastRenderRect);
                    }
                }
            }
        });
    }

    private void OnFrameRendered() {
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.Resource!.Timeline.RenderManager.FrameRendered += this.RenderManagerOnFrameRendered;
        this.updatePreviewExecutor.InvokeAsync();
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.Resource!.Timeline.RenderManager.FrameRendered -= this.RenderManagerOnFrameRendered;
    }

    private void RenderManagerOnFrameRendered(RenderManager manager) {
        this.updatePreviewExecutor.InvokeAsync();
    }
}