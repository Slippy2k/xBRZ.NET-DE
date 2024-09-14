namespace xBRZNet.Blend
{
    internal sealed class BlendType
    {
        // These blend types must fit into 2 bits.
        public const char None = (char)0; //do not blend
		public const char Normal = (char)1; //a normal indication to blend
        public const char Dominant = (char)2; //a strong indication to blend
    }
}
