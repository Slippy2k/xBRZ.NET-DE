
 private static final class IntPtr {
	private final int[] arr;
	private int ptr;

	public IntPtr(final int[] intArray) {
	 this.arr = intArray;
	}

	public final void position(final int position) {
	 ptr = position;
	}

	public final int get() {
	 return arr[ptr];
	}

	public final void set(final int val) {
	 arr[ptr] = val;
	}
   }
