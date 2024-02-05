using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using SDDBrowser;
using Windows.System;
using Windows.UI.ViewManagement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace WindowsFormsApp1
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        ChromiumWebBrowser currentPage;
        List<string> domains;
        List<string> historyStack;
        List<Tab> tabs = new List<Tab>();
        string lastSite;
        int historyStackIndex;
        bool fromHistory;
        Size lastSize;
        bool loaded;
        protected bool isDragging = false;
        bool wasFullScreen = false;
        protected Rectangle lastRectangle = new Rectangle();
        int resizeWidth = 3;
        int edgeResizeWidth = 50;
        List<Control> edges = new List<Control>();
        UISettings uiSettings;
        Color accentColor;
        Color backColor;
        Color foreColor;
        string edgeSnap = "None";
        Size oldSize;
        bool snapped = false;
        Button newTabButton;


        // window
        private void Main_Load(object sender, EventArgs e)
        {
            currentPage = new ChromiumWebBrowser();
            currentPage.LoadUrl(textURL.Text);
            Content.Controls.Add(currentPage);
            currentPage.Dock = DockStyle.Fill;

            tabs.Add(new Tab(currentPage, new Button()));
            
            updateTabs();

            currentPage.AddressChanged += new EventHandler<AddressChangedEventArgs>(browser_AddressChanged);
            currentPage.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(browser_FrameLoadEnd);
            currentPage.FrameLoadStart += new EventHandler<FrameLoadStartEventArgs>(browser_FrameLoadStart);
            currentPage.TitleChanged += delegate (object titleSender, TitleChangedEventArgs titleArgs)
            {
                currentPage.Name = titleArgs.Title;
            };
            getColours();

            StreamReader reader = File.OpenText("domain_names.csv");
            domains = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                domains.Add(values[0]);
            }

            historyStack = new List<string>();
            historyStackIndex = 0;
            fromHistory = false;
            lastSize = base.Size;
            loaded = true;
            initialiseFormEdge();
            updateNavButtons();
        }


        // https://stackoverflow.com/questions/22780571/scale-windows-forms-window
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (loaded)
            {
                ResizeWidthControl(Content, base.Size);
                ResizeHeightControl(Content, base.Size);
                ResizeWidthControl(contentHeader, base.Size);
                ResizeWidthControl(textURL, base.Size);
                ResizeWidthControl(Tabs, base.Size);
                RepositionWidthPosition(closeButton, base.Size);
                RepositionWidthPosition(maximiseButton, base.Size);
                RepositionWidthPosition(minimiseButton, base.Size);
                lastSize = base.Size;
                if (WindowState == FormWindowState.Maximized)
                {
                    foreach (Control edge in edges)
                    {
                        edge.Enabled = false;
                        edge.Visible = false;
                    }
                }
                else
                {
                    foreach (Control edge in edges)
                    {
                        edge.Enabled = true;
                        edge.Visible = true;
                    }
                }
            }
        }

        private void ResizeWidthControl(Control control, Size newSize)
        {
            int width = newSize.Width - lastSize.Width;
            control.Width += width;
        }

        private void ResizeHeightControl(Control control, Size newSize)
        {
            int height = newSize.Height - lastSize.Height;
            control.Height += height;
        }

        private void RepositionWidthPosition(Control control, Size newSize)
        {
            int width = newSize.Width - lastSize.Width;
            control.Left += width;
        }

        protected void initialiseFormEdge()
        {
            Color borderColor = accentColor;
            MouseDown += new MouseEventHandler(form_MouseDown);
            MouseMove += new MouseEventHandler(form_MouseMove);
            MouseUp += form_MouseUp;

            // bottom
            UserControl uc1 = new UserControl()
            {
                Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
                Height = resizeWidth,
                Width = DisplayRectangle.Width - (resizeWidth * 2),
                Left = resizeWidth,
                Top = DisplayRectangle.Height - resizeWidth,
                BackColor = borderColor,
                Cursor = Cursors.SizeNS,
                Name = "bottom"
            };
            uc1.MouseDown += form_MouseDown;
            uc1.MouseUp += mouseUp;
            uc1.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    Size = new Size(lastRectangle.Width, e.Y - lastRectangle.Y + Height);
                }
            };
            uc1.BringToFront();
            edges.Add(uc1);
            Controls.Add(uc1);

            // right
            UserControl uc2 = new UserControl()
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom),
                Height = DisplayRectangle.Height - (resizeWidth * 2),
                Width = resizeWidth,
                Left = DisplayRectangle.Width - resizeWidth,
                Top = resizeWidth,
                BackColor = borderColor,
                Cursor = Cursors.SizeWE,
                Name = "right"
            };
            uc2.MouseDown += form_MouseDown;
            uc2.MouseUp += mouseUp;
            uc2.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    Size = new Size(e.X - lastRectangle.X + Width, lastRectangle.Height);
                }
            };
            uc2.BringToFront();
            edges.Add(uc2);
            Controls.Add(uc2);

            // bottom-right
            UserControl uc3 = new UserControl()
            {
                Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
                Height = resizeWidth,
                Width = resizeWidth,
                Left = DisplayRectangle.Width - resizeWidth,
                Top = DisplayRectangle.Height - resizeWidth,
                BackColor = borderColor,
                Cursor = Cursors.SizeNWSE,
                Name = "bottomRight"
            };
            uc3.MouseDown += form_MouseDown;
            uc3.MouseUp += mouseUp;
            uc3.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    Size = new Size((e.X - lastRectangle.X + Width), (e.Y - lastRectangle.Y + Height));
                }
            };
            uc3.BringToFront();
            edges.Add(uc3);
            Controls.Add(uc3);

            // top-right
            UserControl uc4 = new UserControl()
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Right),
                Height = resizeWidth,
                Width = resizeWidth,
                Left = DisplayRectangle.Width - resizeWidth,
                Top = 0,
                BackColor = borderColor,
                Cursor = Cursors.SizeNESW,
                Name = "topRight"
            };
            uc4.MouseDown += form_MouseDown;
            uc4.MouseUp += mouseUp;
            uc4.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    int diff = (e.Location.Y - lastRectangle.Y);
                    int y = (Location.Y + diff);
                    int new_height = Height - diff;
                    if (!(diff > 0 && MinimumSize.Height == Height))
                    {
                        Location = new Point(Location.X, y);
                    }
                    else
                    {
                        new_height = Height;
                    }
                    Size = new Size(e.X - lastRectangle.X + Width, new_height);
                }
            };
            uc4.BringToFront();
            edges.Add(uc4);
            Controls.Add(uc4);

            // top
            UserControl uc5 = new UserControl()
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
                Height = resizeWidth,
                Width = DisplayRectangle.Width - (resizeWidth * 2),
                Left = resizeWidth,
                Top = 0,
                BackColor = borderColor,
                Cursor = Cursors.SizeNS,
                Name = "top"
            };
            uc5.MouseDown += form_MouseDown;
            uc5.MouseUp += mouseUp;
            uc5.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    int diff = (e.Location.Y - lastRectangle.Y);
                    int y = (Location.Y + diff);

                    if (!(diff > 0 && MinimumSize.Height == Height))
                    {
                        Location = new Point(Location.X, y);
                        Size = new Size(lastRectangle.Width, Height - diff);
                    }
                }
            };
            uc5.BringToFront();
            edges.Add(uc5);
            Controls.Add(uc5);

            // left
            UserControl uc6 = new UserControl()
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom),
                Height = DisplayRectangle.Height - (resizeWidth * 2),
                Width = resizeWidth,
                Left = 0,
                Top = resizeWidth,
                BackColor = borderColor,
                Cursor = Cursors.SizeWE,
                Name = "left"
            };
            uc6.MouseDown += form_MouseDown;
            uc6.MouseUp += mouseUp;
            uc6.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    int diff = (e.Location.X - lastRectangle.X);
                    int x = (Location.X + diff);

                    if (!(diff > 0 && MinimumSize.Width == Width))
                    {
                        Location = new Point(x, Location.Y);
                        Size = new Size((Width - diff), Height);
                    }
                    
                }
            };
            uc6.BringToFront();
            edges.Add(uc6);
            Controls.Add(uc6);

            // bottom-left
            UserControl uc7 = new UserControl()
            {
                Anchor = (AnchorStyles.Bottom | AnchorStyles.Left),
                Height = resizeWidth,
                Width = resizeWidth,
                Left = 0,
                Top = DisplayRectangle.Height - resizeWidth,
                BackColor = borderColor,
                Cursor = Cursors.SizeNESW,
                Name = "bottomLeft"
            };
            uc7.MouseDown += form_MouseDown;
            uc7.MouseUp += mouseUp;
            uc7.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    int diff = (e.Location.X - lastRectangle.X);
                    int x = (Location.X + diff);

                    int new_width = Width - diff;
                    if (!(diff > 0 && MinimumSize.Width == Width))
                    {
                        Location = new Point(x, Location.Y);
                    }
                    else
                    {
                        new_width = Width;
                    }
                    Size = new Size(new_width, (e.Y - lastRectangle.Y + Height));
                }
            };
            uc7.BringToFront();
            edges.Add(uc7);
            Controls.Add(uc7);

            // top-left
            UserControl uc8 = new UserControl()
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Left),
                Height = resizeWidth,
                Width = resizeWidth,
                Left = 0,
                Top = 0,
                BackColor = borderColor,
                Cursor = Cursors.SizeNWSE,
                Name = "topLeft"
            };
            uc8.MouseDown += form_MouseDown;
            uc8.MouseUp += mouseUp;
            uc8.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    int dX = (e.Location.X - lastRectangle.X);
                    if (dX > 0 && MinimumSize.Width == Width)
                    {
                        dX = 0;

                    }
                    int dY = (e.Location.Y - lastRectangle.Y);
                    if (dY > 0 && MinimumSize.Height == Height)
                    {
                        dY = 0;

                    }
                    int x = (this.Location.X + dX);
                    int y = (this.Location.Y + dY);
                    

                    this.Location = new Point(x, y);
                    this.Size = new Size((this.Width - dX), (this.Height - dY));
                }

            };
            uc8.BringToFront();
            edges.Add(uc8);
            this.Controls.Add(uc8);
        }


        private void form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastRectangle = new Rectangle(e.Location.X, e.Location.Y, this.Width, this.Height);
                if (snapped)
                {
                    snapped = false;
                    Size = oldSize;
                    Console.WriteLine(snapped);
                }
            }
        }

        private void form_MouseMove(object sender, MouseEventArgs e)
        {
            if (wasFullScreen && isDragging)
            {
                form_MouseDown(sender, e);
                wasFullScreen = false;
            }
            if (isDragging && WindowState == FormWindowState.Normal)
            {
                int x = (Location.X + (e.Location.X - lastRectangle.X));
                int y = (Location.Y + (e.Location.Y - lastRectangle.Y));

                Location = new Point(x, y);
            }
            else if (isDragging && WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                Location = new Point(e.Location.X - Width/2, e.Location.Y);
                wasFullScreen = true;
            }
        }

        delegate void nearEdgeFunction(Rectangle rectangle);
        private void checkEdgeResizing(object sender, MouseEventArgs e, nearEdgeFunction edgeFunction)
        {
            bool[] nearEdges = new bool[4];
            Rectangle screenbounds = Screen.GetWorkingArea(MousePosition);
            Point screenMousePosition = new Point(MousePosition.X - screenbounds.X, MousePosition.Y - screenbounds.Y);
            nearEdges[0] = screenMousePosition.X < edgeResizeWidth;
            nearEdges[1] = screenMousePosition.Y < edgeResizeWidth;
            nearEdges[2] = screenMousePosition.X > screenbounds.Width - edgeResizeWidth;
            nearEdges[3] = screenMousePosition.Y > screenbounds.Height - edgeResizeWidth;
            bool[] topLeft = { true, true, false, false };
            bool[] topRight = { false, true, true, false };
            bool[] bottomLeft = { true, false, false, true };
            bool[] bottomRight = { false, false, true, true };
            bool[] left = { true, false, false, false };
            bool[] top = { false, true, false, false };
            bool[] right = { false, false, true, false };
            bool[] bottom = { false, false, false, true };
            Rectangle rectangle;

            if (nearEdges.SequenceEqual(topLeft))
            {
                edgeSnap = "topLeft";
                rectangle = new Rectangle(screenbounds.X, screenbounds.Y, screenbounds.Width/2, screenbounds.Height/2);
                edgeFunction(rectangle);

            }
            else if (nearEdges.SequenceEqual(topRight))
            {
                edgeSnap = "topRight";
                rectangle = new Rectangle(screenbounds.X + screenbounds.Width / 2, screenbounds.Y, screenbounds.Width / 2, screenbounds.Height / 2);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(bottomLeft))
            {
                edgeSnap = "bottomLeft";
                rectangle = new Rectangle(screenbounds.X, screenbounds.Y + screenbounds.Height / 2, screenbounds.Width / 2, screenbounds.Height / 2);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(bottomRight))
            {
                edgeSnap = "bottomRight";
                rectangle = new Rectangle(screenbounds.X + screenbounds.Width / 2, screenbounds.Y + screenbounds.Height / 2, screenbounds.Width / 2, screenbounds.Height / 2);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(left))
            {
                edgeSnap = "Left";
                rectangle = new Rectangle(screenbounds.X, screenbounds.Y, screenbounds.Width / 2, screenbounds.Height);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(right))
            {
                edgeSnap = "Right";
                rectangle = new Rectangle(screenbounds.X + screenbounds.Width / 2, screenbounds.Y, screenbounds.Width / 2, screenbounds.Height);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(top))
            {
                edgeSnap = "top";
                rectangle = new Rectangle(screenbounds.X, screenbounds.Y, screenbounds.Width, screenbounds.Height);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(bottom))
            {
                edgeSnap = "Bottom";
            }
            else
            {
                edgeSnap = "None";
            }
            //Console.WriteLine(edgeSnap);
            //Console.WriteLine("{0,1}, {1,1}, {2,1}, {3,1}", nearEdges[0], nearEdges[1], nearEdges[2], nearEdges[3]);
        }

        private void drawRectangle(Rectangle rectangle, Color colour)
        {
            Control control = new Control();
            
            Graphics graphics = control.CreateGraphics();
            Brush brush = new SolidBrush(colour);
            graphics.FillRectangle(brush, rectangle);

        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void form_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            nearEdgeFunction resizeWindow = delegate (Rectangle rectangle)
            {
                oldSize = Size;
                Console.WriteLine(snapped);
                if (this.edgeSnap == "top")
                {
                    this.WindowState = FormWindowState.Maximized;
                }
                else {
                    Bounds = rectangle;
                }
                snapped = true;
                Console.WriteLine(snapped);
            };
            checkEdgeResizing(sender, e, resizeWindow);
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

            var errorHtml = string.Format("<html><body><h2>Failed to load URL {0} with error {1} ({2}).</h2></body></html>",
                                              e.FailedUrl, e.ErrorText, e.ErrorCode);

            _ = e.Browser.SetMainFrameDocumentContentAsync(errorHtml);

            //AddressChanged isn't called for failed Urls so we need to manually update the Url TextBox
            textURL.Text = e.FailedUrl;
        }

        private void browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            setTextURL(e.Address);
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
                    }
                }
                fromHistory = false;
                updateNavButtons();

            }
            lastSite = e.Address;
        }

        delegate void SetTextCallback(string text);
        private void setTextURL(string text)
        {
            if (textURL.InvokeRequired)
            {
                var d = new SetTextCallback(setTextURL);
                textURL.Invoke(d, new object[] { text });
            }
            else
            {
                textURL.Text = text;
            }
        }

        private void browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {

        }

        private void browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {

        }

        // colours
        private void getColours()
        {
            uiSettings = new UISettings();
            var windowsAccentColor = uiSettings.GetColorValue(UIColorType.Accent);
            accentColor = convertColor(windowsAccentColor);
            var windowsBackColor = uiSettings.GetColorValue(UIColorType.Background);
            var windowsForeColor = uiSettings.GetColorValue(UIColorType.Foreground);
            backColor = convertColor(windowsBackColor);
            foreColor = convertColor(windowsForeColor);
            foreach (Control cnt in getChildControls(this))
            {
                cnt.BackColor = backColor;
                cnt.ForeColor = foreColor;
            }
        }

        private Color convertColor(Windows.UI.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private List<Control> getChildControls(Control control)
        {
            List<Control> controls = new List<Control>();
            controls.Add(control);
            foreach (Control cnt in control.Controls)
            {
                controls.AddRange(getChildControls(cnt));
            }
            return controls;

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

        // navigation

        private void updateNavButtons()
        {
            setBackButtonEnabled(historyStackIndex > 1);
            setForwardButtonEnabled(historyStackIndex < historyStack.Count);
            if (backButton.Enabled)
            {
                setBackButtonForeColor(foreColor);
            }
            else
            {
                setBackButtonForeColor(Color.Gray);
            }

            if (forwardButton.Enabled)
            {
                setForwardButtonForeColor(foreColor);
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
                Console.WriteLine(textURL.Text);
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
                searchIcon.Text = "W";
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
                || (domains.Any(x => url.Contains(x))
                && char.IsLetterOrDigit(url[0]))
                || Regex.IsMatch(url, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")))
                || Directory.Exists(url);
            return result;
        }


        //buttons
        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void closeButton_Enter(object sender, EventArgs e)
        {
            closeButton.BackColor = Color.Gray;
        }

        private void closeButton_Leave(object sender, EventArgs e)
        {
            closeButton.BackColor = backColor;
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            historyStackIndex--;
            fromHistory = true;
            currentPage.LoadUrl(historyStack[historyStackIndex - 1]);
        }

        private void backButton_Hover(object sender, EventArgs e)
        {
            closeButton.BackColor = Color.Gray;
        }

        private void backButton_Leave(object sender, EventArgs e)
        {
            closeButton.BackColor = backColor;
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            historyStackIndex++;
            fromHistory = true;
            currentPage.LoadUrl(historyStack[historyStackIndex - 1]);
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            currentPage.Reload();
        }

        private void maximiseButton_Click(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
                if (snapped)
                {
                    snapped = false;
                    Size = oldSize;
                    Console.WriteLine(snapped);
                }
            }
            
        }

        private void minimiseButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        // tabs

        private void updateTabs()
        {
            Tabs.Controls.Clear();
            int currentXPos = 0;
            foreach (Tab tab in tabs) 
            {
                Button btn = tab.GetButton();
                btn.Location = new Point(currentXPos, 0);
                Tabs.Controls.Add(btn);
            }
        }

        private void addTab(ChromiumWebBrowser tab)
        {
            Button btn = new Button();
            btn.Click += new EventHandler(changeTabs);
            btn.ForeColor = foreColor;
            btn.BackColor = backColor;
            Tabs.Controls.Add(btn);
        }

        private void changeTabs(object sender, EventArgs e)
        {

        }

        private void onBrowserTitleChange(object sender, TitleChangedEventArgs e)
        {
            Console.WriteLine(e.Title);
        }

    }
}
