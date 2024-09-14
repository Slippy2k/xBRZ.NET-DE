
 private enum Rot {;
	// Cache the 4 rotations of the 9 positions, a to i.
	public static int[] _ = new int[9 * 4];

	static {
	 final int
	  a=0, b=1, c=2,
	  d=3, e=4, f=5,
	  g=6, h=7, i=8;

	 final int[] deg0 = new int[] {
	  a,b,c,
	  d,e,f,
	  g,h,i };

	 final int[] deg90 = new int[] {
	  g,d,a,
	  h,e,b,
	  i,f,c };

	 final int[] deg180 = new int[] {
	  i,h,g,
	  f,e,d,
	  c,b,a };

	 final int[] deg270 = new int[] {
	  c,f,i,
	  b,e,h,
	  a,d,g };

	 final int[][] rotation = new int[][] {
	  deg0, deg90, deg180, deg270
	 };

	 for (int rotDeg = 0; rotDeg < 4; rotDeg++)
	  for (int x = 0; x < 9; x++)
	   _[(x << 2) + rotDeg] = rotation[rotDeg][x];
	}
   }
