using System.Windows.Forms;

namespace WindowsFormsApp1
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Content = new System.Windows.Forms.Panel();
            this.Tabs = new System.Windows.Forms.Panel();
            this.contentHeader = new System.Windows.Forms.Panel();
            this.Bookmarks = new System.Windows.Forms.Panel();
            this.reloadButton = new System.Windows.Forms.Button();
            this.forwardButton = new System.Windows.Forms.Button();
            this.backButton = new System.Windows.Forms.Button();
            this.searchIcon = new System.Windows.Forms.Button();
            this.textURL = new System.Windows.Forms.TextBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.maximiseButton = new System.Windows.Forms.Button();
            this.minimiseButton = new System.Windows.Forms.Button();
            this.minimiseToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.closeToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.newTabBtn = new System.Windows.Forms.Button();
            this.Tabs.SuspendLayout();
            this.contentHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // Content
            // 
            this.Content.BackColor = System.Drawing.Color.Transparent;
            this.Content.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Content.Cursor = System.Windows.Forms.Cursors.Default;
            this.Content.Location = new System.Drawing.Point(56, 225);
            this.Content.Name = "Content";
            this.Content.Size = new System.Drawing.Size(1140, 695);
            this.Content.TabIndex = 0;
            // 
            // Tabs
            // 
            this.Tabs.BackColor = System.Drawing.Color.Transparent;
            this.Tabs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Tabs.Controls.Add(this.newTabBtn);
            this.Tabs.Cursor = System.Windows.Forms.Cursors.Default;
            this.Tabs.Location = new System.Drawing.Point(56, 155);
            this.Tabs.Name = "Tabs";
            this.Tabs.Size = new System.Drawing.Size(1140, 70);
            this.Tabs.TabIndex = 0;
            // 
            // contentHeader
            // 
            this.contentHeader.BackColor = System.Drawing.Color.Transparent;
            this.contentHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.contentHeader.Controls.Add(this.Bookmarks);
            this.contentHeader.Controls.Add(this.reloadButton);
            this.contentHeader.Controls.Add(this.forwardButton);
            this.contentHeader.Controls.Add(this.backButton);
            this.contentHeader.Controls.Add(this.searchIcon);
            this.contentHeader.Controls.Add(this.textURL);
            this.contentHeader.Cursor = System.Windows.Forms.Cursors.Default;
            this.contentHeader.Location = new System.Drawing.Point(56, 55);
            this.contentHeader.Name = "contentHeader";
            this.contentHeader.Size = new System.Drawing.Size(1140, 100);
            this.contentHeader.TabIndex = 0;
            // 
            // Bookmarks
            // 
            this.Bookmarks.Location = new System.Drawing.Point(0, 57);
            this.Bookmarks.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Bookmarks.Name = "Bookmarks";
            this.Bookmarks.Size = new System.Drawing.Size(1140, 42);
            this.Bookmarks.TabIndex = 5;
            // 
            // reloadButton
            // 
            this.reloadButton.FlatAppearance.BorderSize = 0;
            this.reloadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.reloadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reloadButton.Location = new System.Drawing.Point(100, 0);
            this.reloadButton.Name = "reloadButton";
            this.reloadButton.Size = new System.Drawing.Size(50, 49);
            this.reloadButton.TabIndex = 4;
            this.reloadButton.Text = "O";
            this.reloadButton.UseVisualStyleBackColor = true;
            this.reloadButton.Click += new System.EventHandler(this.reloadButton_Click);
            // 
            // forwardButton
            // 
            this.forwardButton.FlatAppearance.BorderSize = 0;
            this.forwardButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.forwardButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.forwardButton.Location = new System.Drawing.Point(50, 0);
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(50, 49);
            this.forwardButton.TabIndex = 3;
            this.forwardButton.UseVisualStyleBackColor = true;
            this.forwardButton.Click += new System.EventHandler(this.forwardButton_Click);
            this.forwardButton.Paint += new System.Windows.Forms.PaintEventHandler(this.forwardButton_Paint);
            // 
            // backButton
            // 
            this.backButton.FlatAppearance.BorderSize = 0;
            this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.backButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backButton.Location = new System.Drawing.Point(0, 0);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(50, 49);
            this.backButton.TabIndex = 2;
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            this.backButton.Paint += new System.Windows.Forms.PaintEventHandler(this.backButton_Paint);
            // 
            // searchIcon
            // 
            this.searchIcon.FlatAppearance.BorderSize = 0;
            this.searchIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.searchIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchIcon.Location = new System.Drawing.Point(150, 0);
            this.searchIcon.Name = "searchIcon";
            this.searchIcon.Size = new System.Drawing.Size(50, 49);
            this.searchIcon.TabIndex = 1;
            this.searchIcon.Text = "G";
            this.searchIcon.UseVisualStyleBackColor = true;
            // 
            // textURL
            // 
            this.textURL.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textURL.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textURL.Location = new System.Drawing.Point(200, 0);
            this.textURL.Name = "textURL";
            this.textURL.Size = new System.Drawing.Size(940, 32);
            this.textURL.TabIndex = 0;
            this.textURL.Text = "www.google.com";
            this.textURL.TextChanged += new System.EventHandler(this.textURL_TextChanged);
            this.textURL.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textURL_KeyDown);
            // 
            // closeButton
            // 
            this.closeButton.FlatAppearance.BorderSize = 0;
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeButton.Location = new System.Drawing.Point(1125, 8);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(70, 49);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "X";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            this.closeButton.MouseEnter += new System.EventHandler(this.closeButton_Enter);
            this.closeButton.MouseLeave += new System.EventHandler(this.closeButton_Leave);
            // 
            // maximiseButton
            // 
            this.maximiseButton.FlatAppearance.BorderSize = 0;
            this.maximiseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.maximiseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.maximiseButton.Location = new System.Drawing.Point(1054, 8);
            this.maximiseButton.Name = "maximiseButton";
            this.maximiseButton.Size = new System.Drawing.Size(70, 49);
            this.maximiseButton.TabIndex = 2;
            this.maximiseButton.Text = "[]";
            this.maximiseButton.UseVisualStyleBackColor = true;
            this.maximiseButton.Click += new System.EventHandler(this.maximiseButton_Click);
            // 
            // minimiseButton
            // 
            this.minimiseButton.FlatAppearance.BorderSize = 0;
            this.minimiseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.minimiseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.minimiseButton.Location = new System.Drawing.Point(986, 8);
            this.minimiseButton.Name = "minimiseButton";
            this.minimiseButton.Size = new System.Drawing.Size(70, 49);
            this.minimiseButton.TabIndex = 3;
            this.minimiseButton.Text = "-";
            this.minimiseToolTip.SetToolTip(this.minimiseButton, "Minimise");
            this.minimiseButton.UseVisualStyleBackColor = false;
            this.minimiseButton.Click += new System.EventHandler(this.minimiseButton_Click);
            // 
            // newTabBtn
            // 
            this.newTabBtn.Location = new System.Drawing.Point(3, 7);
            this.newTabBtn.Name = "newTabBtn";
            this.newTabBtn.Size = new System.Drawing.Size(50, 50);
            this.newTabBtn.TabIndex = 0;
            this.newTabBtn.Text = "+";
            this.newTabBtn.UseVisualStyleBackColor = true;
            this.newTabBtn.Click += new System.EventHandler(this.newTabBtn_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1260, 985);
            this.Controls.Add(this.Content);
            this.Controls.Add(this.contentHeader);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.minimiseButton);
            this.Controls.Add(this.maximiseButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(1260, 985);
            this.Name = "Main";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Load += new System.EventHandler(this.Main_Load);
            this.Tabs.ResumeLayout(false);
            this.contentHeader.ResumeLayout(false);
            this.contentHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Panel Content;
        private Panel contentHeader;
        private TextBox textURL;
        private Button searchIcon;
        private Button closeButton;
        private Button maximiseButton;
        private Button minimiseButton;
        private Button reloadButton;
        private Button forwardButton;
        private Button backButton;
        private ToolTip minimiseToolTip;
        private ToolTip closeToolTip;
        private Panel Bookmarks;
        private Panel Tabs;
        private Button newTabBtn;
    }
}

