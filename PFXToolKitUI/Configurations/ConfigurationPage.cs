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

namespace PFXToolKitUI.Configurations;

public delegate void ConfigurationPageIsModifiedChangedEventHandler(ConfigurationPage sender);

/// <summary>
/// The base class for a configuration page model. A page is what is presented
/// on the right side of the settings dialog
/// <para>
/// A custom page may wish to implement their own sub-page or sub-section system that implement similar behaviour
/// to these pages (such as <see cref="Apply"/>), or it may be an entirely custom page (e.g. shortcut editor tree)
/// </para>
/// </summary>
public abstract class ConfigurationPage {
    /// <summary>
    /// Gets the configuration context currently applicable to this page. This is updated when the page
    /// is being viewed by the user. An instance of a page cannot be concurrently viewed multiple times,
    /// hence why this property is not a list of configuration contexts
    /// </summary>
    public ConfigurationContext? ActiveContext { get; private set; }

    public bool IsMarkedImmediatelyModified { get; internal set; }

    protected ConfigurationPage() {
    }

    /// <summary>
    /// Applies the current data into the application. This is invoked when the user clicks
    /// the Apply (which just applies) or Save button (which applies then closes the dialog)
    /// </summary>
    /// <param name="errors">
    /// A list of errors encountered while applying changes (e.g. bugs or conflicting
    /// values, maybe a and b cannot both be true)
    /// </param>
    public abstract ValueTask Apply(List<ApplyChangesFailureEntry>? errors);

    /// <summary>
    /// Reverts any changes this page made to the application outside the standard apply/cancel behaviour.
    /// For example, the theme manager page modifies the colours of the UI in real time,
    /// but uses this to reset the colours back to their original state, but if you click apply and then
    /// make changes again, this method will only revert those changes after <see cref="Apply"/>
    /// <para>
    /// This is invoked when the Cancel button is clicked in the UI
    /// </para>
    /// </summary>
    /// <param name="errors">
    /// A list of errors encountered while applying changes (e.g. bugs or conflicting
    /// values, maybe a and b cannot both be true)
    /// </param>
    public virtual ValueTask RevertLiveChanges(List<ApplyChangesFailureEntry>? errors) {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Invoked when a new configuration context is created for use with a configuration manager
    /// in which this page exists in. This is typically invoked recursively
    /// <para>
    /// At this point, the settings dialog will not be fully loaded, and so,
    /// we are free to modify our internal state without notifying of changes
    /// </para>
    /// <para>
    /// Basically, this is where you load data from the application in preparation for the UI.
    /// </para>
    /// </summary>
    /// <param name="context">The context that was created</param>
    /// <returns></returns>
    public virtual ValueTask OnContextCreated(ConfigurationContext context) {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Invoked when the context is no longer in use, meaning the settings dialog was closed.
    /// This method is always called and can be used to for example unregistered global event handlers
    /// </summary>
    /// <param name="context">The context that was destroyed</param>
    /// <returns></returns>
    public virtual ValueTask OnContextDestroyed(ConfigurationContext context) {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Invoked when the active context changes. One of the parameters will be null, unless I forget
    /// to update this comment. This happens when the page is no longer being viewed (either by the user clicking
    /// another page, or closing the dialog), and so maybe the page shouldn't listen to intense application updates
    /// </summary>
    /// <param name="oldContext">The previous context</param>
    /// <param name="newContext">The new context</param>
    protected virtual void OnActiveContextChanged(ConfigurationContext? oldContext, ConfigurationContext? newContext) {
    }

    /// <summary>
    /// Returns true if this page has different data since it was loaded
    /// </summary>
    /// <returns></returns>
    public virtual bool IsModified() {
        return this.IsMarkedImmediatelyModified;
    }

    /// <summary>
    /// Marks this page as immediately modified for the current context, rather than relying on periodic checkups
    /// </summary>
    public void MarkModified() {
        if (!this.IsMarkedImmediatelyModified) {
            this.IsMarkedImmediatelyModified = true;
            this.ActiveContext?.MarkImmediatelyModified(this);
        }
    }

    public void ClearModifiedState() {
        if (this.IsMarkedImmediatelyModified) {
            this.IsMarkedImmediatelyModified = false;
            this.ActiveContext?.ClearModifiedState(this);
        }
    }

    internal static void InternalSetContext(ConfigurationPage page, ConfigurationContext? context) {
        ConfigurationContext? oldContext = page.ActiveContext;
        if (ReferenceEquals(oldContext, context)) {
            return;
        }

        page.ActiveContext = context;
        page.OnActiveContextChanged(oldContext, context);
    }
}