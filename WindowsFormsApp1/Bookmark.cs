using SDDWebBrowser;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SDDBrowser
{
    public class Bookmark
    {
        public Button button;
        public Button closeButton = new Button();
        public string url;
        public string title;
        public BookmarkJson JSONRepresentation;
        internal ContentPanel contentHolder;

        internal Bookmark(string url, string title, ContentPanel cp)
        {
            Generate(url, title, cp);
        }

        internal void Generate(string url, string title, ContentPanel cp)
        {
            contentHolder = cp;
            this.url = url;
            this.title = title;
            if (title == "loading...")
            {
                title = url;
            }
            button = new Button
            {
                Text = title,
            };
            using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
            {
                SizeF size = graphics.MeasureString(button.Text, button.Font);
                Debug.WriteLine("text: {0}, font: {1}", button.Text, button.Font);
                button.Size = new Size((int)size.Width + 40, 40);
            }
            button.Controls.Add(closeButton);
            button.BackColor = cp.GetBackColor();
            button.TextAlign = ContentAlignment.MiddleLeft;
            closeButton.Size = new Size(20, 20);
            closeButton.TextAlign = ContentAlignment.MiddleCenter;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Location = new Point(button.Width - 30, 10);
            closeButton.Paint += CloseButton_Paint;
            closeButton.Click += cp.Bookmark_Close;
            button.Click += cp.Bookmark_OnClick;
            Main.AddHoverColorToButton(button);

            SetJSON();
        }

        private void CloseButton_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(closeButton.ForeColor, 2);
            Brush brush = new SolidBrush(closeButton.ForeColor);
            int margin = 0;
            int fontSize = 8;
            Font font = new Font(closeButton.Font.FontFamily, fontSize);
            e.Graphics.DrawString("x", font, brush, margin, margin);
            //e.Graphics.DrawLine(pen, margin, margin, closeButton.Width - margin, closeButton.Height - margin);
            //e.Graphics.DrawLine(pen, margin, closeButton.Height - margin, closeButton.Width - margin, margin);
            pen.Dispose();
        }

        internal Bookmark(BookmarkJson json, ContentPanel cp)
        {
            contentHolder = cp;
            url = json.Url;
            title = json.Name;
            Generate(url, title, cp);
        }

        internal Bookmark(string HTML, ContentPanel cp)
        {
            url = ContentPanel.GetStringBetween("href=\"", "\"", HTML);
            title = ContentPanel.GetStringBetween(">", "</a>", HTML);
            contentHolder = cp;
            Generate(url, title, cp);
        }

        private void SetJSON()
        {
            JSONRepresentation = new BookmarkJson
            {
                Name = title,
                Url = url,
            };
        }

        public BookmarkJson GetJSON()
        {
            SetJSON();
            return JSONRepresentation;
        }

        public string ToHTML()
        {
            return
                $@"<dt>
                    <a href=""{url}"" >{title}</a>
                </dt>";
        }

        public void Close()
        {
            button.Dispose();
        }

        //alternative generation maybe.
    }
}
