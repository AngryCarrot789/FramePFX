namespace FramePFX.Utils {
    public enum CASLockType_OLD {
        /// <summary>
        /// The <see cref="CASLock__OLD"/> was not originally locked before locking
        /// <para>
        /// This typically requires the caller to invoke <see cref="CASLock__OLD.Unlock()"/> when finished
        /// </para>
        /// </summary>
        WasNotLocked,

        /// <summary>
        /// The <see cref="CASLock__OLD"/> was locked by the current thread
        /// <para>
        /// You typically should not call <see cref="CASLock__OLD.Unlock()"/>, as it was already locked by the current thread
        /// </para>
        /// </summary>
        Thread,

        /// <summary>
        /// The <see cref="CASLock__OLD"/> managed to eventually lock
        /// <para>
        /// This typically requires the caller to invoke <see cref="CASLock__OLD.Unlock()"/> when finished
        /// </para>
        /// </summary>
        Normal,

        /// <summary>
        /// Failed to lock the <see cref="CASLock__OLD"/>. Should only happen when <see cref="CASLock__OLD.TryLock(CASLockType_OLD)"/> is called
        /// </summary>
        Failed
    }

    public static class CASLockTypeExtensions {
        public static bool IsIgnorable(this CASLockType_OLD type) {
            return type == CASLockType_OLD.Thread || type == CASLockType_OLD.Failed;
        }

        public static bool RequireUnlock(this CASLockType_OLD type) {
            return type != CASLockType_OLD.Thread && type != CASLockType_OLD.Failed;
        }

        public static bool WasJustTaken(this CASLockType_OLD type) {
            return type == CASLockType_OLD.WasNotLocked;
        }
    }
}