using CefSharp;
using CefSharp.DevTools.Audits;
using CefSharp.Web;
using CefSharp.WinForms;
using SDDTabs;
using SDDWebBrowser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel.Background;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace SDDBrowser
{
    internal class ContentPanel
    {
        ChromiumWebBrowser currentPage;
        public static string defaultURL = "www.google.com";
        Tab currentTab;
        Panel Content;
        public Panel Tabs;
        TextBox textURL;
        Button searchIcon;
        Button reloadButton;
        Button forwardButton;
        Button backButton;
        Button addBookmarksButton;
        Panel contentHeader;
        Panel Bookmarks;
        public List<Tab> tabs = new List<Tab>();
        Main owner;
        public string position;
        Button newTabBtn;
        string lastSite;
        int historyStackIndex;
        bool fromHistory;
        List<string> historyStack = new List<string>();
        public Rectangle Bounds;
        public Rectangle LastBounds;
        BookmarkFolder bookmarksFolder;
        string bookmarkFileName;
        int tabStartx = 0;
        int bookmarkStartx = 0;

        public ContentPanel(Main form, string position)
        {
            this.position = position;
            owner = form;
            contentHeader = (Panel)form.Controls.Find("contentHeader", true)[0];
            textURL = (TextBox)contentHeader.Controls.Find("textURL", true)[0];
            searchIcon = (Button)contentHeader.Controls.Find("searchIcon", true)[0];
            reloadButton = (Button)contentHeader.Controls.Find("reloadButton", true)[0];
            forwardButton = (Button)contentHeader.Controls.Find("forwardButton", true)[0];
            backButton = (Button)contentHeader.Controls.Find("backButton", true)[0];
            addBookmarksButton = (Button)contentHeader.Controls.Find("addBookmarkButton", true)[0];
            Tabs = (Panel)form.Controls.Find("Tabs", true)[0];
            Content = (Panel)form.Controls.Find("Content", true)[0];
            Bookmarks = (Panel)form.Controls.Find("Bookmarks", true)[0];
            newTabBtn = owner.getNewTabButton();

            addEvents();

            Bounds = new Rectangle(contentHeader.Left, contentHeader.Top, contentHeader.Width, contentHeader.Height + Tabs.Height + Content.Height);
            LastBounds = Bounds;
            updateNavButtons();
            bookmarkFileName = $"Bookmarks_{position}";
        }

        private void addEvents()
        {
            reloadButton.Click += new EventHandler(reloadButton_Click);
            forwardButton.Click += new EventHandler(this.forwardButton_Click);
            backButton.Click += new EventHandler(this.backButton_Click);
            textURL.TextChanged += new EventHandler(this.textURL_TextChanged);
            textURL.KeyDown += new KeyEventHandler(this.textURL_KeyDown);
            addBookmarksButton.Click += new EventHandler(this.addBookmarksButton_Click);
            forwardButton.Paint += new PaintEventHandler(this.forwardButton_Paint);
            backButton.Paint += new PaintEventHandler(this.backButton_Paint);
            owner.Load += Owner_Load;
            Tabs.MouseWheel += Tabs_MouseWheel;
            Bookmarks.MouseWheel += Bookmarks_MouseWheel;
        }

        private void removeEvents()
        {
            reloadButton.Click -= reloadButton_Click;
            forwardButton.Click -= forwardButton_Click;
            backButton.Click -= backButton_Click;
            textURL.TextChanged -= textURL_TextChanged;
            textURL.KeyDown -= textURL_KeyDown;
            addBookmarksButton.Click -= addBookmarksButton_Click;
            forwardButton.Paint -= forwardButton_Paint;
            backButton.Paint -= backButton_Paint;
            owner.Load -= Owner_Load;
            Tabs.MouseWheel -= Tabs_MouseWheel;
            Bookmarks.MouseWheel -= Bookmarks_MouseWheel;
        }

        private void Tabs_MouseWheel(object sender, MouseEventArgs e)
        {
            
            int totalLength = tabs.Sum(t => t.GetButton().Width) + newTabBtn.Width;
            int newStart = tabStartx + e.Delta;
            if (newStart <= 0 && (tabStartx + totalLength > Tabs.Width || e.Delta > 0))
            {
                tabStartx = newStart;
            }
            updateTabs();
        }

        private void Bookmarks_MouseWheel(object sender, MouseEventArgs e)
        {

            int totalLength = bookmarksFolder.bookmarks.Sum(b => b.button.Width);
            int newStart = bookmarkStartx + e.Delta;
            if (newStart <= 0 && (bookmarkStartx + totalLength > Bookmarks.Width || e.Delta > 0))
            {
                bookmarkStartx = newStart;
            }
            updateBookmarks();
        }

        private void Tabs_MouseHover(object sender, EventArgs e)
        {
            if (Control.MouseButtons == MouseButtons.Middle)
            {
                throw new NotImplementedException();
            }
            Debug.WriteLineIf(Control.MouseButtons != MouseButtons.None, Control.MouseButtons);
        }

        private void Tabs_Scroll(object sender, ScrollEventArgs e)
        {
            Debug.WriteLine(Tabs.HorizontalScroll.Value);
        }

        

        private void Owner_Load(object sender, EventArgs e)
        {
            if (File.Exists(bookmarkFileName))
            {
                uploadBookmarks();
            }
            else
            {
                bookmarksFolder = new BookmarkFolder(position);
                downloadBookmarks();
            }
        }

        public void generateAllPanels(Rectangle area)
        {
            removeEvents();

            generateNewTabBtn();
            generateNewBackButton();
            generateNewForwardButton();
            generateNewReloadButton();
            generateNewSearchIcon();
            generateNewAddBookmarksButton();
            generateNewBookmarks();
            generateNewTextURL();
            generateNewTabsPanel(area);
            tabs = new List<Tab>();
            generateNewContentHeader(area);
            generateNewContentPanel(area);

            if (File.Exists(bookmarkFileName))
            {
                uploadBookmarks();
            }
            else
            {
                bookmarksFolder = new BookmarkFolder(position);
                downloadBookmarks();
            }

            addEvents();
            updateTabs();
            Bounds = area;
        }

        public void setSizeAndPosition(Rectangle area)
        {
            Tabs.Height = Tabs.Height;
            Tabs.Width = area.Width;
            Tabs.Location = new Point(area.X, area.Y + contentHeader.Height);
            Content.Width = area.Width;
            Content.Height = area.Height - Tabs.Height - contentHeader.Height;
            Content.Location = new Point(area.X, area.Y + Tabs.Height + contentHeader.Height);
            contentHeader.Width = area.Width;
            contentHeader.Height = contentHeader.Height;
            contentHeader.Location = new Point(area.X, area.Y);
            Bounds = area;
            updateTabs();
            updateNavButtons();
        }

        public void ResizeControls(Size size)
        {
            owner.ResizeWidthControl(Content, size);
            owner.ResizeHeightControl(Content, size);
            owner.ResizeWidthControl(contentHeader, size);
            owner.ResizeWidthControl(textURL, size);
            owner.ResizeWidthControl(Tabs, size);
        }

        public void generateNewTabsPanel(Rectangle area)
        {
            Tabs = new Panel
            {
                Height = Tabs.Height,
                Width = area.Width,
                Location = new Point(area.X, area.Y),
                Name = position,
                BorderStyle = BorderStyle.FixedSingle,
            };
            Tabs.HorizontalScroll.Enabled = true;
            Tabs.BringToFront();
            owner.addControl(Tabs);
        }

        public void generateNewContentPanel(Rectangle area)
        {
            Content = new Panel
            {
                Width = area.Width,
                Height = area.Height - Tabs.Height,
                Location = new Point(area.X, area.Y + Tabs.Height),
                BorderStyle = BorderStyle.FixedSingle,
            };
            Content.BringToFront();
            owner.addControl(Content);
        }

        public void generateNewContentHeader(Rectangle area)
        {
            contentHeader = new Panel
            {
                Location = new Point(area.X, area.Y),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Default,
                Name = "contentHeader",
                Size = contentHeader.Size,
                TabIndex = 0
            };
            int startx = 0;
            int y = 0;
            contentHeader.Controls.Add(backButton);
            backButton.Location = new Point(startx, y);
            startx += backButton.Width;
            contentHeader.Controls.Add(forwardButton);
            forwardButton.Location = new Point(startx, y);
            startx += forwardButton.Width;
            contentHeader.Controls.Add(reloadButton);
            reloadButton.Location = new Point(startx, y);
            startx += reloadButton.Width;
            contentHeader.Controls.Add(searchIcon);
            searchIcon.Location = new Point(startx, y);
            startx += searchIcon.Width;
            contentHeader.Controls.Add(textURL);
            textURL.Location = new Point(startx, y);
            startx += textURL.Width;
            contentHeader.Controls.Add(Bookmarks);
            contentHeader.Controls.Add(addBookmarksButton);
            addBookmarksButton.BringToFront();
            contentHeader.BringToFront();
            owner.addControl(contentHeader);
        }

        public void generateNewTabBtn()
        {
            newTabBtn = new Button()
            {
                Location = new Point(3, 7),
                Name = "newTabBtn",
                Size = newTabBtn.Size,
                TabIndex = 0,
                Text = "+",
                UseVisualStyleBackColor = true,
                BackColor = newTabBtn.BackColor
            };
            newTabBtn.Click += new EventHandler(newTabBtn_Click);
        }

        public void generateNewTextURL()
        {
            textURL = new TextBox()
            {
                BorderStyle = BorderStyle.None,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                Location = new System.Drawing.Point(200, 0),
                Name = "textURL",
                Size = new System.Drawing.Size(940, 32),
                TabIndex = 0,
                Text = "www.google.com",
                BackColor = textURL.BackColor,
                ForeColor = textURL.ForeColor,
            };
        }

        public void generateNewSearchIcon()
        {
            searchIcon = new Button() 
            {
                
                FlatStyle = FlatStyle.Flat,
                Font = searchIcon.Font,
                Location = searchIcon.Location,
                Name = "searchIcon",
                Size = searchIcon.Size,
                TabIndex = 1,
                Text = "G",
                UseVisualStyleBackColor = true,
            };
            this.searchIcon.FlatAppearance.BorderSize = 0;

        }

        public void generateNewAddBookmarksButton()
        {
            addBookmarksButton = new Button()
            {

                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                Font = addBookmarksButton.Font,
                Location = addBookmarksButton.Location,
                Name = "addBookmarksButton",
                Size = addBookmarksButton.Size,
                TabIndex = 1,
                Text = "❤️",
                UseVisualStyleBackColor = true,
                Anchor = AnchorStyles.Right,
            };
            this.addBookmarksButton.FlatAppearance.BorderSize = 0;

        }

        public void generateNewReloadButton()
        {
            reloadButton = new Button() 
            {
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                Font = reloadButton.Font,
                Location = new System.Drawing.Point(100, 0),
                Name = "reloadButton",
                Size = reloadButton.Size,
                TabIndex = 4,
                Text = "O",
                UseVisualStyleBackColor = true,
            };
            this.reloadButton.FlatAppearance.BorderSize = 0;
            
        }

        public void generateNewForwardButton()
        {
            forwardButton = new Button()
            {

                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                Font = forwardButton.Font,
                Location = new System.Drawing.Point(50, 0),
                Name = "forwardButton",
                Size = forwardButton.Size,
                TabIndex = 3,
                UseVisualStyleBackColor = true,

            };
            forwardButton.FlatAppearance.BorderSize = 0;
            forwardButton.Paint += new PaintEventHandler(this.forwardButton_Paint);
            

        }

        public void generateNewBackButton()
        {
            backButton = new Button() 
            {
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                Font = backButton.Font,
                Location = new System.Drawing.Point(0, 0),
                Name = "backButton",
                Size = backButton.Size,
                TabIndex = 2,
                UseVisualStyleBackColor = true,
            };
            this.backButton.FlatAppearance.BorderSize = 0;
            this.backButton.Paint += new System.Windows.Forms.PaintEventHandler(this.backButton_Paint);
        }

        public void generateNewBookmarks()
        {
            Bookmarks = new Panel()
            {
                Location = Bookmarks.Location,
                Margin = Bookmarks.Margin,
                Name = "Bookmarks",
                Size = Bookmarks.Size,
                TabIndex = 5,
            };
        }

        public void generateNewTab(string Url)
        {
            currentPage = new ChromiumWebBrowser();
            currentPage.LoadUrl(Url);
            //Content.Controls.Clear();
            Content.Controls.Add(currentPage);
            currentPage.BringToFront();
            currentPage.Dock = DockStyle.Fill;
            currentPage.AddressChanged += new EventHandler<AddressChangedEventArgs>(browser_AddressChanged);
            currentPage.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(browser_FrameLoadEnd);
            currentPage.FrameLoadStart += new EventHandler<FrameLoadStartEventArgs>(browser_FrameLoadStart);
            currentPage.TitleChanged += delegate (object titleSender, TitleChangedEventArgs titleArgs)
            {
                currentPage.Name = titleArgs.Title;
            };

            currentTab = addTab(currentPage);
            transferTabEventsTo(this, currentTab);
            changeTabs(currentTab);

        }

        public void SetTabs(List<Tab> newTabList, ContentPanel fromPanel)
        {
            tabs = newTabList;
            updateTabs();
            fromPanel.transferTabsEventsTo(this, tabs);
            Content.Controls.Clear();
            foreach (Tab tab in tabs)
            {
                Content.Controls.Add(tab.GetBrowser());
            }
            changeTabs(tabs[tabs.Count - 1]);
        }

        public void ExtendTabs(List<Tab> newTabList)
        {
            tabs.AddRange(newTabList);
            
            foreach (Tab tab in newTabList)
            {
                Content.Controls.Add(tab.GetBrowser());
            }
            if (tabs.Count > 0)
            {
                changeTabs(tabs[tabs.Count - 1]);
            }
            Console.WriteLine("tab count:");
            Console.WriteLine(tabs.Count);
            updateTabs();
        }

        public void TabsButtonMouseUp(object sender, MouseEventArgs e)
        {
            Tab tab = getTabsButton((Button)sender);
            tab.isMouseDown = false;
            if (!tab.isDragging)
            {
                changeTabs(tab);
            }
            tab.isDragging = false;
        }

        public void TabsButtonMouseDown(object sender, MouseEventArgs e)
        {
            Tab tab = getTabsButton((Button)sender);
            if (tab != null)
            {
                tab.isMouseDown = true;
            }
            
        }

        private void changeTabs(Tab tab)
        {
            currentPage = tab.GetBrowser();
            currentTab = tab;
            textURL.Text = tab.GetBrowser().Address;
            tab.GetBrowser().BringToFront();
            historyStack = tab.getHistory();
            historyStackIndex = tab.getHistoryIndex();
            updateNavButtons();
        }

        public void newTabBtn_Click(object sender, EventArgs e)
        {
            generateNewTab(defaultURL);
        }

        public void closeTab(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            bool checkButton(Tab t)
            {
                return (t.GetCloseButton() == btn);
            }
            Tab tab = tabs.Find(checkButton);
            tabs.Remove(tab);
            Content.Controls.Remove(tab.GetBrowser());
            updateTabs();
            if (currentTab == tab)
            {
                foreach (Control browser in Content.Controls)
                {
                    if (Content.Controls.GetChildIndex(browser) == 0)
                    {
                        currentPage = (ChromiumWebBrowser)browser;
                        bool checkBrowser(Tab testTab)
                        {
                            return (testTab.GetBrowser() == currentPage);
                        }
                        Tab currentTab = tabs.Find(checkBrowser);
                        if (currentTab != null)
                        {
                            changeTabs(currentTab);
                        }
                    }
                }
            }
        }

        delegate void VoidCallback();
        private void removeBtnControl(Button button)
        {
            if (owner.isDead || Tabs.IsDisposed || button.IsDisposed) return;
            if (Tabs.InvokeRequired || button.InvokeRequired)
            {
                var d = new SetButtonCallback(removeBtnControl);
                Tabs.Invoke(d, new object[] { button });
            }
            else
            {
                Tabs.Controls.Remove(button);
            }
        }

        private void removeBookmarkControl(Button button)
        {
            if (owner.isDead || Bookmarks.IsDisposed || button.IsDisposed) return;
            if (Bookmarks.InvokeRequired || button.InvokeRequired)
            {
                var d = new SetButtonCallback(removeBtnControl);
                Bookmarks.Invoke(d, new object[] { button });
            }
            else
            {
                Bookmarks.Controls.Remove(button);
            }
        }

        private void updateTabs()
        {
            if (owner.isDead) return;
            //Debug.WriteLine("Tabs: {0}, Controls:{1}", tabs.Count, Tabs.Controls.Count);
            tabs = tabs.Distinct().ToList();

            int currentXPos = tabStartx;
            IEnumerable<Button> buttons = tabs.Select(tab => tab.GetButton());
            Control[] controls = new Control[Tabs.Controls.Count];
            Tabs.Controls.CopyTo(controls, 0);
            foreach (Control control in controls)
            {
                if (control != newTabBtn && !buttons.Contains(control))
                {
                    removeBtnControl((Button)control);

                }
            }

            foreach (Tab tab in tabs)
            {
                Button btn = tab.GetButton();
                changeBtnLocation(btn, new Point(currentXPos, 0));
                if (!Tabs.Controls.Contains(btn))
                {
                    addBtnControl(btn);
                }
                currentXPos += btn.Width;
            }

            //Debug.WriteLine("Tabs: {0}, Controls:{1}", tabs.Count, Tabs.Controls.Count);
            if (!Tabs.Controls.Contains(newTabBtn))
            {
                addBtnControl(newTabBtn);
            }
            setNewTabButtonLocation(new Point(currentXPos, newTabBtn.Location.Y));
        }


        private void updateBookmarks()
        {
            if (owner.isDead) return;

            int currentXPos = 0;
            IEnumerable<Button> buttons = bookmarksFolder.bookmarks.Select(tab => tab.button);
            Control[] controls = new Control[Bookmarks.Controls.Count];
            Bookmarks.Controls.CopyTo(controls, 0);
            foreach (Control control in controls)
            {
                if (!buttons.Contains(control))
                {
                    removeBookmarkControl((Button)control);

                }
            }

            foreach (Bookmark bookmark in bookmarksFolder.bookmarks)
            {
                Button btn = bookmark.button;
                changeBtnLocation(btn, new Point(currentXPos, 0));
                if (!Bookmarks.Controls.Contains(btn))
                {
                    addBookmarkControl(btn);
                }
                currentXPos += btn.Width;
            }

        }


        delegate void SetButtonPointCallback(Button button, Point point);
        private void changeBtnLocation(Button button, Point point)
        {
            if (button.InvokeRequired)
            {
                var d = new SetButtonPointCallback(changeBtnLocation);
                button.Invoke(d, new object[] { button, point });
            }
            else
            {
                button.Location = point;
            }
        }

        delegate void SetButtonCallback(Button button);
        private void addBtnControl(Button button)
        {
            if (owner.isDead || Tabs.IsDisposed || button.IsDisposed) return;
            if (Tabs.InvokeRequired || button.InvokeRequired)
            {
                var d = new SetButtonCallback(addBtnControl);
                if (owner.IsHandleCreated)
                {
                    Tabs.Invoke(d, new object[] { button });
                }
                else
                {
                    Action invokeTabs = () => Tabs.Invoke(d, new object[] { button });
                    owner.needsHandle.Add(invokeTabs);
                }
            }
            else
            {
                Tabs.Controls.Add(button);
            }
        }

        private void addBookmarkControl(Button button)
        {
            if (owner.isDead || Bookmarks.IsDisposed || button.IsDisposed) return;
            if (Bookmarks.InvokeRequired || button.InvokeRequired)
            {
                var d = new SetButtonCallback(addBtnControl);
                if (owner.IsHandleCreated)
                {
                    Bookmarks.Invoke(d, new object[] { button });
                }
                else
                {
                    Action invokeTabs = () => Bookmarks.Invoke(d, new object[] { button });
                    owner.needsHandle.Add(invokeTabs);
                }
            }
            else
            {
                Bookmarks.Controls.Add(button);
            }
        }

        delegate void SetTabsCallback(List<Tab> tabs);


        delegate void SetPointCallback(Point point);
        private void setNewTabButtonLocation(Point point)
        {
            if (owner.isDead) { return; }
            if (newTabBtn.InvokeRequired)
            {
                var d = new SetPointCallback(setNewTabButtonLocation);
                newTabBtn.Invoke(d, new object[] { point });
            }
            else
            {
                newTabBtn.Location = point;
            }
        }

        delegate void SetVoidCallback();
        private void clearTabs()
        {
            if (Tabs.InvokeRequired)
            {
                var d = new SetVoidCallback(clearTabs);
                Tabs.Invoke(d, new object[] { });
            }
            else
            {
                Tabs.Controls.Clear();
            }
        }

        private Tab addTab(ChromiumWebBrowser tab)
        {
            Tab newTab = new Tab(tab, updateTabs);

            Button btn = newTab.GetButton();

            btn.ForeColor = owner.foreColor;
            btn.BackColor = owner.backColor;

            Tabs.Controls.Add(btn);
            tabs.Add(newTab);
            updateTabs();
            return newTab;
        }

        public void transferTabsEventsTo(ContentPanel contentPanel, List<Tab> tabs)
        { 
            foreach (Tab tab in tabs)
            {
                transferTabEventsTo(contentPanel, tab);
            }
        }

            public void transferTabEventsTo(ContentPanel contentPanel, Tab tab)
        {
            Button btn = tab.GetButton();
            RemoveEvent(btn, "EventMouseDown");
            RemoveEvent(btn, "EventMouseUp");
            RemoveEvent(btn, "EventMouseMove");


            btn.MouseDown += new MouseEventHandler(contentPanel.TabsButtonMouseDown);
            btn.MouseUp += new MouseEventHandler(contentPanel.TabsButtonMouseUp);
            btn.MouseMove += new MouseEventHandler(contentPanel.owner.TabsButtonMouseMove);

            btn.ForeColor = owner.foreColor;
            btn.BackColor = owner.backColor;

            Button closeBtn = tab.GetCloseButton();
            closeBtn.Click -= closeTab;
            closeBtn.Click += new EventHandler(contentPanel.closeTab);
        }

        // Remove all event handlers from the control's named event.
        private void RemoveEvent(Control ctl, string event_name)
        {
            FieldInfo field_info = typeof(Control).GetField(event_name,
                BindingFlags.Static | BindingFlags.NonPublic);

            PropertyInfo property_info = ctl.GetType().GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);

            object obj = field_info.GetValue(ctl);
            EventHandlerList event_handlers =
                (EventHandlerList)property_info.GetValue(ctl, null);
            event_handlers.RemoveHandler(obj, event_handlers[obj]);
        }

        public Tab getTabsButton(Button btn)
        {
            bool checkTabButtons(Tab testTab)
            {
                return (testTab.GetButton() == btn);
            }
            Tab tab = tabs.Find(checkTabButtons);
            return tab;
        }

        // Browser - to purge
        private void OnBrowserLoadError(object sender, LoadErrorEventArgs e) // https://github.com/cefsharp/CefSharp.MinimalExample/blob/master/CefSharp.MinimalExample.WinForms/BrowserForm.cs#L32
        {
            //Actions that trigger a download will raise an aborted error.
            //Aborted is generally safe to ignore
            if (e.ErrorCode == CefErrorCode.Aborted)
            {
                return;
            }

            var errorHtml = string.Format("<html><body><h2>Failed to load URL {0} with error {1} ({2}).</h2></body></html>",
                                              e.FailedUrl, e.ErrorText, e.ErrorCode);

            _ = e.Browser.SetMainFrameDocumentContentAsync(errorHtml);

            //AddressChanged isn't called for failed Urls so we need to manually update the Url TextBox
            textURL.Text = e.FailedUrl;
        }

        private void browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            if (owner.isDead) { return; }
            if (sender == currentPage)
            {
                owner.setTextURL(e.Address);
            }
            if (historyStack.Count == 0 || e.Address != lastSite)
            {
                if (!fromHistory)
                {
                    if (historyStackIndex < historyStack.Count)
                    {
                        historyStack.RemoveRange(historyStackIndex - 1, historyStack.Count - historyStackIndex);
                    }
                    if (historyStack.Count == historyStackIndex)
                    {
                        historyStack.Add(e.Address);
                        historyStackIndex++;
                        currentTab.setHistoryIndex(historyStackIndex);
                    }
                }
                fromHistory = false;
                updateNavButtons();

            }
            lastSite = e.Address;
        }

        private void browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {

        }

        private void browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {

        }

        // navigation

        public void updateNavButtons()
        {
            if (owner.isDead) return;
            setBackButtonEnabled(historyStackIndex > 1);
            setForwardButtonEnabled(historyStackIndex < historyStack.Count);
            if (backButton.Enabled)
            {
                setBackButtonForeColor(owner.foreColor);
            }
            else
            {
                setBackButtonForeColor(Color.Gray);
            }

            if (forwardButton.Enabled)
            {
                setForwardButtonForeColor(owner.foreColor);
            }
            else
            {
                setForwardButtonForeColor(Color.Gray);
            }
        }

        delegate void SetBoolCallback(bool enabled);
        private void setBackButtonEnabled(bool enabled)
        {
            if (owner.isDead) return;
            if (backButton.InvokeRequired && !backButton.IsDisposed)
            {
                var d = new SetBoolCallback(setBackButtonEnabled);
                try
                {
                    owner.Invoke(d, new object[] { enabled });
                }
                catch (ObjectDisposedException e)
                {

                }
            }
            else
            {
                backButton.Enabled = enabled;
            }
        }

        private void setForwardButtonEnabled(bool enabled)
        {
            if (owner.isDead) return;
            if (forwardButton.InvokeRequired)
            {
                var d = new SetBoolCallback(setForwardButtonEnabled);
                forwardButton.Invoke(d, new object[] { enabled });
            }
            else
            {
                forwardButton.Enabled = enabled;
            }
        }

        delegate void SetColorCallback(Color color);

        private void setBackButtonForeColor(Color color)
        {
            if (owner.isDead) return;
            if (backButton.InvokeRequired)
            {
                var d = new SetColorCallback(setBackButtonForeColor);
                backButton.Invoke(d, new object[] { color });
            }
            else
            {
                backButton.ForeColor = color;
                backButton.UseVisualStyleBackColor = true;
            }
        }

        private void setForwardButtonForeColor(Color color)
        {
            if (owner.isDead) return;
            if (forwardButton.InvokeRequired)
            {
                var d = new SetColorCallback(setForwardButtonForeColor);
                owner.Invoke(d, new object[] { color });
            }
            else
            {
                forwardButton.ForeColor = color;
                forwardButton.UseVisualStyleBackColor = true;
            }
        }

        private void textURL_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                enterText();
            }
        }

        private void enterText()
        {
            if (CheckUrl(textURL.Text))
            {
                Console.WriteLine("loading \"" + textURL.Text + "\"");
                currentPage.LoadUrl(textURL.Text);
            }
            else
            {
                // NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                // queryString.Add("q", textURL.Text);  // https://stackoverflow.com/questions/829080/how-to-build-a-query-string-for-a-url-in-c
                string queryString = Uri.EscapeDataString(textURL.Text); // https://stackoverflow.com/questions/14574695/converting-a-string-to-a-url-safe-string-for-twilio
                currentPage.LoadUrl("https://www.google.com/search?q=" + queryString);
            }
        }

        private void textURL_TextChanged(object sender, EventArgs e)
        {
            if (CheckUrl(textURL.Text))
            {
                searchIcon.Text = "🌐";
            }
            else
            {
                searchIcon.Text = "G";
            }
        }

        protected bool CheckUrl(string url)
        {
            string urlWithProtocol = "http://" + url;
            bool result = Uri.IsWellFormedUriString(url.ToString(), UriKind.Absolute)
                || (Uri.IsWellFormedUriString(urlWithProtocol, UriKind.Absolute)
                && (url.EndsWith("/")
                || (owner.domains.Any(x => url.Contains(x))
                && char.IsLetterOrDigit(url[0]))
                || Regex.IsMatch(url, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")))
                || Directory.Exists(url);
            return result;
        }

        //buttons
        private void backButton_Click(object sender, EventArgs e)
        {
            if (historyStackIndex > 1)
            {
                historyStackIndex--;
                currentTab.setHistoryIndex(historyStackIndex);
                fromHistory = true;
                currentPage.LoadUrl(historyStack[historyStackIndex - 1]);
            }
        }

        private void backButton_Hover(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.Gray;
        }

        private void backButton_Leave(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = owner.backColor;
        }


        private void forwardButton_Click(object sender, EventArgs e)
        {
            if (historyStackIndex < historyStack.Count())
            {
                historyStackIndex++;
                currentTab.setHistoryIndex(historyStackIndex);
                fromHistory = true;
                currentPage.LoadUrl(historyStack[historyStackIndex - 1]);
            }
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            currentPage.Reload();
        }

        private void forwardButton_Paint(object sender, PaintEventArgs e)
        {
            //https://stackoverflow.com/questions/72644619/how-to-change-the-forecolor-of-a-disabled-button
            Button btn = (Button)sender;
            var solidBrush = new SolidBrush(btn.ForeColor);
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.DrawString(">", btn.Font, solidBrush, e.ClipRectangle, stringFormat);
            solidBrush.Dispose();
            stringFormat.Dispose();
        }

        private void backButton_Paint(object sender, PaintEventArgs e)
        {
            //https://stackoverflow.com/questions/72644619/how-to-change-the-forecolor-of-a-disabled-button
            Button btn = (Button)sender;
            var solidBrush = new SolidBrush(btn.ForeColor);
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.DrawString("<", btn.Font, solidBrush, e.ClipRectangle, stringFormat);
            solidBrush.Dispose();
            stringFormat.Dispose();
        }

        private void addBookmarksButton_Click(object sender, EventArgs e)
        {
            string title = currentTab.GetButton().Text;
            string url = currentPage.Address;
            if (!bookmarksFolder.bookmarks.Select(b => b.url).Contains(url))
            {
                addBookmark(url, title);
                downloadBookmarks();

            }
            else
            {

            }
            
        }

        public void addBookmark(string url, string title)
        {
            Bookmark bookmark = new Bookmark(url, title, this);
            bookmarksFolder.bookmarks.Add(bookmark);
            updateBookmarks();
            addBookmarksButton.BringToFront();
        }

        public void removeBookmark(string url)
        {
            Bookmark bookmark = bookmarksFolder.Find(b => b.url == url)[0];
            bookmarksFolder.removeBookmark(bookmark);
        }

        public void downloadBookmarks()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(bookmarksFolder.getJSON(), options);
            File.WriteAllText(bookmarkFileName, jsonString);
        }

        public void uploadBookmarks()
        {
            BookmarkFolderJson json = JsonSerializer.Deserialize<BookmarkFolderJson>(File.ReadAllText(bookmarkFileName));
            bookmarksFolder = new BookmarkFolder(json, this);
            updateBookmarks();
        }

        public void exportBookmarks()
        {
            string html = $@"<html>
                            <head>
                                <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
                                <title>Bookmarks</title>
                            </head>
                            <body>
                                <h1>Bookmarks</h1>
                                <dl>
                                    <p>
                                    </p>
                                    <dt>
                                        <h3>Bookmarks Bar</h3>
                                        <dl>
                                            <p>
                                            </p>
                                            {bookmarksFolder.toHTML()}
                                        </d1><p>
                                        </p>
                                    </dt>
                                    <dt>
                                        
                                    </dt> 
                                </dl><p>
                                </p>
                            </body>
                            </html>";
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            fileDialog.RestoreDirectory = true;
            fileDialog.Title = "Save As";
            fileDialog.DefaultExt = "html";
            fileDialog.Filter = "HTML Document (*.html)|*.html";
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(fileDialog.FileName, html);
            }
            
        }

        public void importBookmarks()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            fileDialog.RestoreDirectory = true;
            fileDialog.Title = "Open";
            fileDialog.DefaultExt = "html";
            fileDialog.Filter = "HTML Document (*.html)|*.html";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string html = File.ReadAllText(fileDialog.FileName);
                List<string> BookmarkFolders = getHTMLTagContent("dl", html);
                BookmarkFolders.RemoveAt(0);
                for (int i = 0; i < BookmarkFolders.Count; i++)
                {
                    string BookmarkFolder = BookmarkFolders[i];
                    if (BookmarkFolder.Trim().EndsWith(">Bookmarks bar</h3>"))
                    {
                        int dlOffset = 1;
                        string folder = BookmarkFolders[i];
                        bookmarksFolder.addHTML(folder, this, dlOffset);
                        dlOffset -= Regex.Matches(folder, "</DL>").Count;
                        while (dlOffset > 0)
                        {
                            i++;
                            dlOffset++;
                            folder = BookmarkFolders[i];
                            bookmarksFolder.addHTML(folder, this, dlOffset);
                            dlOffset -= Regex.Matches(folder, "</DL>").Count;
                        }
                    }
                }
            }
        }


        public static string getStringBetween(string s1, string s2, string mainString)
        {
            //https://stackoverflow.com/questions/17252615/get-string-between-two-strings-in-a-string
            int pFrom = mainString.IndexOf(s1) + s1.Length;
            int pTo = mainString.Substring(pFrom, mainString.Length - pFrom).IndexOf(s2);

            String result = mainString.Substring(pFrom, pTo - pFrom);
            return result;
        }

        public static List<string> getHTMLTagContent(string tag, string html)
        {
            List<string> sections = html.ToUpper().Split(new string[] { $"<{tag.ToUpper()}>" }, StringSplitOptions.None).ToList();
            
            return sections;
        }
        

        internal void bookmark_OnClick(object sender, EventArgs e)
        {
            Bookmark bookmark = bookmarksFolder.bookmarks.Where(b => b.button == sender).FirstOrDefault();
            generateNewTab(bookmark.url);

        }


        internal void bookmark_Close(object sender, EventArgs e)
        {
            Bookmark bookmark = bookmarksFolder.bookmarks.Where(b => b.closeButton == sender).FirstOrDefault();
            bookmarksFolder.removeBookmark(bookmark);
            bookmark.close();
            updateBookmarks();

        }

        public Color getBackColor()
        {
            return owner.backColor;
        }

        public Color getForeColor()
        {
            return owner.foreColor;
        }

        public Color getAccentColor()
        {
            return owner.accentColor;
        }

    }


}
