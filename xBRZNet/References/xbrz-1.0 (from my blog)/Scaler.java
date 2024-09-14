
 private interface Scaler {
	int scale();
	void blendLineSteep(int col, OutputMatrix out);
	void blendLineSteepAndShallow(int col, OutputMatrix out);
	void blendLineShallow(int col, OutputMatrix out);
	void blendLineDiagonal(int col, OutputMatrix out);
	void blendCorner(int col, OutputMatrix out);
   }
