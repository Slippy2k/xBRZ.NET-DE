using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace xBRZNet.Extensions
{
    public static class BitmapExtensions
    {
		public static Bitmap FromPixelArray(this Bitmap newImage, int[] bitmapData)
		{
			var rectangle = new Rectangle(0, 0, newImage.Width, newImage.Height);
			var newBitmapData = newImage.LockBits(rectangle, ImageLockMode.ReadWrite, newImage.PixelFormat);
			// Get the address of the first line.
			var newBitmapPointer = newBitmapData.Scan0;
			//http://stackoverflow.com/a/1917036/294804
			int count = newBitmapData.Stride * newImage.Height / 4;
			// Copy the RGB values back to the bitmap
			Marshal.Copy(bitmapData, 0, newBitmapPointer, count);
			// Unlock the bits.
			newImage.UnlockBits(newBitmapData);

			return newImage;
		}

		//https://msdn.microsoft.com/en-us/library/ms229672(v=vs.90).aspx
        public static int[] ToIntArray(this Bitmap image)
        {
            // Lock the bitmap's bits.
            var rectangle = new Rectangle(0, 0, image.Width, image.Height);
            var bitmapData = image.LockBits(rectangle, ImageLockMode.ReadWrite, image.PixelFormat);
            // Get the address of the first line.
            IntPtr bitmapPointer = bitmapData.Scan0;
            //http://stackoverflow.com/a/13273799/294804
            if (bitmapData.Stride < 0)
            {
                bitmapPointer += bitmapData.Stride * (image.Height - 1);
            }
            //http://stackoverflow.com/a/1917036/294804
            // Declare an array to hold the bytes of the bitmap. 
            int count = bitmapData.Stride * image.Height / 4;
            var values = new int[count];
            // Copy the RGB values into the array.
            Marshal.Copy(bitmapPointer, values, 0, count);
            // Unlock the bits.
            image.UnlockBits(bitmapData);

            return values;
        }

		//http://stackoverflow.com/a/2016509/294804
		public static Bitmap ChangeFormat(this Bitmap image, PixelFormat format)
		{
			var newFormatImage = new Bitmap(image.Width, image.Height, format);
			using (var gr = Graphics.FromImage(newFormatImage))
			{
				gr.DrawImage(image, new Rectangle(0, 0, newFormatImage.Width, newFormatImage.Height));
			}
			return newFormatImage;
		}

		/// <summary>
		/// Resize the image to the specified width and height.
		/// http://stackoverflow.com/a/24199315
		/// </summary>
		/// <param name="image">The image to resize.</param>
		/// <param name="width">The width to resize to.</param>
		/// <param name="height">The height to resize to.</param>
		/// <returns>The resized image.</returns>
		public static Bitmap ResizeBitmap(this Bitmap image, int width, int height)
		{
			var destRect = new Rectangle(0, 0, width, height);
			var destImage = new Bitmap(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.AssumeLinear;
				graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
				graphics.SmoothingMode = SmoothingMode.None;
				graphics.PixelOffsetMode = PixelOffsetMode.None;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			return destImage;
		}
    }
}
