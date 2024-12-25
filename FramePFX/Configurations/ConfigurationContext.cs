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

using FramePFX.Utils.RDA;

namespace FramePFX.Configurations;

public delegate void ConfigurationContextActivePageChangedEventHandler(ConfigurationContext context, ConfigurationPage? oldPage, ConfigurationPage? newPage);

public delegate void ConfigurationContextEventHandler(ConfigurationContext context);

public class ConfigurationContext {
    private HashSet<ConfigurationPage>? modifiedPages;

    private ConfigurationPage? activePage; // the page we are currently viewing

    public ConfigurationPage? ActivePage => this.activePage;

    /// <summary>
    /// Gets the pages that are currently marked as modified. This might be updated
    /// periodically and/or immediately when a page self-marks itself as modified.
    /// This collection should not be relied on entirely to check the modified state is the main point
    /// </summary>
    public IEnumerable<ConfigurationPage> ModifiedPages => this.modifiedPages ?? Enumerable.Empty<ConfigurationPage>();

    public event ConfigurationContextActivePageChangedEventHandler? ActivePageChanged;
    public event ConfigurationContextEventHandler? ModifiedPagesUpdated;

    private int modificationLevelForModifiedPages, lastModificationLevelForNotification;
    private readonly RateLimitedDispatchAction updateIsModifiedAction;

    public ConfigurationContext() {
        this.updateIsModifiedAction = BaseRateLimitedDispatchAction.ForDispatcherSync(() => {
            if (this.lastModificationLevelForNotification != this.modificationLevelForModifiedPages) {
                this.lastModificationLevelForNotification = this.modificationLevelForModifiedPages;
                this.ModifiedPagesUpdated?.Invoke(this);
            }
        }, TimeSpan.FromMilliseconds(250), DispatchPriority.Background);
    }

    /// <summary>
    /// Sets the page that is currently being viewed
    /// </summary>
    /// <param name="page"></param>
    public void SetViewPage(ConfigurationPage? page) {
        ConfigurationPage? oldPage = this.activePage;
        if (oldPage == page)
            return;

        if (oldPage != null) {
            if (oldPage.IsModified()) {
                this.MarkImmediatelyModified(oldPage);
            }
            else {
                this.ClearModifiedState(oldPage);
            }

            ConfigurationPage.InternalSetContext(oldPage, null);
        }

        if (page != null) {
            ConfigurationPage.InternalSetContext(this.activePage = page, this);
            if (page.IsModified()) {
                this.MarkImmediatelyModified(page);
            }
        }

        this.ActivePageChanged?.Invoke(this, oldPage, page);
    }

    /// <summary>
    /// Marks the page as modified immediately, rather than letting the periodic modification scanner check it
    /// </summary>
    /// <param name="page"></param>
    public void MarkImmediatelyModified(ConfigurationPage page) {
        if (this.modifiedPages == null || !this.modifiedPages.Contains(page)) {
            (this.modifiedPages ??= new HashSet<ConfigurationPage>()).Add(page);
            this.modificationLevelForModifiedPages++;
            this.updateIsModifiedAction.InvokeAsync();
        }
    }

    public void ClearModifiedState(ConfigurationPage page) {
        if (this.modifiedPages != null && this.modifiedPages.Remove(page)) {
            this.modificationLevelForModifiedPages++;
            this.updateIsModifiedAction.InvokeAsync();
        }
    }

    public void OnCreated() {
    }

    public void OnDestroyed() {
    }
}