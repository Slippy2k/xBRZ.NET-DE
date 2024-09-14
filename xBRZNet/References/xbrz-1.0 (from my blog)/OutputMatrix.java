
 //access matrix area, top-left at position "out" for image with given width
 private static final class OutputMatrix {
	private final IntPtr out;
	private int outi;
	private final int outWidth;
	private final int n;
	private int nr;

	public OutputMatrix(
	 final int scale,
	 final int[] out,
	 final int outWidth
	) {
	 this.n = (scale - 2) * (maxRots * maxScaleSq);
	 this.out = new IntPtr(out);
	 this.outWidth = outWidth;
	}

	public void move(
	 final int rotDeg,
	 final int outi
	) {
	 this.nr = n + rotDeg * maxScaleSq;
	 this.outi = outi;
	}

	public final IntPtr ref(final int i, final int j) {
	 final IntPair rot = matrixRotation[nr + i * maxScale + j];
	 out.position(outi + rot.J + rot.I * outWidth);
	 return out;
	}
   }
