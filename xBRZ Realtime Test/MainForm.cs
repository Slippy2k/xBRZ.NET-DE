#define USE_XBRZ
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using xBRZNet;
using xBRZNet.Extensions;

namespace xBRZ_Realtime_Test
{
	public partial class MainForm : Form
	{
		private const string spriteBgPath = @"..\..\Images\Super Mario background.png";
		private const string spriteYoshiPath = @"..\..\Images\Yoshi.png";
		private const int scaleSize = 3;
		private const int fps = 30;
		private readonly Graphics grWin;
		private readonly Bitmap bmBuff;
		private readonly Graphics grBuff;
		private readonly List<Sprite> sprites = new List<Sprite>();

		public MainForm()
		{
			InitializeComponent();
			grWin = CreateGraphics();
			bmBuff = new Bitmap(ClientSize.Width, ClientSize.Height);
			grBuff = Graphics.FromImage(bmBuff);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			sprites.Add(new Sprite { image = new Bitmap(spriteBgPath) });
			sprites.Add(new Sprite { x = 50, y = 50, xSpeed = 1, image = new Bitmap(spriteYoshiPath) });

			timer.Interval = 1000 / fps;
			timer.Start();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			MoveSprites();
			DrawSprites();
			GC.Collect();
		}

		private void MoveSprites()
		{
			Sprite sp = sprites[1];
			if (sp.x > 180) sp.xSpeed = -1;
			else if (sp.x < 50) sp.xSpeed = 1;
			sp.x += sp.xSpeed;
		}

		private void DrawSprites()
		{
			foreach (Sprite sp in sprites)
			{
				grBuff.DrawImageUnscaled(sp.image, sp.x, sp.y);
			}
#if USE_XBRZ
			var scaled = xBRZ_Scaled(bmBuff);
#else
			var scaled = Linear_Scaled(bmBuff);
#endif
			grWin.DrawImageUnscaled(scaled, 0, 0);
		}

		private static Bitmap xBRZ_Scaled(Bitmap bmBuff)
		{
			return new xBRZScaler().ScaleImage(bmBuff, scaleSize);
		}

		private static Bitmap Linear_Scaled(Bitmap originalImage)
		{
			return originalImage.ResizeBitmap(originalImage.Width * scaleSize, originalImage.Height * scaleSize);
		}
	}
}
