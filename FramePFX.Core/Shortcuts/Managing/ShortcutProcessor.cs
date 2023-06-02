using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FrameControlEx.Core.Actions;
using FrameControlEx.Core.Actions.Contexts;
using FrameControlEx.Core.Shortcuts.Inputs;
using FrameControlEx.Core.Shortcuts.Usage;

namespace FrameControlEx.Core.Shortcuts.Managing {
    /// <summary>
    /// A shortcut processor. This is used for each focus root (which is typically a window), and
    /// should only really be used by a single thread at a time (not designed to be thread safe)
    /// <para>
    /// This processor will manages its own input strokes and active usages
    /// </para>
    /// </summary>
    public class ShortcutProcessor {
        private readonly List<GroupedShortcut> shortcutList;

        /// <summary>
        /// A reference to the manager that created this processor
        /// </summary>
        public ShortcutManager Manager { get; }

        /// <summary>
        /// All of this processor's active shortcut usages
        /// </summary>
        public Dictionary<IShortcutUsage, GroupedShortcut> ActiveUsages { get; }

        /// <summary>
        /// This processor's current data context
        /// </summary>
        public IDataContext CurrentDataContext { get; set; }

        public ShortcutProcessor(ShortcutManager manager) {
            this.Manager = manager;
            this.ActiveUsages = new Dictionary<IShortcutUsage, GroupedShortcut>();
            this.shortcutList = new List<GroupedShortcut>(5);
        }

        protected virtual void AccumulateShortcuts(IInputStroke stroke, string focusedGroup) {
            this.Manager.CollectShortcutsWithPrimaryStroke(stroke, focusedGroup, this.shortcutList);
        }

        protected virtual List<GroupedShortcut> GetInstantActivationShortcuts() {
            List<GroupedShortcut> instantActivate = this.shortcutList.Where(x => !x.Shortcut.HasSecondaryStrokes).ToList();
            this.shortcutList.RemoveAll(x => !x.Shortcut.HasSecondaryStrokes);
            return instantActivate;
        }

        protected virtual async Task<bool> OnUnexpectedCompletedUsage(IShortcutUsage usage, GroupedShortcut shortcut) {
            try {
                return await this.OnSecondShortcutUsageCompleted(usage, shortcut);
            }
            finally {
                // The OnKeyStroke/OnMouseStroke functions immediately return the return of OnUnexpectedCompletedUsage,
                // so clearing this is safe to do
                this.ActiveUsages.Clear();
            }
        }

        public async Task<bool> OnKeyStroke(string focusedGroup, KeyStroke stroke) {
            if (this.ActiveUsages.Count < 1) {
                this.AccumulateShortcuts(stroke, focusedGroup);
                if (this.shortcutList.Count < 1) {
                    return this.OnNoSuchShortcutForKeyStroke(focusedGroup, stroke);
                }

                bool result = false;
                List<GroupedShortcut> instantActivate = this.GetInstantActivationShortcuts();
                foreach (GroupedShortcut s in instantActivate) {
                    result |= await this.OnShortcutActivated(s);
                }

                if (this.shortcutList.Count < 1) {
                    return result;
                }

                // All shortcuts here have secondary input strokes, because the code above
                // will attempt to execute the first shortcut that has no second input strokes.
                // In most cases, the list should only ever have 1 item with no secondary inputs, or be full of
                // shortcuts that all have secondary inputs (because logically, that's how a key map should work...
                // why would you want multiple shortcuts to activate on the same key stroke?)
                foreach (GroupedShortcut mc in this.shortcutList) {
                    if (mc.Shortcut is IKeyboardShortcut shortcut) {
                        IKeyboardShortcutUsage usage = shortcut.CreateKeyUsage();
                        this.ActiveUsages[usage] = mc;
                        this.OnShortcutUsageCreated(usage, mc);
                    }
                }

                this.shortcutList.Clear();
                if (this.ActiveUsages.Count > 0) {
                    return result | this.OnShortcutUsagesCreated();
                }
                else {
                    return result | this.OnNoSuchShortcutForKeyStroke(focusedGroup, stroke);
                }
            }
            else {
                List<KeyValuePair<IShortcutUsage, GroupedShortcut>> valid = new List<KeyValuePair<IShortcutUsage, GroupedShortcut>>();
                foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in this.ActiveUsages) {
                    // Just in case, check if it's already completed. By default, it never should be
                    if (pair.Key.IsCompleted) {
                        return await this.OnUnexpectedCompletedUsage(pair.Key, pair.Value);
                    }

                    bool strokeAccepted;
                    if (pair.Key is IKeyboardShortcutUsage usage) {
                        if (usage.IsCurrentStrokeKeyBased) {
                            if (this.ShouldIgnoreKeyStroke(usage, pair.Value, stroke, usage.CurrentKeyStroke)) {
                                valid.Add(pair);
                                continue;
                            }

                            strokeAccepted = usage.OnKeyStroke(stroke);
                        }
                        else if (usage.PreviousStroke is KeyStroke lastKey) { // the below check is needed for MouseKeyboardShortcutUsages to work
                            if (this.ShouldIgnoreKeyStroke(usage, pair.Value, stroke, lastKey)) {
                                valid.Add(pair);
                                continue;
                            }

                            strokeAccepted = usage.OnKeyStroke(stroke);
                        }
                        else {
                            strokeAccepted = false;
                        }
                    }
                    else {
                        continue;
                    }

                    if (strokeAccepted) {
                        if (usage.IsCompleted) {
                            try {
                                return await this.OnSecondShortcutUsageCompleted(pair.Key, pair.Value);
                            }
                            finally {
                                this.ActiveUsages.Clear();
                            }
                        }
                        else if (this.OnSecondShortcutUsageProgressed(pair.Key, pair.Value)) {
                            valid.Add(pair);
                        }
                    }
                    else if (!this.OnCancelUsageForNoSuchNextKeyStroke(pair.Key, pair.Value, stroke)) {
                        valid.Add(pair);
                    }
                }

                this.ActiveUsages.Clear();
                if (valid.Count < 1) {
                    return this.OnNoSuchShortcutForKeyStroke(focusedGroup, stroke);
                }
                else {
                    foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in valid) {
                        this.ActiveUsages[pair.Key] = pair.Value;
                    }

                    return this.OnSecondShortcutUsagesProgressed();
                }
            }
        }

        public async Task<bool> OnMouseStroke(string focusedGroup, MouseStroke stroke) {
            if (this.ActiveUsages.Count < 1) {
                this.AccumulateShortcuts(stroke, focusedGroup);
                if (this.shortcutList.Count < 1) {
                    return this.OnNoSuchShortcutForMouseStroke(focusedGroup, stroke);
                }

                bool result = false;
                List<GroupedShortcut> instantActivate = this.GetInstantActivationShortcuts();
                foreach (GroupedShortcut s in instantActivate) {
                    result |= await this.OnShortcutActivated(s);
                }

                if (this.shortcutList.Count < 1) {
                    return result;
                }

                foreach (GroupedShortcut mc in this.shortcutList) {
                    if (mc.Shortcut is IMouseShortcut shortcut) {
                        IMouseShortcutUsage usage = shortcut.CreateMouseUsage();
                        this.ActiveUsages[usage] = mc;
                        this.OnShortcutUsageCreated(usage, mc);
                    }
                }

                this.shortcutList.Clear();
                if (this.ActiveUsages.Count > 0) {
                    return result | this.OnShortcutUsagesCreated();
                }
                else {
                    return result | this.OnNoSuchShortcutForMouseStroke(focusedGroup, stroke);
                }
            }
            else {
                List<KeyValuePair<IShortcutUsage, GroupedShortcut>> valid = new List<KeyValuePair<IShortcutUsage, GroupedShortcut>>();
                foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in this.ActiveUsages) {
                    // Just in case, check if it's already completed. By default, it never should be
                    if (pair.Key.IsCompleted) {
                        return await this.OnUnexpectedCompletedUsage(pair.Key, pair.Value);
                    }

                    bool strokeAccepted;
                    if (pair.Key is IMouseShortcutUsage usage) {
                        if (usage.IsCurrentStrokeMouseBased) {
                            strokeAccepted = usage.OnMouseStroke(stroke);
                        }
                        // Maybe try to implement something here that allows mouse button release to be processed?
                        // Handling mouse up makes the shortcuts way harder to manage, because there's so many edge
                        // cases to consider, e.g. how do you handle double/triple/etc click while ignoring mouse down/up
                        // if the next usage expects a mouse up/down, while also checking checking all of the active usages.
                        // Just too much extra work... might try and re-implement it some day
                        //
                        // It works with keys, because they can only be "clicked" once, unlike mouse input, which can have
                        // multiple clicks. Also thought about implementing a key stroke click count... but operating systems
                        // don't typically do that so i'd have to implement it myself :(
                        else {
                            strokeAccepted = false;
                        }
                    }
                    else {
                        continue;
                    }

                    if (strokeAccepted) {
                        if (usage.IsCompleted) {
                            try {
                                return await this.OnSecondShortcutUsageCompleted(pair.Key, pair.Value);
                            }
                            finally {
                                this.ActiveUsages.Clear();
                            }
                        }
                        else if (this.OnSecondShortcutUsageProgressed(pair.Key, pair.Value)) {
                            valid.Add(pair);
                        }
                    }
                    else if (!this.OnCancelUsageForNoSuchNextMouseStroke(pair.Key, pair.Value, stroke)) {
                        valid.Add(pair);
                    }
                }

                this.ActiveUsages.Clear();
                if (valid.Count < 1) {
                    return this.OnNoSuchShortcutForMouseStroke(focusedGroup, stroke);
                }
                else {
                    foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in valid) {
                        this.ActiveUsages[pair.Key] = pair.Value;
                    }

                    return this.OnSecondShortcutUsagesProgressed();
                }
            }
        }

        /// <summary>
        /// Called when no shortcut usages are active and the given key stroke does not correspond to a shortcut
        /// </summary>
        /// <param name="group"></param>
        /// <param name="stroke">The received keyboard stroke</param>
        /// <returns>The key stroke event outcome. True = Handled/Cancelled, False = Ignored/Continue</returns>
        public virtual bool OnNoSuchShortcutForKeyStroke(string group, in KeyStroke stroke) {
            return false;
        }

        /// <summary>
        /// Called when no shortcut usages are active and the given mouse stroke does not correspond to a shortcut
        /// </summary>
        /// <param name="group"></param>
        /// <param name="stroke">The received mouse input stroke</param>
        /// <returns>The mouse stroke event outcome. True = Handled/Cancelled, False = Ignored/Continue</returns>
        public virtual bool OnNoSuchShortcutForMouseStroke(string group, in MouseStroke stroke) {
            return false;
        }

        /// <summary>
        /// Called when there were active shortcut usages, but the input received did not correspond to one of the usage's next stroke, and
        /// as a result, the given shortcut usage is about to be cancelled. However, it will remain active if this function returns false
        /// </summary>
        /// <param name="stroke">The key stroke that was received</param>
        /// <returns>Whether to cancel the usage or not. True = cancel, False = keep</returns>
        public virtual bool OnCancelUsageForNoSuchNextKeyStroke(IShortcutUsage usage, GroupedShortcut shortcut, in KeyStroke stroke) {
            return this.OnCancelUsage(usage, shortcut);
        }

        /// <summary>
        /// Called when there were active shortcut usages, but the input received did not correspond to one of the usage's next stroke, and
        /// as a result, the given shortcut usage is about to be cancelled. However, it will remain active if this function returns false
        /// </summary>
        /// <param name="stroke">The mouse stroke that was received</param>
        /// <returns>Whether to cancel the usage or not. True = cancel, False = keep</returns>
        public virtual bool OnCancelUsageForNoSuchNextMouseStroke(IShortcutUsage usage, GroupedShortcut shortcut, in MouseStroke stroke) {
            return this.OnCancelUsage(usage, shortcut);
        }

        public virtual bool OnCancelUsage(IShortcutUsage usage, GroupedShortcut shortcut) {
            return true;
        }

        /// <summary>
        /// Called when a shortcut usage is created. This may be called multiple times for a single input stroke.
        /// <see cref="OnShortcutUsagesCreated"/> is called after all possible usages are created
        /// </summary>
        /// <param name="usage">The usage that was created</param>
        /// <param name="shortcut">A managed shortcut that created the usage</param>
        public virtual void OnShortcutUsageCreated(IShortcutUsage usage, GroupedShortcut shortcut) {

        }

        /// <summary>
        /// Called when one or more shortcut usages were created. <see cref="OnShortcutUsageCreated"/> is called for
        /// each usage created, whereas this is called after all possible usages were created during an input stroke
        /// </summary>
        /// <returns>The mouse stroke event outcome. True = Handled/Cancelled, False = Ignored/Continue</returns>
        public virtual bool OnShortcutUsagesCreated() {
            return true;
        }

        /// <summary>
        /// Called when a shortcut usage is progressed, but not completed
        /// </summary>
        /// <returns>
        /// Whether the usage is allowed to be progressed further or not
        /// </returns>
        public virtual bool OnSecondShortcutUsageProgressed(IShortcutUsage usage, GroupedShortcut shortcut) {
            return true;
        }

        public virtual bool OnSecondShortcutUsagesProgressed() {
            return true;
        }

        /// <summary>
        /// Called when a shortcut usage was completed. By default, this calls <see cref="OnShortcutActivated"/> to activate the shortcut
        /// </summary>
        /// <param name="usage">The usage that was completed</param>
        /// <param name="shortcut">The managed shortcut that created the usage</param>
        /// <returns>The mouse stroke event outcome. True = Handled/Cancelled, False = Ignored/Continue</returns>
        public virtual Task<bool> OnSecondShortcutUsageCompleted(IShortcutUsage usage, GroupedShortcut shortcut) {
            return this.OnShortcutActivated(shortcut);
        }

        /// <summary>
        /// Called when a shortcut wants to be activated (either the usage chain was complete, or the primary input was pressed and there were no secondary inputs)
        /// </summary>
        /// <returns>The mouse stroke event outcome. True = Handled/Cancelled, False = Ignored/Continue</returns>
        public virtual async Task<bool> OnShortcutActivated(GroupedShortcut shortcut) {
            IDataContext context = this.CurrentDataContext;
            if (context == null) {
                return false;
            }

            foreach (object obj in context.Context) {
                if (obj is IShortcutHandler handler && await handler.OnShortcutActivated(this, shortcut)) {
                    return true;
                }
                else if (obj is IShortcutToCommand converter) {
                    ICommand command = converter.GetCommandForShortcut(shortcut.FullPath);
                    if (command is BaseAsyncRelayCommand asyncCommand) {
                        IoC.BroadcastShortcutActivity($"Activating shortcut: {shortcut} via command...");
                        if (await asyncCommand.TryExecuteAsync(null)) {
                            IoC.BroadcastShortcutActivity($"Activating shortcut: {shortcut} via command... Complete!");
                            return true;
                        }
                    }
                    else if (command != null && command.CanExecute(null)) {
                        IoC.BroadcastShortcutActivity($"Activated shortcut: {shortcut} via command... Complete!");
                        command.Execute(null);
                        return true;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(shortcut.ActionId)) {
                return false;
            }

            if (shortcut.ActionContext != null) {
                DataContext newCtx = new DataContext();
                newCtx.Merge(context);
                newCtx.Merge(shortcut.ActionContext);
                context = newCtx;
            }

            IoC.BroadcastShortcutActivity($"Activating shortcut action: {shortcut} -> {shortcut.ActionId}...");
            if (await ActionManager.Instance.Execute(shortcut.ActionId, context)) {
                IoC.BroadcastShortcutActivity($"Activating shortcut action: {shortcut} -> {shortcut.ActionId}... Complete!");
                return true;
            }

            IoC.BroadcastShortcutActivity($"Activating shortcut action: {shortcut} -> {shortcut.ActionId}... Incomplete!");
            return false;
        }

        /// <summary>
        /// Whether to ignore the received key stroke
        /// </summary>
        /// <param name="usage"></param>
        /// <param name="shortcut"></param>
        /// <param name="input"></param>
        /// <param name="currentUsageKeyStroke"></param>
        /// <returns></returns>
        protected virtual bool ShouldIgnoreKeyStroke(IKeyboardShortcutUsage usage, GroupedShortcut shortcut, KeyStroke input, KeyStroke currentUsageKeyStroke) {
            if (currentUsageKeyStroke.IsKeyRelease && !input.IsKeyRelease) {
                if (this.ShouldIgnorePressWhenRequiredStrokeIsRelease(usage, shortcut, input)) {
                    return true;
                }
            }

            if (input.IsKeyRelease && !usage.IsCompleted && !currentUsageKeyStroke.IsKeyRelease) {
                if (this.ShouldIgnoreReleaseWhenRequiredStrokeIsPress(usage, shortcut, input)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Whether to ignore the received key stroke when it is a key down stroke, but the usage requires a key release. By default,
        /// this returns true. This is just for finer control over the behaviour that allows the key release to be used
        /// </summary>
        /// <param name="usage">The usage being checked</param>
        /// <param name="shortcut">The managed shortcut that created the usage</param>
        /// <param name="stroke">The input stroke</param>
        /// <returns>Whether to ignore the input stroke or not. When ignored, the usage will still remain active</returns>
        public virtual bool ShouldIgnorePressWhenRequiredStrokeIsRelease(IKeyboardShortcutUsage usage, GroupedShortcut shortcut, in KeyStroke stroke) {
            return true;
        }

        /// <summary>
        /// Whether to ignore the received key stroke when it is a key up stroke, but the usage requires a key press. By default,
        /// this returns true. This is just for finer control over the behaviour that allows the key release to be used
        /// </summary>
        /// <param name="usage">The usage being checked</param>
        /// <param name="shortcut">The managed shortcut that created the usage</param>
        /// <param name="stroke">The input stroke</param>
        /// <returns>Whether to ignore the input stroke or not. When ignored, the usage will still remain active</returns>
        public virtual bool ShouldIgnoreReleaseWhenRequiredStrokeIsPress(IKeyboardShortcutUsage usage, GroupedShortcut shortcut, in KeyStroke stroke) {
            return true;
        }
    }
}