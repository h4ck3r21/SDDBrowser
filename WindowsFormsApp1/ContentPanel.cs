using CefSharp;
using CefSharp.WinForms;
using SDDTabs;
using SDDWebBrowser;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        Panel contentHeader;
        public List<Tab> tabs = new List<Tab>();
        Main owner;
        public string position;
        Button newTabBtn;
        string lastSite;
        int historyStackIndex;
        bool fromHistory;
        List<string> historyStack;
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
            Tabs = (Panel)form.Controls.Find("Tabs", true)[0];
            Content = (Panel)form.Controls.Find("Content", true)[0];
            newTabBtn = owner.getNewTabButton();
            reloadButton.Click += new EventHandler(reloadButton_Click);
            forwardButton.Click += new EventHandler(this.forwardButton_Click);
            backButton.Click += new EventHandler(this.backButton_Click);
            textURL.TextChanged += new EventHandler(this.textURL_TextChanged);
            textURL.KeyDown += new KeyEventHandler(this.textURL_KeyDown);
            forwardButton.Paint += new PaintEventHandler(this.forwardButton_Paint);
            backButton.Paint += new PaintEventHandler(this.backButton_Paint);

        }

        public void generateAllPanels(Rectangle area)
        {
            generateNewBackButton();
            generateNewForwardButton();
            generateNewReloadButton();
            generateNewSearchIcon();
            generateNewTextURL();
            generateNewContentHeader(area);
            generateNewTabsPanel(area);
            generateNewContentPanel(area);
        }

        public void setSizeAndPosition(Rectangle area)
        {
            Tabs.Height = Tabs.Height;
            Tabs.Width = area.Width;
            Tabs.Location = new Point(area.X, area.Y);
            Content.Width = area.Width;
            Content.Height = area.Height - Tabs.Height;
            Content.Location = new Point(area.X, area.Y + Tabs.Height);
            contentHeader.Width = area.Width;
            contentHeader.Height = contentHeader.Height;
            contentHeader.Location = new Point(area.X, area.Y);
        }

        public void generateNewTabsPanel(Rectangle area)
        {
            Tabs = new Panel
            {
                Height = Tabs.Height,
                Width = area.Width,
                Location = new Point(area.X, area.Y)
            };
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
            };
            Content.BringToFront();
            owner.addControl(Content);
        }

        public void generateNewContentHeader(Rectangle area)
        {
            contentHeader = new Panel
            {
                Width = area.Width,
                Height = contentHeader.Height,
                Location = new Point(area.X, area.Y),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Default,
                Name = "contentHeader",
                Size = new Size(1140, 100),
                TabIndex = 0
            };
            //this.contentHeader.Controls.Add(owner.Bookmarks);
            contentHeader.Controls.Add(reloadButton);
            contentHeader.Controls.Add(forwardButton);
            contentHeader.Controls.Add(backButton);
            contentHeader.Controls.Add(searchIcon);
            contentHeader.Controls.Add(textURL);
            contentHeader.BringToFront();
            owner.addControl(contentHeader);
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
                Text = "www.google.com"
            };
        }

        public void generateNewSearchIcon()
        {
            searchIcon = new Button();
            this.searchIcon.FlatAppearance.BorderSize = 0;
            this.searchIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.searchIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchIcon.Location = new System.Drawing.Point(150, 0);
            this.searchIcon.Name = "searchIcon";
            this.searchIcon.Size = new System.Drawing.Size(50, 49);
            this.searchIcon.TabIndex = 1;
            this.searchIcon.Text = "G";
            this.searchIcon.UseVisualStyleBackColor = true;

        }

        public void generateNewReloadButton()
        {
            reloadButton = new Button();
            this.reloadButton.FlatAppearance.BorderSize = 0;
            this.reloadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.reloadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reloadButton.Location = new System.Drawing.Point(100, 0);
            this.reloadButton.Name = "reloadButton";
            this.reloadButton.Size = new System.Drawing.Size(50, 49);
            this.reloadButton.TabIndex = 4;
            this.reloadButton.Text = "O";
            this.reloadButton.UseVisualStyleBackColor = true;
            
        }

        public void generateNewForwardButton()
        {
            forwardButton = new Button();
            this.forwardButton.FlatAppearance.BorderSize = 0;
            this.forwardButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.forwardButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.forwardButton.Location = new System.Drawing.Point(50, 0);
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(50, 49);
            this.forwardButton.TabIndex = 3;
            this.forwardButton.UseVisualStyleBackColor = true;
            this.forwardButton.Paint += new System.Windows.Forms.PaintEventHandler(this.forwardButton_Paint);

        }

        public void generateNewBackButton()
        {
            backButton = new Button();
            this.backButton.FlatAppearance.BorderSize = 0;
            this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.backButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backButton.Location = new System.Drawing.Point(0, 0);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(50, 49);
            this.backButton.TabIndex = 2;
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Paint += new System.Windows.Forms.PaintEventHandler(this.backButton_Paint);
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
            changeTabs(currentTab);

        }

        public void SetTabs(List<Tab> newTabList)
        {
            tabs = newTabList;
            updateTabs();
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
            updateTabs();
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

        private void TabsButtonMouseDown(object sender, MouseEventArgs e)
        {
            Tab tab = getTabsButton((Button)sender);
            tab.isMouseDown = true;
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

        private void updateTabs()
        {
            if (owner.isDead) return;
            clearTabs();
            int currentXPos = 0;
            foreach (Tab tab in tabs)
            {
                Button btn = tab.GetButton();
                changeBtnLocation(btn, new Point(currentXPos, 0));
                addBtnControl(btn);
                currentXPos += btn.Width;
            }
            addBtnControl(newTabBtn);
            setNewTabButtonLocation(new Point(currentXPos, newTabBtn.Location.Y));
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

        delegate void SetTabsCallback(List<Tab> tabs);


        delegate void SetPointCallback(Point point);
        private void setNewTabButtonLocation(Point point)
        {
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
            btn.MouseDown += new MouseEventHandler(TabsButtonMouseDown);
            btn.MouseUp += new MouseEventHandler(TabsButtonMouseUp);
            btn.MouseMove += new MouseEventHandler(owner.TabsButtonMouseMove);

            btn.ForeColor = owner.foreColor;
            btn.BackColor = owner.backColor;

            Button closeBtn = newTab.GetCloseButton();
            closeBtn.Click += new EventHandler(closeTab);

            Tabs.Controls.Add(btn);
            tabs.Add(newTab);
            updateTabs();
            return newTab;
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
            if (backButton.InvokeRequired)
            {
                var d = new SetBoolCallback(setBackButtonEnabled);
                backButton.Invoke(d, new object[] { enabled });
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
            if (forwardButton.InvokeRequired)
            {
                var d = new SetColorCallback(setForwardButtonForeColor);
                forwardButton.Invoke(d, new object[] { color });
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

    }

}
