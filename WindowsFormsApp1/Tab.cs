﻿using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDDBrowser
{
    internal class Tab
    {
        ChromiumWebBrowser browser;
        Button button;
        Button closeButton = new Button();
        List<string> history = new List<string>();
        int historyIndex = 0;
        public delegate void voidFunction();
        voidFunction updateTabs;

        public Tab(ChromiumWebBrowser browser, voidFunction updateTabs) 
        {
            this.updateTabs = updateTabs;
            this.browser = browser;
            button = new Button();
            this.browser.TitleChanged += new EventHandler<TitleChangedEventArgs>(this.browser_OnTitleChange);
            button.Controls.Add(closeButton);
            closeButton.Text = "x";
            closeButton.Size = new Size(30, 30);
            closeButton.TextAlign = ContentAlignment.MiddleCenter;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderSize = 0;
        }

        public Button GetButton()
        {
            return button;
        }

        public ChromiumWebBrowser GetBrowser()
        {
            return browser;
        }

        public Button GetCloseButton()
        {
            return closeButton;
        }

        public List<String> getHistory() 
        {
            return history;
        }

        public void setHistory(List<string> newHistory)
        {
            history = newHistory;
        }

        public int getHistoryIndex()
        {
            return historyIndex;
        }

        public void setHistoryIndex(int index)
        {
            historyIndex = index;
        }

        delegate void SetStringCallback(string text);

        public void SetButtonText(string text) 
        {
            if (button.InvokeRequired)
            {
                var d = new SetStringCallback(SetButtonText);
                button.Invoke(d, new object[] { text });
            }
            else
            {
                button.Text = text;
                int width;
                using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
                {
                    SizeF size = graphics.MeasureString(button.Text, button.Font);
                    width = (int)size.Width + 100;
                }
                button.Size = new Size(width, 50);
                closeButton.Location = new Point(button.Width - 40, 10);
            }
        }

        private void browser_OnTitleChange(object sender, TitleChangedEventArgs e)
        {
            SetButtonText(e.Title);
            updateTabs();
        }
    }
}
