using System;
using xBRZNet.Common;
using xBRZNet.Scalers;

namespace xBRZNet.Color
{
	internal class ColorDist
	{
		protected readonly ScalerCfg cfg;

		public ColorDist(ScalerCfg cfg)
		{
			this.cfg = cfg;
		}

		public double DistYCbCr(int pix1, int pix2)
		{
			// This is pointless. The small time it saves is counteracted
			// by the time spent doing the check every pixel.
			//if (pix1 == pix2)
			//	return 0;

			//http://en.wikipedia.org/wiki/YCbCr#ITU-R_BT.601_conversion
			//YCbCr conversion is a matrix multiplication => take advantage of linearity by subtracting first!
			int diffR = ((pix1 & Mask.Red) - (pix2 & Mask.Red)) >> 16; //we may delay division by 255 to after matrix multiplication
			int diffG = ((pix1 & Mask.Green) - (pix2 & Mask.Green)) >> 8;
			int diffB = (pix1 & Mask.Blue) - (pix2 & Mask.Blue); //subtraction for int is noticeable faster than for double

			const double kB = 0.0722; //ITU-R BT.709 conversion
			const double kR = 0.2126;
			const double kG = 1 - kB - kR;

			const double scaleB = 0.5 / (1 - kB);
			const double scaleR = 0.5 / (1 - kR);

			double y = kR * diffR + kG * diffG + kB * diffB; //[!], analog YCbCr!
			double cB = scaleB * (diffB - y);
			double cR = scaleR * (diffR - y);

			// Skip division by 255.
			// Also skip square root here by pre-squaring the config option equalColorTolerance.
			double yLum = cfg.LuminanceWeight * y;
			double result = (yLum * yLum) + (cB * cB) + (cR * cR);
			return result;
		}
	}
}
