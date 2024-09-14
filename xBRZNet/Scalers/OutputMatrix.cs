using xBRZNet.Common;

namespace xBRZNet.Scalers
{
    //access matrix area, top-left at position "out" for image with given width
    internal sealed class OutputMatrix
    {
        private readonly IntPtr outPtr;
		private int outi;
		private readonly int outWidth;
        private readonly int n;
        private int nr;

        private const int maxScale = 5; // Highest possible scale
        private const int maxScaleSq = maxScale * maxScale;

        public OutputMatrix(int scale, int[] outPtr, int outWidth)
        {
            n = (scale - 2) * (Rot.maxRotations * maxScaleSq);
            this.outPtr = new IntPtr(outPtr);
            this.outWidth = outWidth;
        }

        public void Move(int rotDeg, int outi)
        {
            nr = n + rotDeg * maxScaleSq;
            this.outi = outi;
        }

        public IntPtr Ref(int i, int j)
        {
            IntPair rot = matrixRotation[nr + i * maxScale + j];
            outPtr.Position(outi + rot.j + rot.i * outWidth);
            return outPtr;
        }

        //calculate input matrix coordinates after rotation at program startup
		private static readonly IntPair[] matrixRotation;

        static OutputMatrix()
        {
			matrixRotation = new IntPair[(maxScale - 1) * maxScaleSq * Rot.maxRotations];
            for (int n = 2; n < maxScale + 1; n++)
                for (int r = 0; r < Rot.maxRotations; r++)
                {
                    int nr = (n - 2) * (Rot.maxRotations * maxScaleSq) + r * maxScaleSq;
                    for (int i = 0; i < maxScale; i++)
                        for (int j = 0; j < maxScale; j++)
                            matrixRotation[nr + i * maxScale + j] = BuildMatrixRotation(r, i, j, n);
                }
        }

        private static IntPair BuildMatrixRotation(int rotDeg, int i, int j, int n)
        {
            int iOld, jOld;

            if (rotDeg == 0)
            {
                iOld = i;
                jOld = j;
            }
            else
            {
                //old coordinates before rotation!
                IntPair old = BuildMatrixRotation(rotDeg - 1, i, j, n);
                iOld = n - 1 - old.j;
                jOld = old.i;
            }

            return new IntPair(iOld, jOld);
        }
    }
}
