using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SDDBrowser;

namespace SDDTabs
{
    public class Tab
    {
        readonly ChromiumWebBrowser browser;
        readonly RoundedButton button;
        readonly Button closeButton = new Button();
        List<string> history = new List<string>();
        int historyIndex = 0;
        public delegate void voidFunction();
        public bool isDragging = false;
        public bool isMouseDown = false;
        public Point whereMouseDown;
        public Rectangle lastRectangle = new Rectangle();
        readonly voidFunction updateTabs;

        public Tab(ChromiumWebBrowser browser, voidFunction updateTabs)
        {
            this.updateTabs = updateTabs;
            this.browser = browser;
            button = new RoundedButton(20, 3);
            this.browser.TitleChanged += new EventHandler<TitleChangedEventArgs>(this.Browser_OnTitleChange);
            button.Controls.Add(closeButton);
            SDDWebBrowser.Main.AddHoverColorToButton(button);
            closeButton.Size = new Size(20, 20);
            closeButton.TextAlign = ContentAlignment.MiddleCenter;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.MouseHover += delegate (object sender, EventArgs e)
            {
                closeButton.Visible = true;
                closeButton.BringToFront();
            };
            closeButton.MouseLeave += delegate (object sender, EventArgs e)
            {
                closeButton.SendToBack();
            };
            closeButton.Paint += CloseButton_Paint;
            SetButtonText("loading...");
        }

        private void CloseButton_Paint(object sender, PaintEventArgs e)
        {
            Brush brush = new SolidBrush(closeButton.ForeColor);
            SizeF size = e.Graphics.MeasureString("x", closeButton.Font);
            e.Graphics.DrawString("x", closeButton.Font, brush, (closeButton.Width - size.Width) /2, (closeButton.Height - size.Height) / 2);
        }

        internal RoundedButton GetButton()
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

        public List<String> GetHistory()
        {
            return history;
        }

        public void SetHistory(List<string> newHistory)
        {
            history = newHistory;
        }

        public int GetHistoryIndex()
        {
            return historyIndex;
        }

        public void SetHistoryIndex(int index)
        {
            historyIndex = index;
        }

        delegate void SetStringCallback(string text);

        public void SetButtonText(string text)
        {
            if (button.InvokeRequired)
            {
                var d = new SetStringCallback(SetButtonText);
                try
                {
                    button.Invoke(d, new object[] { text });
                }
                catch (ObjectDisposedException)
                {

                }
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
                closeButton.BringToFront();
            }
        }

        private void Browser_OnTitleChange(object sender, TitleChangedEventArgs e)
        {
            SetButtonText(e.Title);
            updateTabs();
        }

    }
}
