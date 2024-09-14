namespace Image_Resize_xBRZ
{
	partial class AppForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.imageChoiceGroupBox = new System.Windows.Forms.GroupBox();
            this.imageChoiceBrowseButton = new System.Windows.Forms.Button();
            this.imageChoiceTextBox = new System.Windows.Forms.TextBox();
            this.convertButton = new System.Windows.Forms.Button();
            this.outputInfoGroupBox = new System.Windows.Forms.GroupBox();
            this.outputInfoTextBox = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.imageChoiceGroupBox.SuspendLayout();
            this.outputInfoGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageChoiceGroupBox
            // 
            this.imageChoiceGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imageChoiceGroupBox.Controls.Add(this.imageChoiceBrowseButton);
            this.imageChoiceGroupBox.Controls.Add(this.imageChoiceTextBox);
            this.imageChoiceGroupBox.Location = new System.Drawing.Point(9, 10);
            this.imageChoiceGroupBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.imageChoiceGroupBox.Name = "imageChoiceGroupBox";
            this.imageChoiceGroupBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.imageChoiceGroupBox.Size = new System.Drawing.Size(843, 47);
            this.imageChoiceGroupBox.TabIndex = 0;
            this.imageChoiceGroupBox.TabStop = false;
            this.imageChoiceGroupBox.Text = "Ein Bild auswählen";
            // 
            // imageChoiceBrowseButton
            // 
            this.imageChoiceBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.imageChoiceBrowseButton.Location = new System.Drawing.Point(742, 17);
            this.imageChoiceBrowseButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.imageChoiceBrowseButton.Name = "imageChoiceBrowseButton";
            this.imageChoiceBrowseButton.Size = new System.Drawing.Size(96, 19);
            this.imageChoiceBrowseButton.TabIndex = 1;
            this.imageChoiceBrowseButton.Text = "Durchsuchen...";
            this.imageChoiceBrowseButton.UseVisualStyleBackColor = true;
            this.imageChoiceBrowseButton.Click += new System.EventHandler(this.Click_imageChoiceBrowseButton);
            // 
            // imageChoiceTextBox
            // 
            this.imageChoiceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imageChoiceTextBox.Location = new System.Drawing.Point(4, 17);
            this.imageChoiceTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.imageChoiceTextBox.Name = "imageChoiceTextBox";
            this.imageChoiceTextBox.Size = new System.Drawing.Size(734, 20);
            this.imageChoiceTextBox.TabIndex = 0;
            // 
            // convertButton
            // 
            this.convertButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.convertButton.Location = new System.Drawing.Point(374, 62);
            this.convertButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.convertButton.Name = "convertButton";
            this.convertButton.Size = new System.Drawing.Size(112, 41);
            this.convertButton.TabIndex = 1;
            this.convertButton.Text = "Konvertieren";
            this.convertButton.UseVisualStyleBackColor = true;
            this.convertButton.Click += new System.EventHandler(this.Click_convertButton);
            // 
            // outputInfoGroupBox
            // 
            this.outputInfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputInfoGroupBox.Controls.Add(this.outputInfoTextBox);
            this.outputInfoGroupBox.Location = new System.Drawing.Point(9, 108);
            this.outputInfoGroupBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.outputInfoGroupBox.Name = "outputInfoGroupBox";
            this.outputInfoGroupBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.outputInfoGroupBox.Size = new System.Drawing.Size(843, 349);
            this.outputInfoGroupBox.TabIndex = 2;
            this.outputInfoGroupBox.TabStop = false;
            this.outputInfoGroupBox.Text = "Ausgabe Information";
            // 
            // outputInfoTextBox
            // 
            this.outputInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputInfoTextBox.Location = new System.Drawing.Point(4, 17);
            this.outputInfoTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.outputInfoTextBox.Multiline = true;
            this.outputInfoTextBox.Name = "outputInfoTextBox";
            this.outputInfoTextBox.ReadOnly = true;
            this.outputInfoTextBox.Size = new System.Drawing.Size(835, 328);
            this.outputInfoTextBox.TabIndex = 0;
            // 
            // openFileDialog
            // 
            this.openFileDialog.AddExtension = false;
            this.openFileDialog.Title = "Bitte wähle eine Bilddatei aus";
            // 
            // AppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(861, 467);
            this.Controls.Add(this.outputInfoGroupBox);
            this.Controls.Add(this.convertButton);
            this.Controls.Add(this.imageChoiceGroupBox);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MinimumSize = new System.Drawing.Size(323, 298);
            this.Name = "AppForm";
            this.Text = "Image Resize xBRZ";
            this.imageChoiceGroupBox.ResumeLayout(false);
            this.imageChoiceGroupBox.PerformLayout();
            this.outputInfoGroupBox.ResumeLayout(false);
            this.outputInfoGroupBox.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox imageChoiceGroupBox;
		private System.Windows.Forms.Button imageChoiceBrowseButton;
		private System.Windows.Forms.TextBox imageChoiceTextBox;
		private System.Windows.Forms.Button convertButton;
		private System.Windows.Forms.GroupBox outputInfoGroupBox;
		private System.Windows.Forms.TextBox outputInfoTextBox;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
	}
}

