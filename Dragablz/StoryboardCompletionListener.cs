using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace Dragablz {
    internal class StoryboardCompletionListener {
        private readonly Storyboard _storyboard;
        private readonly Action<Storyboard> _continuation;

        public StoryboardCompletionListener(Storyboard storyboard, Action<Storyboard> continuation) {
            if (storyboard == null)
                throw new ArgumentNullException("storyboard");
            if (continuation == null)
                throw new ArgumentNullException("continuation");

            this._storyboard = storyboard;
            this._continuation = continuation;

            this._storyboard.Completed += this.StoryboardOnCompleted;
        }

        private void StoryboardOnCompleted(object sender, EventArgs eventArgs) {
            this._storyboard.Completed -= this.StoryboardOnCompleted;
            this._continuation(this._storyboard);
        }
    }

    internal static class StoryboardCompletionListenerExtension {
        private static readonly IDictionary<Storyboard, Action<Storyboard>> ContinuationIndex = new Dictionary<Storyboard, Action<Storyboard>>();

        public static void WhenComplete(this Storyboard storyboard, Action<Storyboard> continuation) {
            // ReSharper disable once ObjectCreationAsStatement
            new StoryboardCompletionListener(storyboard, continuation);
        }
    }
}