namespace xBRZNet.Blend
{
    internal class BlendResult
    {
        public char f { get; set; }
        public char g { get; set; }
        public char j { get; set; }
        public char k { get; set; }

        public void Reset()
        {
            f = g = j = k = (char)0;
        }
    }
}
