namespace FramePFX.ThreadSafety
{
    public class MutexArray
    {
        private readonly object[] locks;

        public MutexArray(int count)
        {
            this.locks = new object[count];
        }

        public Locker Lock(LockType type)
        {
            return new Locker(this, type);
        }

        internal static void SetLockState(MutexArray mutex, LockType type, bool state)
        {
            int value = (int) type;
            for (int i = 1; i < 3; i++)
            {
            }
        }
    }
}