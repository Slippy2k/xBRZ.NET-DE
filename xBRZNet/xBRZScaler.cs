using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using xBRZNet.Blend;
using xBRZNet.Color;
using xBRZNet.Common;
using xBRZNet.Extensions;
using xBRZNet.Scalers;

namespace xBRZNet
{
	/*
		-------------------------------------------------------------------------
		| xBRZ: "Scale by rules" - high quality image upscaling filter by Zenju |
		-------------------------------------------------------------------------
		using a modified approach of xBR:
		http://board.byuu.org/viewtopic.php?f=10&t=2248
		- new rule set preserving small image features
		- support multithreading
		- support 64 bit architectures
		- support processing image slices
		
		-> map source (srcWidth * srcHeight) to target (scale * width x scale * height)
		image, optionally processing a half-open slice of rows [yFirst, yLast) only
		-> color format: ARGB (BGRA char order), alpha channel unused
		-> support for source/target pitch in chars!
		-> if your emulator changes only a few image slices during each cycle
		(e.g. Dosbox) then there's no need to run xBRZ on the complete image:
		Just make sure you enlarge the source image slice by 2 rows on top and
		2 on bottom (this is the additional range the xBRZ algorithm is using
		during analysis)
		Caveat: If there are multiple changed slices, make sure they do not overlap
		after adding these additional rows in order to avoid a memory race condition 
		if you are using multiple threads for processing each enlarged slice!

		THREAD-SAFETY: - parts of the same image may be scaled by multiple threads
		as long as the [yFirst, yLast) ranges do not overlap!
		- there is a minor inefficiency for the first row of a slice, so avoid
		processing single rows only
	*/
	public class xBRZScaler
	{
		// scaleSize = 2 to 5
		public Bitmap ScaleImage(Bitmap image, int scaleSize, ScalerCfg config = null)
		{
			config = config ?? new ScalerCfg();

			Bitmap fixedFormatImage = image.ChangeFormat(PixelFormat.Format32bppRgb);
			int[] rgbValues = fixedFormatImage.ToIntArray();

			int scaleFactor = scaleSize;
			var scaledRbgValues = new int[rgbValues.Length * (scaleFactor * scaleFactor)];

			ScaleImage(scaleSize, rgbValues, scaledRbgValues, fixedFormatImage.Width, fixedFormatImage.Height, config, 0, int.MaxValue);

			var newImage = new Bitmap(fixedFormatImage.Width * scaleFactor, fixedFormatImage.Height * scaleFactor, fixedFormatImage.PixelFormat);

			newImage.FromPixelArray(scaledRbgValues);

			return newImage;
		}

		private static readonly Dictionary<int, IScaler> scalers = new Dictionary<int, IScaler> {
			{ 2, new Scaler2X() },
			{ 3, new Scaler3X() },
			{ 4, new Scaler4X() },
			{ 5, new Scaler5X() } };

		public void ScaleImage(int scaleSize, int[] src, int[] trg, int srcWidth, int srcHeight, ScalerCfg cfg, int yFirst, int yLast)
		{
			scaler = scalers[scaleSize];
			this.cfg = cfg;
			colorDistance = new ColorDist(cfg);
			colorEqualizer = new ColorEq(cfg);
			ScaleImageImpl(src, trg, srcWidth, srcHeight, yFirst, yLast);
		}

		private ScalerCfg cfg;
		private IScaler scaler;
		private OutputMatrix outputMatrix;
		private readonly BlendResult blendResult = new BlendResult();

		private ColorDist colorDistance;
		private ColorEq colorEqualizer;

		//fill block with the given color
		private static void FillBlock(int[] trg, int trgi, int pitch, int col, int blockSize)
		{
			for (int y = 0; y < blockSize; ++y, trgi += pitch)
				for (int x = 0; x < blockSize; ++x)
					trg[trgi + x] = col;
		}

		//detect blend direction
		private void PreProcessCorners(Kernel_4x4 ker)
		{
			blendResult.Reset();

			if ((ker.f == ker.g && ker.j == ker.k) || (ker.f == ker.j && ker.g == ker.k))
				return;

			Func<int,int,double> dist = colorDistance.DistYCbCr;

			const int weight = 4;
			double jg = dist(ker.i, ker.f) + dist(ker.f, ker.c) + dist(ker.n, ker.k) + dist(ker.k, ker.h) + weight * dist(ker.j, ker.g);
			double fk = dist(ker.e, ker.j) + dist(ker.j, ker.o) + dist(ker.b, ker.g) + dist(ker.g, ker.l) + weight * dist(ker.f, ker.k);

			if (jg < fk)
			{
				char gradient =
					cfg.DominantDirectionThreshold * jg < fk
					? BlendType.Dominant
					: BlendType.Normal;
				
				if (ker.f != ker.g && ker.f != ker.j)
					blendResult.f = gradient;

				if (ker.k != ker.j && ker.k != ker.g)
					blendResult.k = gradient;
			}
			else if (fk < jg)
			{
				char gradient =
					cfg.DominantDirectionThreshold * fk < jg
					? BlendType.Dominant
					: BlendType.Normal;

				if (ker.j != ker.f && ker.j != ker.k)
					blendResult.j = gradient;

				if (ker.g != ker.f && ker.g != ker.k)
					blendResult.g = gradient;
			}
		}

		/*
			input kernel area naming convention:
			-------------
			| A | B | C |
			----|---|---|
			| D | E | F | //input pixel is at position E
			----|---|---|
			| G | H | I |
			-------------
			blendInfo: result of preprocessing all four corners of pixel "e"
		*/
		private void ScalePixel(IScaler scaler, int rotDeg, Kernel_3x3 ker, int trgi, char blendInfo)
		{
			// int a = ker._[Rot._[(0 << 2) + rotDeg]];
			int b = ker._[Rot._[(1 << 2) + rotDeg]];
			int c = ker._[Rot._[(2 << 2) + rotDeg]];
			int d = ker._[Rot._[(3 << 2) + rotDeg]];
			int e = ker._[Rot._[(4 << 2) + rotDeg]];
			int f = ker._[Rot._[(5 << 2) + rotDeg]];
			int g = ker._[Rot._[(6 << 2) + rotDeg]];
			int h = ker._[Rot._[(7 << 2) + rotDeg]];
			int i = ker._[Rot._[(8 << 2) + rotDeg]];

			char blend = BlendInfo.Rotate(blendInfo, rotDeg);

			if (BlendInfo.GetBottomR(blend) == BlendType.None) return;

			ColorEq eq = colorEqualizer;
			ColorDist dist = colorDistance;

			bool doLineBlend;

			if (BlendInfo.GetBottomR(blend) >= BlendType.Dominant)
				doLineBlend = true;

			//make sure there is no second blending in an adjacent
			//rotation for this pixel: handles insular pixels, mario eyes
			//but support double-blending for 90� corners
			else if (BlendInfo.GetTopR(blend) != BlendType.None && !eq.IsColorEqual(e, g))
				doLineBlend = false;
			else if (BlendInfo.GetBottomL(blend) != BlendType.None && !eq.IsColorEqual(e, c))
				doLineBlend = false;
			//no full blending for L-shapes; blend corner only (handles "mario mushroom eyes")
			else if (eq.IsColorEqual(g, h) && eq.IsColorEqual(h, i) && eq.IsColorEqual(i, f) && eq.IsColorEqual(f, c) && !eq.IsColorEqual(e, i))
				doLineBlend = false;
			else
				doLineBlend = true;

			//choose most similar color
			int px = dist.DistYCbCr(e, f) <= dist.DistYCbCr(e, h) ? f : h;

			OutputMatrix outMatrix = outputMatrix;
			outMatrix.Move(rotDeg, trgi);

			if (!doLineBlend)
			{
				scaler.BlendCorner(px, outMatrix);
				return;
			}

			//test sample: 70% of values max(fg, hc) / min(fg, hc)
			//are between 1.1 and 3.7 with median being 1.9
			double fg = dist.DistYCbCr(f, g);
			double hc = dist.DistYCbCr(h, c);

			bool haveShallowLine = cfg.SteepDirectionThreshold * fg <= hc && e != g && d != g;
			bool haveSteepLine = cfg.SteepDirectionThreshold * hc <= fg && e != c && b != c;

			if (haveShallowLine)
			{
				if (haveSteepLine)
					scaler.BlendLineSteepAndShallow(px, outMatrix);
				else
					scaler.BlendLineShallow(px, outMatrix);
			}
			else
			{
				if (haveSteepLine)
					scaler.BlendLineSteep(px, outMatrix);
				else
					scaler.BlendLineDiagonal(px, outMatrix);
			}
		}

		//scaler policy: see "Scaler2x" reference implementation
		private void ScaleImageImpl(int[] src, int[] trg, int srcWidth, int srcHeight, int yFirst, int yLast)
		{
			yFirst = Math.Max(yFirst, 0);
			yLast = Math.Min(yLast, srcHeight);

			if (yFirst >= yLast || srcWidth <= 0)
				return;

			int trgWidth = srcWidth * scaler.Scale;

			//temporary buffer for "on the fly preprocessing"
			var preProcBuffer = new char[srcWidth];

			var ker4 = new Kernel_4x4();

			//initialize preprocessing buffer for first row:
			//detect upper left and right corner blending
			//this cannot be optimized for adjacent processing
			//stripes; we must not allow for a memory race condition!
			if (yFirst > 0)
			{
				int y = yFirst - 1;

				int sM1 = srcWidth * Math.Max(y - 1, 0);
				int s0 = srcWidth * y; //center line
				int sP1 = srcWidth * Math.Min(y + 1, srcHeight - 1);
				int sP2 = srcWidth * Math.Min(y + 2, srcHeight - 1);

				for (int x = 0; x < srcWidth; ++x)
				{
					int xM1 = Math.Max(x - 1, 0);
					int xP1 = Math.Min(x + 1, srcWidth - 1);
					int xP2 = Math.Min(x + 2, srcWidth - 1);

					//read sequentially from memory as far as possible
					ker4.a = src[sM1 + xM1];
					ker4.b = src[sM1 + x];
					ker4.c = src[sM1 + xP1];
					ker4.d = src[sM1 + xP2];

					ker4.e = src[s0 + xM1];
					ker4.f = src[s0 + x];
					ker4.g = src[s0 + xP1];
					ker4.h = src[s0 + xP2];

					ker4.i = src[sP1 + xM1];
					ker4.j = src[sP1 + x];
					ker4.k = src[sP1 + xP1];
					ker4.l = src[sP1 + xP2];

					ker4.m = src[sP2 + xM1];
					ker4.n = src[sP2 + x];
					ker4.o = src[sP2 + xP1];
					ker4.p = src[sP2 + xP2];

					PreProcessCorners(ker4); // writes to blendResult
					/*
					preprocessing blend result:
					---------
					| F | G | //evalute corner between F, G, J, K
					----|---| //input pixel is at position F
					| J | K |
					---------
					*/
					preProcBuffer[x] = BlendInfo.SetTopR(preProcBuffer[x], blendResult.j);

					if (x + 1 < srcWidth)
						preProcBuffer[x + 1] = BlendInfo.SetTopL(preProcBuffer[x + 1], blendResult.k);
				}
			}

			outputMatrix = new OutputMatrix(scaler.Scale, trg, trgWidth);

			var ker3 = new Kernel_3x3();

			for (int y = yFirst; y < yLast; ++y)
			{
				//consider MT "striped" access
				int trgi = scaler.Scale * y * trgWidth;

				int sM1 = srcWidth * Math.Max(y - 1, 0);
				int s0 = srcWidth * y; //center line
				int sP1 = srcWidth * Math.Min(y + 1, srcHeight - 1);
				int sP2 = srcWidth * Math.Min(y + 2, srcHeight - 1);

				char blendXy1 = (char)0;

				for (int x = 0; x < srcWidth; ++x, trgi += scaler.Scale)
				{
					int xM1 = Math.Max(x - 1, 0);
					int xP1 = Math.Min(x + 1, srcWidth - 1);
					int xP2 = Math.Min(x + 2, srcWidth - 1);

					//evaluate the four corners on bottom-right of current pixel
					//blend_xy for current (x, y) position

					//read sequentially from memory as far as possible
					ker4.a = src[sM1 + xM1];
					ker4.b = src[sM1 + x];
					ker4.c = src[sM1 + xP1];
					ker4.d = src[sM1 + xP2];

					ker4.e = src[s0 + xM1];
					ker4.f = src[s0 + x];
					ker4.g = src[s0 + xP1];
					ker4.h = src[s0 + xP2];

					ker4.i = src[sP1 + xM1];
					ker4.j = src[sP1 + x];
					ker4.k = src[sP1 + xP1];
					ker4.l = src[sP1 + xP2];

					ker4.m = src[sP2 + xM1];
					ker4.n = src[sP2 + x];
					ker4.o = src[sP2 + xP1];
					ker4.p = src[sP2 + xP2];

					PreProcessCorners(ker4); // writes to blendResult

					/*
						preprocessing blend result:
						---------
						| F | G | //evaluate corner between F, G, J, K
						----|---| //current input pixel is at position F
						| J | K |
						---------
					*/

					//all four corners of (x, y) have been determined at
					//this point due to processing sequence!
					char blendXy = BlendInfo.SetBottomR(preProcBuffer[x], blendResult.f);

					//set 2nd known corner for (x, y + 1)
					blendXy1 = BlendInfo.SetTopR(blendXy1, blendResult.j);
					//store on current buffer position for use on next row
					preProcBuffer[x] = blendXy1;

					//set 1st known corner for (x + 1, y + 1) and
					//buffer for use on next column
					blendXy1 = BlendInfo.SetTopL((char)0, blendResult.k);

					if (x + 1 < srcWidth)
					{
						//set 3rd known corner for (x + 1, y)
						preProcBuffer[x + 1] = BlendInfo.SetBottomL(preProcBuffer[x + 1], blendResult.g);
					}

					//fill block of size scale * scale with the given color
					//  //place *after* preprocessing step, to not overwrite the
					//  //results while processing the the last pixel!
					FillBlock(trg, trgi, trgWidth, src[s0 + x], scaler.Scale);

					//blend four corners of current pixel
					if (blendXy == 0)
						continue;

					const int a = 0, b = 1, c = 2, d = 3, e = 4, f = 5, g = 6, h = 7, i = 8;

					//read sequentially from memory as far as possible
					ker3._[a] = src[sM1 + xM1];
					ker3._[b] = src[sM1 + x];
					ker3._[c] = src[sM1 + xP1];

					ker3._[d] = src[s0 + xM1];
					ker3._[e] = src[s0 + x];
					ker3._[f] = src[s0 + xP1];

					ker3._[g] = src[sP1 + xM1];
					ker3._[h] = src[sP1 + x];
					ker3._[i] = src[sP1 + xP1];

					ScalePixel(scaler, (int)RotationDegree.ROT_0, ker3, trgi, blendXy);
					ScalePixel(scaler, (int)RotationDegree.ROT_90, ker3, trgi, blendXy);
					ScalePixel(scaler, (int)RotationDegree.ROT_180, ker3, trgi, blendXy);
					ScalePixel(scaler, (int)RotationDegree.ROT_270, ker3, trgi, blendXy);
				}
			}
		}
	}
}
