using System;

namespace FramePFX.Core.Configurations
{
    public class FastStringBuf
    {
        private char[] buffer;
        public int count;

        public FastStringBuf(int initialCapacity)
        {
            this.buffer = new char[initialCapacity];
        }

        public void append(char ch)
        {
            this.EnsureCapacityForAddition(1);
            this.buffer[this.count++] = ch;
        }

        public void append(char[] data)
        {
            int len = data.Length;
            if (len > 0)
            {
                this.EnsureCapacityForAddition(len);
                Array.Copy(data, 0, this.buffer, this.count, len);
                this.count += len;
            }
        }

        // Using reflection to access the string's `char[] value` field is about 8% slower
        // (1 / (1400ms)) * (1520ms) = 1.085
        // private static Field CHARS_FIELD = Reflect.getField(() -> String.class.getDeclaredField("value"));

        public void append(string str)
        {
            this.append(str, 0, str.Length);
        }

        public void append(object value)
        {
            this.append(value != null ? value.ToString() : "null");
        }

        public void append(string str, int startIndex, int endIndex)
        {
            int len = endIndex - startIndex;
            if (len > 0)
            {
                this.EnsureCapacityForAddition(len);
                str.CopyTo(startIndex, this.buffer, this.count, len);
                this.count += len;
            }
        }

        public void append(string str, int startIndex)
        {
            this.append(str, startIndex, str.Length);
        }

        public void append(char[] array, int startIndex, int endIndex)
        {
            int len = endIndex - startIndex;
            if (len > 0)
            {
                this.EnsureCapacityForAddition(len);
                Array.Copy(array, startIndex, this.buffer, this.count, len);
                this.count += len;
            }
        }

        public void append(int i)
        {
            this.append(i.ToString());
        }

        public void setCharAt(int index, char ch)
        {
            this.buffer[index] = ch;
        }

        private void EnsureCapacityForAddition(int additional)
        {
            if (((this.count + additional) - this.buffer.Length) > 0)
            {
                this.grow(this.count + additional);
            }
        }

        private void grow(int minCapacity)
        {
            int oldCapacity = this.buffer.Length;
            int newCapacity = oldCapacity + (oldCapacity >> 1);
            if (newCapacity - minCapacity < 0)
                newCapacity = minCapacity;

            char[] buff = new char[newCapacity];
            for (int i = 0, end = Math.Min(this.count, newCapacity); i < end; i++)
            {
                buff[i] = this.buffer[i];
            }

            this.buffer = buff;
        }

        public void getChars(int startIndex, int endIndex, char[] dest, int destStart)
        {
            Array.Copy(this.buffer, startIndex, dest, destStart, endIndex - startIndex);
        }

        public void putChars(int startIndex, int endIndex, char[] src, int srcStart)
        {
            Array.Copy(src, srcStart, this.buffer, startIndex, endIndex - startIndex);
        }

        public override string ToString()
        {
            return new String(this.buffer, 0, this.count);
        }
    }
}