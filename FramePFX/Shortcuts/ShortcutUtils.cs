using System.Text;

namespace FramePFX.Shortcuts
{
    public static class ShortcutUtils
    {
        public static string[] ToFull(string parent, params string[] children)
        {
            string[] array = new string[children.Length];
            for (int i = 0; i < children.Length; i++)
                array[i] = Join(parent, children[i]);
            return array;
        }

        public static string[] ToFull(string parent, string childA, string childB)
        {
            return new string[]
            {
                Join(parent, childA),
                Join(parent, childB),
            };
        }

        public static string Join(string a, string b)
        {
            if (a == null || b == null)
            {
                return a ?? b;
            }

            int lenA = a.Length, lenB = b.Length;
            if (lenA < 1)
            {
                return lenB < 1 ? null : b;
            }
            else if (lenB < 1)
            {
                return a;
            }
            else if (a[lenA - 1] == '/')
            {
                return b[0] == '/' ? (a + b.Substring(1)) : (a + b);
            }
            else if (b[0] == '/')
            {
                return a + b;
            }
            else
            {
                return new StringBuilder(lenA + lenB + 1).Append(a).Append('/').Append(b).ToString();
            }
        }
    }
}