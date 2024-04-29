using CefSharp;
using CefSharp.WinForms;
using SDDTabs;
using SDDWebBrowser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SDDBrowser
{
    internal class ContentPanel
    {
        /*
        This class is responsible for containing all the elements, data and methods 
        that are required for each content panel, that is the container of each window
        within the split screen.
         */
        ChromiumWebBrowser currentPage;
        public static string defaultURL = "www.google.com";
        public const int TabGap = 5;
        Tab currentTab;
        Panel Content;
        public Panel TabsPanel;
        TextBox textURL;
        Button searchIcon;
        Button reloadButton;
        Button forwardButton;
        Button backButton;
        Button addBookmarksButton;
        Button closeButton;
        Panel contentHeader;
        Panel Bookmarks;
        public List<Tab> tabs = new List<Tab>();
        readonly Main owner;
        public string position;
        Button newTabBtn;
        string lastSite;
        int historyStackIndex;
        bool fromHistory;
        List<string> historyStack = new List<string>();
        public Rectangle Bounds;
        public Rectangle LastBounds;
        BookmarkFolder bookmarksFolder;
        readonly string bookmarkFileName;
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
            TabsPanel = (Panel)form.Controls.Find("TabsPanel", true)[0];
            Content = (Panel)form.Controls.Find("ContentPanel", true)[0];
            Bookmarks = (Panel)form.Controls.Find("BookmarksPanel", true)[0];
            newTabBtn = owner.GetNewTabButton();

            AddEvents();

            Bounds = new Rectangle(contentHeader.Left, contentHeader.Top, contentHeader.Width, contentHeader.Height + TabsPanel.Height + Content.Height);
            LastBounds = Bounds;
            UpdateNavButtons();
            bookmarkFileName = $"Bookmarks_{position}";
        }

        private void AddEvents()
        {
            reloadButton.Click += new EventHandler(ReloadButton_Click);
            forwardButton.Click += new EventHandler(this.ForwardButton_Click);
            backButton.Click += new EventHandler(this.BackButton_Click);
            textURL.TextChanged += new EventHandler(this.TextURL_TextChanged);
            textURL.KeyDown += new KeyEventHandler(this.TextURL_KeyDown);
            addBookmarksButton.Click += new EventHandler(this.AddBookmarksButton_Click);
            forwardButton.Paint += new PaintEventHandler(this.ForwardButton_Paint);
            backButton.Paint += new PaintEventHandler(this.BackButton_Paint);
            owner.Load += Owner_Load;
            TabsPanel.MouseWheel += Tabs_MouseWheel;
            Bookmarks.MouseWheel += Bookmarks_MouseWheel;
        }

        private void RemoveEvents()
        {
            reloadButton.Click -= ReloadButton_Click;
            forwardButton.Click -= ForwardButton_Click;
            backButton.Click -= BackButton_Click;
            textURL.TextChanged -= TextURL_TextChanged;
            textURL.KeyDown -= TextURL_KeyDown;
            addBookmarksButton.Click -= AddBookmarksButton_Click;
            forwardButton.Paint -= ForwardButton_Paint;
            backButton.Paint -= BackButton_Paint;
            owner.Load -= Owner_Load;
            TabsPanel.MouseWheel -= Tabs_MouseWheel;
            Bookmarks.MouseWheel -= Bookmarks_MouseWheel;
        }

        private void Tabs_MouseWheel(object sender, MouseEventArgs e)
        {

            int totalLength = tabs.Sum(t => t.GetButton().Width) + newTabBtn.Width;
            int newStart = tabStartx + e.Delta;
            if (newStart <= 0 && (tabStartx + totalLength > TabsPanel.Width || e.Delta > 0))
            {
                tabStartx = newStart;
            }
            UpdateTabs();
        }

        private void Bookmarks_MouseWheel(object sender, MouseEventArgs e)
        {

            int totalLength = bookmarksFolder.Bookmarks.Sum(b => b.button.Width);
            int newStart = bookmarkStartx + e.Delta;
            if (newStart <= 0 && (bookmarkStartx + totalLength > Bookmarks.Width || e.Delta > 0))
            {
                bookmarkStartx = newStart;
            }
            UpdateBookmarks();
        }



        private void Owner_Load(object sender, EventArgs e)
        {
            try
            {
                UploadBookmarks();
            } 
            catch
            {
                bookmarksFolder = new BookmarkFolder(position);
                DownloadBookmarks();
            }
        }

        public void GenerateAllPanels(Rectangle area)
        {
            RemoveEvents();

            GenerateNewTabBtn();
            GenerateNewBackButton();
            GenerateNewForwardButton();
            GenerateNewReloadButton();
            GenerateNewSearchIcon();
            GenerateNewAddBookmarksButton();
            GenerateNewCloseButton();
            GenerateNewBookmarks();
            GenerateNewTextURL();
            GenerateNewTabsPanel(area);
            tabs = new List<Tab>();
            GenerateNewContentHeader(area);
            GenerateNewContentPanel(area);


            try
            {
                UploadBookmarks();
            }
            catch
            {
                bookmarksFolder = new BookmarkFolder(position);
                DownloadBookmarks();
            }

            AddEvents();
            UpdateTabs();
            Bounds = area;
        }

        public void SetSizeAndPosition(Rectangle area)
        {
            TabsPanel.Height = TabsPanel.Height;
            TabsPanel.Width = area.Width;
            TabsPanel.Location = new Point(area.X, area.Y + contentHeader.Height);
            Content.Width = area.Width;
            Content.Height = area.Height - TabsPanel.Height - contentHeader.Height;
            Content.Location = new Point(area.X, area.Y + TabsPanel.Height + contentHeader.Height);
            contentHeader.Width = area.Width;
            contentHeader.Height = contentHeader.Height;
            contentHeader.Location = new Point(area.X, area.Y);
            textURL.Width = area.Width - 6 * searchIcon.Width;
            Bounds = area;
            UpdateTabs();
            UpdateNavButtons();
        }

        public void ResizeControls(Size size)
        {
            owner.ResizeWidthControl(Content, size);
            owner.ResizeHeightControl(Content, size);
            owner.ResizeWidthControl(contentHeader, size);
            owner.ResizeWidthControl(textURL, size);
            owner.ResizeWidthControl(TabsPanel, size);
        }

        public void GenerateNewTabsPanel(Rectangle area)
        {
            TabsPanel = new Panel
            {
                Height = TabsPanel.Height,
                Width = area.Width,
                Location = new Point(area.X, area.Y),
                Name = position,
                BorderStyle = BorderStyle.FixedSingle,
            };
            TabsPanel.HorizontalScroll.Enabled = true;
            TabsPanel.BringToFront();
            owner.AddControl(TabsPanel);
        }

        public void GenerateNewContentPanel(Rectangle area)
        {
            Content = new Panel
            {
                Width = area.Width,
                Height = area.Height - TabsPanel.Height,
                Location = new Point(area.X, area.Y + TabsPanel.Height),
                BorderStyle = BorderStyle.FixedSingle,
            };
            Content.BringToFront();
            owner.AddControl(Content);
        }

        public void GenerateNewCloseButton()
        {
            closeButton = new Button
            {
                Anchor = AnchorStyles.Right,
                Size = addBookmarksButton.Size,
                Location = new Point(Bounds.Width - 2 * addBookmarksButton.Width, 0),
                Text = "x",
                BackColor = owner.backColor,
                FlatStyle = FlatStyle.Flat,

            };
            closeButton.Click += delegate (object s, EventArgs e) { Close(); };
            closeButton.FlatAppearance.BorderSize = 0;
            Main.AddHoverColorToButton(closeButton);
        }

        public void GenerateNewContentHeader(Rectangle area)
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
            contentHeader.Controls.Add(Bookmarks);
            contentHeader.Controls.Add(addBookmarksButton);
            contentHeader.Controls.Add(closeButton);
            addBookmarksButton.BringToFront();
            closeButton.BringToFront();
            contentHeader.BringToFront();
            owner.AddControl(contentHeader);
        }

        public void GenerateNewTabBtn()
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
            newTabBtn.Click += new EventHandler(NewTabBtn_Click);
            Main.AddHoverColorToButton(newTabBtn);
        }

        public void GenerateNewTextURL()
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

        public void GenerateNewSearchIcon()
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
                BackColor = searchIcon.BackColor
            };
            searchIcon.FlatAppearance.BorderSize = 0;
            Main.AddHoverColorToButton(searchIcon);

        }

        public void GenerateNewAddBookmarksButton()
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
                BackColor = addBookmarksButton.BackColor
            };
            this.addBookmarksButton.FlatAppearance.BorderSize = 0;
            Main.AddHoverColorToButton(addBookmarksButton);

        }

        public void GenerateNewReloadButton()
        {
            reloadButton = new Button()
            {
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                Font = reloadButton.Font,
                Location = new System.Drawing.Point(100, 0),
                Name = "reloadButton",
                Size = reloadButton.Size,
                TabIndex = 4,
                UseVisualStyleBackColor = true,
                BackColor = reloadButton.BackColor
            };
            reloadButton.Paint += owner.ReloadButton_Paint;
            reloadButton.FlatAppearance.BorderSize = 0;
            Main.AddHoverColorToButton(reloadButton);
        }

        public void GenerateNewForwardButton()
        {
            forwardButton = new Button()
            {

                FlatStyle = FlatStyle.Flat,
                Font = forwardButton.Font,
                Location = new System.Drawing.Point(50, 0),
                Name = "forwardButton",
                Size = forwardButton.Size,
                TabIndex = 3,
                UseVisualStyleBackColor = true,
                BackColor = forwardButton.BackColor
            };
            forwardButton.FlatAppearance.BorderSize = 0;
            forwardButton.Paint += new PaintEventHandler(this.ForwardButton_Paint);
            Main.AddHoverColorToButton(forwardButton);
        }

        public void GenerateNewBackButton()
        {
            backButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                Font = backButton.Font,
                Location = new Point(0, 0),
                Name = "backButton",
                Size = backButton.Size,
                TabIndex = 2,
                UseVisualStyleBackColor = true,
                BackColor = backButton.BackColor
            };
            this.backButton.FlatAppearance.BorderSize = 0;
            this.backButton.Paint += new PaintEventHandler(this.BackButton_Paint);
            Main.AddHoverColorToButton(backButton);
        }

        public void GenerateNewBookmarks()
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

        public void GenerateNewTab(string Url)
        {
            currentPage = new ChromiumWebBrowser();
            currentPage.LoadUrl(Url);
            //Content.Controls.Clear();
            Content.Controls.Add(currentPage);
            currentPage.BringToFront();
            currentPage.Dock = DockStyle.Fill;
            currentPage.AddressChanged += new EventHandler<AddressChangedEventArgs>(Browser_AddressChanged);
            currentPage.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(Browser_FrameLoadEnd);
            currentPage.FrameLoadStart += new EventHandler<FrameLoadStartEventArgs>(Browser_FrameLoadStart);
            currentPage.TitleChanged += delegate (object titleSender, TitleChangedEventArgs titleArgs)
            {
                currentPage.Name = titleArgs.Title;
            };

            currentTab = AddTab(currentPage);
            TransferTabEventsTo(this, currentTab);
            ChangeTabs(currentTab);

        }

        public void SetTabs(List<Tab> newTabList, ContentPanel fromPanel)
        {
            tabs = newTabList;
            UpdateTabs();
            fromPanel.TransferTabsEventsTo(this, tabs);
            Content.Controls.Clear();
            foreach (Tab tab in tabs)
            {
                Content.Controls.Add(tab.GetBrowser());
            }
            ChangeTabs(tabs[tabs.Count - 1]);
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
                ChangeTabs(tabs[tabs.Count - 1]);
            }
            Console.WriteLine("tab count:");
            Console.WriteLine(tabs.Count);
            UpdateTabs();
        }

        public void TabsButtonMouseUp(object sender, MouseEventArgs e)
        {
            Tab tab = GetTabsButton((Button)sender);
            tab.isMouseDown = false;
            if (!tab.isDragging)
            {
                ChangeTabs(tab);
            }
            tab.isDragging = false;
        }

        public void TabsButtonMouseDown(object sender, MouseEventArgs e)
        {
            Tab tab = GetTabsButton((Button)sender);
            if (tab != null)
            {
                tab.isMouseDown = true;
                tab.whereMouseDown = Control.MousePosition;
            }

        }

        private void ChangeTabs(Tab tab)
        {
            currentPage = tab.GetBrowser();
            currentTab = tab;
            SetTextURL(tab.GetBrowser().Address);
            tab.GetBrowser().BringToFront();
            historyStack = tab.GetHistory();
            historyStackIndex = tab.GetHistoryIndex();
            UpdateNavButtons();
            UpdateTabs();
        }

        public void NewTabBtn_Click(object sender, EventArgs e)
        {
            GenerateNewTab(defaultURL);
        }

        public void CloseTab(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            bool checkButton(Tab t)
            {
                return (t.GetCloseButton() == btn);
            }
            Tab tab = tabs.Find(checkButton);
            if (tab == null) { return; }
            tabs.Remove(tab);
            Content.Controls.Remove(tab.GetBrowser());
            UpdateTabs();
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
                            ChangeTabs(currentTab);
                        }
                        UpdateTabs();
                    }
                }
                if (currentTab == tab)
                {
                    GenerateNewTab(defaultURL);
                }
            }
        }

        delegate void VoidCallback();
        private void RemoveBtnControl(Button button)
        {
            if (owner.isDead || TabsPanel.IsDisposed || button.IsDisposed) return;
            if (TabsPanel.InvokeRequired || button.InvokeRequired)
            {
                var d = new SetButtonCallback(RemoveBtnControl);
                TabsPanel.Invoke(d, new object[] { button });
            }
            else
            {
                TabsPanel.Controls.Remove(button);
            }
        }

        private void RemoveBookmarkControl(Button button)
        {
            if (owner.isDead || Bookmarks.IsDisposed || button.IsDisposed) return;
            if (Bookmarks.InvokeRequired || button.InvokeRequired)
            {
                var d = new SetButtonCallback(RemoveBtnControl);
                Bookmarks.Invoke(d, new object[] { button });
            }
            else
            {
                Bookmarks.Controls.Remove(button);
            }
        }

        public void UpdateTabs()
        {
            if (owner.isDead) return;
            //Debug.WriteLine("Tabs: {0}, Controls:{1}", tabs.Count, Tabs.Controls.Count);
            tabs = tabs.Distinct().ToList();

            int currentXPos = tabStartx;
            IEnumerable<Button> buttons = tabs.Select(tab => tab.GetButton());
            Control[] controls = new Control[TabsPanel.Controls.Count];
            TabsPanel.Controls.CopyTo(controls, 0);
            foreach (Control control in controls)
            {
                if (control != newTabBtn && !buttons.Contains(control))
                {
                    RemoveBtnControl((Button)control);

                }
            }

            foreach (Tab tab in tabs)
            {
                RoundedButton btn = tab.GetButton();
                ChangeBtnLocation(btn, new Point(currentXPos, TabGap));
                ChangeBtnHeight(btn, TabsPanel.Height - 2 * TabGap);
                if (!TabsPanel.Controls.Contains(btn))
                {
                    AddBtnControl(btn);
                }
                if (tab == currentTab)
                {
                    btn.SetBorderColor(GetAccentColor());
                }
                else
                {
                    btn.SetBorderColor(GetForeColor());
                }
                currentXPos += btn.Width + TabGap;
            }

            //Debug.WriteLine("Tabs: {0}, Controls:{1}", tabs.Count, Tabs.Controls.Count);
            if (!TabsPanel.Controls.Contains(newTabBtn))
            {
                AddBtnControl(newTabBtn);
            }
            SetNewTabButtonLocation(new Point(currentXPos, newTabBtn.Location.Y));
        }


        private void UpdateBookmarks()
        {
            if (owner.isDead) return;

            int currentXPos = 0;
            IEnumerable<Button> buttons = bookmarksFolder.Bookmarks.Select(tab => tab.button);
            Control[] controls = new Control[Bookmarks.Controls.Count];
            Bookmarks.Controls.CopyTo(controls, 0);
            foreach (Control control in controls)
            {
                if (!buttons.Contains(control))
                {
                    RemoveBookmarkControl((Button)control);

                }
            }

            foreach (Bookmark bookmark in bookmarksFolder.Bookmarks)
            {
                Button btn = bookmark.button;
                ChangeBtnLocation(btn, new Point(currentXPos, 0));
                if (!Bookmarks.Controls.Contains(btn))
                {
                    AddBookmarkControl(btn);
                }
                currentXPos += btn.Width;
            }

        }


        delegate void SetButtonPointCallback(Button button, Point point);
        private void ChangeBtnLocation(Button button, Point point)
        {
            if (button.InvokeRequired)
            {
                var d = new SetButtonPointCallback(ChangeBtnLocation);
                button.Invoke(d, new object[] { button, point });
            }
            else
            {
                button.Location = point;
            }
        }

        delegate void SetButtonIntCallback(Button button, int integer);
        private void ChangeBtnHeight(Button button, int height)
        {
            if (button.InvokeRequired)
            {
                var d = new SetButtonIntCallback(ChangeBtnHeight);
                button.Invoke(d, new object[] { button, height });
            }
            else
            {
                button.Height = height;
            }
        }

        delegate void SetButtonCallback(Button button);
        private void AddBtnControl(Button button)
        {
            if (owner.isDead || TabsPanel.IsDisposed || button.IsDisposed) return;
            if (TabsPanel.InvokeRequired || button.InvokeRequired)
            {
                var d = new SetButtonCallback(AddBtnControl);
                if (owner.IsHandleCreated)
                {
                    TabsPanel.Invoke(d, new object[] { button });
                }
                else
                {
                    void invokeTabs() => TabsPanel.Invoke(d, new object[] { button });
                    owner.needsHandle.Add(invokeTabs);
                }
            }
            else
            {
                TabsPanel.Controls.Add(button);
            }
        }

        private void AddBookmarkControl(Button button)
        {
            if (owner.isDead || Bookmarks.IsDisposed || button.IsDisposed) return;
            if (Bookmarks.InvokeRequired || button.InvokeRequired)
            {
                var d = new SetButtonCallback(AddBtnControl);
                if (owner.IsHandleCreated)
                {
                    Bookmarks.Invoke(d, new object[] { button });
                }
                else
                {
                    void invokeTabs() => Bookmarks.Invoke(d, new object[] { button });
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
        private void SetNewTabButtonLocation(Point point)
        {
            if (owner.isDead) { return; }
            if (newTabBtn.InvokeRequired)
            {
                var d = new SetPointCallback(SetNewTabButtonLocation);
                newTabBtn.Invoke(d, new object[] { point });
            }
            else
            {
                newTabBtn.Location = point;
            }
        }

        delegate void SetVoidCallback();
        private void ClearTabs()
        {
            if (TabsPanel.InvokeRequired)
            {
                var d = new SetVoidCallback(ClearTabs);
                TabsPanel.Invoke(d, new object[] { });
            }
            else
            {
                TabsPanel.Controls.Clear();
            }
        }

        private Tab AddTab(ChromiumWebBrowser tab)
        {
            Tab newTab = new Tab(tab, UpdateTabs);

            RoundedButton btn = newTab.GetButton();

            btn.ForeColor = GetForeColor();
            btn.BackColor = GetBackColor();
            btn.SetBorderColor(GetAccentColor());

            TabsPanel.Controls.Add(btn);
            tabs.Add(newTab);
            UpdateTabs();
            return newTab;
        }

        public void TransferTabsEventsTo(ContentPanel contentPanel, List<Tab> tabs)
        {
            foreach (Tab tab in tabs)
            {
                TransferTabEventsTo(contentPanel, tab);
            }
        }

        public void TransferTabEventsTo(ContentPanel contentPanel, Tab tab)
        {
            Button btn = tab.GetButton();
            RemoveEvent(btn, "EventMouseDown");
            RemoveEvent(btn, "EventMouseUp");
            RemoveEvent(btn, "EventMouseMove");
            ChromiumWebBrowser browser = tab.GetBrowser();


            btn.MouseDown += new MouseEventHandler(contentPanel.TabsButtonMouseDown);
            btn.MouseUp += new MouseEventHandler(contentPanel.TabsButtonMouseUp);
            btn.MouseMove += new MouseEventHandler(contentPanel.owner.TabsButtonMouseMove);
            browser.LoadError += new EventHandler<LoadErrorEventArgs>(OnBrowserLoadError);

            btn.ForeColor = owner.foreColor;
            btn.BackColor = owner.backColor;

            Button closeBtn = tab.GetCloseButton();
            closeBtn.Click -= CloseTab;
            closeBtn.Click += new EventHandler(contentPanel.CloseTab);

            tab.SetUpdateTabsFunction(contentPanel.UpdateTabs);
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

        public Tab GetTabsButton(Button btn)
        {
            bool checkTabButtons(Tab testTab)
            {
                return (testTab.GetButton() == btn);
            }
            Tab tab = tabs.Find(checkTabButtons);
            return tab;
        }

        // Browser
        private void OnBrowserLoadError(object sender, LoadErrorEventArgs e) // https://github.com/cefsharp/CefSharp.MinimalExample/blob/master/CefSharp.MinimalExample.WinForms/BrowserForm.cs#L32
        {
            //Actions that trigger a download will raise an aborted error.
            //Aborted is generally safe to ignore
            if (e.ErrorCode == CefErrorCode.Aborted)
            {
                return;
            }

            string error = Properties.Resources.error;
            var errorHtml = string.Format(error, e.FailedUrl, e.ErrorText, e.ErrorCode, Properties.Resources.errorCss);
            e.Browser.SetMainFrameDocumentContentAsync(errorHtml);

            //AddressChanged isn't called for failed Urls so we need to manually update the Url TextBox
            SetTextURL(e.FailedUrl);
        }

        delegate void SetTextCallback(string text);
        public void SetTextURL(string text)
        {
            if (textURL.InvokeRequired)
            {
                var d = new SetTextCallback(SetTextURL);
                textURL.Invoke(d, new object[] { text });
            }
            else
            {
                textURL.Text = text;
            }
        }

        private void Browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            if (owner.isDead) { return; }
            if (sender == currentPage)
            {
                owner.SetTextURL(e.Address);
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
                        currentTab.SetHistoryIndex(historyStackIndex);
                    }
                }
                fromHistory = false;
                UpdateNavButtons();

            }
            lastSite = e.Address;
        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {

        }

        private void Browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {

        }

        // navigation

        public void UpdateNavButtons()
        {
            if (owner.isDead) return;
            const int dilution = 4;

            SetBackButtonEnabled(historyStackIndex > 1);
            SetForwardButtonEnabled(historyStackIndex < historyStack.Count);
            if (backButton.Enabled)
            {
                SetBackButtonForeColor(GetForeColor());
            }
            else
            {
                Color bc = GetBackColor();
                Color fc = GetForeColor();
                Color color = Color.FromArgb(255,
                    (bc.R * dilution + fc.R) / (1 + dilution),
                    (bc.G * dilution + fc.G) / (1 + dilution),
                    (bc.B * dilution + fc.B) / (1 + dilution)
                    );
                SetBackButtonForeColor(color);
            }

            if (forwardButton.Enabled)
            {
                SetForwardButtonForeColor(GetForeColor());
            }
            else
            {
                Color bc = GetBackColor();
                Color fc = GetForeColor();
                Color color = Color.FromArgb(255,
                    (bc.R * dilution + fc.R) / (1 + dilution),
                    (bc.G * dilution + fc.G) / (1 + dilution),
                    (bc.B * dilution + fc.B) / (1 + dilution)
                    );
                SetForwardButtonForeColor(color);
            }
        }

        delegate void SetBoolCallback(bool enabled);
        private void SetBackButtonEnabled(bool enabled)
        {
            if (owner.isDead) return;
            if (backButton.InvokeRequired && !backButton.IsDisposed)
            {
                var d = new SetBoolCallback(SetBackButtonEnabled);
                try
                {
                    owner.Invoke(d, new object[] { enabled });
                }
                catch (ObjectDisposedException)
                {

                }
            }
            else
            {
                backButton.Enabled = enabled;
            }
        }

        private void SetForwardButtonEnabled(bool enabled)
        {
            if (owner.isDead) return;
            if (forwardButton.InvokeRequired)
            {
                var d = new SetBoolCallback(SetForwardButtonEnabled);
                forwardButton.Invoke(d, new object[] { enabled });
            }
            else
            {
                forwardButton.Enabled = enabled;
            }
        }

        delegate void SetColorCallback(Color color);

        private void SetBackButtonForeColor(Color color)
        {
            if (owner.isDead) return;
            if (backButton.InvokeRequired)
            {
                var d = new SetColorCallback(SetBackButtonForeColor);
                backButton.Invoke(d, new object[] { color });
            }
            else
            {
                backButton.ForeColor = color;
                backButton.UseVisualStyleBackColor = true;
            }
        }

        private void SetForwardButtonForeColor(Color color)
        {
            if (owner.isDead) return;
            if (forwardButton.InvokeRequired)
            {
                var d = new SetColorCallback(SetForwardButtonForeColor);
                owner.Invoke(d, new object[] { color });
            }
            else
            {
                forwardButton.ForeColor = color;
                forwardButton.UseVisualStyleBackColor = true;
            }
        }

        private void TextURL_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                EnterText();
            }
        }

        private void EnterText()
        {
            if (CheckUrl(textURL.Text))
            {
                Console.WriteLine("loading \"" + textURL.Text + "\"");
                currentTab.SetButtonText("loading...");
                UpdateTabs();
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

        private void TextURL_TextChanged(object sender, EventArgs e)
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
            // checks if url follows a url format.
            if (url == "")
            {
                return false;
            }
            string urlWithProtocol = "http://" + url;
            string domainPattern = string.Join("|", owner.urlDomains.Select(x => x + "/"));
            bool result =
                Uri.IsWellFormedUriString(url.ToString(), UriKind.Absolute)
                || (url.EndsWith("/") && Regex.IsMatch(url, @"[a-zA-Z.]"))
                || (char.IsLetterOrDigit(url[0]) && Regex.IsMatch(url, domainPattern))
                || owner.urlDomains.Any(x => url.EndsWith(x))
                || Regex.IsMatch(url, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")
                || Directory.Exists(url)
                || url.StartsWith("http://")
                || url.StartsWith("https://");
            return result;
        }

        //buttons
        private void BackButton_Click(object sender, EventArgs e)
        {
            if (historyStackIndex > 1)
            {
                historyStackIndex--;
                currentTab.SetHistoryIndex(historyStackIndex);
                fromHistory = true;
                currentPage.LoadUrl(historyStack[historyStackIndex - 1]);
            }
        }

        private void BackButton_Hover(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.Gray;
        }

        private void BackButton_Leave(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = owner.backColor;
        }


        private void ForwardButton_Click(object sender, EventArgs e)
        {
            if (historyStackIndex < historyStack.Count())
            {
                historyStackIndex++;
                currentTab.SetHistoryIndex(historyStackIndex);
                fromHistory = true;
                currentPage.LoadUrl(historyStack[historyStackIndex - 1]);
            }
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            currentPage.Reload();
        }

        private void ForwardButton_Paint(object sender, PaintEventArgs e)
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

        private void BackButton_Paint(object sender, PaintEventArgs e)
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

        //bookmarks

        private void AddBookmarksButton_Click(object sender, EventArgs e)
        {
            string title = currentTab.GetButton().Text;
            string url = currentPage.Address;
            if (!bookmarksFolder.Bookmarks.Select(b => b.url).Contains(url))
            {
                AddBookmark(url, title);
                DownloadBookmarks();

            }
            else
            {

            }

        }

        public void AddBookmark(string url, string title)
        {
            Bookmark bookmark = new Bookmark(url, title, this);
            bookmarksFolder.Bookmarks.Add(bookmark);
            UpdateBookmarks();
            addBookmarksButton.BringToFront();
        }

        public void RemoveBookmark(string url)
        {
            Bookmark bookmark = bookmarksFolder.Find(b => b.url == url)[0];
            bookmarksFolder.RemoveBookmark(bookmark);
        }

        public void DownloadBookmarks()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(bookmarksFolder.GetJSON(), options);
            File.WriteAllText(bookmarkFileName, jsonString);
        }

        public void UploadBookmarks()
        {
            BookmarkFolderJson json = JsonSerializer.Deserialize<BookmarkFolderJson>(File.ReadAllText(bookmarkFileName));
            bookmarksFolder = new BookmarkFolder(json, this);
            UpdateBookmarks();
        }

        public void ExportBookmarks()
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
                                            {bookmarksFolder.ToHTML()}
                                        </d1><p>
                                        </p>
                                    </dt>
                                    <dt>
                                        
                                    </dt> 
                                </dl><p>
                                </p>
                            </body>
                            </html>";
            SaveFileDialog fileDialog = new SaveFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RestoreDirectory = true,
                Title = "Save As",
                DefaultExt = "html",
                Filter = "HTML Document (*.html)|*.html",
                CheckFileExists = true,
                CheckPathExists = true
            };
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(fileDialog.FileName, html);
            }

        }

        public void ImportBookmarks()
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RestoreDirectory = true,
                Title = "Open",
                DefaultExt = "html",
                Filter = "HTML Document (*.html)|*.html"
            };
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string html = File.ReadAllText(fileDialog.FileName);
                List<string> BookmarkFolders = GetHTMLTagContent("dl", html);
                BookmarkFolders.RemoveAt(0);
                for (int i = 0; i < BookmarkFolders.Count; i++)
                {
                    string BookmarkFolder = BookmarkFolders[i];
                    if (BookmarkFolder.Trim().EndsWith(">Bookmarks bar</h3>"))
                    {
                        int dlOffset = 1;
                        string folder = BookmarkFolders[i];
                        bookmarksFolder.AddHTML(folder, this, dlOffset);
                        dlOffset -= Regex.Matches(folder, "</DL>").Count;
                        while (dlOffset > 0)
                        {
                            i++;
                            dlOffset++;
                            folder = BookmarkFolders[i];
                            bookmarksFolder.AddHTML(folder, this, dlOffset);
                            dlOffset -= Regex.Matches(folder, "</DL>").Count;
                        }
                    }
                }
            }
        }


        public static string GetStringBetween(string s1, string s2, string mainString)
        {
            //https://stackoverflow.com/questions/17252615/get-string-between-two-strings-in-a-string
            int pFrom = mainString.IndexOf(s1) + s1.Length;
            int pTo = mainString.Substring(pFrom, mainString.Length - pFrom).IndexOf(s2);

            String result = mainString.Substring(pFrom, pTo - pFrom);
            return result;
        }

        public static List<string> GetHTMLTagContent(string tag, string html)
        {
            List<string> sections = html.ToUpper().Split(new string[] { $"<{tag.ToUpper()}>" }, StringSplitOptions.None).ToList();

            return sections;
        }


        internal void Bookmark_OnClick(object sender, EventArgs e)
        {
            Bookmark bookmark = bookmarksFolder.Bookmarks.Where(b => b.button == sender).FirstOrDefault();
            GenerateNewTab(bookmark.url);

        }


        internal void Bookmark_Close(object sender, EventArgs e)
        {
            Bookmark bookmark = bookmarksFolder.Bookmarks.Where(b => b.closeButton == sender).FirstOrDefault();
            bookmarksFolder.RemoveBookmark(bookmark);
            bookmark.Close();
            UpdateBookmarks();

        }

        public Color GetBackColor()
        {
            return owner.backColor;
        }

        public Color GetForeColor()
        {
            return owner.foreColor;
        }

        public Color GetAccentColor()
        {
            return owner.accentColor;
        }

        private void Close()
        {
            owner.RemoveContentPanel(this);
            Content.Dispose();
            TabsPanel.Dispose();
            textURL.Dispose();
            searchIcon.Dispose();
            reloadButton.Dispose();
            forwardButton.Dispose();
            backButton.Dispose();
            addBookmarksButton.Dispose();
            closeButton.Dispose();
            contentHeader.Dispose();
            Bookmarks.Dispose();
            foreach (Button button in tabs.Select(t => t.GetButton())) { button.Dispose(); }
        }

    }


}
