namespace FramePFX.Utils {
    public enum CASLockType {
        /// <summary>
        /// The <see cref="CASLock"/> was not originally locked before locking
        /// <para>
        /// This typically requires the caller to invoke <see cref="CASLock.Unlock()"/> when finished
        /// </para>
        /// </summary>
        WasNotLocked,

        /// <summary>
        /// The <see cref="CASLock"/> was locked by the current thread
        /// <para>
        /// You typically should not call <see cref="CASLock.Unlock()"/>, as it was already locked by the current thread
        /// </para>
        /// </summary>
        Thread,

        /// <summary>
        /// The <see cref="CASLock"/> managed to eventually lock
        /// <para>
        /// This typically requires the caller to invoke <see cref="CASLock.Unlock()"/> when finished
        /// </para>
        /// </summary>
        Normal,

        /// <summary>
        /// Failed to lock the <see cref="CASLock"/>. Should only happen when <see cref="CASLock.TryLock(CASLockType)"/> is called
        /// </summary>
        Failed
    }

    public static class CASLockTypeExtensions {
        public static bool IsIgnorable(this CASLockType type) {
            return type == CASLockType.Thread || type == CASLockType.Failed;
        }

        public static bool RequireUnlock(this CASLockType type) {
            return type != CASLockType.Thread && type != CASLockType.Failed;
        }

        public static bool WasJustTaken(this CASLockType type) {
            return type == CASLockType.WasNotLocked;
        }
    }
}