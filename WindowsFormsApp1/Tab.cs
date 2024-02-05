using CefSharp;
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
        public Tab(ChromiumWebBrowser browser, Button btn) 
        {
            this.browser = browser;
            button = btn;
            int width;
            using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
            {
                SizeF size = graphics.MeasureString("Hello there", new Font("Segoe UI", 11, FontStyle.Regular, GraphicsUnit.Point));
                width = (int)size.Width;
            }
            button.Size = new Size(width, 50);
            this.browser.TitleChanged += new EventHandler<TitleChangedEventArgs>(this.browser_OnTitleChange);
        }

        public Button GetButton()
        {
            return button;
        }

        public ChromiumWebBrowser GetBrowser()
        {
            return browser;
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
                button.Width = button.Text.Length * 10 + 50;
            }
        }

        private void browser_OnTitleChange(object sender, TitleChangedEventArgs e)
        {
            SetButtonText(e.Title);
        }
    }
}
