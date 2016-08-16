﻿namespace xBRZNet.Common
{
    public class IntPtr
    {
        private readonly int[] _array;
        private int _ptr;

        public IntPtr(int[] array)
        {
            _array = array;
        }

        public void Position(int position)
        {
            _ptr = position;
        }

        public int Get()
        {
            return _array[_ptr];
        }

        public void Set(int val)
        {
            _array[_ptr] = val;
        }
    }
}
