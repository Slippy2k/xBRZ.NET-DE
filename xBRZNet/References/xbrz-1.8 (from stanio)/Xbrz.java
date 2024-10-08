package net.sourceforge.xbrz;

import static net.sourceforge.xbrz.BlendInfo.*;
import static net.sourceforge.xbrz.BlendType.*;
import static net.sourceforge.xbrz.RotationDegree.*;

public class Xbrz {
    private Scaler scaler;
    private ScalerCfg cfg;
    private ColorDistance dist;
    private boolean withAlpha;

    public Xbrz(int factor) {
        this(factor, true);
    }

    public Xbrz(int factor, boolean withAlpha) {
        this(factor, withAlpha, new ScalerCfg());
    }

    public Xbrz(int factor, boolean withAlpha, ScalerCfg cfg) {
        this(factor, withAlpha, cfg, ColorDistance.yCbCr(cfg.luminanceWeight));
    }

    public Xbrz(int factor, boolean withAlpha, ScalerCfg cfg, ColorDistance colorDistance) {
        this.scaler = Scaler.forFactor(factor, withAlpha);
        this.cfg = cfg;
        this.dist = withAlpha ? ColorDistance.withAlpha(colorDistance) : colorDistance;
        this.withAlpha = withAlpha;
    }

    public int scale() {
        return scaler.scale();
    }

    private final double dist(int pix1, int pix2) { return dist.calc(pix1, pix2); }

    private final boolean eq(int pix1, int pix2) { return dist(pix1, pix2) < cfg.equalColorTolerance; }

    private final BlendResult preProcessCornersResult = new BlendResult();

    /* detect blend direction

    preprocessing blend result:
    ---------
    | F | G |   evaluate corner between F, G, J, K
    |---+---|   current input pixel is at position F
    | J | K |
    ---------   F, G, J, K corners of "BlendType" */
    private BlendResult preProcessCorners(Kernel_4x4 ker) {
        BlendResult result = preProcessCornersResult.reset();

        if ((ker.f == ker.g &&
             ker.j == ker.k) ||
            (ker.f == ker.j &&
             ker.g == ker.k))
            return result;

        final double jg = dist(ker.i, ker.f) + dist(ker.f, ker.c) + dist(ker.n, ker.k) + dist(ker.k, ker.h) + cfg.centerDirectionBias * dist(ker.j, ker.g);
        final double fk = dist(ker.e, ker.j) + dist(ker.j, ker.o) + dist(ker.b, ker.g) + dist(ker.g, ker.l) + cfg.centerDirectionBias * dist(ker.f, ker.k);

        if (jg < fk)
        {
            final boolean dominantGradient = cfg.dominantDirectionThreshold * jg < fk;
            if (ker.f != ker.g && ker.f != ker.j)
                result.blend_f = dominantGradient ? BLEND_DOMINANT : BLEND_NORMAL;

            if (ker.k != ker.j && ker.k != ker.g)
                result.blend_k = dominantGradient ? BLEND_DOMINANT : BLEND_NORMAL;
        }
        else if (fk < jg)
        {
            final boolean dominantGradient = cfg.dominantDirectionThreshold * fk < jg;
            if (ker.j != ker.f && ker.j != ker.k)
                result.blend_j = dominantGradient ? BLEND_DOMINANT : BLEND_NORMAL;

            if (ker.g != ker.f && ker.g != ker.k)
                result.blend_g = dominantGradient ? BLEND_DOMINANT : BLEND_NORMAL;
        }
        return result;
    }

    private final BlendInfo blendPixelInfo = new BlendInfo();

    private void blendPixel(RotationDegree rotDeg,
                            Kernel_3x3 ker,
                            OutputMatrix out,
                            BlendInfo blendInfo) //result of preprocessing all four corners of pixel "e"
    {
        BlendInfo blend = blendPixelInfo.reset(blendInfo, rotDeg);

        if (blend.getBottomR() >= BLEND_NORMAL)
        {
            ker.rotDeg(rotDeg);
            out.rotDeg(rotDeg);

            final int e = ker.e();
            final int f = ker.f();
            final int h = ker.h();

            final int g = ker.g();
            final int c = ker.c();
            final int i = ker.i();

            boolean doLineBlend;

            if (blend.getBottomR() >= BLEND_DOMINANT)
                doLineBlend = true;

            //make sure there is no second blending in an adjacent rotation for this pixel: handles insular pixels, mario eyes
            else if (blend.getTopR() != BLEND_NONE && !eq(e, g)) //but support double-blending for 90° corners
                doLineBlend = false;
            else if (blend.getBottomL() != BLEND_NONE && !eq(e, c))
                doLineBlend = false;

            //no full blending for L-shapes; blend corner only (handles "mario mushroom eyes")
            else if (!eq(e, i) && eq(g, h) && eq(h, i) && eq(i, f) && eq(f, c))
                doLineBlend = false;

            else
                doLineBlend = true;

            final int px = dist(e, f) <= dist(e, h) ? f : h; //choose most similar color

            if (doLineBlend)
            {
                final double fg = dist(f, g);
                final double hc = dist(h, c);

                final boolean haveShallowLine = cfg.steepDirectionThreshold * fg <= hc && e != g && ker.d() != g;
                final boolean haveSteepLine   = cfg.steepDirectionThreshold * hc <= fg && e != c && ker.b() != c;

                if (haveShallowLine)
                {
                    if (haveSteepLine)
                        scaler.blendLineSteepAndShallow(px, out);
                    else
                        scaler.blendLineShallow(px, out);
                }
                else
                {
                    if (haveSteepLine)
                        scaler.blendLineSteep(px, out);
                    else
                        scaler.blendLineDiagonal(px, out);
                }
            }
            else
                scaler.blendCorner(px, out);
        }
    }

    public void scaleImage(int[] src, int[] trg, int srcWidth, int srcHeight) {
        int yFirst = 0;
        int yLast = srcHeight;

        byte[] preProcBuf = new byte[srcWidth];
        Kernel_4x4 ker4 = new Kernel_4x4(src, srcWidth, srcHeight, withAlpha);
        OutputMatrix out = new OutputMatrix(scaler.scale(), trg, srcWidth * scaler.scale());

        //initialize preprocessing buffer for first row of current stripe: detect upper left and right corner blending
        {
            ker4.positionY(yFirst - 1);

            {
                final BlendResult res = preProcessCorners(ker4);
                clearAddTopL(preProcBuf, 0, res.blend_k); //set 1st known corner for (0, yFirst)
            }

            for (int x = 0; x < srcWidth; ++x)
            {
                ker4.shift();     //shift previous kernel to the left
                ker4.readDhlp(x); // (x, yFirst - 1) is at position F

                final BlendResult res = preProcessCorners(ker4);
                addTopR(preProcBuf, x, res.blend_j); //set 2nd known corner for (x, yFirst)

                if (x + 1 < srcWidth)
                    clearAddTopL(preProcBuf, x + 1, res.blend_k); //set 1st known corner for (x + 1, yFirst)
            }
        }
        //------------------------------------------------------------------------------------

        Kernel_3x3 ker3 = new Kernel_3x3(ker4);
        BlendInfo blend_xy = new BlendInfo();
        BlendInfo blend_xy1 = new BlendInfo();

        for (int y = yFirst; y < yLast; ++y)
        {
            out.positionY(y);
            //initialize at position x = -1
            ker4.positionY(y);

            //corner blending for current (x, y + 1) position
            {
                final BlendResult res = preProcessCorners(ker4);
                blend_xy1.clearAddTopL(res.blend_k); //set 1st known corner for (0, y + 1) and buffer for use on next column

                addBottomL(preProcBuf, 0, res.blend_g); //set 3rd known corner for (0, y)
            }

            for (int x = 0; x < srcWidth; ++x, out.incrementX())
            {
                ker4.shift();     //shift previous kernel to the left
                ker4.readDhlp(x); // (x, y) is at position F

                //evaluate the four corners on bottom-right of current pixel
                blend_xy.val = preProcBuf[x]; //for current (x, y) position
                {
                    final BlendResult res = preProcessCorners(ker4);
                    blend_xy.addBottomR(res.blend_f); //all four corners of (x, y) have been determined at this point due to processing sequence!

                    blend_xy1.addTopR(res.blend_j); //set 2nd known corner for (x, y + 1)
                    preProcBuf[x] = blend_xy1.val; //store on current buffer position for use on next row

                    if (x + 1 < srcWidth)
                    {
                        //blend_xy1 -> blend_x1y1
                        blend_xy1.clearAddTopL(res.blend_k); //set 1st known corner for (x + 1, y + 1) and buffer for use on next column

                        addBottomL(preProcBuf, x + 1, res.blend_g); //set 3rd known corner for (x + 1, y)
                    }
                }

                out.fillBlock(ker4.f);

                //blend all four corners of current pixel
                if (blend_xy.blendingNeeded())
                {
                    blendPixel(ROT_0,   ker3, out, blend_xy);
                    blendPixel(ROT_90,  ker3, out, blend_xy);
                    blendPixel(ROT_180, ker3, out, blend_xy);
                    blendPixel(ROT_270, ker3, out, blend_xy);
                }
            }
        }
    }

    public static void scaleImage(int factor, boolean hasAlpha, int[] src, int[] trg, int srcWidth, int srcHeight) {
        new Xbrz(factor, hasAlpha).scaleImage(src, trg, srcWidth, srcHeight);
    }
}
