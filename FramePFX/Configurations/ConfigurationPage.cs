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

namespace FramePFX.Configurations;

public delegate void ConfigurationPageIsModifiedChangedEventHandler(ConfigurationPage sender);

/// <summary>
/// The base class for a configuration page model. A page is what is presented
/// on the right side of the settings dialog
/// <para>
/// A custom page may wish to implement sub-configuration sections that implement similar behaviour
/// to pages such as <see cref="Apply"/>, or it may be entirely custom (e.g. shortcut editor tree)
/// </para>
/// </summary>
public abstract class ConfigurationPage
{
    /// <summary>
    /// Gets the configuration context currently applicable to this page. This is updated when the page
    /// is being viewed by the user. An instance of a page cannot be concurrently viewed multiple times,
    /// hence why this property is not a list of configuration contexts
    /// </summary>
    public ConfigurationContext? ActiveContext { get; private set; }

    public bool IsMarkedImmediatelyModified { get; internal set; }
    
    protected ConfigurationPage()
    {
    }
    
    /// <summary>
    /// Applies the current data into the application. This is invoked when the user clicks
    /// the Apply (which just applies) or Save button (which applies then closes the dialog)
    /// </summary>
    public abstract ValueTask Apply();

    /// <summary>
    /// Invoked when a new configuration context is created for use with a
    /// configuration manager in which this page exists in.
    /// At this point, the settings dialog will not be fully loaded, and so,
    /// we are free to modify our internal state without notifying of changes
    /// </summary>
    /// <param name="context">The context that was created</param>
    /// <returns></returns>
    public virtual ValueTask OnContextCreated(ConfigurationContext context)
    {
        return ValueTask.CompletedTask;
    }
    
    /// <summary>
    /// Invoked when the context is no longer in use, meaning the settings dialog was closed.
    /// This method is always called and can be used to for example unregistered global event handlers
    /// </summary>
    /// <param name="context">The context that was destroyed</param>
    /// <returns></returns>
    public virtual ValueTask OnContextDestroyed(ConfigurationContext context)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Returns true if this page has different data since it was loaded
    /// </summary>
    /// <returns></returns>
    public virtual bool IsModified()
    {
        return this.IsMarkedImmediatelyModified;
    }

    /// <summary>
    /// Marks this page as immediately modified for the current context, rather than relying on periodic checkups
    /// </summary>
    public void MarkModified()
    {
        this.IsMarkedImmediatelyModified = true;
        this.ActiveContext?.MarkImmediatelyModified(this);
    }

    public void ClearModifiedState()
    {
        this.IsMarkedImmediatelyModified = false;
        this.ActiveContext?.ClearModifiedState(this);
    }

    public static void InternalSetContext(ConfigurationPage page, ConfigurationContext? context)
    {
        page.ActiveContext = context;
    }
}