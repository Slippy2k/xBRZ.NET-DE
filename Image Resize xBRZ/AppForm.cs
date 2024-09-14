using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using xBRZNet;

namespace Image_Resize_xBRZ
{
	public partial class AppForm : Form
	{
		private const int scaleSize = 5;

		public AppForm()
		{
			InitializeComponent();
		}

		private void Click_imageChoiceBrowseButton(object sender, EventArgs e)
		{
			ClearText();

			// Initialize input file dialog.
			openFileDialog.Reset();

			// Set the starting location to look for a file.
			openFileDialog.InitialDirectory = imageChoiceTextBox.Text;
			openFileDialog.Filter = "Bilddateien (*.jpg, *.jpeg, *.png, *.gif)|*.jpg;*.jpeg;*.png;*.gif|Alle Dateien (*.*)|*.*";

			// Display the dialog.
			DialogResult enResult = openFileDialog.ShowDialog();
			if (enResult == DialogResult.OK)
			{
				// Store the file name in the corresponding text box.
				imageChoiceTextBox.Text = openFileDialog.FileName;
			}
		}

		private void Click_convertButton(object sender, EventArgs e)
		{
			string inputPath = imageChoiceTextBox.Text;
			if (string.IsNullOrEmpty(inputPath))
			{
				SetText("Keine Datei ausgewählt.", inputPath);
				return;
			}
			if (!File.Exists(inputPath))
			{
				SetText("Datei \"{0}\" existiert nicht.", inputPath);
				return;
			}

			SetText("Bild konvertieren \"{0}\".", inputPath);

			try
			{
				// Attempt to resize the input image and save to the output path.
				var originalImage = new Bitmap(inputPath);
				AddText("Originalgröße: {0} Breite, {1} Höhe.", originalImage.Width, originalImage.Height);
                var scaledImage = new xBRZScaler().ScaleImage(originalImage, scaleSize);
                AddText("\n");
                string outputPath =
                    Path.Combine(
                        Path.GetDirectoryName(inputPath),
                        Path.GetFileNameWithoutExtension(inputPath) + "-xbrz.png");
                scaledImage.Save(outputPath, ImageFormat.Png);
                AddText("In Bilddatei gespeichert \"{0}\".", outputPath);
                AddText("Skalierte Größe: {0} Breite, {1} Höhe.", scaledImage.Width, scaledImage.Height);
			}
			catch
			{
				SetText("Bild konnte nicht konvertiert werden \"{0}\".", inputPath);
			}
		}

		private void ClearText()
		{
			outputInfoTextBox.Text = string.Empty;
		}

		private void SetText(string format, params object[] args)
		{
			outputInfoTextBox.Text = string.Format(format, args) + Environment.NewLine;
		}

		private void AddText(string format, params object[] args)
		{
			outputInfoTextBox.Text += string.Format(format, args) + Environment.NewLine;
		}
	}
}
