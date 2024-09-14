namespace xBRZNet.Common
{
    internal class IntPtr
    {
        private readonly int[] arr;
        private int ptr;

        public IntPtr(int[] intArray)
        {
            arr = intArray;
        }

        public void Position(int position)
        {
            ptr = position;
        }

        public int Get()
        {
            return arr[ptr];
        }

        public void Set(int val)
        {
            arr[ptr] = val;
        }
    }
}
