using System.Windows.Forms;

namespace SDDWebBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.ContentPanel = new System.Windows.Forms.Panel();
            this.TabsPanel = new System.Windows.Forms.Panel();
            this.newTabBtn = new System.Windows.Forms.Button();
            this.ContentHeader = new System.Windows.Forms.Panel();
            this.AddBookmarkButton = new System.Windows.Forms.Button();
            this.BookmarksPanel = new System.Windows.Forms.Panel();
            this.ReloadButton = new System.Windows.Forms.Button();
            this.ForwardButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.SearchIcon = new System.Windows.Forms.Button();
            this.TextURL = new System.Windows.Forms.TextBox();
            this.CloseButton = new System.Windows.Forms.Button();
            this.MaximiseButton = new System.Windows.Forms.Button();
            this.MinimiseButton = new System.Windows.Forms.Button();
            this.MinimiseToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.CloseToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.BaseMenuStrip = new System.Windows.Forms.MenuStrip();
            this.Settings = new System.Windows.Forms.ToolStripMenuItem();
            this.MoreSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BookmarkSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportButton = new System.Windows.Forms.ToolStripMenuItem();
            this.BookmarkSettingsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingsButton = new System.Windows.Forms.Button();
            this.TabsPanel.SuspendLayout();
            this.ContentHeader.SuspendLayout();
            this.BaseMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ContentPanel
            // 
            this.ContentPanel.BackColor = System.Drawing.Color.Transparent;
            this.ContentPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ContentPanel.Cursor = System.Windows.Forms.Cursors.Default;
            this.ContentPanel.Location = new System.Drawing.Point(56, 231);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.Size = new System.Drawing.Size(1140, 695);
            this.ContentPanel.TabIndex = 0;
            // 
            // TabsPanel
            // 
            this.TabsPanel.BackColor = System.Drawing.Color.Transparent;
            this.TabsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TabsPanel.Controls.Add(this.newTabBtn);
            this.TabsPanel.Cursor = System.Windows.Forms.Cursors.Default;
            this.TabsPanel.Location = new System.Drawing.Point(56, 161);
            this.TabsPanel.Name = "TabsPanel";
            this.TabsPanel.Size = new System.Drawing.Size(1140, 70);
            this.TabsPanel.TabIndex = 0;
            // 
            // newTabBtn
            // 
            this.newTabBtn.Location = new System.Drawing.Point(3, 7);
            this.newTabBtn.Name = "newTabBtn";
            this.newTabBtn.Size = new System.Drawing.Size(50, 50);
            this.newTabBtn.TabIndex = 0;
            this.newTabBtn.Text = "+";
            this.newTabBtn.UseVisualStyleBackColor = true;
            // 
            // ContentHeader
            // 
            this.ContentHeader.BackColor = System.Drawing.Color.Transparent;
            this.ContentHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ContentHeader.Controls.Add(this.AddBookmarkButton);
            this.ContentHeader.Controls.Add(this.BookmarksPanel);
            this.ContentHeader.Controls.Add(this.ReloadButton);
            this.ContentHeader.Controls.Add(this.ForwardButton);
            this.ContentHeader.Controls.Add(this.BackButton);
            this.ContentHeader.Controls.Add(this.SearchIcon);
            this.ContentHeader.Controls.Add(this.TextURL);
            this.ContentHeader.Cursor = System.Windows.Forms.Cursors.Default;
            this.ContentHeader.Location = new System.Drawing.Point(56, 55);
            this.ContentHeader.Name = "ContentHeader";
            this.ContentHeader.Size = new System.Drawing.Size(1140, 108);
            this.ContentHeader.TabIndex = 0;
            // 
            // AddBookmarkButton
            // 
            this.AddBookmarkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddBookmarkButton.FlatAppearance.BorderSize = 0;
            this.AddBookmarkButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddBookmarkButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AddBookmarkButton.Location = new System.Drawing.Point(1085, -1);
            this.AddBookmarkButton.Name = "AddBookmarkButton";
            this.AddBookmarkButton.Size = new System.Drawing.Size(50, 49);
            this.AddBookmarkButton.TabIndex = 6;
            this.AddBookmarkButton.Text = "❤️";
            this.AddBookmarkButton.UseVisualStyleBackColor = true;
            // 
            // BookmarksPanel
            // 
            this.BookmarksPanel.Location = new System.Drawing.Point(0, 48);
            this.BookmarksPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.BookmarksPanel.Name = "BookmarksPanel";
            this.BookmarksPanel.Size = new System.Drawing.Size(1140, 59);
            this.BookmarksPanel.TabIndex = 5;
            // 
            // ReloadButton
            // 
            this.ReloadButton.FlatAppearance.BorderSize = 0;
            this.ReloadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReloadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReloadButton.Location = new System.Drawing.Point(100, 0);
            this.ReloadButton.Name = "ReloadButton";
            this.ReloadButton.Size = new System.Drawing.Size(50, 49);
            this.ReloadButton.TabIndex = 4;
            this.ReloadButton.UseVisualStyleBackColor = true;
            this.ReloadButton.Paint += new System.Windows.Forms.PaintEventHandler(this.ReloadButton_Paint);
            // 
            // ForwardButton
            // 
            this.ForwardButton.FlatAppearance.BorderSize = 0;
            this.ForwardButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ForwardButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForwardButton.Location = new System.Drawing.Point(50, 0);
            this.ForwardButton.Name = "ForwardButton";
            this.ForwardButton.Size = new System.Drawing.Size(50, 49);
            this.ForwardButton.TabIndex = 3;
            this.ForwardButton.UseVisualStyleBackColor = true;
            // 
            // BackButton
            // 
            this.BackButton.FlatAppearance.BorderSize = 0;
            this.BackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BackButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BackButton.Location = new System.Drawing.Point(0, 0);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(50, 49);
            this.BackButton.TabIndex = 2;
            this.BackButton.UseVisualStyleBackColor = true;
            // 
            // SearchIcon
            // 
            this.SearchIcon.FlatAppearance.BorderSize = 0;
            this.SearchIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SearchIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchIcon.Location = new System.Drawing.Point(150, 0);
            this.SearchIcon.Name = "SearchIcon";
            this.SearchIcon.Size = new System.Drawing.Size(50, 49);
            this.SearchIcon.TabIndex = 1;
            this.SearchIcon.Text = "G";
            this.SearchIcon.UseVisualStyleBackColor = true;
            this.SearchIcon.Click += new System.EventHandler(this.SearchIcon_Click);
            // 
            // TextURL
            // 
            this.TextURL.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextURL.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextURL.Location = new System.Drawing.Point(200, 0);
            this.TextURL.Name = "TextURL";
            this.TextURL.Size = new System.Drawing.Size(867, 32);
            this.TextURL.TabIndex = 0;
            this.TextURL.Text = "www.google.com";
            // 
            // CloseButton
            // 
            this.CloseButton.FlatAppearance.BorderSize = 0;
            this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CloseButton.Location = new System.Drawing.Point(1125, 8);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(70, 49);
            this.CloseButton.TabIndex = 1;
            this.CloseButton.Text = "X";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            this.CloseButton.MouseEnter += new System.EventHandler(this.CloseButton_Enter);
            this.CloseButton.MouseLeave += new System.EventHandler(this.CloseButton_Leave);
            // 
            // MaximiseButton
            // 
            this.MaximiseButton.FlatAppearance.BorderSize = 0;
            this.MaximiseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MaximiseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximiseButton.Location = new System.Drawing.Point(1054, 8);
            this.MaximiseButton.Name = "MaximiseButton";
            this.MaximiseButton.Size = new System.Drawing.Size(70, 49);
            this.MaximiseButton.TabIndex = 2;
            this.MaximiseButton.Text = "[]";
            this.MaximiseButton.UseVisualStyleBackColor = true;
            this.MaximiseButton.Click += new System.EventHandler(this.MaximiseButton_Click);
            // 
            // MinimiseButton
            // 
            this.MinimiseButton.FlatAppearance.BorderSize = 0;
            this.MinimiseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MinimiseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimiseButton.Location = new System.Drawing.Point(986, 8);
            this.MinimiseButton.Name = "MinimiseButton";
            this.MinimiseButton.Size = new System.Drawing.Size(70, 49);
            this.MinimiseButton.TabIndex = 3;
            this.MinimiseButton.Text = "-";
            this.MinimiseToolTip.SetToolTip(this.MinimiseButton, "Minimise");
            this.MinimiseButton.UseVisualStyleBackColor = false;
            this.MinimiseButton.Click += new System.EventHandler(this.MinimiseButton_Click);
            // 
            // BaseMenuStrip
            // 
            this.BaseMenuStrip.BackColor = System.Drawing.Color.Transparent;
            this.BaseMenuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.BaseMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.BaseMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Settings,
            this.BookmarkSettings});
            this.BaseMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.BaseMenuStrip.Name = "BaseMenuStrip";
            this.BaseMenuStrip.Size = new System.Drawing.Size(1260, 33);
            this.BaseMenuStrip.TabIndex = 4;
            this.BaseMenuStrip.Visible = false;
            // 
            // Settings
            // 
            this.Settings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MoreSettingsToolStripMenuItem});
            this.Settings.Name = "Settings";
            this.Settings.Size = new System.Drawing.Size(92, 29);
            this.Settings.Text = "Settings";
            this.Settings.Click += new System.EventHandler(this.Settings_Click);
            // 
            // MoreSettingsToolStripMenuItem
            // 
            this.MoreSettingsToolStripMenuItem.Name = "MoreSettingsToolStripMenuItem";
            this.MoreSettingsToolStripMenuItem.Size = new System.Drawing.Size(225, 34);
            this.MoreSettingsToolStripMenuItem.Text = "More Settings";
            this.MoreSettingsToolStripMenuItem.Click += new System.EventHandler(this.MoreSettingsToolStripMenuItem_Click);
            // 
            // BookmarkSettings
            // 
            this.BookmarkSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ImportButton,
            this.ExportButton,
            this.BookmarkSettingsButton});
            this.BookmarkSettings.Name = "BookmarkSettings";
            this.BookmarkSettings.Size = new System.Drawing.Size(117, 29);
            this.BookmarkSettings.Text = "Bookmarks";
            this.BookmarkSettings.Click += new System.EventHandler(this.BookmarkSettings_Click);
            // 
            // ImportButton
            // 
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Size = new System.Drawing.Size(263, 34);
            this.ImportButton.Text = "Import Bookmarks";
            this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // ExportButton
            // 
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(263, 34);
            this.ExportButton.Text = "Export Bookmarks";
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // BookmarkSettingsButton
            // 
            this.BookmarkSettingsButton.Name = "BookmarkSettingsButton";
            this.BookmarkSettingsButton.Size = new System.Drawing.Size(263, 34);
            this.BookmarkSettingsButton.Text = "More Settings";
            this.BookmarkSettingsButton.Click += new System.EventHandler(this.BookmarkSettingsButton_Click);
            // 
            // SettingsButton
            // 
            this.SettingsButton.Enabled = false;
            this.SettingsButton.Location = new System.Drawing.Point(69, 23);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(75, 23);
            this.SettingsButton.TabIndex = 5;
            this.SettingsButton.Text = "Settings";
            this.SettingsButton.UseVisualStyleBackColor = true;
            this.SettingsButton.Visible = false;
            this.SettingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1260, 985);
            this.Controls.Add(this.SettingsButton);
            this.Controls.Add(this.ContentPanel);
            this.Controls.Add(this.ContentHeader);
            this.Controls.Add(this.TabsPanel);
            this.Controls.Add(this.MinimiseButton);
            this.Controls.Add(this.MaximiseButton);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.BaseMenuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1260, 985);
            this.Name = "Main";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "SDD Browser";
            this.Load += new System.EventHandler(this.Main_Load);
            this.TabsPanel.ResumeLayout(false);
            this.ContentHeader.ResumeLayout(false);
            this.ContentHeader.PerformLayout();
            this.BaseMenuStrip.ResumeLayout(false);
            this.BaseMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Panel ContentPanel;
        private Panel ContentHeader;
        private TextBox TextURL;
        private Button SearchIcon;
        private Button CloseButton;
        private Button MaximiseButton;
        private Button MinimiseButton;
        private Button ReloadButton;
        private Button ForwardButton;
        private Button BackButton;
        private ToolTip MinimiseToolTip;
        private ToolTip CloseToolTip;
        private Panel BookmarksPanel;
        private Panel TabsPanel;
        private Button newTabBtn;
        private Button AddBookmarkButton;
        private MenuStrip BaseMenuStrip;
        private ToolStripMenuItem Settings;
        private ToolStripMenuItem MoreSettingsToolStripMenuItem;
        private ToolStripMenuItem BookmarkSettings;
        private ToolStripMenuItem ImportButton;
        private ToolStripMenuItem ExportButton;
        private ToolStripMenuItem BookmarkSettingsButton;
        private Button SettingsButton;
    }
}

