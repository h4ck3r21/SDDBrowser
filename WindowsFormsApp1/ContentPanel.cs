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

namespace SDDBrowser
{
    internal class ContentPanel
    {
        ChromiumWebBrowser currentPage;
        string defaultURL = "www.google.com";
        Tab currentTab;
        Panel Content;
        Panel Tabs;
        TextBox textURL;
        Button searchIcon;
        Button reloadButton;
        Button forwardButton;
        Button backButton;
        Panel contentHeader;
        List<Tab> tabs;
        Main owner;
        string position = "top";
        Button newTabBtn;
        string lastSite; 
        int historyStackIndex;
        bool fromHistory;
        List<string> historyStack;
        public ContentPanel(Tab tab, Rectangle area, Main form) 
        {
            Button btn = tab.GetButton();
            ChromiumWebBrowser browser = tab.GetBrowser();
            contentHeader = (Panel)form.Controls.Find("contentHeader", true)[0]; ;
            textURL = (TextBox)contentHeader.Controls.Find("textURL", true)[0];
            searchIcon = (Button)contentHeader.Controls.Find("searchIcon", true)[0];
            reloadButton = (Button)contentHeader.Controls.Find("reloadButton", true)[0];
            forwardButton = (Button)contentHeader.Controls.Find("forwardButton", true)[0];
            backButton = (Button)contentHeader.Controls.Find("backButton", true)[0];

            Tabs = new Panel
            {
                Height = Tabs.Height,
                Width = area.Width,
                Location = new Point(area.X, area.Y)
            };
            Content = new Panel
            {
                Width = area.Width,
                Height = area.Height - Tabs.Height,
                Location = new Point(area.X, area.Y + Tabs.Height),
            };
            Content.BringToFront();
            Tabs.BringToFront();
            
            btn.Parent.Controls.Remove(btn);
            browser.Parent.Controls.Remove(browser);
            Tabs.Controls.Add(btn);
            Content.Controls.Add(browser);

            owner = form;
            owner.Controls.Add(Tabs);
            owner.Controls.Add(Content);
            newTabBtn = owner.getNewTabButton();
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

        private void TabsButtonMouseUp(object sender, MouseEventArgs e)
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

        private void newTabBtn_Click(object sender, EventArgs e)
        {
            generateNewTab(defaultURL);
        }

        private void closeTab(object sender, EventArgs e)
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

        private Tab getTabsButton(Button btn)
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

        private void updateNavButtons()
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


    }
}
