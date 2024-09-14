
 private enum BlendInfo {;
	public static final char getTopL(final char b) { return (char)((b) & 0x3); }
	public static final char getTopR(final char b) { return (char)((b >> 2) & 0x3); }
	public static final char getBottomR(final char b) { return (char)((b >> 4) & 0x3); }
	public static final char getBottomL(final char b) { return (char)((b >> 6) & 0x3); }

	public static final char setTopL(final char b, final char bt) { return (char)(b | bt); }
	public static final char setTopR(final char b, final char bt) { return (char)(b | (bt << 2)); }
	public static final char setBottomR(final char b, final char bt) { return (char)(b | (bt << 4)); }
	public static final char setBottomL(final char b, final char bt) { return (char)(b | (bt << 6)); }

	public static final char rotate(
	 final char b,
	 final int rotDeg
	) {
	 assert rotDeg >= 0 && rotDeg < 4 : "RotationDegree enum does not have type: " + rotDeg;

	 final int l = rotDeg << 1;
	 final int r = 8 - l;

	 return (char)(b << l | b >> r);
	}
   }
