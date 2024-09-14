using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using xBRZNet;
using xBRZNet.Extensions;

namespace xBRZTester
{
	internal class Program
	{
		private const string inPath = @"Images";
		private const string outPath = @"Images-out";
		private const int scaleSize = 5;

		private static void Main(string[] args)
        {
			ClearOutFolder(outPath);

			Console.WriteLine("Bilder konvertieren...");
			Stopwatch tim = new Stopwatch();
			tim.Start();

			ConvertImages(inPath, outPath);
			
			tim.Stop();
			Console.WriteLine("Vorgang abgeschlossen, Zeitaufwand = {0}.", tim.Elapsed);
			//Console.ReadKey();
		}

		private static void ConvertImages(string inputPath, string outputPath)
		{
			string fullInputPath = Path.GetFullPath(inputPath);
			string fullOutputPath = Path.GetFullPath(outputPath);
			if (!Directory.Exists(fullOutputPath))
				Directory.CreateDirectory(fullOutputPath);
			foreach (string inputFilePath in Directory.EnumerateFiles(fullInputPath))
			{
				string fileTitle = Path.GetFileNameWithoutExtension(inputFilePath);
				string xbrzOutput = Path.Combine(fullOutputPath, fileTitle + "-xbrz.png");
				string linearOutput = Path.Combine(fullOutputPath, fileTitle + "-linear.png");
				SaveScaledImages(inputFilePath, xbrzOutput, linearOutput);
			}
		}

		private static void SaveScaledImages(string inFile, string xbrzOut, string linearOut)
		{
			var originalImage = new Bitmap(inFile);

			var scaledImage = new xBRZScaler().ScaleImage(originalImage, scaleSize);
			scaledImage.Save(xbrzOut, ImageFormat.Png);

			//var resized = new Bitmap(originalImage, new Size(originalImage.Width * scaleSize, originalImage.Height * scaleSize));
			var resized = originalImage.ResizeBitmap(originalImage.Width * scaleSize, originalImage.Height * scaleSize);
			resized.Save(linearOut, ImageFormat.Png);
		}

		private static void ClearOutFolder(string folder)
		{
			if (!Directory.Exists(folder))
				return;
			foreach (string filePath in Directory.EnumerateFiles(folder))
				File.Delete(filePath);
		}
	}
}
