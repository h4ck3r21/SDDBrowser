namespace SDDPopup
{
    partial class AppPopup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppPopup));
            this.CloseButton = new System.Windows.Forms.Button();
            this.EnterButton = new System.Windows.Forms.Button();
            this.ColorDialog = new System.Windows.Forms.ColorDialog();
            this.ForeColorLabel = new System.Windows.Forms.Label();
            this.BackColorLabel = new System.Windows.Forms.Label();
            this.AccentColor = new System.Windows.Forms.Label();
            this.ForeColorButton = new System.Windows.Forms.Button();
            this.BackColorButton = new System.Windows.Forms.Button();
            this.AccentColorButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CloseButton
            // 
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = new System.Drawing.Point(55, 606);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(145, 86);
            this.CloseButton.TabIndex = 0;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // EnterButton
            // 
            this.EnterButton.Location = new System.Drawing.Point(306, 606);
            this.EnterButton.Name = "EnterButton";
            this.EnterButton.Size = new System.Drawing.Size(145, 86);
            this.EnterButton.TabIndex = 1;
            this.EnterButton.Text = "Submit";
            this.EnterButton.UseVisualStyleBackColor = true;
            this.EnterButton.Click += new System.EventHandler(this.EnterButton_Click);
            // 
            // ForeColorLabel
            // 
            this.ForeColorLabel.AutoSize = true;
            this.ForeColorLabel.Location = new System.Drawing.Point(80, 77);
            this.ForeColorLabel.Name = "ForeColorLabel";
            this.ForeColorLabel.Size = new System.Drawing.Size(80, 20);
            this.ForeColorLabel.TabIndex = 3;
            this.ForeColorLabel.Text = "Text Color";
            // 
            // BackColor
            // 
            this.BackColorLabel.AutoSize = true;
            this.BackColorLabel.Location = new System.Drawing.Point(80, 119);
            this.BackColorLabel.Name = "BackColor";
            this.BackColorLabel.Size = new System.Drawing.Size(136, 20);
            this.BackColorLabel.TabIndex = 4;
            this.BackColorLabel.Text = "Background Color";
            // 
            // AccentColor
            // 
            this.AccentColor.AutoSize = true;
            this.AccentColor.Location = new System.Drawing.Point(80, 162);
            this.AccentColor.Name = "AccentColor";
            this.AccentColor.Size = new System.Drawing.Size(100, 20);
            this.AccentColor.TabIndex = 5;
            this.AccentColor.Text = "Accent Color";
            // 
            // ForeColorButton
            // 
            this.ForeColorButton.Location = new System.Drawing.Point(295, 68);
            this.ForeColorButton.Name = "ForeColorButton";
            this.ForeColorButton.Size = new System.Drawing.Size(81, 38);
            this.ForeColorButton.TabIndex = 6;
            this.ForeColorButton.Text = "Change";
            this.ForeColorButton.UseVisualStyleBackColor = true;
            this.ForeColorButton.Click += new System.EventHandler(this.ForeColorButton_Click);
            // 
            // BackColorButton
            // 
            this.BackColorButton.Location = new System.Drawing.Point(295, 110);
            this.BackColorButton.Name = "BackColorButton";
            this.BackColorButton.Size = new System.Drawing.Size(81, 38);
            this.BackColorButton.TabIndex = 7;
            this.BackColorButton.Text = "Change";
            this.BackColorButton.UseVisualStyleBackColor = true;
            this.BackColorButton.Click += new System.EventHandler(this.BackColorButton_Click);
            // 
            // AccentColorButton
            // 
            this.AccentColorButton.Location = new System.Drawing.Point(295, 153);
            this.AccentColorButton.Name = "AccentColorButton";
            this.AccentColorButton.Size = new System.Drawing.Size(81, 38);
            this.AccentColorButton.TabIndex = 8;
            this.AccentColorButton.Text = "Change";
            this.AccentColorButton.UseVisualStyleBackColor = true;
            this.AccentColorButton.Click += new System.EventHandler(this.AccentColorButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(80, 202);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 30);
            this.label1.TabIndex = 9;
            this.label1.Text = "Text Size";
            // 
            // AppPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CloseButton;
            this.ClientSize = new System.Drawing.Size(530, 751);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.AccentColorButton);
            this.Controls.Add(this.BackColorButton);
            this.Controls.Add(this.ForeColorButton);
            this.Controls.Add(this.AccentColor);
            this.Controls.Add(this.BackColorLabel);
            this.Controls.Add(this.ForeColorLabel);
            this.Controls.Add(this.EnterButton);
            this.Controls.Add(this.CloseButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AppPopup";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AppPopup";
            this.Load += new System.EventHandler(this.AppPopup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Button EnterButton;
        private System.Windows.Forms.ColorDialog ColorDialog;
        private System.Windows.Forms.Label ForeColorLabel;
        private System.Windows.Forms.Label BackColorLabel;
        private System.Windows.Forms.Label AccentColor;
        private System.Windows.Forms.Button ForeColorButton;
        private System.Windows.Forms.Button BackColorButton;
        private System.Windows.Forms.Button AccentColorButton;
        private System.Windows.Forms.Label label1;
    }
}