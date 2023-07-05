using System.Windows.Controls;

namespace FramePFX.DockDragging {
    //
    //
    /*
     * Draggablz is very weird so I might try and implement my own
     * Control hierarchy will like:
     *   DockHostPanel
     *     DockableTabControl
     *       DockableTabItem
     *         <header content>
     *       DockableTabItem
     *         <header content>
     *       <tab control active content>
     * The dock host panel will contain a style that is applied to all dockable tab controls, and the
     * host will create new instances of the tab control and apply that style
     *
     * Might actually just primarily use binding/ICG to generate the tab controls,
     * so that you can have "DockHostViewModel", "DockTabPanelViewModel" and "DockItemViewModel" and
     * therefore those view models can further manage the position and layout of the tab controls and
     * item order (allowing a view model to for example save the state to read/write to disk)
     *
     * The tab item content will be generated through a DataTemplate
     */
    //
    public class DockHostPanel : DockPanel {
    }
}
