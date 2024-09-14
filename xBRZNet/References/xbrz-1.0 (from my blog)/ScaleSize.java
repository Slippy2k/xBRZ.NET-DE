
 public enum ScaleSize {
	Times2(Scaler2x),
	Times3(Scaler3x),
	Times4(Scaler4x),
	Times5(Scaler5x);

	public ScaleSize(final Scaler scaler) {
	 this.scaler = scaler;
	 this.size = scaler.scale();
	}

	public static final ScaleSize cast(final int ordinal) {
	 final int ord1 = Math.max(ordinal, 0);
	 final int ord2 = Math.min(ord1, values().length - 1);
	 return values()[ord2];
	}

	private final Scaler scaler;
	public final int size;
   }
