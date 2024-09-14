using xBRZNet.Common;

namespace xBRZNet.Scalers
{
    internal interface IScaler
    {
        int Scale { get; }
        void BlendLineSteep(int col, OutputMatrix out_);
        void BlendLineSteepAndShallow(int col, OutputMatrix out_);
        void BlendLineShallow(int col, OutputMatrix out_);
        void BlendLineDiagonal(int col, OutputMatrix out_);
        void BlendCorner(int col, OutputMatrix out_);
    }

    internal abstract class ScalerBase
    {
        protected static void AlphaBlendDividingM(int n, int m, IntPtr dstPtr, int col)
        {
            //assert n < 256 : "possible overflow of (col & redMask) * N";
            //assert m < 256 : "possible overflow of (col & redMask) * N + (dst & redMask) * (M - N)";
            //assert 0 < n && n < m : "0 < N && N < M";

            //this works because 8 upper bits are free
            int dst = dstPtr.Get();
            int redComponent = BlendComponentDividingM(Mask.Red, n, m, dst, col);
			int greenComponent = BlendComponentDividingM(Mask.Green, n, m, dst, col);
			int blueComponent = BlendComponentDividingM(Mask.Blue, n, m, dst, col);
            int blend = (redComponent | greenComponent | blueComponent);
			dstPtr.Set(blend | unchecked((int)0xff000000));
		}

		protected static void AlphaBlendShiftingM(int n, int m, int mShift, IntPtr dstPtr, int col)
		{
			int dst = dstPtr.Get();
			int redComponent = BlendComponentShiftingM(Mask.Red, n, m, mShift, dst, col);
			int greenComponent = BlendComponentShiftingM(Mask.Green, n, m, mShift, dst, col);
			int blueComponent = BlendComponentShiftingM(Mask.Blue, n, m, mShift, dst, col);
			int blend = (redComponent | greenComponent | blueComponent);
			dstPtr.Set(blend | unchecked((int)0xff000000));
		}

        private static int BlendComponentDividingM(int mask, int n, int m, int inPixel, int setPixel)
        {
            int inChan = inPixel & mask;
            int setChan = setPixel & mask;
            int blend = setChan * n + inChan * (m - n);
            int component = mask & (blend / m);
            return component;
        }

		private static int BlendComponentShiftingM(int mask, int n, int m, int mShift, int inPixel, int setPixel)
		{
			int inChan = inPixel & mask;
			int setChan = setPixel & mask;
			int blend = setChan * n + inChan * (m - n);
			int component = mask & (blend >> mShift);
			return component;
		}
    }

    internal class Scaler2X : ScalerBase, IScaler
	{
		private const int scale = 2;

		public int Scale { get { return scale; } }

        public void BlendLineShallow(int col, OutputMatrix out_)
        {
            AlphaBlendShiftingM(1, 4, 2, out_.Ref(scale - 1, 0), col);
            AlphaBlendShiftingM(3, 4, 2, out_.Ref(scale - 1, 1), col);
        }

        public void BlendLineSteep(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(0, scale - 1), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(1, scale - 1), col);
        }

        public void BlendLineSteepAndShallow(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(1, 0), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(0, 1), col);
            AlphaBlendDividingM(5, 6, out_.Ref(1, 1), col); //[!] fixes 7/8 used in xBR
        }

        public void BlendLineDiagonal(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 2, 1, out_.Ref(1, 1), col);
        }

        public void BlendCorner(int col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlendDividingM(21, 100, out_.Ref(1, 1), col); //exact: 1 - pi/4 = 0.2146018366
        }
    }

    internal class Scaler3X : ScalerBase, IScaler
	{
		private const int scale = 3;

		public int Scale { get { return scale; } }

        public void BlendLineShallow(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(scale - 1, 0), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(scale - 2, 2), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(scale - 1, 1), col);
            out_.Ref(scale - 1, 2).Set(col);
        }

        public void BlendLineSteep(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(0, scale - 1), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(2, scale - 2), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(1, scale - 1), col);
            out_.Ref(2, scale - 1).Set(col);
        }

        public void BlendLineSteepAndShallow(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(2, 0), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(0, 2), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(2, 1), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(1, 2), col);
            out_.Ref(2, 2).Set(col);
        }

        public void BlendLineDiagonal(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 8, 3, out_.Ref(1, 2), col);
			AlphaBlendShiftingM(1, 8, 3, out_.Ref(2, 1), col);
			AlphaBlendShiftingM(7, 8, 3, out_.Ref(2, 2), col);
        }

        public void BlendCorner(int col, OutputMatrix out_)
        {
            //model a round corner
			AlphaBlendDividingM(45, 100, out_.Ref(2, 2), col); //exact: 0.4545939598
                                                      //alphaBlend(14, 1000, out.ref(2, 1), col); //0.01413008627 -> negligable
                                                      //alphaBlend(14, 1000, out.ref(1, 2), col); //0.01413008627
        }
    }

    internal class Scaler4X : ScalerBase, IScaler
    {
		private const int scale = 4;

		public int Scale { get { return scale; } }

        public void BlendLineShallow(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(scale - 1, 0), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(scale - 2, 2), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(scale - 1, 1), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(scale - 2, 3), col);
            out_.Ref(scale - 1, 2).Set(col);
            out_.Ref(scale - 1, 3).Set(col);
        }

        public void BlendLineSteep(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(0, scale - 1), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(2, scale - 2), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(1, scale - 1), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(3, scale - 2), col);
            out_.Ref(2, scale - 1).Set(col);
            out_.Ref(3, scale - 1).Set(col);
        }

        public void BlendLineSteepAndShallow(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(3, 1), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(1, 3), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(3, 0), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(0, 3), col);
			AlphaBlendDividingM(1, 3, out_.Ref(2, 2), col); //[!] fixes 1/4 used in xBR
            out_.Ref(3, 3).Set(col);
            out_.Ref(3, 2).Set(col);
            out_.Ref(2, 3).Set(col);
        }

        public void BlendLineDiagonal(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 2, 1, out_.Ref(scale - 1, scale / 2), col);
			AlphaBlendShiftingM(1, 2, 1, out_.Ref(scale - 2, scale / 2 + 1), col);
            out_.Ref(scale - 1, scale - 1).Set(col);
        }

        public void BlendCorner(int col, OutputMatrix out_)
        {
            //model a round corner
			AlphaBlendDividingM(68, 100, out_.Ref(3, 3), col); //exact: 0.6848532563
			AlphaBlendDividingM(9, 100, out_.Ref(3, 2), col); //0.08677704501
			AlphaBlendDividingM(9, 100, out_.Ref(2, 3), col); //0.08677704501
        }
    }

    internal class Scaler5X : ScalerBase, IScaler
    {
		private const int scale = 5;

		public int Scale { get { return scale; } }

        public void BlendLineShallow(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(scale - 1, 0), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(scale - 2, 2), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(scale - 3, 4), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(scale - 1, 1), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(scale - 2, 3), col);
            out_.Ref(scale - 1, 2).Set(col);
            out_.Ref(scale - 1, 3).Set(col);
            out_.Ref(scale - 1, 4).Set(col);
            out_.Ref(scale - 2, 4).Set(col);
        }

        public void BlendLineSteep(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(0, scale - 1), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(2, scale - 2), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(4, scale - 3), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(1, scale - 1), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(3, scale - 2), col);
            out_.Ref(2, scale - 1).Set(col);
            out_.Ref(3, scale - 1).Set(col);
            out_.Ref(4, scale - 1).Set(col);
            out_.Ref(4, scale - 2).Set(col);
        }

        public void BlendLineSteepAndShallow(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(0, scale - 1), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(2, scale - 2), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(1, scale - 1), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(scale - 1, 0), col);
			AlphaBlendShiftingM(1, 4, 2, out_.Ref(scale - 2, 2), col);
			AlphaBlendShiftingM(3, 4, 2, out_.Ref(scale - 1, 1), col);
            out_.Ref(2, scale - 1).Set(col);
            out_.Ref(3, scale - 1).Set(col);
            out_.Ref(scale - 1, 2).Set(col);
            out_.Ref(scale - 1, 3).Set(col);
            out_.Ref(4, scale - 1).Set(col);
			AlphaBlendDividingM(2, 3, out_.Ref(3, 3), col);
        }

        public void BlendLineDiagonal(int col, OutputMatrix out_)
        {
			AlphaBlendShiftingM(1, 8, 3, out_.Ref(scale - 1, scale / 2), col);
			AlphaBlendShiftingM(1, 8, 3, out_.Ref(scale - 2, scale / 2 + 1), col);
			AlphaBlendShiftingM(1, 8, 3, out_.Ref(scale - 3, scale / 2 + 2), col);
			AlphaBlendShiftingM(7, 8, 3, out_.Ref(4, 3), col);
			AlphaBlendShiftingM(7, 8, 3, out_.Ref(3, 4), col);
            out_.Ref(4, 4).Set(col);
        }

        public void BlendCorner(int col, OutputMatrix out_)
        {
            //model a round corner
			AlphaBlendDividingM(86, 100, out_.Ref(4, 4), col); //exact: 0.8631434088
			AlphaBlendDividingM(23, 100, out_.Ref(4, 3), col); //0.2306749731
			AlphaBlendDividingM(23, 100, out_.Ref(3, 4), col); //0.2306749731
            //AlphaBlend(8, 1000, out.ref(4, 2), col); //0.008384061834 -> negligable
            //AlphaBlend(8, 1000, out.ref(2, 4), col); //0.008384061834
        }
    }
}
