using System;
using System.Linq;

namespace xBRZNet.Scalers
{
	internal static class Rot
	{
		public const int maxRotations = 4; // Number of 90 degree rotations.
		public const int maxPositions = 9;

		// Cache the 4 rotations of the 9 positions, a to i.
		public static readonly int[] _ = new int[maxRotations * maxPositions];

		static Rot()
		{
			int
				a = 0, b = 1, c = 2,
				d = 3, e = 4, f = 5,
				g = 6, h = 7, i = 8;

			int[] deg0 = new int[] {
				a,b,c,
				d,e,f,
				g,h,i };

			int[] deg90 = new int[] {
				g,d,a,
				h,e,b,
				i,f,c };

			int[] deg180 = new int[] {
				i,h,g,
				f,e,d,
				c,b,a };

			int[] deg270 = new int[] {
				c,f,i,
				b,e,h,
				a,d,g };

			int[][] rotation = new int[][] {
				deg0, deg90, deg180, deg270 };

			for (int rotDeg = 0; rotDeg < 4; rotDeg++)
				for (int x = 0; x < 9; x++)
					_[(x << 2) + rotDeg] = rotation[rotDeg][x];
		}

		//public const int maxRotations = 4; // Number of 90 degree rotations
		//public const int maxPositions = 9;

		//// Cache the 4 rotations of the 9 positions, a to i.
		//// a = 0, b = 1, c = 2,
		//// d = 3, e = 4, f = 5,
		//// g = 6, h = 7, i = 8;
		//public static readonly int[] _ = new int[maxRotations * maxPositions];

		//static Rot()
		//{
		//	int[] rotation = Enumerable.Range(0, maxPositions).ToArray();
		//	int sideLength = (int)Math.Sqrt(maxPositions);
		//	for (int rot = 0; rot < maxRotations; rot++)
		//	{
		//		for (int pos = 0; pos < maxPositions; pos++)
		//		{
		//			_[(pos * maxRotations) + rot] = rotation[pos];
		//		}
		//		rotation = RotateClockwise(rotation, sideLength);
		//	}
		//}

		////http://stackoverflow.com/a/38964502/294804
		//private static int[] RotateClockwise(int[] square1DMatrix, int? sideLength = null)
		//{
		//	var size = sideLength ?? (int)Math.Sqrt(square1DMatrix.Length);
		//	var result = new int[square1DMatrix.Length];

		//	for (var i = 0; i < size; ++i)
		//	{
		//		for (var j = 0; j < size; ++j)
		//		{
		//			result[i * size + j] = square1DMatrix[(size - j - 1) * size + i];
		//		}
		//	}

		//	return result;
		//}
	}
}
