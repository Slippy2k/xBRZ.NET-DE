using System;
using xBRZNet.Scalers;

namespace xBRZNet.Color
{
    internal class ColorEq : ColorDist
    {
		private double eqColorThres;
		
		public ColorEq(ScalerCfg cfg)
			: base(cfg)
		{
			double ect = cfg.EqualColorTolerance;
			eqColorThres = ect * ect;
		}

        public bool IsColorEqual(int color1, int color2)
        {
			return DistYCbCr(color1, color2) < eqColorThres;
        }
    }
}
